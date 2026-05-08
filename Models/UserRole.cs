using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class UserRole
{
    public int UserRoleId { get; set; }

    public int EmployeeId { get; set; }

    public int RoleId { get; set; }

    public int? AssignedBy { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Employee? AssignedByNavigation { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
