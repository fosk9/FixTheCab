/* ============================================================
   PMP RACING — Motorcycle Repair Shop Management System
   Database Schema (SQL Server / T-SQL)
   Naming convention : PascalCase for all identifiers
   ============================================================ */


/* ============================================================
   RESET: drop and recreate the database
   ============================================================ */
/*
USE master;
GO

IF DB_ID('PmpRacing') IS NOT NULL
BEGIN
    ALTER DATABASE PmpRacing SET OFFLINE WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PmpRacing;
END
GO

CREATE DATABASE PmpRacing;
GO

USE PmpRacing;
GO
*/

/* ============================================================
   Employees
   Stores all staff accounts: mechanic, cashier, manager, admin.
   Used for authentication, scheduling, payroll, and audit trails.
   ============================================================ */
CREATE TABLE Employees (
    EmployeeId   INT IDENTITY PRIMARY KEY,
    Name         NVARCHAR(100)  NOT NULL,
    Email        NVARCHAR(100),
    Phone        NVARCHAR(20),
    Role         NVARCHAR(50),               -- mechanic | cashier | manager | admin
    Password     NVARCHAR(255),
    ProfileImagePath NVARCHAR(500),          -- image URL or ~/ path for profile avatar
    Status       NVARCHAR(20)   DEFAULT 'active',  -- active | inactive | locked
    CreatedAt    DATETIME       DEFAULT GETDATE()
);
GO


/* ============================================================
   Customers
   People who bring their bikes in for service.
   A customer can own multiple bikes.
   ============================================================ */
CREATE TABLE Customers (
    CustomerId   INT IDENTITY PRIMARY KEY,
    Name         NVARCHAR(100),
    Phone        NVARCHAR(20),
    Email        NVARCHAR(100),
    CreatedAt    DATETIME  DEFAULT GETDATE()
);
GO


/* ============================================================
   Bikes
   Each bike belongs to one customer.
   A customer may register more than one bike.
   ============================================================ */
CREATE TABLE Bikes (
    BikeId        INT IDENTITY PRIMARY KEY,
    CustomerId    INT,
    LicensePlate  NVARCHAR(20),
    BikeModel     NVARCHAR(100),

    CONSTRAINT FK_Bikes_Customers
        FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);
GO


/* ============================================================
   Services
   Master price list of all available repair / maintenance jobs.
   Mechanics reference this when building a service order.
   ============================================================ */
CREATE TABLE Services (
    ServiceId    INT IDENTITY PRIMARY KEY,
    ServiceName  NVARCHAR(100),
    Price        DECIMAL(12, 2)
);
GO


/* ============================================================
   Parts
   Inventory master for all spare parts held in the shop.
   WarningLevel triggers a low-stock alert when Stock falls below it.
   ============================================================ */
CREATE TABLE Parts (
    PartId        INT IDENTITY PRIMARY KEY,
    PartName      NVARCHAR(100),
    Price         DECIMAL(12, 2),
    Stock         INT,
    WarningLevel  INT   -- alert the manager when Stock <= WarningLevel
);
GO


/* ============================================================
   ServiceReceipts  (UC-01)
   Created by the cashier when a customer arrives.
   Acts as the entry point: one receipt per visit per bike.
   Status values: pending | assigned | in_progress | completed | cancelled
   ============================================================ */
CREATE TABLE ServiceReceipts (
    ReceiptId    INT IDENTITY PRIMARY KEY,
    BikeId       INT,
    CreatedBy    INT,                              -- FK → Employees (cashier)
    Status       NVARCHAR(50) DEFAULT 'pending',
    CreatedAt    DATETIME     DEFAULT GETDATE(),

    CONSTRAINT FK_ServiceReceipts_Bikes
        FOREIGN KEY (BikeId)     REFERENCES Bikes(BikeId),
    CONSTRAINT FK_ServiceReceipts_Employees
        FOREIGN KEY (CreatedBy)  REFERENCES Employees(EmployeeId)
);
GO


