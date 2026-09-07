namespace KeyGo.Core.Models;

public sealed class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Text";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Conversation? Conversation { get; set; }
}
