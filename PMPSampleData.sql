/* ============================================================
   PMP RACING — Sample Data
   Run this AFTER pmp_racing_schema.sql
   All monetary values are in VND.
   ============================================================ */

USE PmpRacing;
GO


/* ============================================================
   Employees
   1 admin, 1 manager, 2 cashiers, 4 mechanics
   Passwords are bcrypt placeholders (never store plain text)
   ============================================================ */
INSERT INTO Employees (Name, Email, Phone, Role, Password, ProfileImagePath, Status) VALUES
-- Admin
(N'Nguyễn Phạm Duy',   'admin@pmpracing.vn',    '0901000001', 'admin',    '123', N'~/img/avatars/admin.png', 'active'),
-- Manager
(N'Nguyễn Hùng Nam Giao',   'manager@pmpracing.vn',  '0901000002', 'manager',  '123', N'~/img/avatars/manager.png', 'active'),
-- Cashiers
(N'Nguyễn Hoàng Việt',    'cashier1@pmpracing.vn', '0901000003', 'cashier',  '123', N'~/img/avatars/cashier1.png', 'active'),
(N'Nguyễn Văn Sĩ',  'cashier2@pmpracing.vn', '0901000004', 'cashier',  '123', N'~/img/avatars/cashier2.png', 'active'),
-- Mechanics
(N'Phạm Minh Phước',     'phuoc@pmpracing.vn',     '0901000005', 'mechanic', '123', N'~/img/avatars/mechanic1.png', 'active'),
(N'Việt Hưng',      'hung@pmpracing.vn',     '0901000006', 'mechanic', '123', N'~/img/avatars/mechanic2.png', 'active'),
(N'Văn',     'van@pmpracing.vn',     '0901000007', 'mechanic', '123', N'~/img/avatars/mechanic3.png', 'active'),
(N'Bách',        'bach@pmpracing.vn',      '0901000008', 'mechanic', '123', N'~/img/avatars/mechanic4.png', 'inactive'); -- on leave
GO

/* ============================================================
   Customers  (8 customers)
   ============================================================ */
INSERT INTO Customers (Name, Phone, Email) VALUES
(N'Nguyễn Thành Long',  '0912345601', 'long.nguyen@gmail.com'),
(N'Trần Minh Phúc',     '0912345602', 'phuc.tran@gmail.com'),
(N'Lê Thị Hoa',         '0912345603', 'hoa.le@gmail.com'),
(N'Phạm Đức Thắng',     '0912345604', 'thang.pham@gmail.com'),
(N'Bùi Thị Lan',        '0912345605', 'lan.bui@gmail.com'),
(N'Đỗ Văn Bình',        '0912345606', 'binh.do@gmail.com'),
(N'Hoàng Thị Mai',      '0912345607', 'mai.hoang@gmail.com'),
(N'Vũ Quang Hải',       '0912345608', 'hai.vu@gmail.com');
GO

/* ============================================================
   Bikes  (10 bikes across 8 customers)
   ============================================================ */
INSERT INTO Bikes (CustomerId, LicensePlate, BikeModel) VALUES
(1, '29B1-12345', N'Honda Wave Alpha'),
(1, '29B1-99999', N'Honda SH 150i'),        -- customer 1 owns 2 bikes
(2, '51G1-23456', N'Yamaha Exciter 155'),
(3, '43C1-34567', N'Honda Vision'),
(4, '29A1-45678', N'Yamaha Sirius'),
(5, '51H1-56789', N'Honda Lead 125'),
(6, '30E1-67890', N'Suzuki Raider R150'),
(7, '92B1-78901', N'Honda Air Blade 125'),
(8, '51F1-89012', N'Yamaha NVX 155'),
(3, '43C1-11111', N'Honda Winner X');       -- customer 3 owns 2 bikes
GO

/* ============================================================
   Services  (price list)
   ============================================================ */
