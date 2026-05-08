/* ============================================================
   PMP RACING — UPDATED SCHEMA
   Multi-role users, subscription packages, shop management,
   worker management, and activity logging
   All monetary values are in VND.
   ============================================================ */

DROP DATABASE IF EXISTS PmpRacing;
CREATE DATABASE PmpRacing;
USE PmpRacing;
GO

/* ============================================================
   1. ROLES — Define system roles
   ============================================================ */
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

/* ============================================================
   2. USERS — Core user accounts (replaces Employees table structure)
   ============================================================ */
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20),
    Password NVARCHAR(255) NOT NULL, -- bcrypt hashed
    FullName NVARCHAR(200) NOT NULL,
    ProfileImagePath NVARCHAR(500),
    Status NVARCHAR(20) DEFAULT 'active', -- active, inactive, suspended
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO

/* ============================================================
   3. USER_ROLES — Many-to-many: users can have multiple roles
   ============================================================ */
CREATE TABLE UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME DEFAULT GETDATE(),
    AssignedBy INT, -- UserId of admin who assigned this role
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    FOREIGN KEY (AssignedBy) REFERENCES Users(UserId),
    UNIQUE(UserId, RoleId)
);
GO

/* ============================================================
   4. SHOPS — Each user can create/own a shop
   ============================================================ */
CREATE TABLE Shops (
    ShopId INT PRIMARY KEY IDENTITY(1,1),
    OwnerId INT NOT NULL,
    ShopName NVARCHAR(200) NOT NULL,
    Address NVARCHAR(500),
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Logo NVARCHAR(500),
    Status NVARCHAR(20) DEFAULT 'active', -- active, inactive, closed
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OwnerId) REFERENCES Users(UserId) ON DELETE CASCADE
);
GO

/* ============================================================
   5. EMPLOYEES — Shop employees (linked to Shop, not standalone users)
   ============================================================ */
CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY IDENTITY(1,1),
    ShopId INT NOT NULL,
    UserId INT, -- NULL if employee has no user account
    Name NVARCHAR(200) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Role NVARCHAR(50), -- 'mechanic', 'cashier', 'manager', etc.
    HireDate DATE,
    Status NVARCHAR(20) DEFAULT 'active', -- active, inactive
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ShopId) REFERENCES Shops(ShopId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE SET NULL
);
GO

/* ============================================================
   6. WORKERS — Workers without user accounts (created by mechanics/managers)
   ============================================================ */
CREATE TABLE Workers (
    WorkerId INT PRIMARY KEY IDENTITY(1,1),
    ShopId INT NOT NULL,
    CreatedByEmployeeId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Specialty NVARCHAR(200), -- e.g., 'Engine repair', 'Tire change'
    Status NVARCHAR(20) DEFAULT 'active', -- active, inactive
    Notes NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ShopId) REFERENCES Shops(ShopId) ON DELETE CASCADE,
    FOREIGN KEY (CreatedByEmployeeId) REFERENCES Employees(EmployeeId)
);
GO

/* ============================================================
   7. SUBSCRIPTION_PLANS — Store pricing for packages
   ============================================================ */
CREATE TABLE SubscriptionPlans (
    PlanId INT PRIMARY KEY IDENTITY(1,1),
    PlanName NVARCHAR(100) NOT NULL, -- 'Basic', 'Pro', 'Premium'
    DurationMonths INT NOT NULL, -- 3, 12
    Price DECIMAL(15, 2) NOT NULL, -- VND
    Features NVARCHAR(MAX), -- JSON or comma-separated features
    Status NVARCHAR(20) DEFAULT 'active', -- active, archived
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO

/* ============================================================
   8. USER_SUBSCRIPTIONS — Track user subscriptions
   ============================================================ */
CREATE TABLE UserSubscriptions (
    SubscriptionId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    PlanId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Price DECIMAL(15, 2) NOT NULL, -- snapshot of price paid
    Status NVARCHAR(20) DEFAULT 'active', -- active, expired, cancelled
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (PlanId) REFERENCES SubscriptionPlans(PlanId)
);
GO

/* ============================================================
   9. CUSTOMERS — Shop customers
   ============================================================ */
CREATE TABLE Customers (
    CustomerId INT PRIMARY KEY IDENTITY(1,1),
    ShopId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(500),
    Status NVARCHAR(20) DEFAULT 'active',
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ShopId) REFERENCES Shops(ShopId) ON DELETE CASCADE
);
GO

/* ============================================================
   10. BIKES — Customer bikes
   ============================================================ */
CREATE TABLE Bikes (
    BikeId INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    LicensePlate NVARCHAR(50) NOT NULL,
    BikeModel NVARCHAR(200),
    Color NVARCHAR(50),
    Year INT,
    Notes NVARCHAR(500),
    Status NVARCHAR(20) DEFAULT 'active',
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId) ON DELETE CASCADE
);
GO

