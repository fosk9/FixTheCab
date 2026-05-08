/* ============================================================
   PMP RACING — Sample Data v2
   Chạy sau PMPSchema_v2.sql
   Tất cả số tiền đơn vị VND.
   ============================================================ */

USE PmpRacing;
GO


/* ============================================================
   ① ROLES
   ============================================================ */
INSERT INTO Roles (RoleName, Description) VALUES
('admin',    N'Quản trị viên hệ thống'),
('owner',    N'Chủ cửa hàng'),
('manager',  N'Quản lý cửa hàng'),
('cashier',  N'Thu ngân'),
('mechanic', N'Thợ sửa xe');
GO
-- RoleId: admin=1, owner=2, manager=3, cashier=4, mechanic=5


/* ============================================================
   ② EMPLOYEES  (không còn cột Role)
   ============================================================ */
INSERT INTO Employees (Name, Email, Phone, Password, ProfileImagePath, Status) VALUES
-- EmployeeId 1
(N'Nguyễn Phạm Duy',        'admin@pmpracing.vn',    '0901000001', '123', N'~/img/avatars/admin.png',     'active'),
-- EmployeeId 2
(N'Nguyễn Hùng Nam Giao',   'manager@pmpracing.vn',  '0901000002', '123', N'~/img/avatars/manager.png',   'active'),
-- EmployeeId 3
(N'Nguyễn Hoàng Việt',      'cashier1@pmpracing.vn', '0901000003', '123', N'~/img/avatars/cashier1.png',  'active'),
-- EmployeeId 4
(N'Nguyễn Văn Sĩ',          'cashier2@pmpracing.vn', '0901000004', '123', N'~/img/avatars/cashier2.png',  'active'),
-- EmployeeId 5
(N'Phạm Minh Phước',        'phuoc@pmpracing.vn',    '0901000005', '123', N'~/img/avatars/mechanic1.png', 'active'),
-- EmployeeId 6
(N'Việt Hưng',               'hung@pmpracing.vn',     '0901000006', '123', N'~/img/avatars/mechanic2.png', 'active'),
-- EmployeeId 7
(N'Văn',                     'van@pmpracing.vn',      '0901000007', '123', N'~/img/avatars/mechanic3.png', 'active'),
-- EmployeeId 8
(N'Bách',                    'bach@pmpracing.vn',     '0901000008', '123', N'~/img/avatars/mechanic4.png', 'inactive');
GO


/* ============================================================
   ③ USER ROLES  (ánh xạ nhiều-nhiều)
   ============================================================ */
-- Employee 1: admin
INSERT INTO UserRoles (EmployeeId, RoleId, AssignedBy) VALUES (1, 1, 1);
-- Employee 2: owner + manager (chủ shop kiêm quản lý)
INSERT INTO UserRoles (EmployeeId, RoleId, AssignedBy) VALUES (2, 2, 1);
INSERT INTO UserRoles (EmployeeId, RoleId, AssignedBy) VALUES (2, 3, 1);
-- Employee 3: cashier
INSERT INTO UserRoles (EmployeeId, RoleId, AssignedBy) VALUES (3, 4, 2);
-- Employee 4: cashier
INSERT INTO UserRoles (EmployeeId, RoleId, AssignedBy) VALUES (4, 4, 2);
-- Employee 5: mechanic
INSERT INTO UserRoles (EmployeeId, RoleId, AssignedBy) VALUES (5, 5, 2);
-- Employee 6: mechanic
INSERT INTO UserRoles (EmployeeId, RoleId, AssignedBy) VALUES (6, 5, 2);
-- Employee 7: mechanic
INSERT INTO UserRoles (EmployeeId, RoleId, AssignedBy) VALUES (7, 5, 2);
-- Employee 8: mechanic (inactive)
INSERT INTO UserRoles (EmployeeId, RoleId, AssignedBy) VALUES (8, 5, 2);
GO


/* ============================================================
   ④ SUBSCRIPTION PLANS
   ============================================================ */
