/* ============================================================
   PMP RACING — Motorcycle Repair Shop Management System
   Database Schema v2  (SQL Server / T-SQL)
   Naming convention : PascalCase for all identifiers

   THAY ĐỔI SO VỚI v1:
   ① Roles & UserRoles       — mỗi user nhiều role, quản lý theo ánh xạ
   ② SubscriptionPlans       — bảng giá gói đăng ký (3 tháng, 1 năm, …)
   ③ Shops                   — mỗi người dùng tạo được 1 cửa hàng
   ④ ShopEmployees           — thợ tạo nhân viên không cần tài khoản hệ thống
   ⑤ ActivationKeys          — lưu key kích hoạt; mỗi Shop có 1 key riêng
   ⑥ SystemActivityLogs      — log hoạt động toàn hệ thống
   ⑦ ShopActivityLogs        — log hoạt động từng cửa hàng
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


/* ===========================================================
   ① ROLES & USER-ROLE MAPPING
   =========================================================== */

/* Danh mục role hệ thống
   Thay vì lưu role dạng chuỗi tự do trong Employees,
   ta tách ra bảng riêng để dễ quản lý và phân quyền.       */
CREATE TABLE Roles (
    RoleId      INT IDENTITY PRIMARY KEY,
    RoleName    NVARCHAR(50)  NOT NULL UNIQUE,   -- admin | manager | cashier | mechanic | owner …
    Description NVARCHAR(255),
    CreatedAt   DATETIME  DEFAULT GETDATE()
);
GO

/* ============================================================
   Employees
   Không còn cột Role đơn trị — phân quyền qua UserRoles.
   ============================================================ */
CREATE TABLE Employees (
    EmployeeId       INT IDENTITY PRIMARY KEY,
    Name             NVARCHAR(100)  NOT NULL,
    Email            NVARCHAR(100),
    Phone            NVARCHAR(20),
    Password         NVARCHAR(255),
    ProfileImagePath NVARCHAR(500),
    Status           NVARCHAR(20)  DEFAULT 'active',   -- active | inactive | locked
    CreatedAt        DATETIME      DEFAULT GETDATE()
);
GO

/* Ánh xạ nhiều-nhiều: 1 nhân viên có thể có nhiều role       */
CREATE TABLE UserRoles (
    UserRoleId   INT IDENTITY PRIMARY KEY,
    EmployeeId   INT  NOT NULL,
    RoleId       INT  NOT NULL,
    AssignedBy   INT,                          -- FK → Employees (người cấp quyền)
    AssignedAt   DATETIME  DEFAULT GETDATE(),

    CONSTRAINT UQ_UserRoles UNIQUE (EmployeeId, RoleId),

    CONSTRAINT FK_UserRoles_Employee
        FOREIGN KEY (EmployeeId)  REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_UserRoles_Role
        FOREIGN KEY (RoleId)      REFERENCES Roles(RoleId),
    CONSTRAINT FK_UserRoles_AssignedBy
        FOREIGN KEY (AssignedBy)  REFERENCES Employees(EmployeeId)
);
GO


/* ===========================================================
   ② SUBSCRIPTION PLANS & SHOP SUBSCRIPTIONS
   =========================================================== */

/* Bảng giá các gói đăng ký
   VD: gói 3 tháng = 299 000 VND, gói 1 năm = 999 000 VND   */
CREATE TABLE SubscriptionPlans (
    PlanId        INT IDENTITY PRIMARY KEY,
    PlanName      NVARCHAR(100)  NOT NULL,        -- "Gói 3 tháng", "Gói 1 năm", …
    DurationDays  INT            NOT NULL,         -- 90 | 365 | …
    Price         DECIMAL(12, 2) NOT NULL,
    Description   NVARCHAR(500),
    IsActive      BIT            DEFAULT 1,        -- ẩn gói cũ mà không xóa
    CreatedAt     DATETIME       DEFAULT GETDATE()
);
GO

