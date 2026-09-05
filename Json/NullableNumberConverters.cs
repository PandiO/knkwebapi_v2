using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Json;

/// <summary>
/// Tolerates an empty/whitespace JSON string as null for a nullable double, instead of the hard
/// JsonException System.Text.Json throws by default. Generic form UIs submit "" for an untouched
/// numeric input (e.g. a freshly created Location's X/Y/Z field) - without this, that crashes the
/// entire request with an opaque, bodiless 400 before any application code (including required-
/// field validation) ever runs.
/// </summary>
public class NullableDoubleConverter : JsonConverter<double?>
{
    public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType == JsonTokenType.Number) return reader.GetDouble();
        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString();
            if (string.IsNullOrWhiteSpace(text)) return null;
            return double.Parse(text, CultureInfo.InvariantCulture);
        }
        throw new JsonException($"Unexpected token {reader.TokenType} when reading a nullable double.");
    }

    public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
    {
        if (value.HasValue) writer.WriteNumberValue(value.Value);
        else writer.WriteNullValue();
    }
}

/// <summary>Same tolerance as <see cref="NullableDoubleConverter"/>, for nullable float (Yaw/Pitch).</summary>
public class NullableFloatConverter : JsonConverter<float?>
{
    public override float? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType == JsonTokenType.Number) return reader.GetSingle();
        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString();
            if (string.IsNullOrWhiteSpace(text)) return null;
            return float.Parse(text, CultureInfo.InvariantCulture);
        }
        throw new JsonException($"Unexpected token {reader.TokenType} when reading a nullable float.");
    }

    public override void Write(Utf8JsonWriter writer, float? value, JsonSerializerOptions options)
    {
        if (value.HasValue) writer.WriteNumberValue(value.Value);
        else writer.WriteNullValue();
    }
}

/// <summary>Same tolerance as <see cref="NullableDoubleConverter"/>, for nullable int fields (e.g. FK ids, counts).</summary>
public class NullableIntConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType == JsonTokenType.Number) return reader.GetInt32();
        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString();
            if (string.IsNullOrWhiteSpace(text)) return null;
            return int.Parse(text, CultureInfo.InvariantCulture);
        }
        throw new JsonException($"Unexpected token {reader.TokenType} when reading a nullable int.");
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value.HasValue) writer.WriteNumberValue(value.Value);
        else writer.WriteNullValue();
    }
}

/// <summary>Same tolerance as <see cref="NullableDoubleConverter"/>, for nullable bool fields (e.g. untouched checkboxes).</summary>
public class NullableBoolConverter : JsonConverter<bool?>
{
    public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType == JsonTokenType.True) return true;
        if (reader.TokenType == JsonTokenType.False) return false;
        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString();
            if (string.IsNullOrWhiteSpace(text)) return null;
            return bool.Parse(text);
        }
        throw new JsonException($"Unexpected token {reader.TokenType} when reading a nullable bool.");
    }

    public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
    {
        if (value.HasValue) writer.WriteBooleanValue(value.Value);
        else writer.WriteNullValue();
    }
}
