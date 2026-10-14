using System.Text.Json.Serialization;

namespace Calculator.Contracts.Authors;

/// <summary>Sent and received as text ("Male", "Female").</summary>
[JsonConverter(typeof(JsonStringEnumConverter<Gender>))]
public enum Gender
{
    Male,
    Female,
}
