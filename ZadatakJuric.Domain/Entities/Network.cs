namespace ZadatakJuric.Domain.Entities;

public class Network
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Message> Messages { get; set; } = new();
    public List<Attribute> Attributes { get; set; } = new();
    
    // EF Core navigation properties
    public int DBCId { get; set; }
    public DBC DBC { get; set; } = null!;

    public void AddMessage(Message message)
    {
        if (Messages.Any(m => m.MessageId == message.MessageId))
            throw new InvalidOperationException($"Message with ID {message.MessageId} already exists in network");
        
        Messages.Add(message);
    }

    public Message? GetMessageById(uint messageId)
    {
        return Messages.FirstOrDefault(m => m.MessageId == messageId);
    }

    public IEnumerable<Signal> GetAllSignals()
    {
        return Messages.SelectMany(m => m.Signals);
    }

    public IEnumerable<Signal> SearchSignals(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<Signal>();

        return GetAllSignals()
            .Where(s => s.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       s.Unit.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       s.Comment.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }
} 