using System;
using System.Collections.Generic;

namespace ShoesShop.Data.Entities;

public partial class Shipment
{
    public int ShipmentId { get; set; }

    public int OrderId { get; set; }

    public string CarrierName { get; set; } = null!;

    public string? TrackingNumber { get; set; }

    public string ShippingStatus { get; set; } = null!;

    public DateTime? ShippedAt { get; set; }

    public DateOnly? EstimatedDeliveryDate { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public string? Note { get; set; }

    public virtual Order Order { get; set; } = null!;
}
