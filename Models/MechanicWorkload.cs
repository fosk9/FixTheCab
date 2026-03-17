using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class MechanicWorkload
{
    public int EmployeeId { get; set; }

    public string MechanicName { get; set; } = null!;

    public int? ActiveJobCount { get; set; }
}
