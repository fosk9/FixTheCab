using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string? ServiceName { get; set; }

    public decimal? Price { get; set; }

    public virtual ICollection<ServiceOrderItem> ServiceOrderItems { get; set; } = new List<ServiceOrderItem>();
}
