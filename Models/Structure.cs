using System;
using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("Structure")]
public class Structure : Domain
{
    [NavigationPair(nameof(Street))]
    [RelatedEntityField(typeof(Street))]
    public int StreetId { get; set; }
    [RelatedEntityField(typeof(Street))]
    public Street Street { get; set; } = null!;
    public int HouseNumber { get; set; }
    
    [NavigationPair(nameof(District))]
    [RelatedEntityField(typeof(District))]
    public int DistrictId { get; set; }
    [RelatedEntityField(typeof(District))]
    public District District { get; set; } = null!;
}
