using System;
using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("User")]
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Uuid { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int cash { get; set; } = 0;
}
