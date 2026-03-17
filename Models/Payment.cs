using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? OrderId { get; set; }

    public string? Method { get; set; }

    public decimal? Amount { get; set; }

    public string? Status { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual ServiceOrder? Order { get; set; }
}