INSERT INTO Services (ServiceName, Price) VALUES
(N'Thay nhớt động cơ',          80000),
(N'Vệ sinh bộ chế hoà khí',     60000),
(N'Thay lốc máy',              350000),
(N'Sửa hệ thống phanh',        120000),
(N'Thay dây côn',               50000),
(N'Kiểm tra & nạp bình điện',   90000),
(N'Thay bugi',                  40000),
(N'Vệ sinh & tra dầu xích',     30000),
(N'Chỉnh đèn xe',               25000),
(N'Đại tu động cơ',            800000),
(N'Thay lốc xe trước',         150000),
(N'Thay lốc xe sau',           150000),
(N'Rửa xe',                     25000),
(N'Thay lọc gió',               45000);
GO

/* ============================================================
   Parts  (spare parts inventory)
   ============================================================ */
INSERT INTO Parts (PartName, Price, Stock, WarningLevel) VALUES
(N'Nhớt Castrol Power1 0.8L',       85000,  50, 10),
(N'Nhớt Motul 5100 1L',            145000,  30,  8),
(N'Bugi NGK CR7HSA',                35000,  80, 15),
(N'Bugi Denso U22ESR-N',            40000,  60, 15),
(N'Dây côn Honda Wave',             48000,  25,  5),
(N'Dây côn Yamaha Exciter',         52000,  20,  5),
(N'Bình điện GS 5Ah',              320000,  10,  3),
(N'Lốc xe trước Honda Vision',     140000,   8,  2),
(N'Lốc xe sau Honda Vision',       140000,   8,  2),
(N'Lọc gió Honda Wave',             40000,  35, 10),
(N'Lọc gió Yamaha Exciter',         45000,  28, 10),
(N'Bố thắng trước Honda Wave',      55000,  40, 10),
(N'Bố thắng sau Honda Wave',        50000,  40, 10),
(N'Xích tải Honda Wave',            95000,  15,  4),
(N'Má phanh đĩa Yamaha Exciter',    75000,  20,  5);
GO

/* ============================================================
   ServiceReceipts
   Mix of statuses: pending / assigned / in_progress / completed
   CreatedBy references cashier employees (EmployeeId 3 & 4)
   ============================================================ */
INSERT INTO ServiceReceipts (BikeId, CreatedBy, Status, CreatedAt) VALUES
-- Completed visits (older)
(1,  3, 'completed',   '2025-03-01 08:30:00'),  -- ReceiptId 1
(3,  3, 'completed',   '2025-03-05 09:15:00'),  -- ReceiptId 2
(5,  4, 'completed',   '2025-03-10 10:00:00'),  -- ReceiptId 3
(7,  4, 'completed',   '2025-03-15 11:20:00'),  -- ReceiptId 4
(9,  3, 'completed',   '2025-03-20 14:00:00'),  -- ReceiptId 5
-- Recent in-progress
(2,  3, 'assigned',    '2025-04-01 08:00:00'),  -- ReceiptId 6  (assigned, not started yet)
(4,  4, 'in_progress', '2025-04-01 09:30:00'),  -- ReceiptId 7
(6,  3, 'in_progress', '2025-04-01 10:45:00'),  -- ReceiptId 8
-- Brand new, waiting for mechanic
(8,  4, 'pending',     '2025-04-01 13:00:00'),  -- ReceiptId 9
(10, 3, 'pending',     '2025-04-01 13:30:00');  -- ReceiptId 10
GO

/* ============================================================
   MechanicAssignments  (UC-06)
   Manager (EmployeeId 2) assigns mechanics to receipts.
   Mechanics: Tuấn=5, Khoa=6, Hùng=7
   ============================================================ */
