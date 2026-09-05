using System;
using System.Text.Json.Serialization;
using knkwebapi_v2.Json;

namespace knkwebapi_v2.Dtos;

public class LocationDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("x")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? X { get; set; }
    [JsonPropertyName("y")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? Y { get; set; }
    [JsonPropertyName("z")]
    [JsonConverter(typeof(NullableDoubleConverter))]
    public double? Z { get; set; }
    [JsonPropertyName("yaw")]
    [JsonConverter(typeof(NullableFloatConverter))]
    public float? Yaw { get; set; }
    [JsonPropertyName("pitch")]
    [JsonConverter(typeof(NullableFloatConverter))]
    public float? Pitch { get; set; }
    [JsonPropertyName("world")]
    public string? World { get; set; }
}
