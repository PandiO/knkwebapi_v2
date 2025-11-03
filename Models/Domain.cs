using System;

namespace knkwebapi_v2.Models;

public class Domain
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool AllowEntry { get; set; } = true;

    public bool AllowExit { get; set; } = true;

    public string WgRegionId { get; set; } = null!;

    public int? LocationId { get; set; }
}
