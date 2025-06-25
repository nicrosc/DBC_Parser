namespace ZadatakJuric.Domain.Entities;

public class DBC
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty; // For duplicate detection
    
    // Navigation properties
    public List<Network> Networks { get; set; } = new();
    public List<Attribute> Attributes { get; set; } = new();

    // Computed properties
    public int TotalMessages => Networks.Sum(n => n.Messages.Count);
    public int TotalSignals => Networks.SelectMany(n => n.Messages).Sum(m => m.Signals.Count);
    public int TotalAttributes => Attributes.Count + Networks.Sum(n => n.Attributes.Count + 
        n.Messages.Sum(m => m.Attributes.Count + m.Signals.Sum(s => s.Attributes.Count)));

    public void AddNetwork(Network network)
    {
        if (Networks.Any(n => n.Name.Equals(network.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Network with name '{network.Name}' already exists in DBC file");
        
        Networks.Add(network);
    }

    public Network? GetNetworkByName(string networkName)
    {
        return Networks.FirstOrDefault(n => n.Name.Equals(networkName, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<Message> GetAllMessages()
    {
        return Networks.SelectMany(n => n.Messages);
    }

    public IEnumerable<Signal> GetAllSignals()
    {
        return Networks.SelectMany(n => n.GetAllSignals());
    }

    public Message? GetMessageById(uint messageId)
    {
        return Networks.SelectMany(n => n.Messages)
                      .FirstOrDefault(m => m.MessageId == messageId);
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

    public bool ValidateIntegrity()
    {
        // Check for duplicate message IDs across all networks
        var allMessages = GetAllMessages().ToList();
        var messageIds = allMessages.Select(m => m.MessageId).ToList();
        return messageIds.Count == messageIds.Distinct().Count();
    }
} 