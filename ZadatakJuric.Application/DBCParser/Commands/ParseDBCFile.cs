using MediatR;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ZadatakJuric.Application.DBCParser.Dto;
using ZadatakJuric.Domain.Entities;
using ZadatakJuric.Domain.Enums;
using ZadatakJuric.Infrastructure.Repositories;

namespace ZadatakJuric.Application.DBCParser.Commands;

public class ParseDBCFileCommand : IRequest<ParseDBCResult>
{
    public string FileContent { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool SaveToDatabase { get; set; } = true;
}

public class ParseDBCResult
{
    public DBCDto Data { get; set; } = new();
    public bool WasAlreadyParsed { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ParseDBCFileCommandHandler : IRequestHandler<ParseDBCFileCommand, ParseDBCResult>
{
    private readonly IDbcRepository _dbcRepository;

    public ParseDBCFileCommandHandler(IDbcRepository dbcRepository)
    {
        _dbcRepository = dbcRepository;
    }

    public async Task<ParseDBCResult> Handle(ParseDBCFileCommand request, CancellationToken cancellationToken)
    {
        var dbc = await ParseDbcContent(request.FileContent, request.FileName);
        
        // Save to database if requested
        if (request.SaveToDatabase)
        {
            // Check if DBC with same hash already exists
            var existingDbc = await _dbcRepository.GetByFileHashAsync(dbc.FileHash);
            if (existingDbc != null)
            {
                return new ParseDBCResult
                {
                    Data = MapToDto(existingDbc),
                    WasAlreadyParsed = true,
                    Message = $"This DBC file has already been parsed and saved on {existingDbc.CreatedAt:yyyy-MM-dd HH:mm:ss}. Showing existing data."
                };
            }

            dbc = await _dbcRepository.SaveAsync(dbc);
        }
        
        return new ParseDBCResult
        {
            Data = MapToDto(dbc),
            WasAlreadyParsed = false,
            Message = $"Successfully parsed DBC file! Found {dbc.TotalMessages} messages with {dbc.TotalSignals} signals."
        };
    }

    private async Task<DBC> ParseDbcContent(string content, string fileName)
    {
        // Simulate async processing - in real implementation, this could be file I/O, database operations, etc.
        await Task.Delay(10, CancellationToken.None);

        var dbc = new DBC
        {
            FileName = fileName,
            FilePath = $"uploads/{fileName}",
            Version = ExtractVersion(content),
            Description = ExtractDescription(content),
            CreatedAt = DateTime.UtcNow,
            FileSize = Encoding.UTF8.GetByteCount(content),
            FileHash = CalculateFileHash(content)
        };

        // Create a default network for the DBC file
        var network = new Network
        {
            Name = Path.GetFileNameWithoutExtension(fileName),
            Description = "Auto-generated network from DBC file"
        };

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Message? currentMessage = null;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            try
            {
                // Parse BO_ (message definition)
                if (line.StartsWith("BO_ "))
                {
                    currentMessage = ParseMessage(line);
                    if (currentMessage != null)
                    {
                        network.AddMessage(currentMessage);
                    }
                }
                // Parse SG_ (signal definition) - signals can start with varying whitespace
                else if (line.TrimStart().StartsWith("SG_ ") && currentMessage != null)
                {
                    var signal = ParseSignal(line);
                    if (signal != null)
                    {
                        currentMessage.AddSignal(signal);
                    }
                }
                // Parse CM_ (comments)
                else if (line.StartsWith("CM_ "))
                {
                    ParseComment(line, network, currentMessage);
                }
                // Parse BA_DEF_ (attribute definitions)
                else if (line.StartsWith("BA_DEF_ "))
                {
                    var attribute = ParseAttributeDefinition(line);
                    if (attribute != null)
                    {
                        network.Attributes.Add(attribute);
                    }
                }
                // Parse BA_DEF_DEF_ (attribute default values)
                else if (line.StartsWith("BA_DEF_DEF_ "))
                {
                    var attribute = ParseAttributeDefault(line);
                    if (attribute != null)
                    {
                        network.Attributes.Add(attribute);
                    }
                }
                // Parse BA_ (attribute values) - simplified
                else if (line.StartsWith("BA_ "))
                {
                    var attribute = ParseAttribute(line);
                    if (attribute != null)
                    {
                        network.Attributes.Add(attribute);
                    }
                }
                // Parse CM_ SG_ (signal comments)
                else if (line.StartsWith("CM_ SG_ ") && currentMessage != null)
                {
                    ParseSignalComment(line, currentMessage);
                }
            }
            catch
            {
                // Skip malformed lines - in production, you might want to log these
                continue;
            }
        }

        dbc.AddNetwork(network);
        return dbc;
    }

    private string CalculateFileHash(string content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes);
    }

