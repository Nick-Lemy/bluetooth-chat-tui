using Terminal.Gui.App;
using Terminal.Gui.Views;
using Chat.Models;
using Chat.Services;
using Chat.Ui;

using var app = Application.Create();
app.Init();

var chat = new ChatService();
try
{
    var session = Connect(app, chat);
    if (session is null) return;

    var (rooms, room, members) = session.Value;
    using var window = new ChatWindow(app, chat, rooms, room, members);
    window.Run();
}
finally
{
    chat.DisposeAsync().GetAwaiter().GetResult();
}

static (RoomInfo[] Rooms, RoomInfo Room, string[] Members)? Connect(IApplication app, IChatService chat)
{
    var name = string.Empty;
    var address = "localhost";

    while (true)
    {
        var login = LoginDialog.Show(app, name, address);
        if (login is null) return null;

        (name, address) = login.Value;

        if (!ServerAddress.TryParse(address, out var host, out var port))
        {
            MessageBox.ErrorQuery(app, "Invalid address", "Enter a host or host:port.", "OK");
            continue;
        }

        try
        {
            chat.UserName = name;
            chat.ConnectAsync(host, port).GetAwaiter().GetResult();

            var rooms = chat.GetRoomsAsync().GetAwaiter().GetResult();
            if (rooms.Length == 0)
            {
                MessageBox.ErrorQuery(app, "No rooms", "The server has no rooms.", "OK");
                return null;
            }

            var initial = rooms.FirstOrDefault(r => !r.IsPrivate) ?? rooms[0];
            var joined = chat.JoinRoomAsync(initial.Id, null).GetAwaiter().GetResult();

            return (rooms, joined.Room, joined.Members);
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery(app, "Cannot connect", ex.Message, "OK");
        }
    }
}
