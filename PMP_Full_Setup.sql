/* ============================================================
   PMP RACING — FULL DATABASE SETUP SCRIPT
   Mục tiêu: Chạy 1 lần duy nhất để thiết lập toàn bộ môi trường
   Tính năng:
   1. Reset Database: Xóa và tạo mới Database PmpRacing
   2. Schema Setup: Tạo toàn bộ bảng, view, procedure
   3. Sample Data: Nạp ít nhất 12 dòng mỗi bảng, 31 ngày doanh thu
   ============================================================ */

USE master;
GO

-- 1. XÓA VÀ TẠO MỚI DATABASE
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

/* ============================================================
   PHẦN 1: SCHEMA (CẤU TRÚC BẢNG)
   ============================================================ */

CREATE TABLE Employees (
    EmployeeId   INT IDENTITY PRIMARY KEY,
    Name         NVARCHAR(100)  NOT NULL,
    Email        NVARCHAR(100),
    Phone        NVARCHAR(20),
    Role         NVARCHAR(50),               -- mechanic | cashier | manager | admin
    Password     NVARCHAR(255),
    ProfileImagePath NVARCHAR(500),
    Status       NVARCHAR(20)   DEFAULT 'active',
    CreatedAt    DATETIME       DEFAULT GETDATE()
);

CREATE TABLE Customers (
    CustomerId   INT IDENTITY PRIMARY KEY,
    Name         NVARCHAR(100),
    Phone        NVARCHAR(20),
    Email        NVARCHAR(100),
    CreatedAt    DATETIME  DEFAULT GETDATE()
);

CREATE TABLE Bikes (
    BikeId        INT IDENTITY PRIMARY KEY,
    CustomerId    INT,
    LicensePlate  NVARCHAR(20),
    BikeModel     NVARCHAR(100),
    CONSTRAINT FK_Bikes_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);

CREATE TABLE Services (
    ServiceId    INT IDENTITY PRIMARY KEY,
    ServiceName  NVARCHAR(100),
    Price        DECIMAL(12, 2)
);

CREATE TABLE Parts (
    PartId        INT IDENTITY PRIMARY KEY,
    PartName      NVARCHAR(100),
    Price         DECIMAL(12, 2),
    Stock         INT,
    WarningLevel  INT
);

CREATE TABLE ServiceReceipts (
    ReceiptId    INT IDENTITY PRIMARY KEY,
    BikeId       INT,
    CreatedBy    INT,
    Status       NVARCHAR(50) DEFAULT 'pending', -- pending | assigned | in_progress | completed | cancelled
    CreatedAt    DATETIME     DEFAULT GETDATE(),
    CONSTRAINT FK_ServiceReceipts_Bikes FOREIGN KEY (BikeId) REFERENCES Bikes(BikeId),
    CONSTRAINT FK_ServiceReceipts_Employees FOREIGN KEY (CreatedBy) REFERENCES Employees(EmployeeId)
);

CREATE TABLE MechanicAssignments (
    AssignmentId  INT IDENTITY PRIMARY KEY,
    ReceiptId     INT       NOT NULL,
    MechanicId    INT       NOT NULL,
    AssignedBy    INT       NOT NULL,
    AssignedAt    DATETIME  DEFAULT GETDATE(),
    Note          NVARCHAR(255),
    CONSTRAINT FK_MechanicAssignments_Receipts FOREIGN KEY (ReceiptId) REFERENCES ServiceReceipts(ReceiptId),
    CONSTRAINT FK_MechanicAssignments_Mechanic FOREIGN KEY (MechanicId) REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_MechanicAssignments_Manager FOREIGN KEY (AssignedBy) REFERENCES Employees(EmployeeId)
);

CREATE TABLE ServiceOrders (
    OrderId      INT IDENTITY PRIMARY KEY,
    ReceiptId    INT,
    MechanicId   INT,
    Status       NVARCHAR(50) DEFAULT 'processing', -- processing | completed | paid | payment_failed
    TotalPrice   DECIMAL(12, 2),
    StartedAt    DATETIME,
    CompletedAt  DATETIME,
    CreatedAt    DATETIME     DEFAULT GETDATE(),
    CONSTRAINT FK_ServiceOrders_Receipts FOREIGN KEY (ReceiptId) REFERENCES ServiceReceipts(ReceiptId),
    CONSTRAINT FK_ServiceOrders_Employees FOREIGN KEY (MechanicId) REFERENCES Employees(EmployeeId)
);