/* Lịch sử đăng ký của từng cửa hàng (xem bảng Shops bên dưới) */
CREATE TABLE ShopSubscriptions (
    SubscriptionId  INT IDENTITY PRIMARY KEY,
    ShopId          INT            NOT NULL,       -- FK → Shops
    PlanId          INT            NOT NULL,       -- FK → SubscriptionPlans
    StartDate       DATE           NOT NULL,
    EndDate         DATE           NOT NULL,       -- StartDate + DurationDays
    AmountPaid      DECIMAL(12, 2) NOT NULL,
    PaymentMethod   NVARCHAR(50),                  -- cash | transfer | qr
    Status          NVARCHAR(20)   DEFAULT 'active', -- active | expired | cancelled
    CreatedBy       INT,                           -- FK → Employees (admin xử lý)
    CreatedAt       DATETIME       DEFAULT GETDATE(),

    CONSTRAINT FK_ShopSubscriptions_Plan
        FOREIGN KEY (PlanId)      REFERENCES SubscriptionPlans(PlanId),
    CONSTRAINT FK_ShopSubscriptions_CreatedBy
        FOREIGN KEY (CreatedBy)   REFERENCES Employees(EmployeeId)
    -- FK tới Shops thêm sau khi bảng Shops được tạo
);
GO


/* ===========================================================
   ③ SHOPS — mỗi người dùng tạo được 1 cửa hàng
   =========================================================== */
CREATE TABLE Shops (
    ShopId       INT IDENTITY PRIMARY KEY,
    OwnerId      INT            NOT NULL UNIQUE,   -- 1 user chỉ có 1 shop
    ShopName     NVARCHAR(200)  NOT NULL,
    Address      NVARCHAR(500),
    Phone        NVARCHAR(20),
    LogoPath     NVARCHAR(500),
    Status       NVARCHAR(20)   DEFAULT 'active',  -- active | suspended | closed
    CreatedAt    DATETIME       DEFAULT GETDATE(),

    CONSTRAINT FK_Shops_Owner
        FOREIGN KEY (OwnerId)  REFERENCES Employees(EmployeeId)
);
GO

-- Thêm FK ngược từ ShopSubscriptions về Shops
ALTER TABLE ShopSubscriptions
    ADD CONSTRAINT FK_ShopSubscriptions_Shop
        FOREIGN KEY (ShopId) REFERENCES Shops(ShopId);
GO


/* ===========================================================
   ④ SHOP EMPLOYEES
   Thợ/manager có thể tạo nhân viên cửa hàng mà KHÔNG cần
   tài khoản hệ thống (Employees).  Đây là nhân sự nội bộ shop.
   =========================================================== */
CREATE TABLE ShopEmployees (
    ShopEmployeeId   INT IDENTITY PRIMARY KEY,
    ShopId           INT            NOT NULL,
    FullName         NVARCHAR(100)  NOT NULL,
    Phone            NVARCHAR(20),
    Position         NVARCHAR(100),              -- "Thợ chính", "Thợ phụ", "Thu ngân", …
    Salary           DECIMAL(12, 2),
    JoinDate         DATE,
    Status           NVARCHAR(20)  DEFAULT 'active',  -- active | inactive | left
    Note             NVARCHAR(500),
    CreatedBy        INT            NOT NULL,    -- FK → Employees (thợ/manager tạo)
    CreatedAt        DATETIME      DEFAULT GETDATE(),

    CONSTRAINT FK_ShopEmployees_Shop
        FOREIGN KEY (ShopId)     REFERENCES Shops(ShopId),
    CONSTRAINT FK_ShopEmployees_CreatedBy
        FOREIGN KEY (CreatedBy)  REFERENCES Employees(EmployeeId)
);
GO


/* ===========================================================
   ⑤ ACTIVATION KEYS
   Mỗi Shop có 1 key kích hoạt riêng.
   Key được sinh khi shop đăng ký gói, dùng để xác thực
   phần mềm client (desktop app, POS, …).
   =========================================================== */
CREATE TABLE ActivationKeys (
    KeyId           INT IDENTITY PRIMARY KEY,
    ShopId          INT            NOT NULL UNIQUE,  -- 1 shop / 1 key active tại 1 thời điểm
    LicenseKey      NVARCHAR(64)   NOT NULL UNIQUE,  -- chuỗi key, VD: XXXX-XXXX-XXXX-XXXX
    SubscriptionId  INT,                             -- gói tương ứng (có thể NULL nếu trial)
    IssuedAt        DATETIME       DEFAULT GETDATE(),
    ExpiresAt       DATETIME       NOT NULL,
    ActivatedAt     DATETIME,                        -- thời điểm client kích hoạt lần đầu
    LastCheckedAt   DATETIME,                        -- lần cuối client ping kiểm tra key
    Status          NVARCHAR(20)   DEFAULT 'pending', -- pending | active | expired | revoked
    IssuedBy        INT,                             -- FK → Employees (admin cấp key)

    CONSTRAINT FK_ActivationKeys_Shop
        FOREIGN KEY (ShopId)          REFERENCES Shops(ShopId),
    CONSTRAINT FK_ActivationKeys_Subscription
        FOREIGN KEY (SubscriptionId)  REFERENCES ShopSubscriptions(SubscriptionId),
    CONSTRAINT FK_ActivationKeys_IssuedBy
        FOREIGN KEY (IssuedBy)        REFERENCES Employees(EmployeeId)
);
GO