INSERT INTO SubscriptionPlans (PlanName, DurationDays, Price, Description) VALUES
(N'Gói 1 tháng',  30,   199000, N'Phù hợp dùng thử'),
(N'Gói 3 tháng',  90,   499000, N'Tiết kiệm hơn gói tháng'),
(N'Gói 1 năm',   365,  1599000, N'Tiết kiệm nhất — tặng 1 tháng');
GO
-- PlanId: 1m=1, 3m=2, 1y=3


/* ============================================================
   ⑤ SHOPS  (OwnerId = Employee 2 — chủ shop kiêm manager)
   ============================================================ */
INSERT INTO Shops (OwnerId, ShopName, Address, Phone, Status) VALUES
(2, N'PMP Racing – Chi nhánh Hà Nội',
   N'12 Trần Duy Hưng, Cầu Giấy, Hà Nội',
   '0901000002', 'active');
GO
-- ShopId = 1


/* ============================================================
   ⑥ SHOP SUBSCRIPTIONS
   ============================================================ */
INSERT INTO ShopSubscriptions
    (ShopId, PlanId, StartDate, EndDate, AmountPaid, PaymentMethod, Status, CreatedBy)
VALUES
(1, 3, '2025-01-01', '2025-12-31', 1599000, 'transfer', 'active', 1);
GO
-- SubscriptionId = 1


/* ============================================================
   ⑦ ACTIVATION KEYS  (1 key per shop)
   ============================================================ */
INSERT INTO ActivationKeys
    (ShopId, LicenseKey, SubscriptionId, IssuedAt, ExpiresAt, ActivatedAt, Status, IssuedBy)
VALUES
(1, 'A1B2C3D4-E5F6A7B8-C9D0E1F2-A3B4C5D6',
    1,
    '2025-01-01 08:00:00',
    '2025-12-31 23:59:59',
    '2025-01-01 09:15:00',
    'active', 1);
GO


/* ============================================================
   ⑧ SHOP EMPLOYEES  (nhân viên nội bộ không cần tài khoản)
   ============================================================ */
INSERT INTO ShopEmployees (ShopId, FullName, Phone, Position, Salary, JoinDate, Status, CreatedBy) VALUES
(1, N'Trần Văn Tèo',   '0911222333', N'Thợ phụ',  5000000, '2025-02-01', 'active', 2),
(1, N'Lê Thị Gấu',    '0911444555', N'Tạp vụ',   4000000, '2025-03-01', 'active', 2),
(1, N'Hoàng Công Cờ',  '0911666777', N'Thợ chính', 8000000, '2024-11-01', 'active', 5);
GO


/* ============================================================
   NGHIỆP VỤ GỐC  (giữ nguyên dữ liệu, thêm ShopId)
   ============================================================ */

-- Customers
INSERT INTO Customers (Name, Phone, Email, ShopId) VALUES
(N'Nguyễn Thành Long',  '0912345601', 'long.nguyen@gmail.com', 1),
(N'Trần Minh Phúc',     '0912345602', 'phuc.tran@gmail.com',   1),
(N'Lê Thị Hoa',         '0912345603', 'hoa.le@gmail.com',      1),
(N'Phạm Đức Thắng',     '0912345604', 'thang.pham@gmail.com',  1),
(N'Bùi Thị Lan',        '0912345605', 'lan.bui@gmail.com',     1),
(N'Đỗ Văn Bình',        '0912345606', 'binh.do@gmail.com',     1),
(N'Hoàng Thị Mai',      '0912345607', 'mai.hoang@gmail.com',   1),
(N'Vũ Quang Hải',       '0912345608', 'hai.vu@gmail.com',      1);
GO

-- Bikes
INSERT INTO Bikes (CustomerId, LicensePlate, BikeModel) VALUES
(1, '29B1-12345', N'Honda Wave Alpha'),
(1, '29B1-99999', N'Honda SH 150i'),
(2, '51G1-23456', N'Yamaha Exciter 155'),
(3, '43C1-34567', N'Honda Vision'),
(4, '29A1-45678', N'Yamaha Sirius'),
(5, '51H1-56789', N'Honda Lead 125'),
(6, '30E1-67890', N'Suzuki Raider R150'),
(7, '92B1-78901', N'Honda Air Blade 125'),
(8, '51F1-89012', N'Yamaha NVX 155'),
(3, '43C1-11111', N'Honda Winner X');
GO

