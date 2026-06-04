using System;
using System.Collections.Generic;

namespace ShoesShop.Data.Entities;

public partial class Address
{
    public int AddressId { get; set; }

    public int UserId { get; set; }

    public string RecipientName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Province { get; set; } = null!;

    public string District { get; set; } = null!;

    public string Ward { get; set; } = null!;

    public string StreetAddress { get; set; } = null!;

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
