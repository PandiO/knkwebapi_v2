using System;
using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("Location")]
public class Location
{
    public int Id { get; set; }
    public string? Name { get; set; } = "Location";
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public string? World { get; set; } = "world";
}
