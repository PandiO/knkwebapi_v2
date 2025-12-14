using System;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos;

public class LocationDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("x")]
    public double? X { get; set; }
    [JsonPropertyName("y")]
    public double? Y { get; set; }
    [JsonPropertyName("z")]
    public double? Z { get; set; }
    [JsonPropertyName("yaw")]
    public float? Yaw { get; set; }
    [JsonPropertyName("pitch")]
    public float? Pitch { get; set; }
    [JsonPropertyName("world")]
    public string? World { get; set; }
}
