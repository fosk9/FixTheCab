using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Password { get; set; }

    public string? ProfileImagePath { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ActivationKey> ActivationKeys { get; set; } = new List<ActivationKey>();

    public virtual ICollection<InvoiceHistory> InvoiceHistories { get; set; } = new List<InvoiceHistory>();

    public virtual ICollection<LeaveRequest> LeaveRequestApprovedByNavigations { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<LeaveRequest> LeaveRequestEmployees { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<MechanicAssignment> MechanicAssignmentAssignedByNavigations { get; set; } = new List<MechanicAssignment>();

    public virtual ICollection<MechanicAssignment> MechanicAssignmentMechanics { get; set; } = new List<MechanicAssignment>();

    public virtual ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();

    public virtual ICollection<ServiceReceipt> ServiceReceipts { get; set; } = new List<ServiceReceipt>();

    public virtual Shop? Shop { get; set; }

    public virtual ICollection<ShopActivityLog> ShopActivityLogs { get; set; } = new List<ShopActivityLog>();

    public virtual ICollection<ShopEmployee> ShopEmployees { get; set; } = new List<ShopEmployee>();

    public virtual ICollection<ShopSubscription> ShopSubscriptions { get; set; } = new List<ShopSubscription>();

    public virtual ICollection<SystemActivityLog> SystemActivityLogs { get; set; } = new List<SystemActivityLog>();

    public virtual ICollection<UserRole> UserRoleAssignedByNavigations { get; set; } = new List<UserRole>();

    public virtual ICollection<UserRole> UserRoleEmployees { get; set; } = new List<UserRole>();

    public virtual ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
}
