using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class SubscriptionPlan
{
    public int PlanId { get; set; }

    public string PlanName { get; set; } = null!;

    public int DurationDays { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ShopSubscription> ShopSubscriptions { get; set; } = new List<ShopSubscription>();
}
