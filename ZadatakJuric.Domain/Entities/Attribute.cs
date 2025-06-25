using ZadatakJuric.Domain.Enums;

namespace ZadatakJuric.Domain.Entities;

public class Attribute
{
    public int Id { get; set; } // EF Core primary key
    public string Name { get; set; } = string.Empty;
    public AttributeValueType ValueType { get; set; }
    public string StringValue { get; set; } = string.Empty;
    public double NumericValue { get; set; }
    public int IntegerValue { get; set; }
    public List<string> EnumValues { get; set; } = new();
    public string SelectedEnumValue { get; set; } = string.Empty;
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    
    // EF Core navigation properties - attributes can belong to different entities
    public int? DBCId { get; set; }
    public DBC? DBC { get; set; }
    
    public int? NetworkId { get; set; }
    public Network? Network { get; set; }
    
    public int? MessageId { get; set; }
    public Message? Message { get; set; }
    
    public int? SignalId { get; set; }
    public Signal? Signal { get; set; }

    public object? GetValue()
    {
        return ValueType switch
        {
            AttributeValueType.String => StringValue,
            AttributeValueType.Integer => IntegerValue,
            AttributeValueType.Float => NumericValue,
            AttributeValueType.Enum => SelectedEnumValue,
            _ => null
        };
    }

    public void SetValue(object value)
    {
        switch (ValueType)
        {
            case AttributeValueType.String:
                StringValue = value?.ToString() ?? string.Empty;
                break;
            case AttributeValueType.Integer:
                if (int.TryParse(value?.ToString(), out var intVal))
                    IntegerValue = intVal;
                break;
            case AttributeValueType.Float:
                if (double.TryParse(value?.ToString(), out var doubleVal))
                    NumericValue = doubleVal;
                break;
            case AttributeValueType.Enum:
                var enumVal = value?.ToString() ?? string.Empty;
                if (EnumValues.Contains(enumVal))
                    SelectedEnumValue = enumVal;
                break;
        }
    }

    public bool IsValidValue(object value)
    {
        return ValueType switch
        {
            AttributeValueType.String => value is string,
            AttributeValueType.Integer => int.TryParse(value?.ToString(), out var intVal) &&
                                        (!MinValue.HasValue || intVal >= MinValue) &&
                                        (!MaxValue.HasValue || intVal <= MaxValue),
            AttributeValueType.Float => double.TryParse(value?.ToString(), out var doubleVal) &&
                                      (!MinValue.HasValue || doubleVal >= MinValue) &&
                                      (!MaxValue.HasValue || doubleVal <= MaxValue),
            AttributeValueType.Enum => EnumValues.Contains(value?.ToString() ?? string.Empty),
            _ => false
        };
    }
} 