CREATE TABLE ServiceOrderItems (
    ItemId      INT IDENTITY PRIMARY KEY,
    OrderId     INT,
    ServiceId   INT,
    Price       DECIMAL(12, 2),
    CONSTRAINT FK_SOItems_Orders FOREIGN KEY (OrderId) REFERENCES ServiceOrders(OrderId),
    CONSTRAINT FK_SOItems_Services FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId)
);

CREATE TABLE OrderParts (
    Id        INT IDENTITY PRIMARY KEY,
    OrderId   INT,
    PartId    INT,
    Quantity  INT,
    Price     DECIMAL(12, 2),
    CONSTRAINT FK_OrderParts_Orders FOREIGN KEY (OrderId) REFERENCES ServiceOrders(OrderId),
    CONSTRAINT FK_OrderParts_Parts FOREIGN KEY (PartId) REFERENCES Parts(PartId)
);

CREATE TABLE Payments (
    PaymentId   INT IDENTITY PRIMARY KEY,
    OrderId     INT,
    Method      NVARCHAR(50),
    Amount      DECIMAL(12, 2),
    Status      NVARCHAR(50),
    PaidAt      DATETIME,
    CONSTRAINT FK_Payments_Orders FOREIGN KEY (OrderId) REFERENCES ServiceOrders(OrderId)
);

CREATE TABLE WorkSchedules (
    ScheduleId   INT IDENTITY PRIMARY KEY,
    EmployeeId   INT,
    WorkDate     DATE,
    Shift        NVARCHAR(20),
    CheckIn      DATETIME,
    CheckOut     DATETIME,
    CONSTRAINT FK_WorkSchedules_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId)
);

CREATE TABLE LeaveRequests (
    RequestId    INT IDENTITY PRIMARY KEY,
    EmployeeId   INT,
    LeaveDate    DATE,
    Reason       NVARCHAR(255),
    Status       NVARCHAR(50)  DEFAULT 'pending',
    ApprovedBy   INT,
    CONSTRAINT FK_LeaveRequests_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_LeaveRequests_Manager FOREIGN KEY (ApprovedBy) REFERENCES Employees(EmployeeId)
);
GO

-- VIEWS & PROCS
CREATE VIEW PendingAssignmentQueue AS
SELECT sr.ReceiptId, sr.CreatedAt AS ReceivedAt, c.Name AS CustomerName, b.LicensePlate, b.BikeModel, e.Name AS CreatedByCashier
FROM ServiceReceipts sr JOIN Bikes b ON b.BikeId = sr.BikeId JOIN Customers c ON c.CustomerId = b.CustomerId JOIN Employees e ON e.EmployeeId = sr.CreatedBy
WHERE sr.Status = 'pending';
GO

CREATE VIEW MechanicWorkload AS
SELECT emp.EmployeeId, emp.Name AS MechanicName, COUNT(so.OrderId) AS ActiveJobCount
FROM Employees emp LEFT JOIN ServiceOrders so ON so.MechanicId = emp.EmployeeId AND so.Status = 'processing'
WHERE emp.Role = 'mechanic' AND emp.Status = 'active'
GROUP BY emp.EmployeeId, emp.Name;
GO

/* ============================================================
   PHẦN 2: SAMPLE DATA (DỮ LIỆU MẪU)
   ============================================================ */

