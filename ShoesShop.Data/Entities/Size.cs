using System;
using System.Collections.Generic;

namespace ShoesShop.Data.Entities;

public partial class Size
{
    public int SizeId { get; set; }

    public string SizeValue { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}