INSERT INTO MechanicAssignments (ReceiptId, MechanicId, AssignedBy, AssignedAt, Note) VALUES
-- Historical completed jobs
(1, 5, 2, '2025-03-01 08:35:00', N'Routine oil change, assign to Tuấn'),
(2, 6, 2, '2025-03-05 09:20:00', N'Exciter brake system, Khoa handles sportbikes'),
(3, 7, 2, '2025-03-10 10:05:00', N'Lead tyre swap, assign to Hùng'),
(4, 5, 2, '2025-03-15 11:25:00', N'Air Blade carb clean, Tuấn specialises in Honda'),
(5, 6, 2, '2025-03-20 14:05:00', N'NVX electrical check'),
-- Active assignments
(6, 7, 2, '2025-04-01 08:10:00', N'SH 150i full service, Hùng available'),
(7, 5, 2, '2025-04-01 09:35:00', N'Vision tyre – Tuấn'),
(8, 6, 2, '2025-04-01 10:50:00', N'Raider engine, Khoa knows Suzuki');
GO

/* ============================================================
   ServiceOrders
   Linked to receipts.  StartedAt / CompletedAt support UC-06 productivity tracking.
   ============================================================ */
INSERT INTO ServiceOrders (ReceiptId, MechanicId, Status, TotalPrice, StartedAt, CompletedAt, CreatedAt) VALUES
-- Completed orders
(1, 5, 'completed',  165000, '2025-03-01 08:40:00', '2025-03-01 10:00:00', '2025-03-01 08:40:00'),  -- OrderId 1
(2, 6, 'completed',  270000, '2025-03-05 09:25:00', '2025-03-05 11:30:00', '2025-03-05 09:25:00'),  -- OrderId 2
(3, 7, 'completed',  330000, '2025-03-10 10:10:00', '2025-03-10 12:00:00', '2025-03-10 10:10:00'),  -- OrderId 3
(4, 5, 'completed',  145000, '2025-03-15 11:30:00', '2025-03-15 13:00:00', '2025-03-15 11:30:00'),  -- OrderId 4
(5, 6, 'completed',  215000, '2025-03-20 14:10:00', '2025-03-20 15:45:00', '2025-03-20 14:10:00'),  -- OrderId 5
-- Active orders (in progress)
(7, 5, 'processing', 290000, '2025-04-01 09:40:00', NULL,                  '2025-04-01 09:40:00'),  -- OrderId 6
(8, 6, 'processing', 860000, '2025-04-01 10:55:00', NULL,                  '2025-04-01 10:55:00');  -- OrderId 7
GO

/* ============================================================
   ServiceOrderItems  (services per order, price snapshotted)
   ============================================================ */
INSERT INTO ServiceOrderItems (OrderId, ServiceId, Price) VALUES
-- Order 1: Wave Alpha – oil change + spark plug
(1, 1,  80000),   -- oil change
(1, 7,  40000),   -- spark plug
(1, 13, 25000),   -- wash
-- Order 2: Exciter – brakes + chain
(2, 4, 120000),   -- brake repair
(2, 8,  30000),   -- chain lube
(2, 7,  40000),   -- spark plug
-- Order 3: Lead 125 – front tyre + oil
(3, 11, 150000),  -- front tyre
(3, 1,   80000),  -- oil change
(3, 13,  25000),  -- wash
-- Order 4: Air Blade – carb clean + air filter
(4, 2,  60000),   -- carb clean
(4, 14, 45000),   -- air filter
-- Order 5: NVX – battery + spark plug
(5, 6,  90000),   -- battery check & charge
(5, 7,  40000),   -- spark plug
(5, 13, 25000),   -- wash
-- Order 6 (active): Vision – rear tyre + oil
(6, 12, 150000),  -- rear tyre
(6, 1,   80000),  -- oil change
-- Order 7 (active): Raider – engine overhaul + oil
(7, 10, 800000),  -- engine overhaul
(7, 1,   80000);  -- oil change
GO

/* ============================================================
   OrderParts  (parts used per order, unit price snapshotted)
   ============================================================ */