/* ===========================================================
   ⑥ SYSTEM ACTIVITY LOGS — log hoạt động toàn hệ thống
   Ghi mọi thao tác quan trọng: login, tạo shop, cấp key,
   thay đổi plan, khóa tài khoản, …
   =========================================================== */
CREATE TABLE SystemActivityLogs (
    LogId        BIGINT IDENTITY PRIMARY KEY,
    ActorId      INT,                        -- FK → Employees (người thực hiện; NULL = system)
    Action       NVARCHAR(100)  NOT NULL,    -- 'LOGIN' | 'CREATE_SHOP' | 'ISSUE_KEY' | …
    EntityType   NVARCHAR(50),               -- 'Shop' | 'Employee' | 'ActivationKey' | …
    EntityId     INT,                        -- ID của bản ghi bị tác động
    Detail       NVARCHAR(MAX),              -- JSON hoặc mô tả tự do
    IpAddress    NVARCHAR(45),               -- hỗ trợ IPv4 & IPv6
    UserAgent    NVARCHAR(500),
    CreatedAt    DATETIME  DEFAULT GETDATE(),

    CONSTRAINT FK_SysLog_Actor
        FOREIGN KEY (ActorId)  REFERENCES Employees(EmployeeId)
);
GO

CREATE INDEX IX_SysLog_Actor     ON SystemActivityLogs (ActorId);
CREATE INDEX IX_SysLog_Action    ON SystemActivityLogs (Action);
CREATE INDEX IX_SysLog_CreatedAt ON SystemActivityLogs (CreatedAt);
GO


/* ===========================================================
   ⑦ SHOP ACTIVITY LOGS — log hoạt động từng cửa hàng
   Ghi mọi thao tác nghiệp vụ trong phạm vi một shop:
   tạo phiếu, phân công, thanh toán, nhập kho, …
   =========================================================== */
CREATE TABLE ShopActivityLogs (
    LogId        BIGINT IDENTITY PRIMARY KEY,
    ShopId       INT            NOT NULL,    -- log thuộc cửa hàng nào
    ActorId      INT,                        -- FK → Employees  (có thể NULL nếu khách)
    Action       NVARCHAR(100)  NOT NULL,    -- 'CREATE_RECEIPT' | 'ASSIGN_MECHANIC' | …
    EntityType   NVARCHAR(50),               -- 'ServiceReceipt' | 'Payment' | 'Part' | …
    EntityId     INT,
    Detail       NVARCHAR(MAX),
    IpAddress    NVARCHAR(45),
    CreatedAt    DATETIME  DEFAULT GETDATE(),

    CONSTRAINT FK_ShopLog_Shop
        FOREIGN KEY (ShopId)   REFERENCES Shops(ShopId),
    CONSTRAINT FK_ShopLog_Actor
        FOREIGN KEY (ActorId)  REFERENCES Employees(EmployeeId)
);
GO

CREATE INDEX IX_ShopLog_Shop      ON ShopActivityLogs (ShopId);
CREATE INDEX IX_ShopLog_Actor     ON ShopActivityLogs (ActorId);
CREATE INDEX IX_ShopLog_CreatedAt ON ShopActivityLogs (CreatedAt);
GO


/* ============================================================
   CÁC BẢNG NGHIỆP VỤ GỐC (giữ nguyên, chỉ cập nhật FK)
   ============================================================ */

CREATE TABLE Customers (
    CustomerId   INT IDENTITY PRIMARY KEY,
    Name         NVARCHAR(100),
    Phone        NVARCHAR(20),
    Email        NVARCHAR(100),
    ShopId       INT,                        -- khách thuộc cửa hàng nào
    CreatedAt    DATETIME  DEFAULT GETDATE(),

    CONSTRAINT FK_Customers_Shop
        FOREIGN KEY (ShopId)  REFERENCES Shops(ShopId)
);
GO

CREATE TABLE Bikes (
    BikeId        INT IDENTITY PRIMARY KEY,
    CustomerId    INT,
    LicensePlate  NVARCHAR(20),
    BikeModel     NVARCHAR(100),

    CONSTRAINT FK_Bikes_Customers
        FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);
