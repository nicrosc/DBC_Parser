namespace ZadatakJuric.Domain.Entities;

public class Message
{
    public int Id { get; set; } // EF Core primary key
    public uint MessageId { get; set; } // Original CAN message ID
    public string Name { get; set; } = string.Empty;
    public byte Size { get; set; } // Message size in bytes
    public string Sender { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public List<Signal> Signals { get; set; } = new();
    public List<Attribute> Attributes { get; set; } = new();
    
    // EF Core navigation properties
    public int NetworkId { get; set; }
    public Network Network { get; set; } = null!;

    public void AddSignal(Signal signal)
    {
        if (Signals.Any(s => s.Name.Equals(signal.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Signal with name '{signal.Name}' already exists in message '{Name}'");
        
        // Validate signal fits within message size
        var maxBitPosition = signal.StartBit + signal.Length - 1;
        var requiredBytes = (maxBitPosition / 8) + 1;
        
        if (requiredBytes > Size)
            throw new InvalidOperationException($"Signal '{signal.Name}' exceeds message size. Required: {requiredBytes} bytes, Available: {Size} bytes");
        
        Signals.Add(signal);
    }

    public Signal? GetSignalByName(string signalName)
    {
        return Signals.FirstOrDefault(s => s.Name.Equals(signalName, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsValidMessageId()
    {
        // Standard CAN ID: 0-0x7FF (11-bit)
        // Extended CAN ID: 0-0x1FFFFFFF (29-bit)
        return MessageId <= 0x1FFFFFFF;
    }
} 