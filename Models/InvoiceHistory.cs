using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class InvoiceHistory
{
    public int HistoryId { get; set; }

    public int OrderId { get; set; }

    public int? ChangedBy { get; set; }

    public string? PreviousStatus { get; set; }

    public string? NewStatus { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Note { get; set; }

    public DateTime? ChangedAt { get; set; }

    public virtual Employee? ChangedByNavigation { get; set; }

    public virtual ServiceOrder Order { get; set; } = null!;
}
