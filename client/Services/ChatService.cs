using System.Net.Sockets;
using System.Text.Json;
using Chat.Models;

namespace Chat.Services;
public sealed class ChatService : IChatService
{
    public const int DefaultPort = 11_000;
    public Room GeneralRoom => Rooms[0];
    public List<Room> Rooms { get; } = [
        new()
        {
            Name = "General",
            Description = "The default room for all users."
        }
    ];
    private TcpClient? _client;
    private StreamWriter? _writer;
    private string _userName = "Anonymous";
    public event Action<Message>? MessageReceived;
    public event Action? Disconnected;

    public string UserName {
        get => _userName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("User name cannot be null or whitespace.", nameof(value));
            _userName = value;
        }}

    public bool IsConnected => _client?.Connected ?? false;

    public void CreateRoom(string name, string description, int? code)
    {
        if(code < 1000 || code > 9999)
            throw new ArgumentOutOfRangeException(nameof(code), "Room code must be between 1000 and 9999.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room name cannot be null or whitespace.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Room description cannot be null or whitespace.", nameof(description));
        Rooms.Add(new Room { Name = name, Description = description, Code = code ?? null });
    }

    public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(host, port, cancellationToken);

        var stream = _client.GetStream();
        var reader = new StreamReader(stream);
        _writer = new StreamWriter(stream) { AutoFlush = true };

        _ = ReceiveLoopAsync(reader);
    }

    public async Task SendAsync(string message, Guid? roomId)
    {
        if (_writer is null)
            throw new InvalidOperationException("Not connected. Call ConnectAsync first.");

        var chatMessage = new Message(UserName, message, timestamp: DateTimeOffset.UtcNow, roomid: roomId ?? GeneralRoom.Id);
        await _writer.WriteLineAsync(JsonSerializer.Serialize(chatMessage));
    }

    private async Task ReceiveLoopAsync(StreamReader reader)
    {
        try
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var message = JsonSerializer.Deserialize<Message>(line);
                if (message is not null) MessageReceived?.Invoke(message);
            }
        }
        catch (IOException)
        {
        }
        finally
        {
            Disconnected?.Invoke();
        }
    }

    public static bool TryParseServerAddress(string value, out string host, out int port)
    {
        host = value.Trim();
        port = DefaultPort;
        if (host.Length == 0) return false;

        var parts = host.Split(':');
        if (parts.Length == 1)
        {
            host = parts[0];
            return host.Length > 0;
        }
        if (parts.Length == 2)
        {
            host = parts[0];
            return host.Length > 0 && int.TryParse(parts[1], out port);
        }
        return false;
    }

    public ValueTask DisposeAsync()
    {
        _writer?.Dispose();
        _client?.Dispose();
        return ValueTask.CompletedTask;
    }

    public Guid GetRoomIdByName(string roomName)
    {
        var room = Rooms.FirstOrDefault(r => r.Name.Equals(roomName, StringComparison.OrdinalIgnoreCase));
        if (room is null)
            throw new ArgumentException($"Room with name '{roomName}' not found.", nameof(roomName));
        return room.Id;
    }

    public Room GetRoomById(Guid roomId)
    {
        var room = Rooms.FirstOrDefault(r => r.Id == roomId);
        if (room is null)
            throw new ArgumentException($"Room with ID '{roomId}' not found.", nameof(roomId));
        return room;
    }

    public void JoinRoom(Guid roomId, string userName, int? code)
    {
        var room = GetRoomById(roomId);
        if (!room.Members.Contains(userName))
        {
            if (room.Code is not null && room.Code != code)
                throw new UnauthorizedAccessException("Incorrect code for private room.");
            room.Members.Add(userName);
        }
    }

}
