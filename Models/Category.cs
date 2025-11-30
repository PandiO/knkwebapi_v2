using System;
using knkwebapi_v2.Attributes;

namespace knkwebapi_v2.Models;

[FormConfigurableEntity("Category")]
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public int? ItemtypeId { get; set; }

    [NavigationPair(nameof(ParentCategory))]
    [RelatedEntityField(typeof(Category))]
    public int? ParentCategoryId { get; set; }

    [RelatedEntityField(typeof(Category))]
    public Category? ParentCategory { get; set; }
}
