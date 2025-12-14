using System.ComponentModel.DataAnnotations;
using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("MinecraftBlockRef")]
public class MinecraftBlockRef
{
    public int Id { get; set; }

    [Required]
    [MaxLength(191)]
    public string NamespaceKey { get; set; } = null!;

    public string? BlockStateString { get; set; }

    public string? LogicalType { get; set; }

    public string? IconUrl { get; set; }
}
