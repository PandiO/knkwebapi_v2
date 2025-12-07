using System;
using System.Collections.ObjectModel;
using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("Street")]
public class Street
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    [RelatedEntityField(typeof(District))]
    public ICollection<District> Districts { get; set; } = new Collection<District>();
    [RelatedEntityField(typeof(Structure))]
    public ICollection<Structure> Structures { get; set; } = new Collection<Structure>();
}
