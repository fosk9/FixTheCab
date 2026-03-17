using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class MechanicAssignment
{
    public int AssignmentId { get; set; }

    public int ReceiptId { get; set; }

    public int MechanicId { get; set; }

    public int AssignedBy { get; set; }

    public DateTime? AssignedAt { get; set; }

    public string? Note { get; set; }

    public virtual Employee AssignedByNavigation { get; set; } = null!;

    public virtual Employee Mechanic { get; set; } = null!;

    public virtual ServiceReceipt Receipt { get; set; } = null!;
}
