using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class PendingAssignmentQueue
{
    public int ReceiptId { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerPhone { get; set; }

    public string? LicensePlate { get; set; }

    public string? BikeModel { get; set; }

    public string CreatedByCashier { get; set; } = null!;
}
