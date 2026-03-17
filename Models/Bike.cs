using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class Bike
{
    public int BikeId { get; set; }

    public int? CustomerId { get; set; }

    public string? LicensePlate { get; set; }

    public string? BikeModel { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<ServiceReceipt> ServiceReceipts { get; set; } = new List<ServiceReceipt>();
}
