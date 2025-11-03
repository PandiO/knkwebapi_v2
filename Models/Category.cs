using System;

namespace knkwebapi_v2.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public int? ItemtypeId { get; set; }

    public int? ParentCategoryId { get; set; }
}
