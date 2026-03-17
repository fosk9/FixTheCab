using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class OrderPart
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? PartId { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }

    public virtual ServiceOrder? Order { get; set; }

    public virtual Part? Part { get; set; }
}
