using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class ServiceOrder
{
    public int OrderId { get; set; }

    public int? ReceiptId { get; set; }

    public int? MechanicId { get; set; }

    public string? Status { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<InvoiceHistory> InvoiceHistories { get; set; } = new List<InvoiceHistory>();

    public virtual Employee? Mechanic { get; set; }

    public virtual ICollection<OrderPart> OrderParts { get; set; } = new List<OrderPart>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ServiceReceipt? Receipt { get; set; }

    public virtual ICollection<ServiceOrderItem> ServiceOrderItems { get; set; } = new List<ServiceOrderItem>();
}
