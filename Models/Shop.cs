using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class Shop
{
    public int ShopId { get; set; }

    public int OwnerId { get; set; }

    public string ShopName { get; set; } = null!;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? LogoPath { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ActivationKey? ActivationKey { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual Employee Owner { get; set; } = null!;

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();

    public virtual ICollection<ServiceReceipt> ServiceReceipts { get; set; } = new List<ServiceReceipt>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public virtual ICollection<ShopActivityLog> ShopActivityLogs { get; set; } = new List<ShopActivityLog>();

    public virtual ICollection<ShopBankAccount> ShopBankAccounts { get; set; } = new List<ShopBankAccount>();

    public virtual ICollection<ShopEmployee> ShopEmployees { get; set; } = new List<ShopEmployee>();

    public virtual ICollection<ShopSubscription> ShopSubscriptions { get; set; } = new List<ShopSubscription>();

    public virtual ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
}