INSERT INTO OrderParts (OrderId, PartId, Quantity, Price) VALUES
-- Order 1: Wave Alpha
(1, 1,  1,  85000),   -- Castrol 0.8L
(1, 3,  1,  35000),   -- NGK bugi
-- Order 2: Exciter brakes
(2, 15, 1,  75000),   -- disc brake pads Exciter
(2, 14, 1,  95000),   -- chain
-- Order 3: Lead 125 tyre
(3, 8,  1, 140000),   -- front tyre Lead (Vision part reused)
(3, 1,  1,  85000),   -- oil
-- Order 4: Air Blade carb
(4, 10, 1,  40000),   -- air filter Wave (compatible)
-- Order 5: NVX battery
(5, 7,  1, 320000),   -- battery 5Ah
(5, 4,  1,  40000),   -- Denso spark plug
-- Order 6 (active): Vision tyre
(6, 9,  1, 140000),   -- rear tyre Vision
(6, 2,  1, 145000),   -- Motul 1L oil
-- Order 7 (active): Raider engine overhaul – multiple parts
(7, 2,  2, 145000),   -- 2× Motul oil
(7, 4,  2,  40000);   -- 2× spark plug
GO

/* ============================================================
   Payments  (UC-05)
   ============================================================ */
INSERT INTO Payments (OrderId, Method, Amount, Status, PaidAt) VALUES
(1, 'cash',     165000, 'paid', '2025-03-01 10:05:00'),
(2, 'qr',       270000, 'paid', '2025-03-05 11:35:00'),
(3, 'transfer', 330000, 'paid', '2025-03-10 12:10:00'),
(4, 'cash',     145000, 'paid', '2025-03-15 13:05:00'),
(5, 'qr',       215000, 'paid', '2025-03-20 15:50:00');
-- Orders 6 & 7 are still in progress — no payment yet
GO

/* ============================================================
   InvoiceHistory
   Every status transition logged for audit and reporting (UC-07)
   ChangedBy: cashiers (3,4) close orders; mechanics (5,6,7) start them
   ============================================================ */
INSERT INTO InvoiceHistory (OrderId, ChangedBy, PreviousStatus, NewStatus, TotalPrice, Note, ChangedAt) VALUES
-- Order 1 lifecycle
(1, 5, NULL,          'processing', 165000, N'Mechanic Tuấn started the job',              '2025-03-01 08:40:00'),
(1, 5, 'processing',  'completed',  165000, N'Job finished, ready for cashier',            '2025-03-01 10:00:00'),
(1, 3, 'completed',   'paid',       165000, N'Cash payment received',                      '2025-03-01 10:05:00'),
-- Order 2 lifecycle
(2, 6, NULL,          'processing', 270000, N'Mechanic Khoa started brake job',            '2025-03-05 09:25:00'),
(2, 6, 'processing',  'completed',  270000, N'Brakes replaced and tested',                 '2025-03-05 11:30:00'),
(2, 3, 'completed',   'paid',       270000, N'Paid via QR – PayOS confirmed',              '2025-03-05 11:35:00'),
-- Order 3 lifecycle
(3, 7, NULL,          'processing', 330000, N'Mechanic Hùng started tyre swap',            '2025-03-10 10:10:00'),
(3, 7, 'processing',  'completed',  330000, N'Front tyre replaced',                        '2025-03-10 12:00:00'),
(3, 4, 'completed',   'paid',       330000, N'Bank transfer confirmed',                    '2025-03-10 12:10:00'),
-- Order 4 lifecycle
(4, 5, NULL,          'processing', 145000, N'Tuấn started carb clean',                   '2025-03-15 11:30:00'),
(4, 5, 'processing',  'completed',  145000, N'Carb cleaned and air filter replaced',       '2025-03-15 13:00:00'),
(4, 3, 'completed',   'paid',       145000, N'Cash payment',                               '2025-03-15 13:05:00'),
-- Order 5 lifecycle
(5, 6, NULL,          'processing', 215000, N'Khoa started battery + spark plug',          '2025-03-20 14:10:00'),
(5, 6, 'processing',  'completed',  215000, N'Battery charged and spark plug replaced',    '2025-03-20 15:45:00'),
(5, 4, 'completed',   'paid',       215000, N'QR payment – PayOS confirmed',               '2025-03-20 15:50:00'),
-- Orders 6 & 7 — only one event so far (just started)
(6, 5, NULL,          'processing', 290000, N'Tuấn started rear tyre swap on Vision',      '2025-04-01 09:40:00'),
(7, 6, NULL,          'processing', 860000, N'Khoa started Raider engine overhaul',        '2025-04-01 10:55:00');
GO

