using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class ServiceOrderItem
{
    public int ItemId { get; set; }

    public int? OrderId { get; set; }

    public int? ServiceId { get; set; }

    public decimal? Price { get; set; }

    public virtual ServiceOrder? Order { get; set; }

    public virtual Service? Service { get; set; }
}