-- 1. Employees (15 users)
INSERT INTO Employees (Name, Email, Phone, Role, Password, ProfileImagePath, Status) VALUES
(N'Nguyễn Duy Admin', 'admin@pmpracing.vn', '0901000001', 'admin', '123', '~/img/avatars/admin.png', 'active'),
(N'Lê Nam Giao', 'manager@pmpracing.vn', '0901000002', 'manager', '123', '~/img/avatars/manager.png', 'active'),
(N'Thu Ngân 1', 'cashier1@pmpracing.vn', '0901000003', 'cashier', '123', '~/img/avatars/cashier1.png', 'active'),
(N'Thu Ngân 2', 'cashier2@pmpracing.vn', '0901000004', 'cashier', '123', '~/img/avatars/cashier2.png', 'active'),
(N'Phạm Minh Phước', 'phuoc@pmpracing.vn', '0901000005', 'mechanic', '123', '~/img/avatars/mechanic1.png', 'active'),
(N'Nguyễn Văn Tuấn', 'tuan@pmpracing.vn', '0901000006', 'mechanic', '123', '~/img/avatars/mechanic2.png', 'active'),
(N'Trần Việt Hưng', 'hung@pmpracing.vn', '0901000007', 'mechanic', '123', '~/img/avatars/mechanic3.png', 'active'),
(N'Lê Văn Khoa', 'khoa@pmpracing.vn', '0901000008', 'mechanic', '123', '~/img/avatars/mechanic4.png', 'active'),
(N'Đặng Văn Bình', 'binh@pmpracing.vn', '0901000009', 'mechanic', '123', '~/img/avatars/mechanic5.png', 'active'),
(N'Hoàng Văn Bách', 'bach@pmpracing.vn', '0901000010', 'mechanic', '123', '~/img/avatars/mechanic6.png', 'active'),
(N'Vũ Quang Hải', 'hai@pmpracing.vn', '0901000011', 'mechanic', '123', '~/img/avatars/mechanic1.png', 'active'),
(N'Trịnh Văn Sĩ', 'si@pmpracing.vn', '0901000012', 'mechanic', '123', '~/img/avatars/mechanic2.png', 'active'),
(N'Phan Thanh Long', 'long@pmpracing.vn', '0901000013', 'mechanic', '123', '~/img/avatars/mechanic3.png', 'active'),
(N'Lý Hồng Phúc', 'phuc@pmpracing.vn', '0901000014', 'mechanic', '123', '~/img/avatars/mechanic4.png', 'active'),
(N'Mai Anh Tuấn', 'tuan_mai@pmpracing.vn', '0901000015', 'mechanic', '123', '~/img/avatars/mechanic5.png', 'inactive');

-- 2. Customers (15 users)
INSERT INTO Customers (Name, Phone, Email) VALUES
(N'Nguyễn Thành Long', '0912345001', 'long@gmail.com'), (N'Trần Minh Phúc', '0912345002', 'phuc@gmail.com'),
(N'Lê Thị Hoa', '0912345003', 'hoa@gmail.com'), (N'Phạm Đức Thắng', '0912345004', 'thang@gmail.com'),
(N'Bùi Thị Lan', '0912345005', 'lan@gmail.com'), (N'Đỗ Văn Bình', '0912345006', 'binh@gmail.com'),
(N'Hoàng Thị Mai', '0912345007', 'mai@gmail.com'), (N'Vũ Quang Hải', '0912345008', 'hai@gmail.com'),
(N'Lý Mạc Sầu', '0912345009', 'sau@gmail.com'), (N'Quách Tĩnh', '0912345010', 'tinh@gmail.com'),
(N'Hoàng Dung', '0912345011', 'dung@gmail.com'), (N'Dương Quá', '0912345012', 'qua@gmail.com'),
(N'Tiểu Long Nữ', '0912345013', 'nu@gmail.com'), (N'Trương Vô Kỵ', '0912345014', 'ky@gmail.com'),
(N'Triệu Mẫn', '0912345015', 'man@gmail.com');

-- 3. Bikes (15 bikes)
INSERT INTO Bikes (CustomerId, LicensePlate, BikeModel) VALUES
(1, '29B1-11111', 'Honda Wave'), (1, '29B1-22222', 'Honda SH'), (2, '51G1-33333', 'Yamaha Exciter'),
(3, '43C1-44444', 'Honda Vision'), (4, '29A1-55555', 'Yamaha Sirius'), (5, '51H1-66666', 'Honda Lead'),
(6, '30E1-77777', 'Suzuki Raider'), (7, '92B1-88888', 'Honda Air Blade'), (8, '51F1-99999', 'Yamaha NVX'),
(9, '29B1-00001', 'Honda Winner'), (10, '29B1-00002', 'Vespa Sprint'), (11, '29B1-00003', 'Honda Future'),
(12, '29B1-00004', 'Yamaha Janus'), (13, '29B1-00005', 'Honda Cub'), (14, '29B1-00006', 'Suzuki Satria');

-- 4. Services (15 items)
INSERT INTO Services (ServiceName, Price) VALUES
(N'Thay nhớt', 80000), (N'Vệ sinh nồi', 150000), (N'Sửa phanh', 120000), (N'Thay bugi', 40000),
(N'Vệ sinh xích', 30000), (N'Rửa xe', 30000), (N'Thay lọc gió', 50000), (N'Cân vành', 100000),
(N'Sửa điện', 200000), (N'Đại tu máy', 1500000), (N'Thay lốp', 50000), (N'Chỉnh đèn', 20000),
(N'Bảo dưỡng tổng thể', 500000), (N'Vệ sinh kim phun', 120000), (N'Thay nước mát', 60000);

