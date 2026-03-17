using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class Part
{
    public int PartId { get; set; }

    public string? PartName { get; set; }

    public decimal? Price { get; set; }

    public int? Stock { get; set; }

    public int? WarningLevel { get; set; }

    public virtual ICollection<OrderPart> OrderParts { get; set; } = new List<OrderPart>();
}