/* ============================================================
   11. SERVICES — Service catalog (per shop)
   ============================================================ */
CREATE TABLE Services (
    ServiceId INT PRIMARY KEY IDENTITY(1,1),
    ShopId INT NOT NULL,
    ServiceName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    Price DECIMAL(15, 2) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'active',
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ShopId) REFERENCES Shops(ShopId) ON DELETE CASCADE
);
GO

/* ============================================================
   12. PARTS — Spare parts inventory (per shop)
   ============================================================ */
CREATE TABLE Parts (
    PartId INT PRIMARY KEY IDENTITY(1,1),
    ShopId INT NOT NULL,
    PartName NVARCHAR(200) NOT NULL,
    Price DECIMAL(15, 2) NOT NULL,
    Stock INT DEFAULT 0,
    WarningLevel INT DEFAULT 5,
    Status NVARCHAR(20) DEFAULT 'active',
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ShopId) REFERENCES Shops(ShopId) ON DELETE CASCADE
);
GO

/* ============================================================
   13. SERVICE_RECEIPTS — Service request records
   ============================================================ */
CREATE TABLE ServiceReceipts (
    ReceiptId INT PRIMARY KEY IDENTITY(1,1),
    ShopId INT NOT NULL,
    BikeId INT NOT NULL,
    CreatedByEmployeeId INT NOT NULL,
    Status NVARCHAR(20) DEFAULT 'pending', -- pending, assigned, in_progress, completed
    Notes NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CompletedAt DATETIME,
    FOREIGN KEY (ShopId) REFERENCES Shops(ShopId),
    FOREIGN KEY (BikeId) REFERENCES Bikes(BikeId),
    FOREIGN KEY (CreatedByEmployeeId) REFERENCES Employees(EmployeeId)
);
GO

/* ============================================================
   14. MECHANIC_ASSIGNMENTS — Assign mechanics/workers to receipts
   ============================================================ */
CREATE TABLE MechanicAssignments (
    AssignmentId INT PRIMARY KEY IDENTITY(1,1),
    ReceiptId INT NOT NULL,
    EmployeeId INT, -- NULL if assigned to Worker
    WorkerId INT, -- NULL if assigned to Employee
    AssignedBy INT NOT NULL,
    AssignedAt DATETIME DEFAULT GETDATE(),
    Note NVARCHAR(500),
    CompletedAt DATETIME,
    FOREIGN KEY (ReceiptId) REFERENCES ServiceReceipts(ReceiptId),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (AssignedBy) REFERENCES Employees(EmployeeId)
);
GO

/* ============================================================
   15. SERVICE_ORDERS — Detailed service orders
   ============================================================ */
CREATE TABLE ServiceOrders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    ReceiptId INT NOT NULL,
    AssignmentId INT,
    Status NVARCHAR(20) DEFAULT 'pending', -- pending, processing, completed, cancelled
    TotalPrice DECIMAL(15, 2),
    StartedAt DATETIME,
    CompletedAt DATETIME,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ReceiptId) REFERENCES ServiceReceipts(ReceiptId),
    FOREIGN KEY (AssignmentId) REFERENCES MechanicAssignments(AssignmentId)
);
GO

/* ============================================================
   16. SERVICE_ORDER_ITEMS — Services in each order
   ============================================================ */
CREATE TABLE ServiceOrderItems (
    OrderItemId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ServiceId INT NOT NULL,
    Price DECIMAL(15, 2) NOT NULL, -- snapshot
    Quantity INT DEFAULT 1,
    FOREIGN KEY (OrderId) REFERENCES ServiceOrders(OrderId) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId)
);
GO

/* ============================================================
   17. ORDER_PARTS — Parts used in each order
   ============================================================ */
CREATE TABLE OrderParts (
    OrderPartId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    PartId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(15, 2) NOT NULL, -- snapshot
    FOREIGN KEY (OrderId) REFERENCES ServiceOrders(OrderId) ON DELETE CASCADE,
    FOREIGN KEY (PartId) REFERENCES Parts(PartId)
);
GO

/* ============================================================
   18. PAYMENTS — Payment records
   ============================================================ */
CREATE TABLE Payments (
    PaymentId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    Method NVARCHAR(50), -- cash, card, qr, transfer
    Amount DECIMAL(15, 2),
    Status NVARCHAR(20) DEFAULT 'pending', -- pending, paid, failed, refunded
    PaidAt DATETIME,
    Notes NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrderId) REFERENCES ServiceOrders(OrderId)
);
GO

/* ============================================================
   19. INVOICE_HISTORY — Audit trail for status changes
   ============================================================ */
