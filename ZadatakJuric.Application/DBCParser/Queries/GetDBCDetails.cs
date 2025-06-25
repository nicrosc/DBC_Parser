using MediatR;
using ZadatakJuric.Application.DBCParser.Dto;
using ZadatakJuric.Infrastructure.Repositories;

namespace ZadatakJuric.Application.DBCParser.Queries;

public class GetDBCDetailsQuery : IRequest<DBCDto?>
{
    public int Id { get; set; }
}

public class GetDBCDetailsQueryHandler : IRequestHandler<GetDBCDetailsQuery, DBCDto?>
{
    private readonly IDbcRepository _dbcRepository;

    public GetDBCDetailsQueryHandler(IDbcRepository dbcRepository)
    {
        _dbcRepository = dbcRepository;
    }

    public async Task<DBCDto?> Handle(GetDBCDetailsQuery request, CancellationToken cancellationToken)
    {
        var dbc = await _dbcRepository.GetByIdAsync(request.Id);
        
        if (dbc == null)
            return null;

        return MapToDto(dbc);
    }

    private DBCDto MapToDto(Domain.Entities.DBC dbc)
    {
        return new DBCDto
        {
            Id = dbc.Id,
            FileName = dbc.FileName,
            FilePath = dbc.FilePath,
            FileSize = dbc.FileSize,
            FileHash = dbc.FileHash,
            Version = dbc.Version,
            Description = dbc.Description,
            CreatedAt = dbc.CreatedAt,
            TotalNetworks = dbc.Networks.Count,
            TotalMessages = dbc.TotalMessages,
            TotalSignals = dbc.TotalSignals,
            TotalAttributes = dbc.TotalAttributes,
            Networks = dbc.Networks.Select(MapNetworkToDto).ToList()
        };
    }

    private NetworkDto MapNetworkToDto(Domain.Entities.Network network)
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

    private MessageDto MapMessageToDto(Domain.Entities.Message message)
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

    private SignalDto MapSignalToDto(Domain.Entities.Signal signal)
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