using Chat.Models;

namespace Chat.Services;

public interface IChatService : IAsyncDisposable
{
    Room GeneralRoom {get; }
    List<Room> Rooms {get; }
    event Action<Message>? MessageReceived;
    event Action? Disconnected;
    string UserName { get; set;}
    bool IsConnected { get; }
    Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default);
    Task SendAsync(string message, Guid? roomId);
    Guid GetRoomIdByName(string roomName);
    Room GetRoomById(Guid roomId);
    void JoinRoom(Guid roomId, string userName, int? code);
    void CreateRoom(string name, string description, int? code);
}
