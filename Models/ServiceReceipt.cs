using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class ServiceReceipt
{
    public int ReceiptId { get; set; }

    public int? ShopId { get; set; }

    public int? BikeId { get; set; }

    public int? CreatedBy { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Bike? Bike { get; set; }

    public virtual Employee? CreatedByNavigation { get; set; }

    public virtual ICollection<MechanicAssignment> MechanicAssignments { get; set; } = new List<MechanicAssignment>();

    public virtual ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();

    public virtual Shop? Shop { get; set; }
}