GO

CREATE TABLE Services (
    ServiceId    INT IDENTITY PRIMARY KEY,
    ShopId       INT,                        -- dịch vụ thuộc cửa hàng (NULL = global)
    ServiceName  NVARCHAR(100),
    Price        DECIMAL(12, 2),

    CONSTRAINT FK_Services_Shop
        FOREIGN KEY (ShopId)  REFERENCES Shops(ShopId)
);
GO

CREATE TABLE Parts (
    PartId        INT IDENTITY PRIMARY KEY,
    ShopId        INT,                        -- kho thuộc cửa hàng
    PartName      NVARCHAR(100),
    Price         DECIMAL(12, 2),
    Stock         INT,
    WarningLevel  INT,

    CONSTRAINT FK_Parts_Shop
        FOREIGN KEY (ShopId)  REFERENCES Shops(ShopId)
);
GO

CREATE TABLE ServiceReceipts (
    ReceiptId    INT IDENTITY PRIMARY KEY,
    ShopId       INT,                              -- phiếu thuộc cửa hàng nào
    BikeId       INT,
    CreatedBy    INT,
    Status       NVARCHAR(50) DEFAULT 'pending',
    CreatedAt    DATETIME     DEFAULT GETDATE(),

    CONSTRAINT FK_ServiceReceipts_Shop
        FOREIGN KEY (ShopId)     REFERENCES Shops(ShopId),
    CONSTRAINT FK_ServiceReceipts_Bikes
        FOREIGN KEY (BikeId)     REFERENCES Bikes(BikeId),
    CONSTRAINT FK_ServiceReceipts_Employees
        FOREIGN KEY (CreatedBy)  REFERENCES Employees(EmployeeId)
);
GO

CREATE TABLE MechanicAssignments (
    AssignmentId  INT IDENTITY PRIMARY KEY,
    ReceiptId     INT       NOT NULL,
    MechanicId    INT       NOT NULL,
    AssignedBy    INT       NOT NULL,
    AssignedAt    DATETIME  DEFAULT GETDATE(),
    Note          NVARCHAR(255),

    CONSTRAINT FK_MechanicAssignments_Receipts
        FOREIGN KEY (ReceiptId)   REFERENCES ServiceReceipts(ReceiptId),
    CONSTRAINT FK_MechanicAssignments_Mechanic
        FOREIGN KEY (MechanicId)  REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_MechanicAssignments_Manager
        FOREIGN KEY (AssignedBy)  REFERENCES Employees(EmployeeId)
);
GO

CREATE TABLE ServiceOrders (
    OrderId      INT IDENTITY PRIMARY KEY,
    ReceiptId    INT,
    MechanicId   INT,
    Status       NVARCHAR(50) DEFAULT 'processing',
    TotalPrice   DECIMAL(12, 2),
    StartedAt    DATETIME,
    CompletedAt  DATETIME,
    CreatedAt    DATETIME     DEFAULT GETDATE(),

    CONSTRAINT FK_ServiceOrders_Receipts
        FOREIGN KEY (ReceiptId)   REFERENCES ServiceReceipts(ReceiptId),
    CONSTRAINT FK_ServiceOrders_Employees
        FOREIGN KEY (MechanicId)  REFERENCES Employees(EmployeeId)
);
GO

CREATE TABLE ServiceOrderItems (
    ItemId      INT IDENTITY PRIMARY KEY,
    OrderId     INT,
    ServiceId   INT,
    Price       DECIMAL(12, 2),

    CONSTRAINT FK_SOItems_Orders
        FOREIGN KEY (OrderId)    REFERENCES ServiceOrders(OrderId),
    CONSTRAINT FK_SOItems_Services
        FOREIGN KEY (ServiceId)  REFERENCES Services(ServiceId)
);
GO

CREATE TABLE OrderParts (
    Id        INT IDENTITY PRIMARY KEY,
    OrderId   INT,
    PartId    INT,
    Quantity  INT,
    Price     DECIMAL(12, 2),

    CONSTRAINT FK_OrderParts_Orders
        FOREIGN KEY (OrderId)  REFERENCES ServiceOrders(OrderId),
    CONSTRAINT FK_OrderParts_Parts
        FOREIGN KEY (PartId)   REFERENCES Parts(PartId)
);
GO

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

