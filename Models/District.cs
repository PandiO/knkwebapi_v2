using System;
using System.Collections.ObjectModel;
using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("District")]
public class District : Domain
{
    [NavigationPair(nameof(Town))]
    [RelatedEntityField(typeof(Town))]
    public int TownId { get; set; }
    [RelatedEntityField(typeof(Town))]
    public Town Town { get; set; } = null!;
    [NavigationPair(nameof(Streets))]
    [RelatedEntityField(typeof(Street))]
    public ICollection<int> StreetIds { get; set; } = new Collection<int>();
    [RelatedEntityField(typeof(Street))]
    public ICollection<Street> Streets { get; set; } = new Collection<Street>();
    [RelatedEntityField(typeof(Structure))]
    public ICollection<Structure> Structures { get; set; } = new Collection<Structure>();
}
