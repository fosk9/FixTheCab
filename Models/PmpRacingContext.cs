using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PMPRacing.Models;

public partial class PmpRacingContext : DbContext
{
    public PmpRacingContext()
    {
    }

    public PmpRacingContext(DbContextOptions<PmpRacingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivationKey> ActivationKeys { get; set; }

    public virtual DbSet<Bike> Bikes { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<InvoiceHistory> InvoiceHistories { get; set; }

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; }

    public virtual DbSet<MechanicAssignment> MechanicAssignments { get; set; }

    public virtual DbSet<MechanicWorkload> MechanicWorkloads { get; set; }

    public virtual DbSet<OrderPart> OrderParts { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PendingAssignmentQueue> PendingAssignmentQueues { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceOrder> ServiceOrders { get; set; }

    public virtual DbSet<ServiceOrderItem> ServiceOrderItems { get; set; }

    public virtual DbSet<ServiceReceipt> ServiceReceipts { get; set; }

    public virtual DbSet<Shop> Shops { get; set; }

    public virtual DbSet<ShopActivityLog> ShopActivityLogs { get; set; }

    public virtual DbSet<ShopBankAccount> ShopBankAccounts { get; set; }

    public virtual DbSet<ShopEmployee> ShopEmployees { get; set; }

    public virtual DbSet<ShopSubscription> ShopSubscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<SystemActivityLog> SystemActivityLogs { get; set; }

    public virtual DbSet<SystemBankAccount> SystemBankAccounts { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<WorkSchedule> WorkSchedules { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=LAUDAITINHAI\\SQLEXPRESS;Database= PMPRacing;User Id=sa;Password=123;TrustServerCertificate=True;");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var ConnectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivationKey>(entity =>
        {
            entity.HasKey(e => e.KeyId).HasName("PK__Activati__21F5BE47BAFEC5BC");

            entity.HasIndex(e => e.LicenseKey, "UQ__Activati__45E1DD6FD7059BFB").IsUnique();

            entity.HasIndex(e => e.ShopId, "UQ__Activati__67C557C807C81567").IsUnique();

            entity.Property(e => e.ActivatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.IssuedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LastCheckedAt).HasColumnType("datetime");
            entity.Property(e => e.LicenseKey).HasMaxLength(64);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending");

            entity.HasOne(d => d.IssuedByNavigation).WithMany(p => p.ActivationKeys)
                .HasForeignKey(d => d.IssuedBy)
                .HasConstraintName("FK_ActivationKeys_IssuedBy");

            entity.HasOne(d => d.Shop).WithOne(p => p.ActivationKey)
                .HasForeignKey<ActivationKey>(d => d.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivationKeys_Shop");

            entity.HasOne(d => d.Subscription).WithMany(p => p.ActivationKeys)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("FK_ActivationKeys_Subscription");
        });

        modelBuilder.Entity<Bike>(entity =>
        {
            entity.HasKey(e => e.BikeId).HasName("PK__Bikes__7DC817217F2B625E");

            entity.Property(e => e.BikeModel).HasMaxLength(100);
            entity.Property(e => e.LicensePlate).HasMaxLength(20);

            entity.HasOne(d => d.Customer).WithMany(p => p.Bikes)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Bikes_Customers");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D893072AAF");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.Shop).WithMany(p => p.Customers)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FK_Customers_Shop");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04F11B21D2CE1");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ProfileImagePath).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active");
        });

        modelBuilder.Entity<InvoiceHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__InvoiceH__4D7B4ABDDAABA919");

            entity.ToTable("InvoiceHistory");

            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NewStatus).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.PreviousStatus).HasMaxLength(50);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.InvoiceHistories)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("FK_InvoiceHistory_Employees");

            entity.HasOne(d => d.Order).WithMany(p => p.InvoiceHistories)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceHistory_Orders");
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__LeaveReq__33A8517A6E028E90");

            entity.Property(e => e.Reason).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("pending");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.LeaveRequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_LeaveRequests_Manager");

            entity.HasOne(d => d.Employee).WithMany(p => p.LeaveRequestEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_LeaveRequests_Employees");
        });

        modelBuilder.Entity<MechanicAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Mechanic__32499E774E897C91");

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(255);

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.MechanicAssignmentAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MechanicAssignments_Manager");

            entity.HasOne(d => d.Mechanic).WithMany(p => p.MechanicAssignmentMechanics)
                .HasForeignKey(d => d.MechanicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MechanicAssignments_Mechanic");

            entity.HasOne(d => d.Receipt).WithMany(p => p.MechanicAssignments)
                .HasForeignKey(d => d.ReceiptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MechanicAssignments_Receipts");
        });

        modelBuilder.Entity<MechanicWorkload>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("MechanicWorkload");

            entity.Property(e => e.MechanicName).HasMaxLength(100);
        });

        modelBuilder.Entity<OrderPart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderPar__3214EC07E6E062F3");

            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderParts)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderParts_Orders");

            entity.HasOne(d => d.Part).WithMany(p => p.OrderParts)
                .HasForeignKey(d => d.PartId)
                .HasConstraintName("FK_OrderParts_Parts");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.PartId).HasName("PK__Parts__7C3F0D5018EBA305");

            entity.Property(e => e.PartName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Shop).WithMany(p => p.Parts)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FK_Parts_Shop");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A3861DA2AC2");

            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Method).HasMaxLength(50);
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Payments_Orders");
        });

        modelBuilder.Entity<PendingAssignmentQueue>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("PendingAssignmentQueue");

            entity.Property(e => e.BikeModel).HasMaxLength(100);
            entity.Property(e => e.CreatedByCashier).HasMaxLength(100);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.LicensePlate).HasMaxLength(20);
            entity.Property(e => e.ReceivedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A987DA163");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B616024F410DE").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB00A77607CEE");

            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ServiceName).HasMaxLength(100);

            entity.HasOne(d => d.Shop).WithMany(p => p.Services)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FK_Services_Shop");
        });

        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__ServiceO__C3905BCFB9744CC1");

            entity.Property(e => e.CompletedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StartedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("processing");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Mechanic).WithMany(p => p.ServiceOrders)
                .HasForeignKey(d => d.MechanicId)
                .HasConstraintName("FK_ServiceOrders_Employees");

            entity.HasOne(d => d.Receipt).WithMany(p => p.ServiceOrders)
                .HasForeignKey(d => d.ReceiptId)
                .HasConstraintName("FK_ServiceOrders_Receipts");
        });

        modelBuilder.Entity<ServiceOrderItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__ServiceO__727E838BCB10B069");

            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.ServiceOrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_SOItems_Orders");

            entity.HasOne(d => d.Service).WithMany(p => p.ServiceOrderItems)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK_SOItems_Services");
        });

        modelBuilder.Entity<ServiceReceipt>(entity =>
        {
            entity.HasKey(e => e.ReceiptId).HasName("PK__ServiceR__CC08C420F226385B");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("pending");

            entity.HasOne(d => d.Bike).WithMany(p => p.ServiceReceipts)
                .HasForeignKey(d => d.BikeId)
                .HasConstraintName("FK_ServiceReceipts_Bikes");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ServiceReceipts)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_ServiceReceipts_Employees");

            entity.HasOne(d => d.Shop).WithMany(p => p.ServiceReceipts)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FK_ServiceReceipts_Shop");
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.ShopId).HasName("PK__Shops__67C557C9CD22BAFB");

            entity.HasIndex(e => e.OwnerId, "UQ__Shops__819385B921B51A4E").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LogoPath).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ShopName).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active");

            entity.HasOne(d => d.Owner).WithOne(p => p.Shop)
                .HasForeignKey<Shop>(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Shops_Owner");
        });

        modelBuilder.Entity<ShopActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__ShopActi__5E548648702B09BA");

            entity.HasIndex(e => e.ActorId, "IX_ShopLog_Actor");

            entity.HasIndex(e => e.CreatedAt, "IX_ShopLog_CreatedAt");

            entity.HasIndex(e => e.ShopId, "IX_ShopLog_Shop");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(45);

            entity.HasOne(d => d.Actor).WithMany(p => p.ShopActivityLogs)
                .HasForeignKey(d => d.ActorId)
                .HasConstraintName("FK_ShopLog_Actor");

            entity.HasOne(d => d.Shop).WithMany(p => p.ShopActivityLogs)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShopLog_Shop");
        });

        modelBuilder.Entity<ShopBankAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__ShopBank__349DA5A63700C15A");

            entity.HasIndex(e => e.ShopId, "IX_ShopBankAccounts_Shop");

            entity.Property(e => e.AccountName).HasMaxLength(225);
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(225)
                .IsUnicode(false);
            entity.Property(e => e.BankName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.QrImageCode).HasMaxLength(500);

            entity.HasOne(d => d.Shop).WithMany(p => p.ShopBankAccounts)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShopBankAccounts_Shop");
        });

        modelBuilder.Entity<ShopEmployee>(entity =>
        {
            entity.HasKey(e => e.ShopEmployeeId).HasName("PK__ShopEmpl__4E36741D0D089B14");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.Salary).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ShopEmployees)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShopEmployees_CreatedBy");

            entity.HasOne(d => d.Shop).WithMany(p => p.ShopEmployees)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShopEmployees_Shop");
        });

        modelBuilder.Entity<ShopSubscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__ShopSubs__9A2B249DDB22FBCE");

            entity.Property(e => e.AmountPaid).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ShopSubscriptions)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_ShopSubscriptions_CreatedBy");

            entity.HasOne(d => d.Plan).WithMany(p => p.ShopSubscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShopSubscriptions_Plan");

            entity.HasOne(d => d.Shop).WithMany(p => p.ShopSubscriptions)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShopSubscriptions_Shop");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Subscrip__755C22B711E55D76");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PlanName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
        });

        modelBuilder.Entity<SystemActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__SystemAc__5E548648175B929E");

            entity.HasIndex(e => e.Action, "IX_SysLog_Action");

            entity.HasIndex(e => e.ActorId, "IX_SysLog_Actor");

            entity.HasIndex(e => e.CreatedAt, "IX_SysLog_CreatedAt");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(d => d.Actor).WithMany(p => p.SystemActivityLogs)
                .HasForeignKey(d => d.ActorId)
                .HasConstraintName("FK_SysLog_Actor");
        });

        modelBuilder.Entity<SystemBankAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__SystemBa__349DA5A6F73EC625");

            entity.Property(e => e.AccountName).HasMaxLength(225);
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(225)
                .IsUnicode(false);
            entity.Property(e => e.BankName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.QrImageCode).HasMaxLength(500);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__UserRole__3D978A352EB3E686");

            entity.HasIndex(e => new { e.EmployeeId, e.RoleId }, "UQ_UserRoles").IsUnique();

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.UserRoleAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK_UserRoles_AssignedBy");

            entity.HasOne(d => d.Employee).WithMany(p => p.UserRoleEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Employee");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Role");
        });

        modelBuilder.Entity<WorkSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__WorkSche__9C8A5B49A8026D1B");

            entity.Property(e => e.CheckIn).HasColumnType("datetime");
            entity.Property(e => e.CheckOut).HasColumnType("datetime");
            entity.Property(e => e.Shift).HasMaxLength(20);

            entity.HasOne(d => d.Employee).WithMany(p => p.WorkSchedules)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_WorkSchedules_Employees");

            entity.HasOne(d => d.Shop).WithMany(p => p.WorkSchedules)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FK_WorkSchedules_Shop");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