    private Message? ParseMessage(string line)
    {
        // BO_ 1234 MessageName: 8 NodeName
        var match = Regex.Match(line, @"BO_\s+(\d+)\s+([^:]+):\s*(\d+)\s*(.*)");
        if (!match.Success) return null;

        return new Message
        {
            MessageId = uint.Parse(match.Groups[1].Value),
            Name = match.Groups[2].Value.Trim(),
            Size = byte.Parse(match.Groups[3].Value),
            Sender = match.Groups[4].Value.Trim()
        };
    }

    private Signal? ParseSignal(string line)
    {
        // SG_ SignalName : 0|8@1+ (1,0) [0|255] "unit" Receiver1,Receiver2
        // Use simpler parsing for reliability - complex regex can be added later
        var trimmedLine = line.Trim();
        var simpleMatch = Regex.Match(trimmedLine, @"SG_\s+([^:\s]+)\s*:");
        if (simpleMatch.Success)
        {
            var signalName = simpleMatch.Groups[1].Value.Trim();
            
            // Try to extract more details with safer patterns
            var startBit = 0;
            var length = 8;
            var factor = 1.0;
            var offset = 0.0;
            var minimum = 0.0;
            var maximum = 255.0;
            var unit = "";
            
            // Extract start bit and length: 0|8
            var bitMatch = Regex.Match(trimmedLine, @":\s*(\d+)\|(\d+)");
            if (bitMatch.Success)
            {
                int.TryParse(bitMatch.Groups[1].Value, out startBit);
                int.TryParse(bitMatch.Groups[2].Value, out length);
            }
            
            // Extract factor and offset: (1.0,0.0)
            var factorMatch = Regex.Match(trimmedLine, @"\(([^,]+),([^)]+)\)");
            if (factorMatch.Success)
            {
                double.TryParse(factorMatch.Groups[1].Value, out factor);
                double.TryParse(factorMatch.Groups[2].Value, out offset);
            }
            
            // Extract min and max: [0|255]
            var rangeMatch = Regex.Match(trimmedLine, @"\[([^|]*)\|([^\]]*)\]");
            if (rangeMatch.Success)
            {
                double.TryParse(rangeMatch.Groups[1].Value, out minimum);
                double.TryParse(rangeMatch.Groups[2].Value, out maximum);
            }
            
            // Extract unit: "unit"
            var unitMatch = Regex.Match(trimmedLine, @"""([^""]*)""");
            if (unitMatch.Success)
            {
                unit = unitMatch.Groups[1].Value;
            }
            
            return new Signal
            {
                Name = signalName,
                StartBit = (byte)Math.Max(0, Math.Min(255, startBit)),
                Length = (byte)Math.Max(1, Math.Min(64, length)),
                ByteOrder = ByteOrder.LittleEndian,
                ValueType = SignalType.Unsigned,
                Factor = factor,
                Offset = offset,
                Minimum = minimum,
                Maximum = maximum,
                Unit = unit,
                Receivers = new List<string>()
            };
        }
        
        return null;
    }

    private void ParseComment(string line, Network network, Message? currentMessage)
    {
        // CM_ "Network comment";
        // CM_ BO_ 1234 "Message comment";
        var match = Regex.Match(line, @"CM_\s+""([^""]+)""");
        if (match.Success && currentMessage == null)
        {
            network.Description = match.Groups[1].Value;
        }
    }

    private Domain.Entities.Attribute? ParseAttributeDefinition(string line)
    {
        // BA_DEF_ "AttributeName" STRING;
        // BA_DEF_ BO_ "GenMsgBackgroundColor" STRING ;
        var match = Regex.Match(line, @"BA_DEF_\s+(?:BO_\s+)?""([^""]+)""\s+(\w+)");
        if (!match.Success) return null;

        return new Domain.Entities.Attribute
        {
            Name = match.Groups[1].Value,
            ValueType = GetAttributeValueType(match.Groups[2].Value),
            StringValue = "",
            DefaultValue = ""
        };
    }

    private Domain.Entities.Attribute? ParseAttributeDefault(string line)
    {
        // BA_DEF_DEF_ "GenMsgBackgroundColor" "#ffffff";
        var match = Regex.Match(line, @"BA_DEF_DEF_\s+""([^""]+)""\s+""?([^"";]+)""?");
        if (!match.Success) return null;

        return new Domain.Entities.Attribute
        {
            Name = match.Groups[1].Value,
            ValueType = AttributeValueType.String,
            StringValue = match.Groups[2].Value.Trim(';'),
            DefaultValue = match.Groups[2].Value.Trim(';')
        };
    }