-- Services
INSERT INTO Services (ShopId, ServiceName, Price) VALUES
(1, N'Thay nhớt động cơ',         80000),
(1, N'Vệ sinh bộ chế hoà khí',    60000),
(1, N'Thay lốc máy',             350000),
(1, N'Sửa hệ thống phanh',       120000),
(1, N'Thay dây côn',              50000),
(1, N'Kiểm tra & nạp bình điện',  90000),
(1, N'Thay bugi',                 40000),
(1, N'Vệ sinh & tra dầu xích',    30000),
(1, N'Chỉnh đèn xe',              25000),
(1, N'Đại tu động cơ',           800000),
(1, N'Thay lốc xe trước',        150000),
(1, N'Thay lốc xe sau',          150000),
(1, N'Rửa xe',                    25000),
(1, N'Thay lọc gió',              45000);
GO

-- Parts
INSERT INTO Parts (ShopId, PartName, Price, Stock, WarningLevel) VALUES
(1, N'Nhớt Castrol Power1 0.8L',      85000,  50, 10),
(1, N'Nhớt Motul 5100 1L',           145000,  30,  8),
(1, N'Bugi NGK CR7HSA',               35000,  80, 15),
(1, N'Bugi Denso U22ESR-N',           40000,  60, 15),
(1, N'Dây côn Honda Wave',            48000,  25,  5),
(1, N'Dây côn Yamaha Exciter',        52000,  20,  5),
(1, N'Bình điện GS 5Ah',            320000,  10,  3),
(1, N'Lốc xe trước Honda Vision',    140000,   8,  2),
(1, N'Lốc xe sau Honda Vision',      140000,   8,  2),
(1, N'Lọc gió Honda Wave',            40000,  35, 10),
(1, N'Lọc gió Yamaha Exciter',        45000,  28, 10),
(1, N'Bố thắng trước Honda Wave',     55000,  40, 10),
(1, N'Bố thắng sau Honda Wave',       50000,  40, 10),
(1, N'Xích tải Honda Wave',           95000,  15,  4),
(1, N'Má phanh đĩa Yamaha Exciter',   75000,  20,  5);
GO

-- ServiceReceipts
INSERT INTO ServiceReceipts (ShopId, BikeId, CreatedBy, Status, CreatedAt) VALUES
(1, 1,  3, 'completed',   '2025-03-01 08:30:00'),
(1, 3,  3, 'completed',   '2025-03-05 09:15:00'),
(1, 5,  4, 'completed',   '2025-03-10 10:00:00'),
(1, 7,  4, 'completed',   '2025-03-15 11:20:00'),
(1, 9,  3, 'completed',   '2025-03-20 14:00:00'),
(1, 2,  3, 'assigned',    '2025-04-01 08:00:00'),
(1, 4,  4, 'in_progress', '2025-04-01 09:30:00'),
(1, 6,  3, 'in_progress', '2025-04-01 10:45:00'),
(1, 8,  4, 'pending',     '2025-04-01 13:00:00'),
(1, 10, 3, 'pending',     '2025-04-01 13:30:00');
GO

-- MechanicAssignments
INSERT INTO MechanicAssignments (ReceiptId, MechanicId, AssignedBy, AssignedAt, Note) VALUES
(1, 5, 2, '2025-03-01 08:35:00', N'Routine oil change, assign to Phước'),
(2, 6, 2, '2025-03-05 09:20:00', N'Exciter brake system, Hưng handles sportbikes'),
(3, 7, 2, '2025-03-10 10:05:00', N'Lead tyre swap, assign to Văn'),
(4, 5, 2, '2025-03-15 11:25:00', N'Air Blade carb clean, Phước specialises in Honda'),
(5, 6, 2, '2025-03-20 14:05:00', N'NVX electrical check'),
(6, 7, 2, '2025-04-01 08:10:00', N'SH 150i full service, Văn available'),
(7, 5, 2, '2025-04-01 09:35:00', N'Vision tyre – Phước'),
(8, 6, 2, '2025-04-01 10:50:00', N'Raider engine, Hưng knows Suzuki');
GO