CREATE TABLE InvoiceHistory (
    HistoryId      INT IDENTITY PRIMARY KEY,
    OrderId        INT           NOT NULL,
    ChangedBy      INT,
    PreviousStatus NVARCHAR(50),
    NewStatus      NVARCHAR(50),
    TotalPrice     DECIMAL(12, 2),
    Note           NVARCHAR(500),
    ChangedAt      DATETIME      DEFAULT GETDATE(),

    CONSTRAINT FK_InvoiceHistory_Orders
        FOREIGN KEY (OrderId)   REFERENCES ServiceOrders(OrderId),
    CONSTRAINT FK_InvoiceHistory_Employees
        FOREIGN KEY (ChangedBy) REFERENCES Employees(EmployeeId)
);
GO

CREATE TABLE WorkSchedules (
    ScheduleId   INT IDENTITY PRIMARY KEY,
    ShopId       INT,
    EmployeeId   INT,
    WorkDate     DATE,
    Shift        NVARCHAR(20),
    CheckIn      DATETIME,
    CheckOut     DATETIME,

    CONSTRAINT FK_WorkSchedules_Shop
        FOREIGN KEY (ShopId)      REFERENCES Shops(ShopId),
    CONSTRAINT FK_WorkSchedules_Employees
        FOREIGN KEY (EmployeeId)  REFERENCES Employees(EmployeeId)
);
GO

CREATE TABLE LeaveRequests (
    RequestId    INT IDENTITY PRIMARY KEY,
    EmployeeId   INT,
    LeaveDate    DATE,
    Reason       NVARCHAR(255),
    Status       NVARCHAR(50)  DEFAULT 'pending',
    ApprovedBy   INT,

    CONSTRAINT FK_LeaveRequests_Employees
        FOREIGN KEY (EmployeeId)  REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_LeaveRequests_Manager
        FOREIGN KEY (ApprovedBy)  REFERENCES Employees(EmployeeId)
);
GO

CREATE TABLE SystemBankAccounts (
    AccountId     INT IDENTITY PRIMARY KEY,
    AccountNumber VARCHAR(225)   NOT NULL,
    AccountName   NVARCHAR(225)  NOT NULL,
    BankName      NVARCHAR(255)  NOT NULL,
    QrImageCode   NVARCHAR(500),           -- Mã ảnh QR hoặc đường dẫn ảnh
    IsActive      BIT            DEFAULT 1, -- 1: Đang dùng, 0: Đã ngưng sử dụng
    CreatedAt     DATETIME       DEFAULT GETDATE()
);
GO

CREATE TABLE ShopBankAccounts (
    AccountId     INT IDENTITY PRIMARY KEY,
    ShopId        INT            NOT NULL,  -- FK liên kết với bảng Shops
    AccountNumber VARCHAR(225)   NOT NULL,
    AccountName   NVARCHAR(225)  NOT NULL,
    BankName      NVARCHAR(255)  NOT NULL,
    QrImageCode   NVARCHAR(500),           -- Mã ảnh QR hoặc đường dẫn ảnh của shop
    IsActive      BIT            DEFAULT 1,
    CreatedAt     DATETIME       DEFAULT GETDATE(),

    CONSTRAINT FK_ShopBankAccounts_Shop
        FOREIGN KEY (ShopId) REFERENCES Shops(ShopId)
);
GO

CREATE INDEX IX_ShopBankAccounts_Shop ON ShopBankAccounts (ShopId);
GO


/* ============================================================
   VIEWS
   ============================================================ */

CREATE VIEW PendingAssignmentQueue AS
SELECT
    sr.ReceiptId,
    sr.ShopId,
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

CREATE VIEW MechanicWorkload AS
SELECT
    emp.EmployeeId,
    emp.Name                         AS MechanicName,
    COUNT(so.OrderId)                AS ActiveJobCount
FROM Employees       emp
JOIN UserRoles       ur  ON ur.EmployeeId = emp.EmployeeId
JOIN Roles           r   ON r.RoleId      = ur.RoleId
                        AND r.RoleName    = 'mechanic'
LEFT JOIN ServiceOrders so
       ON so.MechanicId = emp.EmployeeId
      AND so.Status     = 'processing'
WHERE emp.Status = 'active'
GROUP BY emp.EmployeeId, emp.Name;
GO


/* ============================================================
   STORED PROCEDURES
   ============================================================ */

