using System.Text.Json;
using System.Text.Json.Serialization;
using Minimap.Simulation;

namespace Minimap.App;

/// <summary>Serializes <see cref="SimVec2I"/> as a JSON array of exactly two integers: <c>[x, y]</c>.</summary>
public sealed class SimVec2IJsonConverter : JsonConverter<SimVec2I>
{
    public override SimVec2I Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected a JSON array of two integers for SimVec2I.");

        if (!reader.Read())
            throw new JsonException("Unexpected end of JSON while reading SimVec2I.");

        var x = ReadIntComponent(ref reader, "X");
        if (!reader.Read())
            throw new JsonException("Expected a second integer in SimVec2I array.");

        var y = ReadIntComponent(ref reader, "Y");
        if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("SimVec2I JSON array must contain exactly two integers.");

        return new SimVec2I(x, y);
    }

    public override void Write(Utf8JsonWriter writer, SimVec2I value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteEndArray();
    }

    private static int ReadIntComponent(ref Utf8JsonReader reader, string axis)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out var value))
            throw new JsonException($"SimVec2I {axis} must be an integer.");
        return value;
    }
}
