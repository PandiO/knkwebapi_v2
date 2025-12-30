using System;
using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("User")]
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Uuid { get; set; } = null!;
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int Coins { get; set; } = 250;
    public int Gems { get; set; } = 50;
}
