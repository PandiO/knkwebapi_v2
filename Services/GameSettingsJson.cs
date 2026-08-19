using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Services;

internal static class GameSettingsJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize<T>(T? value)
    {
        return JsonSerializer.Serialize(value, Options);
    }

    public static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
        catch
        {
            return default;
        }
    }

    public static List<T> DeserializeList<T>(string? json)
    {
        return Deserialize<List<T>>(json) ?? new List<T>();
    }
}
