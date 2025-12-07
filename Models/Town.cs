using System;
using System.Collections.ObjectModel;
using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("Town")]
public class Town : Domain
{
    [NavigationPair(nameof(Streets))]
    [RelatedEntityField(typeof(Street))]
    public ICollection<Street> Streets { get; set; } = new Collection<Street>();
    [NavigationPair(nameof(Districts))]
    [RelatedEntityField(typeof(District))]
    public ICollection<District> Districts { get; set; } = new Collection<District>();
}
