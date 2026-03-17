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

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceOrder> ServiceOrders { get; set; }

    public virtual DbSet<ServiceOrderItem> ServiceOrderItems { get; set; }

    public virtual DbSet<ServiceReceipt> ServiceReceipts { get; set; }

    public virtual DbSet<WorkSchedule> WorkSchedules { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=(local);Database= PmpRacing;User Id=sa;Password=123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bike>(entity =>
        {
            entity.HasKey(e => e.BikeId).HasName("PK__Bikes__7DC817218B51E47D");

            entity.Property(e => e.BikeModel).HasMaxLength(100);
            entity.Property(e => e.LicensePlate).HasMaxLength(20);

            entity.HasOne(d => d.Customer).WithMany(p => p.Bikes)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Bikes_Customers");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D886C410A9");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04F11B06FEA69");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active");
        });

        modelBuilder.Entity<InvoiceHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__InvoiceH__4D7B4ABD106BCD0D");

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
            entity.HasKey(e => e.RequestId).HasName("PK__LeaveReq__33A8517A8C2542E1");

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
            entity.HasKey(e => e.AssignmentId).HasName("PK__Mechanic__32499E771DA159D0");

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
            entity.HasKey(e => e.Id).HasName("PK__OrderPar__3214EC07F335941E");

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
            entity.HasKey(e => e.PartId).HasName("PK__Parts__7C3F0D50C3DE93C1");

            entity.Property(e => e.PartName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A38E56747A5");

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

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB00A937AB3B6");

            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ServiceName).HasMaxLength(100);
        });

        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__ServiceO__C3905BCF0A6A0D00");

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
            entity.HasKey(e => e.ItemId).HasName("PK__ServiceO__727E838B97E74CD7");

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
            entity.HasKey(e => e.ReceiptId).HasName("PK__ServiceR__CC08C420CA6DB106");

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
        });

        modelBuilder.Entity<WorkSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__WorkSche__9C8A5B498F1AF86E");

            entity.Property(e => e.CheckIn).HasColumnType("datetime");
            entity.Property(e => e.CheckOut).HasColumnType("datetime");
            entity.Property(e => e.Shift).HasMaxLength(20);

            entity.HasOne(d => d.Employee).WithMany(p => p.WorkSchedules)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_WorkSchedules_Employees");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
