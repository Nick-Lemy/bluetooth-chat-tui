namespace Chat.Models;

public class Message(
    string sender,
    string text,
    Guid roomid,
    DateTimeOffset timestamp = default)
{
    public string Sender { get; } = sender;
    public string Text { get; } = text;
    public Guid RoomId { get; } = roomid;
    public DateTimeOffset Timestamp { get; } = timestamp == default ? DateTimeOffset.UtcNow : timestamp;
}