CREATE TABLE InvoiceHistory (
    HistoryId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ChangedByEmployeeId INT NOT NULL,
    PreviousStatus NVARCHAR(50),
    NewStatus NVARCHAR(50),
    TotalPrice DECIMAL(15, 2),
    Note NVARCHAR(500),
    ChangedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrderId) REFERENCES ServiceOrders(OrderId),
    FOREIGN KEY (ChangedByEmployeeId) REFERENCES Employees(EmployeeId)
);
GO

/* ============================================================
   20. WORK_SCHEDULES — Employee schedules (per shop)
   ============================================================ */
CREATE TABLE WorkSchedules (
    ScheduleId INT PRIMARY KEY IDENTITY(1,1),
    ShopId INT NOT NULL,
    EmployeeId INT NOT NULL,
    WorkDate DATE NOT NULL,
    Shift NVARCHAR(20), -- 'morning', 'afternoon', 'night'
    CheckIn DATETIME,
    CheckOut DATETIME,
    Status NVARCHAR(20) DEFAULT 'pending', -- pending, checked_in, checked_out
    FOREIGN KEY (ShopId) REFERENCES Shops(ShopId),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId)
);
GO

/* ============================================================
   21. LEAVE_REQUESTS — Employee leave/time-off requests
   ============================================================ */
CREATE TABLE LeaveRequests (
    LeaveId INT PRIMARY KEY IDENTITY(1,1),
    ShopId INT NOT NULL,
    EmployeeId INT NOT NULL,
    LeaveDate DATE NOT NULL,
    Reason NVARCHAR(500),
    Status NVARCHAR(20) DEFAULT 'pending', -- pending, approved, rejected
    ApprovedBy INT, -- EmployeeId of approver
    ApprovedAt DATETIME,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ShopId) REFERENCES Shops(ShopId),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    FOREIGN KEY (ApprovedBy) REFERENCES Employees(EmployeeId)
);
GO

/* ============================================================
   22. SYSTEM_ACTIVITY_LOGS — System-wide activity logging
   ============================================================ */
CREATE TABLE SystemActivityLogs (
    LogId BIGINT PRIMARY KEY IDENTITY(1,1),
    UserId INT,
    ActivityType NVARCHAR(100), -- 'user_login', 'user_created', 'shop_created', 'subscription_purchased', etc.
    EntityType NVARCHAR(100), -- 'User', 'Shop', 'Order', 'Subscription'
    EntityId INT, -- ID of related entity
    Description NVARCHAR(MAX),
    IPAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    Status NVARCHAR(20), -- 'success', 'failed'
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE SET NULL,
    INDEX idx_UserId (UserId),
    INDEX idx_ActivityType (ActivityType),
    INDEX idx_CreatedAt (CreatedAt DESC)
);
GO

/* ============================================================
   23. SHOP_ACTIVITY_LOGS — Per-shop activity logging
   ============================================================ */
CREATE TABLE ShopActivityLogs (
    LogId BIGINT PRIMARY KEY IDENTITY(1,1),
    ShopId INT NOT NULL,
    EmployeeId INT,
    ActivityType NVARCHAR(100), -- 'receipt_created', 'order_started', 'order_completed', 'payment_received', etc.
    EntityType NVARCHAR(100), -- 'Receipt', 'Order', 'Payment', 'Employee'
    EntityId INT, -- ID of related entity
    Description NVARCHAR(MAX),
    Details NVARCHAR(MAX), -- JSON for additional details
    Status NVARCHAR(20), -- 'success', 'failed'
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ShopId) REFERENCES Shops(ShopId) ON DELETE CASCADE,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId) ON DELETE SET NULL,
    INDEX idx_ShopId (ShopId),
    INDEX idx_ActivityType (ActivityType),
    INDEX idx_CreatedAt (CreatedAt DESC)
);
GO

/* ============================================================
   INDEXES for common queries
   ============================================================ */
CREATE INDEX idx_UserRoles_UserId ON UserRoles(UserId);
CREATE INDEX idx_UserRoles_RoleId ON UserRoles(RoleId);
CREATE INDEX idx_Employees_ShopId ON Employees(ShopId);
CREATE INDEX idx_Employees_UserId ON Employees(UserId);
CREATE INDEX idx_Workers_ShopId ON Workers(ShopId);
CREATE INDEX idx_Customers_ShopId ON Customers(ShopId);
CREATE INDEX idx_Bikes_CustomerId ON Bikes(CustomerId);
CREATE INDEX idx_ServiceReceipts_ShopId ON ServiceReceipts(ShopId);
CREATE INDEX idx_ServiceOrders_ReceiptId ON ServiceOrders(ReceiptId);
CREATE INDEX idx_Payments_OrderId ON Payments(OrderId);
CREATE INDEX idx_UserSubscriptions_UserId ON UserSubscriptions(UserId);
GO

PRINT 'PMP Racing Updated Schema created successfully!';