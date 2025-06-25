using ZadatakJuric.Domain.Enums;

namespace ZadatakJuric.Domain.Entities;

public class Signal
{
    public int Id { get; set; } // EF Core primary key
    public string Name { get; set; } = string.Empty;
    public byte StartBit { get; set; }
    public byte Length { get; set; } // Length in bits
    public ByteOrder ByteOrder { get; set; } = ByteOrder.LittleEndian;
    public SignalType ValueType { get; set; } = SignalType.Unsigned;
    public double Factor { get; set; } = 1.0;
    public double Offset { get; set; } = 0.0;
    public double Minimum { get; set; } = 0.0;
    public double Maximum { get; set; } = 0.0;
    public string Unit { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public List<string> Receivers { get; set; } = new();
    public List<Attribute> Attributes { get; set; } = new();
    
    // EF Core navigation properties
    public int MessageId { get; set; }
    public Message Message { get; set; } = null!;

    public double ConvertRawToPhysical(long rawValue)
    {
        return (rawValue * Factor) + Offset;
    }

    public long ConvertPhysicalToRaw(double physicalValue)
    {
        return (long)Math.Round((physicalValue - Offset) / Factor);
    }

    public bool IsValidValue(double value)
    {
        return value >= Minimum && value <= Maximum;
    }

    public long GetMaxRawValue()
    {
        return ValueType == SignalType.Signed 
            ? (1L << (Length - 1)) - 1 
            : (1L << Length) - 1;
    }

    public long GetMinRawValue()
    {
        return ValueType == SignalType.Signed 
            ? -(1L << (Length - 1)) 
            : 0;
    }

    public bool IsValidSignalLayout()
    {
        // Check if signal fits within 64 bits (typical CAN message constraint)
        return StartBit + Length <= 64 && Length > 0 && Length <= 64;
    }
} 