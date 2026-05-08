using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class ShopSubscription
{
    public int SubscriptionId { get; set; }

    public int ShopId { get; set; }

    public int PlanId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal AmountPaid { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ActivationKey> ActivationKeys { get; set; } = new List<ActivationKey>();

    public virtual Employee? CreatedByNavigation { get; set; }

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual Shop Shop { get; set; } = null!;
}
