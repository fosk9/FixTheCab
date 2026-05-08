using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class ActivationKey
{
    public int KeyId { get; set; }

    public int ShopId { get; set; }

    public string LicenseKey { get; set; } = null!;

    public int? SubscriptionId { get; set; }

    public DateTime? IssuedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? ActivatedAt { get; set; }

    public DateTime? LastCheckedAt { get; set; }

    public string? Status { get; set; }

    public int? IssuedBy { get; set; }

    public virtual Employee? IssuedByNavigation { get; set; }

    public virtual Shop Shop { get; set; } = null!;

    public virtual ShopSubscription? Subscription { get; set; }
}