/* Phân công thợ — cập nhật guard check role qua UserRoles     */
CREATE PROCEDURE AssignMechanicToReceipt
    @ReceiptId   INT,
    @MechanicId  INT,
    @ManagerId   INT,
    @Note        NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM ServiceReceipts
        WHERE ReceiptId = @ReceiptId AND Status = 'pending'
    )
    BEGIN
        RAISERROR('Receipt is not in pending status and cannot be assigned.', 16, 1);
        RETURN;
    END

    -- Kiểm tra qua UserRoles thay vì cột Role cũ
    IF NOT EXISTS (
        SELECT 1
        FROM Employees  e
        JOIN UserRoles  ur ON ur.EmployeeId = e.EmployeeId
        JOIN Roles      r  ON r.RoleId      = ur.RoleId
        WHERE e.EmployeeId = @MechanicId
          AND r.RoleName   = 'mechanic'
          AND e.Status     = 'active'
    )
    BEGIN
        RAISERROR('The selected employee is not an active mechanic.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO MechanicAssignments (ReceiptId, MechanicId, AssignedBy, Note)
        VALUES (@ReceiptId, @MechanicId, @ManagerId, @Note);

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


/* Sinh và lưu activation key cho một shop
   Gọi sau khi ShopSubscription được tạo thành công.

   Usage:
     EXEC IssueActivationKey
         @ShopId         = 1,
         @SubscriptionId = 1,
         @IssuedBy       = 1,     -- admin EmployeeId
         @DurationDays   = 365;
   ============================================================ */
CREATE PROCEDURE IssueActivationKey
    @ShopId         INT,
    @SubscriptionId INT,
    @IssuedBy       INT,
    @DurationDays   INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Thu hồi key cũ còn active (nếu có)
    UPDATE ActivationKeys
    SET    Status = 'revoked'
    WHERE  ShopId = @ShopId
      AND  Status = 'active';

    DECLARE @NewKey   NVARCHAR(64) = UPPER(
        SUBSTRING(REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', ''), 1,  8) + '-' +
        SUBSTRING(REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', ''), 1,  8) + '-' +
        SUBSTRING(REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', ''), 1,  8) + '-' +
        SUBSTRING(REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', ''), 1,  8)
    );
    DECLARE @ExpiresAt DATETIME = DATEADD(DAY, @DurationDays, GETDATE());

    INSERT INTO ActivationKeys
        (ShopId, LicenseKey, SubscriptionId, ExpiresAt, Status, IssuedBy)
    VALUES
        (@ShopId, @NewKey, @SubscriptionId, @ExpiresAt, 'pending', @IssuedBy);

    -- Trả về key vừa tạo cho caller
    SELECT @NewKey AS LicenseKey, @ExpiresAt AS ExpiresAt;
END;
GO


/* Ghi log hệ thống — wrapper tiện dụng cho application layer
   Usage:
     EXEC WriteSystemLog
         @ActorId    = 1,
         @Action     = 'ISSUE_KEY',
         @EntityType = 'ActivationKey',
         @EntityId   = 3,
         @Detail     = N'Key issued for shop PMP Q3',
         @IpAddress  = '192.168.1.10';
   ============================================================ */
CREATE PROCEDURE WriteSystemLog
    @ActorId    INT           = NULL,
    @Action     NVARCHAR(100),
    @EntityType NVARCHAR(50)  = NULL,
    @EntityId   INT           = NULL,
    @Detail     NVARCHAR(MAX) = NULL,
    @IpAddress  NVARCHAR(45)  = NULL,
    @UserAgent  NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO SystemActivityLogs
        (ActorId, Action, EntityType, EntityId, Detail, IpAddress, UserAgent)
    VALUES
        (@ActorId, @Action, @EntityType, @EntityId, @Detail, @IpAddress, @UserAgent);
END;
GO


/* Ghi log cửa hàng
   Usage:
     EXEC WriteShopLog
         @ShopId     = 1,
         @ActorId    = 3,
         @Action     = 'CREATE_RECEIPT',
         @EntityType = 'ServiceReceipt',
         @EntityId   = 11,
         @Detail     = N'Phiếu tiếp nhận xe Honda Wave BKS 29B1-12345';
   ============================================================ */
CREATE PROCEDURE WriteShopLog
    @ShopId     INT,
    @ActorId    INT           = NULL,
    @Action     NVARCHAR(100),
    @EntityType NVARCHAR(50)  = NULL,
    @EntityId   INT           = NULL,
    @Detail     NVARCHAR(MAX) = NULL,
    @IpAddress  NVARCHAR(45)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ShopActivityLogs
        (ShopId, ActorId, Action, EntityType, EntityId, Detail, IpAddress)
    VALUES
        (@ShopId, @ActorId, @Action, @EntityType, @EntityId, @Detail, @IpAddress);
END;
GO