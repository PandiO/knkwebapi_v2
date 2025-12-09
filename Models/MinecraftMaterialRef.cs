using System.ComponentModel.DataAnnotations;

namespace knkwebapi_v2.Models;

public class MinecraftMaterialRef
{
    public int Id { get; set; }

    [Required]
    [MaxLength(191)]
    public string NamespaceKey { get; set; } = null!;

    public string? LegacyName { get; set; }

    [Required]
    public string Category { get; set; } = null!;
}