/* ============================================================
   MechanicAssignments  (UC-06)
   Manager explicitly assigns a mechanic to a service receipt.
   Keeps a full audit trail: every (re-)assignment is recorded.
   The latest active row for a ReceiptId is the current assignment.

   Workflow:
     1. Cashier creates a ServiceReceipt  (Status = 'pending')
     2. Manager opens the pending queue and picks a mechanic
     3. A row is inserted here; ServiceReceipts.Status → 'assigned'
     4. The mechanic picks up the job and ServiceOrders.Status → 'processing'
   ============================================================ */
CREATE TABLE MechanicAssignments (
    AssignmentId  INT IDENTITY PRIMARY KEY,
    ReceiptId     INT       NOT NULL,    -- the bike job being assigned
    MechanicId    INT       NOT NULL,    -- FK → Employees (mechanic)
    AssignedBy    INT       NOT NULL,    -- FK → Employees (manager)
    AssignedAt    DATETIME  DEFAULT GETDATE(),
    Note          NVARCHAR(255),         -- optional manager note

    CONSTRAINT FK_MechanicAssignments_Receipts
        FOREIGN KEY (ReceiptId)   REFERENCES ServiceReceipts(ReceiptId),
    CONSTRAINT FK_MechanicAssignments_Mechanic
        FOREIGN KEY (MechanicId)  REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_MechanicAssignments_Manager
        FOREIGN KEY (AssignedBy)  REFERENCES Employees(EmployeeId)
);
GO


/* ============================================================
   ServiceOrders  (UC-02)
   Detailed repair order created by the mechanic after inspecting
   the bike.  Linked to the receipt and carries the assigned mechanic.
   Status values: processing | completed | cancelled
   ============================================================ */
CREATE TABLE ServiceOrders (
    OrderId      INT IDENTITY PRIMARY KEY,
    ReceiptId    INT,
    MechanicId   INT,                      -- denormalised from MechanicAssignments for fast queries
    Status       NVARCHAR(50) DEFAULT 'processing',
    TotalPrice   DECIMAL(12, 2),
    StartedAt    DATETIME,                 -- UC-06: track time-on-job for productivity
    CompletedAt  DATETIME,
    CreatedAt    DATETIME     DEFAULT GETDATE(),

    CONSTRAINT FK_ServiceOrders_Receipts
        FOREIGN KEY (ReceiptId)   REFERENCES ServiceReceipts(ReceiptId),
    CONSTRAINT FK_ServiceOrders_Employees
        FOREIGN KEY (MechanicId)  REFERENCES Employees(EmployeeId)
);
GO


/* ============================================================
   ServiceOrderItems
   Line items: each service task (e.g. "Oil change") inside an order.
   Price is snapshotted at order time so historical orders are stable
   even if the master price list changes later.
   ============================================================ */
CREATE TABLE ServiceOrderItems (
    ItemId      INT IDENTITY PRIMARY KEY,
    OrderId     INT,
    ServiceId   INT,
    Price       DECIMAL(12, 2),   -- price at time of order (snapshot)

    CONSTRAINT FK_SOItems_Orders
        FOREIGN KEY (OrderId)    REFERENCES ServiceOrders(OrderId),
    CONSTRAINT FK_SOItems_Services
        FOREIGN KEY (ServiceId)  REFERENCES Services(ServiceId)
);
GO


/* ============================================================
   OrderParts
   Parts consumed during the repair.  Quantity × Price = cost line.
   Price is snapshotted at order time.
   ============================================================ */
CREATE TABLE OrderParts (
    Id        INT IDENTITY PRIMARY KEY,
    OrderId   INT,
    PartId    INT,
    Quantity  INT,
    Price     DECIMAL(12, 2),   -- unit price at time of order (snapshot)

    CONSTRAINT FK_OrderParts_Orders
        FOREIGN KEY (OrderId)  REFERENCES ServiceOrders(OrderId),
    CONSTRAINT FK_OrderParts_Parts
        FOREIGN KEY (PartId)   REFERENCES Parts(PartId)
);
GO