-- ServiceOrders
INSERT INTO ServiceOrders (ReceiptId, MechanicId, Status, TotalPrice, StartedAt, CompletedAt, CreatedAt) VALUES
(1, 5, 'completed',  165000, '2025-03-01 08:40:00', '2025-03-01 10:00:00', '2025-03-01 08:40:00'),
(2, 6, 'completed',  270000, '2025-03-05 09:25:00', '2025-03-05 11:30:00', '2025-03-05 09:25:00'),
(3, 7, 'completed',  330000, '2025-03-10 10:10:00', '2025-03-10 12:00:00', '2025-03-10 10:10:00'),
(4, 5, 'completed',  145000, '2025-03-15 11:30:00', '2025-03-15 13:00:00', '2025-03-15 11:30:00'),
(5, 6, 'completed',  215000, '2025-03-20 14:10:00', '2025-03-20 15:45:00', '2025-03-20 14:10:00'),
(7, 5, 'processing', 290000, '2025-04-01 09:40:00', NULL,                  '2025-04-01 09:40:00'),
(8, 6, 'processing', 860000, '2025-04-01 10:55:00', NULL,                  '2025-04-01 10:55:00');
GO

-- ServiceOrderItems
INSERT INTO ServiceOrderItems (OrderId, ServiceId, Price) VALUES
(1, 1,  80000), (1, 7,  40000), (1, 13, 25000),
(2, 4, 120000), (2, 8,  30000), (2, 13, 25000), (2, 7, 40000), (2, 5, 50000),
(3, 11,150000), (3, 1,  80000), (3, 13, 25000),
(4, 2,  60000), (4, 14, 45000),
(5, 6,  90000), (5, 7,  40000), (5, 13, 25000),
(6, 12,150000), (6, 1,  80000),
(7, 10,800000), (7, 1,  80000);
GO

-- OrderParts
INSERT INTO OrderParts (OrderId, PartId, Quantity, Price) VALUES
(1, 1,  1,  85000), (1, 3,  1,  35000),
(2, 15, 1,  75000), (2, 14, 1,  95000),
(3, 8,  1, 140000), (3, 1,  1,  85000),
(4, 10, 1,  40000),
(5, 7,  1, 320000), (5, 4,  1,  40000),
(6, 9,  1, 140000), (6, 2,  1, 145000),
(7, 2,  2, 145000), (7, 4,  2,  40000);
GO

-- Payments
INSERT INTO Payments (OrderId, Method, Amount, Status, PaidAt) VALUES
(1, 'cash',     165000, 'paid', '2025-03-01 10:05:00'),
(2, 'qr',       270000, 'paid', '2025-03-05 11:35:00'),
(3, 'transfer', 330000, 'paid', '2025-03-10 12:10:00'),
(4, 'cash',     145000, 'paid', '2025-03-15 13:05:00'),
(5, 'qr',       215000, 'paid', '2025-03-20 15:50:00');
GO

