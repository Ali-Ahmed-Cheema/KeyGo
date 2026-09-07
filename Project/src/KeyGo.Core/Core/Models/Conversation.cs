namespace KeyGo.Core.Models;

public sealed class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "New Conversation";
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<ChatMessage> Messages { get; set; } = new();
}
