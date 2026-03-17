using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class LeaveRequest
{
    public int RequestId { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly? LeaveDate { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public int? ApprovedBy { get; set; }

    public virtual Employee? ApprovedByNavigation { get; set; }

    public virtual Employee? Employee { get; set; }
}