/* ============================================================
   WorkSchedules  (UC-04)
   Sample week: 2025-03-31 to 2025-04-04
   ============================================================ */
INSERT INTO WorkSchedules (EmployeeId, WorkDate, Shift, CheckIn, CheckOut) VALUES
-- Cashier 1 (Lê Thị Thu Ngân) – morning all week
(3, '2025-03-31', 'morning',   '2025-03-31 07:58:00', '2025-03-31 12:05:00'),
(3, '2025-04-01', 'morning',   '2025-04-01 08:02:00', NULL),                 -- still at work
-- Cashier 2 (Phạm Văn Thu Ngân) – afternoon all week
(4, '2025-03-31', 'afternoon', '2025-03-31 13:00:00', '2025-03-31 18:00:00'),
(4, '2025-04-01', 'afternoon', '2025-04-01 13:05:00', NULL),
-- Mechanic Tuấn – morning
(5, '2025-03-31', 'morning',   '2025-03-31 07:55:00', '2025-03-31 12:00:00'),
(5, '2025-04-01', 'morning',   '2025-04-01 08:00:00', NULL),
-- Mechanic Khoa – morning
(6, '2025-03-31', 'morning',   '2025-03-31 08:10:00', '2025-03-31 12:15:00'), -- arrived 10 min late
(6, '2025-04-01', 'morning',   '2025-04-01 07:59:00', NULL),
-- Mechanic Hùng – afternoon
(7, '2025-03-31', 'afternoon', '2025-03-31 13:02:00', '2025-03-31 18:00:00'),
(7, '2025-04-01', 'afternoon', '2025-04-01 13:00:00', NULL);
GO

/* ============================================================
   LeaveRequests  (UC-09)
   ============================================================ */
INSERT INTO LeaveRequests (EmployeeId, LeaveDate, Reason, Status, ApprovedBy) VALUES
-- Mechanic Đạt (EmployeeId 8) – approved leave (hence status = inactive)
(8, '2025-04-01', N'Đám cưới ở quê',          'approved', 2),
(8, '2025-04-02', N'Đám cưới ở quê (ngày 2)', 'approved', 2),
-- Mechanic Hùng – pending request next week
(7, '2025-04-07', N'Khám sức khỏe định kỳ',   'pending',  NULL),
-- Cashier 1 – rejected request
(3, '2025-03-28', N'Việc cá nhân',             'rejected', 2);
GO

/* ============================================================
   Quick sanity-check queries
   (comment out before running in production)
   ============================================================ */

-- Check pending queue (should show ReceiptId 9 and 10)
SELECT * FROM PendingAssignmentQueue;

-- Check mechanic workload (active job count per mechanic)
SELECT * FROM MechanicWorkload;

-- Full invoice history for order 1
SELECT * FROM InvoiceHistory WHERE OrderId = 1 ORDER BY ChangedAt;

-- Revenue summary by payment method
SELECT Method, COUNT(*) AS TotalTransactions, SUM(Amount) AS TotalRevenue
FROM Payments
WHERE Status = 'paid'
GROUP BY Method;

-- Productivity: job count and avg completion time per mechanic (completed orders only)
SELECT
    e.Name                                              AS Mechanic,
    COUNT(so.OrderId)                                   AS JobsCompleted,
    AVG(DATEDIFF(MINUTE, so.StartedAt, so.CompletedAt)) AS AvgMinutesPerJob,
    SUM(so.TotalPrice)                                  AS TotalRevenue
FROM ServiceOrders so
JOIN Employees     e ON e.EmployeeId = so.MechanicId
WHERE so.Status = 'completed'
GROUP BY e.EmployeeId, e.Name
ORDER BY TotalRevenue DESC;
GO