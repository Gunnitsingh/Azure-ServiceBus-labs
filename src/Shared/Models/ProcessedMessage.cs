namespace Shared.Models;

public class ProcessedMessage
{
    public string MessageId { get; set; } = string.Empty;

    public DateTime ProcessedAt { get; set; }
}