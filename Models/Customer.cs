using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public int? ShopId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Bike> Bikes { get; set; } = new List<Bike>();

    public virtual Shop? Shop { get; set; }
}