-- InvoiceHistory
INSERT INTO InvoiceHistory (OrderId, ChangedBy, PreviousStatus, NewStatus, TotalPrice, Note, ChangedAt) VALUES
(1, 5, NULL,         'processing', 165000, N'Phước bắt đầu thay nhớt',              '2025-03-01 08:40:00'),
(1, 5, 'processing', 'completed',  165000, N'Hoàn thành, chờ thu ngân',              '2025-03-01 10:00:00'),
(1, 3, 'completed',  'paid',       165000, N'Thanh toán tiền mặt',                   '2025-03-01 10:05:00'),
(2, 6, NULL,         'processing', 270000, N'Hưng bắt đầu sửa phanh Exciter',       '2025-03-05 09:25:00'),
(2, 6, 'processing', 'completed',  270000, N'Phanh đã thay, kiểm tra xong',          '2025-03-05 11:30:00'),
(2, 3, 'completed',  'paid',       270000, N'Thanh toán QR – PayOS',                 '2025-03-05 11:35:00'),
(3, 7, NULL,         'processing', 330000, N'Văn bắt đầu thay lốc Lead',            '2025-03-10 10:10:00'),
(3, 7, 'processing', 'completed',  330000, N'Thay lốc trước hoàn tất',               '2025-03-10 12:00:00'),
(3, 4, 'completed',  'paid',       330000, N'Chuyển khoản ngân hàng',                '2025-03-10 12:10:00'),
(4, 5, NULL,         'processing', 145000, N'Phước vệ sinh chế hoà khí',            '2025-03-15 11:30:00'),
(4, 5, 'processing', 'completed',  145000, N'Vệ sinh xong, thay lọc gió',            '2025-03-15 13:00:00'),
(4, 3, 'completed',  'paid',       145000, N'Tiền mặt',                              '2025-03-15 13:05:00'),
(5, 6, NULL,         'processing', 215000, N'Hưng bắt đầu kiểm tra bình điện NVX', '2025-03-20 14:10:00'),
(5, 6, 'processing', 'completed',  215000, N'Nạp bình, thay bugi',                   '2025-03-20 15:45:00'),
(5, 4, 'completed',  'paid',       215000, N'QR – PayOS',                            '2025-03-20 15:50:00'),
(6, 5, NULL,         'processing', 290000, N'Phước bắt đầu thay lốc sau Vision',    '2025-04-01 09:40:00'),
(7, 6, NULL,         'processing', 860000, N'Hưng bắt đầu đại tu máy Raider',       '2025-04-01 10:55:00');
GO

-- WorkSchedules
INSERT INTO WorkSchedules (ShopId, EmployeeId, WorkDate, Shift, CheckIn, CheckOut) VALUES
(1, 3, '2025-03-31', 'morning',   '2025-03-31 07:58:00', '2025-03-31 12:05:00'),
(1, 3, '2025-04-01', 'morning',   '2025-04-01 08:02:00', NULL),
(1, 4, '2025-03-31', 'afternoon', '2025-03-31 13:00:00', '2025-03-31 18:00:00'),
(1, 4, '2025-04-01', 'afternoon', '2025-04-01 13:05:00', NULL),
(1, 5, '2025-03-31', 'morning',   '2025-03-31 07:55:00', '2025-03-31 12:00:00'),
(1, 5, '2025-04-01', 'morning',   '2025-04-01 08:00:00', NULL),
(1, 6, '2025-03-31', 'morning',   '2025-03-31 08:10:00', '2025-03-31 12:15:00'),
(1, 6, '2025-04-01', 'morning',   '2025-04-01 07:59:00', NULL),
(1, 7, '2025-03-31', 'afternoon', '2025-03-31 13:02:00', '2025-03-31 18:00:00'),
(1, 7, '2025-04-01', 'afternoon', '2025-04-01 13:00:00', NULL);
GO

-- LeaveRequests
INSERT INTO LeaveRequests (EmployeeId, LeaveDate, Reason, Status, ApprovedBy) VALUES
(8, '2025-04-01', N'Đám cưới ở quê',          'approved', 2),
(8, '2025-04-02', N'Đám cưới ở quê (ngày 2)', 'approved', 2),
(7, '2025-04-07', N'Khám sức khỏe định kỳ',   'pending',  NULL),
(3, '2025-03-28', N'Việc cá nhân',             'rejected', 2);
GO


/* ============================================================
   SYSTEM & SHOP ACTIVITY LOG SAMPLES
   ============================================================ */
