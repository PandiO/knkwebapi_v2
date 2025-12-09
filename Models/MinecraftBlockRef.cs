using System.ComponentModel.DataAnnotations;

namespace knkwebapi_v2.Models;

public class MinecraftBlockRef
{
    public int Id { get; set; }

    [Required]
    [MaxLength(191)]
    public string NamespaceKey { get; set; } = null!;

    public string? BlockStateString { get; set; }

    public string? LogicalType { get; set; }
}
