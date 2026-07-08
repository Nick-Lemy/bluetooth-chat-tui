namespace Chat.Models;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int? Code { get; set; }
    public List<string> Members { get; set; } = [];
    public List<Message> Messages { get; set; } = [];
}
