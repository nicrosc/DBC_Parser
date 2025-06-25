using ZadatakJuric.Domain.Entities;

namespace ZadatakJuric.Application.DBCParser.Dto;

public class DBCDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalNetworks { get; set; }
    public int TotalMessages { get; set; }
    public int TotalSignals { get; set; }
    public int TotalAttributes { get; set; }
    public List<NetworkDto> Networks { get; set; } = new();
}

public class NetworkDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<MessageDto> Messages { get; set; } = new();
    public List<AttributeDto> Attributes { get; set; } = new();
}

public class MessageDto
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte Size { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public List<SignalDto> Signals { get; set; } = new();
    public List<AttributeDto> Attributes { get; set; } = new();
}

public class SignalDto
{
    public string Name { get; set; } = string.Empty;
    public byte StartBit { get; set; }
    public byte Length { get; set; }
    public string ByteOrder { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;
    public double Factor { get; set; }
    public double Offset { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public List<string> Receivers { get; set; } = new();
    public List<AttributeDto> Attributes { get; set; } = new();
}

public class AttributeDto
{
    public string Name { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}

// DTOs for overview page
public class DBCOverviewResponse
{
    public List<DBCOverviewItemDto> Networks { get; set; } = new();
    public int TotalNetworks { get; set; }
    public int TotalMessages { get; set; }
    public int TotalSignals { get; set; }
    public int TotalAttributes { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}

public class DBCOverviewItemDto
{
    public int Id { get; set; }
    public string NetworkName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalMessages { get; set; }
    public int TotalSignals { get; set; }
    public int TotalAttributes { get; set; }
}