    private Domain.Entities.Attribute? ParseAttribute(string line)
    {
        // BA_ "AttributeName" STRING;
        var match = Regex.Match(line, @"BA_\s+""([^""]+)""\s+(\w+)");
        if (!match.Success) return null;

        return new Domain.Entities.Attribute
        {
            Name = match.Groups[1].Value,
            ValueType = GetAttributeValueType(match.Groups[2].Value),
            StringValue = "",
            DefaultValue = ""
        };
    }

    private void ParseSignalComment(string line, Message currentMessage)
    {
        // CM_ SG_ 1060 CUV "Cell_Under_Voltage";
        var match = Regex.Match(line, @"CM_\s+SG_\s+\d+\s+([^\s]+)\s+""([^""]+)""");
        if (match.Success)
        {
            var signalName = match.Groups[1].Value;
            var comment = match.Groups[2].Value;
            
            var signal = currentMessage.Signals.FirstOrDefault(s => s.Name == signalName);
            if (signal != null)
            {
                signal.Comment = comment;
                
                // Also add as an attribute
                signal.Attributes.Add(new Domain.Entities.Attribute
                {
                    Name = "Comment",
                    ValueType = AttributeValueType.String,
                    StringValue = comment,
                    DefaultValue = comment
                });
            }
        }
    }

    private AttributeValueType GetAttributeValueType(string typeString)
    {
        return typeString.ToUpper() switch
        {
            "STRING" => AttributeValueType.String,
            "INT" => AttributeValueType.Integer,
            "FLOAT" => AttributeValueType.Float,
            "ENUM" => AttributeValueType.Enum,
            _ => AttributeValueType.String
        };
    }

    private string ExtractVersion(string content)
    {
        var match = Regex.Match(content, @"VERSION\s+""([^""]+)""");
        return match.Success ? match.Groups[1].Value : "1.0";
    }

    private string ExtractDescription(string content)
    {
        var match = Regex.Match(content, @"CM_\s+""([^""]+)""");
        return match.Success ? match.Groups[1].Value : "Parsed from uploaded DBC file";
    }

    private DBCDto MapToDto(DBC dbc)
    {
        return new DBCDto
        {
            Id = dbc.Id,
            FileName = dbc.FileName,
            FilePath = dbc.FilePath,
            FileSize = dbc.FileSize,
            Version = dbc.Version,
            Description = dbc.Description,
            CreatedAt = dbc.CreatedAt,
            FileHash = dbc.FileHash,
            TotalNetworks = dbc.Networks.Count,
            TotalMessages = dbc.TotalMessages,
            TotalSignals = dbc.TotalSignals,
            TotalAttributes = dbc.TotalAttributes,
            Networks = dbc.Networks.Select(MapNetworkToDto).ToList()
        };
    }

    private NetworkDto MapNetworkToDto(Network network)
    {
        return new NetworkDto
        {
            Id = network.Id,
            Name = network.Name,
            Description = network.Description,
            Messages = network.Messages.Select(MapMessageToDto).ToList(),
            Attributes = network.Attributes.Select(MapAttributeToDto).ToList()
        };
    }

    private MessageDto MapMessageToDto(Message message)
    {
        return new MessageDto
        {
            Id = message.MessageId,
            Name = message.Name,
            Size = message.Size,
            Sender = message.Sender,
            Comment = message.Comment,
            Signals = message.Signals.Select(MapSignalToDto).ToList(),
            Attributes = message.Attributes.Select(MapAttributeToDto).ToList()
        };
    }

    private SignalDto MapSignalToDto(Signal signal)
    {
        return new SignalDto
        {
            Name = signal.Name,
            StartBit = signal.StartBit,
            Length = signal.Length,
            ByteOrder = signal.ByteOrder.ToString(),
            ValueType = signal.ValueType.ToString(),
            Factor = signal.Factor,
            Offset = signal.Offset,
            Minimum = signal.Minimum,
            Maximum = signal.Maximum,
            Unit = signal.Unit,
            Comment = signal.Comment,
            Receivers = signal.Receivers.ToList(),
            Attributes = signal.Attributes.Select(MapAttributeToDto).ToList()
        };
    }

    private AttributeDto MapAttributeToDto(Domain.Entities.Attribute attribute)
    {
        return new AttributeDto
        {
            Name = attribute.Name,
            ValueType = attribute.ValueType.ToString(),
            Value = attribute.GetValue()?.ToString() ?? "",
            Comment = attribute.DefaultValue
        };
    }
}