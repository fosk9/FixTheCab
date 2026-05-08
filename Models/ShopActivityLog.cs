using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class ShopActivityLog
{
    public long LogId { get; set; }

    public int ShopId { get; set; }

    public int? ActorId { get; set; }

    public string Action { get; set; } = null!;

    public string? EntityType { get; set; }

    public int? EntityId { get; set; }

    public string? Detail { get; set; }

    public string? IpAddress { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Employee? Actor { get; set; }

    public virtual Shop Shop { get; set; } = null!;
}