-- 5. Parts (15 items)
INSERT INTO Parts (PartName, Price, Stock, WarningLevel) VALUES
(N'Nhớt Castrol', 100000, 50, 10), (N'Bugi NGK', 45000, 100, 20), (N'Lốp Michelin', 800000, 10, 2),
(N'Má phanh Honda', 150000, 30, 5), (N'Dây curoa', 350000, 15, 3), (N'Bình điện GS', 450000, 20, 4),
(N'Lọc gió', 60000, 40, 10), (N'Nhông sên dĩa', 400000, 12, 3), (N'Nước làm mát', 70000, 25, 5),
(N'Bóng đèn Phillips', 120000, 30, 5), (N'Củ đề', 600000, 5, 1), (N'IC mở tua', 1200000, 5, 1),
(N'Gương chiếu hậu', 80000, 40, 10), (N'Dầu phanh DOT4', 90000, 20, 5), (N'Vòi bơm không săm', 30000, 100, 20);

-- 6. Dữ liệu Doanh thu & Công việc cho 31 ngày gần nhất
DECLARE @date DATETIME = DATEADD(DAY, -30, GETDATE());
DECLARE @i INT = 1;
WHILE @i <= 60 -- Tạo 60 đơn hàng trong 31 ngày
BEGIN
    DECLARE @bikeId INT = (@i % 15) + 1;
    DECLARE @cashierId INT = (@i % 2) + 3;
    DECLARE @mechId INT = (@i % 10) + 5;
    DECLARE @total DECIMAL(12,2) = 200000 + (RAND() * 800000);
    
    -- Tạo Receipt
    INSERT INTO ServiceReceipts (BikeId, CreatedBy, Status, CreatedAt)
    VALUES (@bikeId, @cashierId, 'completed', @date);
    DECLARE @rid INT = SCOPE_IDENTITY();

    -- Tạo Assignment
    INSERT INTO MechanicAssignments (ReceiptId, MechanicId, AssignedBy, AssignedAt)
    VALUES (@rid, @mechId, 2, @date);

    -- Tạo Order
    INSERT INTO ServiceOrders (ReceiptId, MechanicId, Status, TotalPrice, StartedAt, CompletedAt, CreatedAt)
    VALUES (@rid, @mechId, 'paid', @total, @date, DATEADD(HOUR, 2, @date), @date);
    DECLARE @oid INT = SCOPE_IDENTITY();

    -- Thêm Service Item
    INSERT INTO ServiceOrderItems (OrderId, ServiceId, Price)
    VALUES (@oid, (@i % 15)+1, 100000);

    -- Tạo Payment
    INSERT INTO Payments (OrderId, Method, Amount, Status, PaidAt)
    VALUES (@oid, CASE WHEN @i % 3 = 0 THEN 'qr' ELSE 'cash' END, @total, 'paid', @date);

    SET @date = DATEADD(HOUR, 12, @date); -- Mỗi ngày khoảng 2 đơn
    SET @i = @i + 1;
END

-- 7. Work Schedules & Leave Requests (12+ rows)
DECLARE @schedDate DATE = DATEADD(DAY, -7, GETDATE());
DECLARE @j INT = 1;
WHILE @j <= 30
BEGIN
    INSERT INTO WorkSchedules (EmployeeId, WorkDate, Shift, CheckIn)
    VALUES ((@j % 10)+5, @schedDate, CASE WHEN @j % 2 = 0 THEN 'morning' ELSE 'afternoon' END, CAST(@schedDate AS DATETIME));
    
    IF @j <= 15
    BEGIN
        INSERT INTO LeaveRequests (EmployeeId, LeaveDate, Reason, Status, ApprovedBy)
        VALUES ((@j % 10)+5, DATEADD(DAY, @j, GETDATE()), N'Nghỉ phép cá nhân', 'pending', NULL);
    END

    SET @schedDate = DATEADD(DAY, 1, @schedDate);
    SET @j = @j + 1;
END
GO

PRINT 'Database PmpRacing setup completed successfully with extensive sample data.';
GO