/* ============================================================
   Payments  (UC-05)
   One or more payment attempts per order.
   Method: cash | qr | transfer
   Status: pending | paid | failed | refunded
   ============================================================ */
CREATE TABLE Payments (
    PaymentId   INT IDENTITY PRIMARY KEY,
    OrderId     INT,
    Method      NVARCHAR(50),
    Amount      DECIMAL(12, 2),
    Status      NVARCHAR(50),
    PaidAt      DATETIME,

    CONSTRAINT FK_Payments_Orders
        FOREIGN KEY (OrderId)  REFERENCES ServiceOrders(OrderId)
);
GO


/* ============================================================
   InvoiceHistory  (UC-05 / UC-07)
   Immutable audit log: every status transition of a service order
   (draft → processing → completed → paid) is appended here.
   Provides the full lifecycle of every bill for reporting and
   dispute resolution.  Rows are never updated or deleted.
   ============================================================ */
CREATE TABLE InvoiceHistory (
    HistoryId      INT IDENTITY PRIMARY KEY,
    OrderId        INT           NOT NULL,
    ChangedBy      INT,                         -- FK → Employees (who triggered the change)
    PreviousStatus NVARCHAR(50),
    NewStatus      NVARCHAR(50),
    TotalPrice     DECIMAL(12, 2),              -- order total at the moment of this event
    Note           NVARCHAR(500),               -- reason or description for the change
    ChangedAt      DATETIME      DEFAULT GETDATE(),

    CONSTRAINT FK_InvoiceHistory_Orders
        FOREIGN KEY (OrderId)     REFERENCES ServiceOrders(OrderId),
    CONSTRAINT FK_InvoiceHistory_Employees
        FOREIGN KEY (ChangedBy)   REFERENCES Employees(EmployeeId)
);
GO


/* ============================================================
   WorkSchedules  (UC-04)
   Weekly shift roster for mechanics and cashiers.
   Shift: morning | afternoon
   Check-in / check-out are recorded by the system on the day.
   ============================================================ */
CREATE TABLE WorkSchedules (
    ScheduleId   INT IDENTITY PRIMARY KEY,
    EmployeeId   INT,
    WorkDate     DATE,
    Shift        NVARCHAR(20),    -- morning | afternoon
    CheckIn      DATETIME,
    CheckOut     DATETIME,

    CONSTRAINT FK_WorkSchedules_Employees
        FOREIGN KEY (EmployeeId)  REFERENCES Employees(EmployeeId)
);
GO


/* ============================================================
   LeaveRequests  (UC-09)
   Employees submit leave requests; managers approve or reject them.
   Status: pending | approved | rejected
   ApprovedBy is NULL until the manager acts on the request.
   ============================================================ */
CREATE TABLE LeaveRequests (
    RequestId    INT IDENTITY PRIMARY KEY,
    EmployeeId   INT,
    LeaveDate    DATE,
    Reason       NVARCHAR(255),
    Status       NVARCHAR(50)  DEFAULT 'pending',
    ApprovedBy   INT,           -- FK → Employees (manager); NULL while pending

    CONSTRAINT FK_LeaveRequests_Employees
        FOREIGN KEY (EmployeeId)  REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_LeaveRequests_Manager
        FOREIGN KEY (ApprovedBy)  REFERENCES Employees(EmployeeId)
);
GO


/* ============================================================
   HELPER VIEW — PendingAssignmentQueue  (UC-06)
   Manager opens this view to see every receipt that is still
   'pending' (no mechanic assigned yet), along with the customer
   name, bike model and licence plate.
   ============================================================ */
CREATE VIEW PendingAssignmentQueue AS
SELECT
    sr.ReceiptId,
    sr.CreatedAt         AS ReceivedAt,
    c.Name               AS CustomerName,
    c.Phone              AS CustomerPhone,
    b.LicensePlate,
    b.BikeModel,
    e.Name               AS CreatedByCashier
