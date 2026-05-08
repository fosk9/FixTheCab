using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class ShopEmployee
{
    public int ShopEmployeeId { get; set; }

    public int ShopId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Position { get; set; }

    public decimal? Salary { get; set; }

    public DateOnly? JoinDate { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Employee CreatedByNavigation { get; set; } = null!;

    public virtual Shop Shop { get; set; } = null!;
}
