using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class WorkSchedule
{
    public int ScheduleId { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly? WorkDate { get; set; }

    public string? Shift { get; set; }

    public DateTime? CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public virtual Employee? Employee { get; set; }
}