FROM ServiceReceipts  sr
JOIN Bikes            b  ON b.BikeId      = sr.BikeId
JOIN Customers        c  ON c.CustomerId  = b.CustomerId
JOIN Employees        e  ON e.EmployeeId  = sr.CreatedBy
WHERE sr.Status = 'pending';
GO


/* ============================================================
   HELPER VIEW — MechanicWorkload  (UC-06)
   Manager uses this to see how many active jobs each mechanic
   currently has before deciding who to assign next.
   ============================================================ */
CREATE VIEW MechanicWorkload AS
SELECT
    emp.EmployeeId,
    emp.Name                         AS MechanicName,
    COUNT(so.OrderId)                AS ActiveJobCount
FROM Employees       emp
LEFT JOIN ServiceOrders so
       ON so.MechanicId = emp.EmployeeId
      AND so.Status      = 'processing'
WHERE emp.Role = 'mechanic'
  AND emp.Status = 'active'
GROUP BY emp.EmployeeId, emp.Name;
GO


/* ============================================================
   STORED PROCEDURE — AssignMechanicToReceipt  (UC-06)
   Called by the Manager screen when a mechanic is selected
   for a pending service receipt.

   Steps performed atomically:
     1. Validate the receipt is still 'pending'
     2. Validate the chosen employee is an active mechanic
     3. Insert a row into MechanicAssignments (audit trail)
     4. Update ServiceReceipts.Status → 'assigned'

   Usage:
     EXEC AssignMechanicToReceipt
         @ReceiptId  = 5,
         @MechanicId = 3,
         @ManagerId  = 2,
         @Note       = N'Assigned to Tuan – specialises in Honda';
   ============================================================ */
CREATE PROCEDURE AssignMechanicToReceipt
    @ReceiptId   INT,
    @MechanicId  INT,
    @ManagerId   INT,
    @Note        NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Guard: receipt must be pending (not already assigned or in progress)
    IF NOT EXISTS (
        SELECT 1 FROM ServiceReceipts
        WHERE ReceiptId = @ReceiptId AND Status = 'pending'
    )
    BEGIN
        RAISERROR('Receipt is not in pending status and cannot be assigned.', 16, 1);
        RETURN;
    END

    -- Guard: target employee must be an active mechanic
    IF NOT EXISTS (
        SELECT 1 FROM Employees
        WHERE EmployeeId = @MechanicId
          AND Role       = 'mechanic'
          AND Status     = 'active'
    )
    BEGIN
        RAISERROR('The selected employee is not an active mechanic.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Record the assignment in the audit table
        INSERT INTO MechanicAssignments (ReceiptId, MechanicId, AssignedBy, Note)
        VALUES (@ReceiptId, @MechanicId, @ManagerId, @Note);

        -- Advance the receipt status so cashiers/mechanics can see it is taken
        UPDATE ServiceReceipts
        SET    Status = 'assigned'
        WHERE  ReceiptId = @ReceiptId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


/* ============================================================
   STORED PROCEDURE — LogInvoiceStatusChange
   Call this whenever ServiceOrders.Status changes so the
   InvoiceHistory table stays up to date automatically.

   Usage (example – call from application layer or a trigger):
     EXEC LogInvoiceStatusChange
         @OrderId        = 10,
         @ChangedBy      = 4,
         @PreviousStatus = N'processing',
         @NewStatus      = N'completed',
         @TotalPrice     = 350000,
         @Note           = N'All work confirmed by customer';
   ============================================================ */
CREATE PROCEDURE LogInvoiceStatusChange
    @OrderId         INT,
    @ChangedBy       INT,
    @PreviousStatus  NVARCHAR(50),
    @NewStatus       NVARCHAR(50),
    @TotalPrice      DECIMAL(12, 2),
    @Note            NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO InvoiceHistory
        (OrderId, ChangedBy, PreviousStatus, NewStatus, TotalPrice, Note)
    VALUES
        (@OrderId, @ChangedBy, @PreviousStatus, @NewStatus, @TotalPrice, @Note);
END;
GO