INSERT INTO SystemActivityLogs (ActorId, Action, EntityType, EntityId, Detail, IpAddress) VALUES
(1, 'LOGIN',         'Employee',       1, N'Admin đăng nhập',                      '127.0.0.1'),
(1, 'CREATE_SHOP',   'Shop',           1, N'Tạo cửa hàng PMP Racing Hà Nội',       '127.0.0.1'),
(1, 'CREATE_PLAN',   'SubscriptionPlan',3,N'Tạo gói 1 năm 1.599.000 VND',          '127.0.0.1'),
(1, 'CREATE_SUB',    'ShopSubscription',1,N'Kích hoạt gói 1 năm cho shop 1',       '127.0.0.1'),
(1, 'ISSUE_KEY',     'ActivationKey',  1, N'Cấp key cho PMP Racing Hà Nội',        '127.0.0.1'),
(2, 'LOGIN',         'Employee',       2, N'Manager đăng nhập',                    '192.168.1.5'),
(2, 'ASSIGN_ROLE',   'UserRoles',     NULL,N'Cấp role cashier cho Nguyễn Hoàng Việt','192.168.1.5');
GO

INSERT INTO ShopActivityLogs (ShopId, ActorId, Action, EntityType, EntityId, Detail, IpAddress) VALUES
(1, 3, 'CREATE_RECEIPT',    'ServiceReceipt', 1,  N'Tiếp nhận Honda Wave 29B1-12345',      '192.168.1.10'),
(1, 2, 'ASSIGN_MECHANIC',   'MechanicAssignment',1,N'Phân Phước sửa Wave 29B1-12345',     '192.168.1.5'),
(1, 5, 'START_ORDER',       'ServiceOrder',   1,  N'Phước bắt đầu lệnh sửa chữa #1',      '192.168.1.12'),
(1, 5, 'COMPLETE_ORDER',    'ServiceOrder',   1,  N'Hoàn thành lệnh #1',                   '192.168.1.12'),
(1, 3, 'PROCESS_PAYMENT',   'Payment',        1,  N'Thu tiền mặt 165.000 VND cho lệnh #1','192.168.1.10'),
(1, 2, 'ADD_SHOP_EMPLOYEE', 'ShopEmployee',   1,  N'Thêm nhân viên Trần Văn Tèo (thợ phụ)','192.168.1.5');
GO


/* ============================================================
   SANITY CHECK QUERIES
   ============================================================ */

-- 1. Danh sách nhân viên kèm tất cả roles
SELECT e.Name, STRING_AGG(r.RoleName, ', ') AS Roles
FROM Employees  e
JOIN UserRoles  ur ON ur.EmployeeId = e.EmployeeId
JOIN Roles      r  ON r.RoleId      = ur.RoleId
GROUP BY e.EmployeeId, e.Name;

-- 2. Phiếu chờ phân công
SELECT * FROM PendingAssignmentQueue;

-- 3. Khối lượng công việc thợ
SELECT * FROM MechanicWorkload;

-- 4. Trạng thái key kích hoạt
SELECT s.ShopName, ak.LicenseKey, ak.Status, ak.ExpiresAt
FROM ActivationKeys ak
JOIN Shops s ON s.ShopId = ak.ShopId;

-- 5. Nhân viên nội bộ cửa hàng (không có tài khoản hệ thống)
SELECT sh.ShopName, se.FullName, se.Position, se.Status
FROM ShopEmployees se
JOIN Shops sh ON sh.ShopId = se.ShopId;

-- 6. Log hệ thống 7 ngày gần nhất
SELECT ActorId, Action, EntityType, EntityId, CreatedAt
FROM SystemActivityLogs
WHERE CreatedAt >= DATEADD(DAY, -7, GETDATE())
ORDER BY CreatedAt DESC;

-- 7. Log cửa hàng 1
SELECT ActorId, Action, EntityType, EntityId, Detail, CreatedAt
FROM ShopActivityLogs
WHERE ShopId = 1
ORDER BY CreatedAt DESC;

-- 8. Doanh thu theo phương thức thanh toán
SELECT Method, COUNT(*) AS TotalTx, SUM(Amount) AS TotalRevenue
FROM Payments WHERE Status = 'paid'
GROUP BY Method;
GO