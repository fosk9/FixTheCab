/* ============================================================
   PMP RACING — UPDATED SAMPLE DATA
   Run this AFTER pmp_racing_updated_schema.sql
   All monetary values are in VND.
   ============================================================ */

USE PmpRacing;
GO

/* ============================================================
   1. ROLES — Define system roles
   ============================================================ */
INSERT INTO Roles (RoleName, Description) VALUES
('admin',       N'System administrator'),
('shop_owner',  N'Shop owner - can manage shop and employees'),
('manager',     N'Shop manager - manage staff and orders'),
('mechanic',    N'Mechanic - perform repairs'),
('cashier',     N'Cashier - process payments'),
('customer',    N'Customer account');
GO

/* ============================================================
   2. USERS — System users with multiple roles
   ============================================================ */
INSERT INTO Users (Username, Email, Phone, Password, FullName, ProfileImagePath, Status) VALUES
-- Super admin
('admin_system',     'admin@pmpracing.vn',      '0901000001', '123', N'Nguyễn Phạm Duy',            N'~/img/avatars/admin.png',     'active'),
-- Shop Owner 1 (Tuấn) - can own 1 shop and be a mechanic
('tuan_owner',       'tuan.owner@pmpracing.vn', '0901000002', '123', N'Phạm Minh Tuấn',            N'~/img/avatars/owner1.png',    'active'),
-- Shop Owner 2 (Khoa) - can own 1 shop
('khoa_owner',       'khoa.owner@pmpracing.vn', '0901000003', '123', N'Lý Văn Khoa',              N'~/img/avatars/owner2.png',    'active'),
-- Manager (Linh)
('linh_manager',     'linh@pmpracing.vn',       '0901000004', '123', N'Nguyễn Thị Linh',          N'~/img/avatars/manager.png',   'active'),
-- Mechanic (Hùng)
('hung_mechanic',    'hung@pmpracing.vn',       '0901000005', '123', N'Trần Văn Hùng',            N'~/img/avatars/mechanic1.png', 'active'),
-- Mechanic (Bách)
('bach_mechanic',    'bach@pmpracing.vn',       '0901000006', '123', N'Hoàng Minh Bách',          N'~/img/avatars/mechanic2.png', 'active'),
-- Cashier 1 (Ngân)
('ngan_cashier1',    'ngan@pmpracing.vn',       '0901000007', '123', N'Trần Thu Ngân',            N'~/img/avatars/cashier1.png',  'active'),
-- Cashier 2 (Hương)
('huong_cashier2',   'huong@pmpracing.vn',      '0901000008', '123', N'Lê Thị Hương',             N'~/img/avatars/cashier2.png',  'active'),
-- Customer 1
('customer_long',    'long.customer@gmail.com', '0912345601', '123', N'Nguyễn Thành Long',        NULL,                           'active'),
-- Customer 2
('customer_phuc',    'phuc.customer@gmail.com', '0912345602', '123', N'Trần Minh Phúc',           NULL,                           'active');
GO

/* ============================================================
   3. USER_ROLES — Assign roles (users can have multiple roles)
   ============================================================ */
INSERT INTO UserRoles (UserId, RoleId, AssignedBy) VALUES
-- Admin system
(1, 1, 1),  -- admin_system → admin
-- Shop Owner 1 (Tuấn): owner + mechanic
(2, 2, 1),  -- tuan_owner → shop_owner
(2, 4, 1),  -- tuan_owner → mechanic (can also work as mechanic)
-- Shop Owner 2 (Khoa): owner + manager
(3, 2, 1),  -- khoa_owner → shop_owner
(3, 3, 1),  -- khoa_owner → manager
-- Manager (Linh)
(4, 3, 1),  -- linh_manager → manager
-- Mechanic (Hùng)
(5, 4, 1),  -- hung_mechanic → mechanic
-- Mechanic (Bách)
(6, 4, 1),  -- bach_mechanic → mechanic
-- Cashier 1 (Ngân)
(7, 5, 1),  -- ngan_cashier1 → cashier
-- Cashier 2 (Hương)
(8, 5, 1),  -- huong_cashier2 → cashier
-- Customer 1
(9, 6, 1),  -- customer_long → customer
-- Customer 2
(10, 6, 1); -- customer_phuc → customer
GO

/* ============================================================
   4. SHOPS — Each owner/user can create a shop
   ============================================================ */
INSERT INTO Shops (OwnerId, ShopName, Address, Phone, Email, Logo, Status) VALUES
-- Shop owned by Tuấn
(2, N'PMP Racing Tuấn',           N'123 Phố Huế, Hà Nội',              '0243 123 456', 'tuanshop@pmpracing.vn',   N'~/img/shops/logo1.png', 'active'),
-- Shop owned by Khoa
(3, N'PMP Racing Khoa',           N'456 Hoàng Hoa Thám, Hà Nội',       '0243 789 012', 'khoashop@pmpracing.vn',   N'~/img/shops/logo2.png', 'active');
GO

/* ============================================================
   5. EMPLOYEES — Shop employees (can have linked user account or not)
   ============================================================ */
INSERT INTO Employees (ShopId, UserId, Name, Email, Phone, Role, HireDate, Status) VALUES
-- Shop 1 (Tuấn's) - Employees
(1, 2,  N'Phạm Minh Tuấn',        'tuan@pmpracing.vn',      '0901000002', 'mechanic', '2024-01-01', 'active'),
(1, 4,  N'Nguyễn Thị Linh',       'linh@pmpracing.vn',      '0901000004', 'manager',  '2024-02-01', 'active'),
(1, 7,  N'Trần Thu Ngân',         'ngan@pmpracing.vn',      '0901000007', 'cashier',  '2024-03-01', 'active'),
(1, 5,  N'Trần Văn Hùng',         'hung@pmpracing.vn',      '0901000005', 'mechanic', '2024-03-15', 'active'),
-- Shop 2 (Khoa's) - Employees
(2, 3,  N'Lý Văn Khoa',           'khoa@pmpracing.vn',      '0901000003', 'owner',    '2024-01-15', 'active'),
(2, 6,  N'Hoàng Minh Bách',       'bach@pmpracing.vn',      '0901000006', 'mechanic', '2024-02-15', 'active'),
(2, 8,  N'Lê Thị Hương',          'huong@pmpracing.vn',     '0901000008', 'cashier',  '2024-03-01', 'active'),
-- Employee without user account
(1, NULL, N'Nguyễn Sơn',          'son@pmpracing.vn',       '0901000050', 'mechanic', '2025-01-10', 'active'),
(2, NULL, N'Lê Minh Tuấn',        'minhtuan@pmpracing.vn',  '0901000051', 'cashier',  '2025-02-01', 'active');
GO

/* ============================================================
   6. WORKERS — Non-employee workers (created by managers/mechanics)
   ============================================================ */
INSERT INTO Workers (ShopId, CreatedByEmployeeId, Name, Phone, Email, Specialty, Status, Notes) VALUES
-- Shop 1: Temporary workers
(1, 1, N'Võ Cường',        '0912345801', 'cuong.vo@gmail.com', N'Thay lốp xe', 'active', N'Part-time, weekends only'),
(1, 1, N'Đỗ Nam',          '0912345802', 'nam.do@gmail.com',   N'Sửa phanh', 'active', N'Experienced, can train others'),
-- Shop 2: Temporary workers
(2, 6, N'Phạm Công Sơn',   '0912345803', 'son.pham@gmail.com', N'Rửa xe & vệ sinh', 'active', N'New worker'),
(2, 6, N'Bùi Tiến Dũng',   '0912345804', 'dung.bui@gmail.com', N'Sửa điện', 'active', NULL);
GO

/* ============================================================
   7. SUBSCRIPTION_PLANS — Pricing for packages
   ============================================================ */
INSERT INTO SubscriptionPlans (PlanName, DurationMonths, Price, Features, Status) VALUES
(N'Basic', 3, 99000,
 N'Core features: Shop management, 5 employees, limited reports',
 'active'),
(N'Pro', 3, 299000,
 N'Pro features: Up to 20 employees, advanced analytics, API access',
 'active'),
(N'Basic', 12, 320000,
 N'Core features: Shop management, 5 employees, limited reports',
 'active'),
(N'Pro', 12, 990000,
 N'Pro features: Up to 20 employees, advanced analytics, API access',
 'active');
GO

/* ============================================================
   8. USER_SUBSCRIPTIONS — User subscription tracking
   ============================================================ */
INSERT INTO UserSubscriptions (UserId, PlanId, StartDate, EndDate, Price, Status) VALUES
-- Tuấn: Pro 3-month from April 1 to July 1, 2025
(2, 2, '2025-04-01', '2025-07-01', 299000, 'active'),
-- Khoa: Basic 12-month from April 1 to April 1, 2026
(3, 3, '2025-04-01', '2026-04-01', 320000, 'active'),
-- Admin: Free (no subscription)
(1, NULL, NULL, NULL, 0, 'active');
GO

/* ============================================================
   9. CUSTOMERS — Shop customers
   ============================================================ */
INSERT INTO Customers (ShopId, Name, Phone, Email, Address, Status) VALUES
-- Shop 1 (Tuấn's)
(1, N'Nguyễn Thành Long',  '0912345601', 'long.nguyen@gmail.com',    N'789 Nguyễn Huệ, Hà Nội',      'active'),
(1, N'Trần Minh Phúc',     '0912345602', 'phuc.tran@gmail.com',      N'567 Lê Lợi, Hà Nội',         'active'),
(1, N'Lê Thị Hoa',         '0912345603', 'hoa.le@gmail.com',         N'234 Trần Duy Hung, Hà Nội',   'active'),
(1, N'Phạm Đức Thắng',     '0912345604', 'thang.pham@gmail.com',     N'123 Phố Huế, Hà Nội',        'active'),
-- Shop 2 (Khoa's)
(2, N'Bùi Thị Lan',        '0912345605', 'lan.bui@gmail.com',        N'456 Hoàng Hoa Thám, Hà Nội',  'active'),
(2, N'Đỗ Văn Bình',        '0912345606', 'binh.do@gmail.com',        N'890 Trần Phú, Hà Nội',        'active'),
(2, N'Hoàng Thị Mai',      '0912345607', 'mai.hoang@gmail.com',      N'321 Phan Xích Long, Hà Nội',  'active'),
(2, N'Vũ Quang Hải',       '0912345608', 'hai.vu@gmail.com',         N'654 Giảng Võ, Hà Nội',        'active');
GO

/* ============================================================
   10. BIKES — Customer bikes
   ============================================================ */
INSERT INTO Bikes (CustomerId, LicensePlate, BikeModel, Color, Year, Status) VALUES
-- Shop 1 customers
(1, '29B1-12345', N'Honda Wave Alpha',       N'Trắng', 2020, 'active'),
(1, '29B1-99999', N'Honda SH 150i',          N'Đen',   2023, 'active'),
(2, '51G1-23456', N'Yamaha Exciter 155',     N'Đỏ',    2022, 'active'),
(3, '43C1-34567', N'Honda Vision',           N'Xám',   2021, 'active'),
(4, '29A1-45678', N'Yamaha Sirius',          N'Trắng', 2019, 'active'),
-- Shop 2 customers
(5, '51H1-56789', N'Honda Lead 125',         N'Đen',   2022, 'active'),
(6, '30E1-67890', N'Suzuki Raider R150',     N'Xanh',  2021, 'active'),
(7, '92B1-78901', N'Honda Air Blade 125',    N'Đỏ',    2023, 'active'),
(8, '51F1-89012', N'Yamaha NVX 155',         N'Bạc',   2024, 'active'),
(3, '43C1-11111', N'Honda Winner X',         N'Cam',   2023, 'active');
GO

/* ============================================================
   11. SERVICES — Service catalog per shop
   ============================================================ */
INSERT INTO Services (ShopId, ServiceName, Description, Price, Status) VALUES
-- Shop 1 Services
(1, N'Thay nhớt động cơ',       N'Thay nhớt động cơ định kỳ',          80000,   'active'),
(1, N'Vệ sinh bộ chế hoà khí',  N'Vệ sinh carburettor',               60000,   'active'),
(1, N'Thay lốc máy',            N'Thay piston/xi lanh',                350000,  'active'),
(1, N'Sửa hệ thống phanh',      N'Thay má phanh, vệ sinh phanh',      120000,  'active'),
(1, N'Rửa xe',                  N'Rửa xe toàn bộ',                     25000,   'active'),
-- Shop 2 Services
(2, N'Thay nhớt động cơ',       N'Thay nhớt động cơ',                  85000,   'active'),
(2, N'Kiểm tra & nạp bình điện', N'Kiểm tra và nạp điện bình',        90000,   'active'),
(2, N'Thay bugi',               N'Thay bugi mới',                       40000,   'active'),
(2, N'Thay dây côn',            N'Thay dây côn',                        50000,   'active'),
(2, N'Đại tu động cơ',         N'Sửa chữa lớn động cơ',               800000,  'active');
GO

/* ============================================================
   12. PARTS — Spare parts inventory per shop
   ============================================================ */
INSERT INTO Parts (ShopId, PartName, Price, Stock, WarningLevel, Status) VALUES
-- Shop 1 Parts
(1, N'Nhớt Castrol Power1 0.8L',       85000,   50, 10, 'active'),
(1, N'Bugi NGK CR7HSA',                35000,   80, 15, 'active'),
(1, N'Dây côn Honda Wave',             48000,   25,  5, 'active'),
(1, N'Bình điện GS 5Ah',               320000,  10,  3, 'active'),
(1, N'Lốc xe trước Honda Vision',      140000,   8,  2, 'active'),
-- Shop 2 Parts
(2, N'Nhớt Motul 5100 1L',             145000,  30,  8, 'active'),
(2, N'Bugi Denso U22ESR-N',            40000,   60, 15, 'active'),
(2, N'Dây côn Yamaha Exciter',         52000,   20,  5, 'active'),
(2, N'Lọc gió Yamaha Exciter',         45000,   28, 10, 'active'),
(2, N'Má phanh đĩa Yamaha Exciter',    75000,   20,  5, 'active');
GO

/* ============================================================
   13. SERVICE_RECEIPTS — Service requests
   ============================================================ */
INSERT INTO ServiceReceipts (ShopId, BikeId, CreatedByEmployeeId, Status, Notes, CreatedAt) VALUES
-- Shop 1 Receipts
(1, 1, 3, 'completed',   N'Thay nhớt định kỳ',            '2025-03-01 08:30:00'),  -- ReceiptId 1
(1, 3, 3, 'completed',   N'Sửa phanh',                    '2025-03-05 09:15:00'),  -- ReceiptId 2
(1, 5, 4, 'in_progress', N'Nạp bình điện',                '2025-04-01 10:00:00'),  -- ReceiptId 3
(1, 7, 3, 'pending',     N'Chưa được gán người',          '2025-04-01 13:00:00'),  -- ReceiptId 4
-- Shop 2 Receipts
(2, 6, 7, 'completed',   N'Thay lốc xe',                  '2025-03-15 11:20:00'),  -- ReceiptId 5
(2, 8, 7, 'in_progress', N'Đại tu động cơ',               '2025-04-01 09:30:00'),  -- ReceiptId 6
(2, 10, 7, 'assigned',   N'Đã gán cho thợ',               '2025-04-01 10:45:00');  -- ReceiptId 7
GO

/* ============================================================
   14. MECHANIC_ASSIGNMENTS — Assign mechanics/workers
   ============================================================ */
INSERT INTO MechanicAssignments (ReceiptId, EmployeeId, WorkerId, AssignedBy, AssignedAt, Note) VALUES
-- Shop 1 Assignments
(1, 1,  NULL, 1, '2025-03-01 08:35:00', N'Tuấn xử lý'),
(2, 4,  NULL, 1, '2025-03-05 09:20:00', N'Hùng sửa phanh'),
(3, 4,  NULL, 1, '2025-04-01 10:05:00', N'Hùng nạp bình'),
(4, NULL, 1, 1, '2025-04-01 13:05:00', N'Gán cho worker Võ Cường'),
-- Shop 2 Assignments
(5, 6,  NULL, 3, '2025-03-15 11:25:00', N'Bách thay lốc'),
(6, 6,  NULL, 3, '2025-04-01 09:35:00', N'Bách đại tu động cơ'),
(7, NULL, 3, 3, '2025-04-01 10:50:00', N'Gán cho worker Phạm Công Sơn');
GO

/* ============================================================
   15. SERVICE_ORDERS — Service orders
   ============================================================ */
INSERT INTO ServiceOrders (ReceiptId, AssignmentId, Status, TotalPrice, StartedAt, CompletedAt, CreatedAt) VALUES
(1, 1, 'completed',  165000, '2025-03-01 08:40:00', '2025-03-01 10:00:00', '2025-03-01 08:40:00'),
(2, 2, 'completed',  270000, '2025-03-05 09:25:00', '2025-03-05 11:30:00', '2025-03-05 09:25:00'),
(3, 3, 'processing', 90000,  '2025-04-01 10:10:00', NULL,                  '2025-04-01 10:10:00'),
(4, 4, 'pending',    NULL,   NULL,                  NULL,                  '2025-04-01 13:05:00'),
(5, 5, 'completed',  330000, '2025-03-15 11:30:00', '2025-03-15 13:00:00', '2025-03-15 11:30:00'),
(6, 6, 'processing', 860000, '2025-04-01 09:40:00', NULL,                  '2025-04-01 09:40:00'),
(7, 7, 'pending',    NULL,   NULL,                  NULL,                  '2025-04-01 10:50:00');
GO

/* ============================================================
   16. SERVICE_ORDER_ITEMS — Services per order
   ============================================================ */
INSERT INTO ServiceOrderItems (OrderId, ServiceId, Price, Quantity) VALUES
-- Order 1
(1, 1, 80000, 1),
(1, 5, 25000, 1),
-- Order 2
(2, 4, 120000, 1),
(2, 5, 25000, 1),
-- Order 3
(3, 2, 60000, 1),
-- Order 5
(5, 6, 90000, 1),
-- Order 6
(6, 5, 800000, 1),
(6, 1, 85000, 1);
GO

/* ============================================================
   17. ORDER_PARTS — Parts used
   ============================================================ */
INSERT INTO OrderParts (OrderId, PartId, Quantity, Price) VALUES
-- Order 1
(1, 1, 1, 85000),
-- Order 2
(2, 5, 1, 48000),
-- Order 3
(3, 2, 1, 145000),
-- Order 5
(5, 4, 1, 320000),
-- Order 6
(6, 7, 2, 145000),
(6, 8, 2, 40000);
GO

/* ============================================================
   18. PAYMENTS — Payment records
   ============================================================ */
INSERT INTO Payments (OrderId, Method, Amount, Status, PaidAt, Notes) VALUES
(1, 'cash',     165000, 'paid',    '2025-03-01 10:05:00', N'Tiền mặt'),
(2, 'transfer', 270000, 'paid',    '2025-03-05 11:35:00', N'Chuyển khoản'),
(3, 'qr',       90000,  'pending', NULL,                  N'Chờ thanh toán'),
(5, 'cash',     330000, 'paid',    '2025-03-15 13:05:00', N'Tiền mặt'),
(6, 'qr',       860000, 'pending', NULL,                  N'Chờ thanh toán');
GO

/* ============================================================
   19. INVOICE_HISTORY — Audit trail
   ============================================================ */
INSERT INTO InvoiceHistory (OrderId, ChangedByEmployeeId, PreviousStatus, NewStatus, TotalPrice, Note, ChangedAt) VALUES
-- Order 1 lifecycle
(1, 1, NULL,          'processing', 165000, N'Tuấn bắt đầu',              '2025-03-01 08:40:00'),
(1, 1, 'processing',  'completed',  165000, N'Tuấn hoàn thành',           '2025-03-01 10:00:00'),
(1, 3, 'completed',   'paid',       165000, N'Ngân nhận tiền mặt',        '2025-03-01 10:05:00'),
-- Order 2 lifecycle
(2, 4, NULL,          'processing', 270000, N'Hùng bắt đầu sửa phanh',   '2025-03-05 09:25:00'),
(2, 4, 'processing',  'completed',  270000, N'Hùng hoàn thành',          '2025-03-05 11:30:00'),
(2, 3, 'completed',   'paid',       270000, N'Ngân nhận chuyển khoản',    '2025-03-05 11:35:00');
GO

/* ============================================================
   20. WORK_SCHEDULES — Employee schedules
   ============================================================ */
INSERT INTO WorkSchedules (ShopId, EmployeeId, WorkDate, Shift, CheckIn, CheckOut, Status) VALUES
-- Shop 1
(1, 1, '2025-04-01', 'morning',   '2025-04-01 07:58:00', '2025-04-01 12:05:00', 'checked_out'),
(1, 3, '2025-04-01', 'morning',   '2025-04-01 08:02:00', NULL,                  'checked_in'),
(1, 4, '2025-04-01', 'afternoon', '2025-04-01 13:00:00', NULL,                  'checked_in'),
-- Shop 2
(2, 6, '2025-04-01', 'morning',   '2025-04-01 07:59:00', NULL,                  'checked_in'),
(2, 7, '2025-04-01', 'afternoon', '2025-04-01 13:05:00', NULL,                  'checked_in');
GO

/* ============================================================
   21. LEAVE_REQUESTS — Leave requests
   ============================================================ */
INSERT INTO LeaveRequests (ShopId, EmployeeId, LeaveDate, Reason, Status, ApprovedBy, ApprovedAt) VALUES
(1, 4, '2025-04-15', N'Nghỉ riêng', 'pending', NULL, NULL),
(2, 6, '2025-04-20', N'Bệnh', 'approved', 3, '2025-04-01 10:00:00');
GO

/* ============================================================
   22. SYSTEM_ACTIVITY_LOGS — System-wide logging
   ============================================================ */
INSERT INTO SystemActivityLogs (UserId, ActivityType, EntityType, EntityId, Description, IPAddress, Status, CreatedAt) VALUES
(1, 'system_startup', 'System', 0, N'Hệ thống khởi động', '127.0.0.1', 'success', '2025-04-01 07:00:00'),
(2, 'user_login', 'User', 2, N'Tuấn đăng nhập', '192.168.1.100', 'success', '2025-04-01 07:30:00'),
(3, 'user_login', 'User', 3, N'Khoa đăng nhập', '192.168.1.101', 'success', '2025-04-01 07:35:00'),
(2, 'shop_created', 'Shop', 1, N'Tạo cửa hàng "PMP Racing Tuấn"', '192.168.1.100', 'success', '2025-03-20 10:00:00'),
(3, 'shop_created', 'Shop', 2, N'Tạo cửa hàng "PMP Racing Khoa"', '192.168.1.101', 'success', '2025-03-22 14:00:00'),
(2, 'subscription_purchased', 'Subscription', 1, N'Tuấn mua gói Pro 3 tháng', '192.168.1.100', 'success', '2025-04-01 08:00:00'),
(3, 'subscription_purchased', 'Subscription', 2, N'Khoa mua gói Basic 12 tháng', '192.168.1.101', 'success', '2025-04-01 08:05:00');
GO

/* ============================================================
   23. SHOP_ACTIVITY_LOGS — Per-shop logging
   ============================================================ */
INSERT INTO ShopActivityLogs (ShopId, EmployeeId, ActivityType, EntityType, EntityId, Description, Status, CreatedAt) VALUES
-- Shop 1 activities
(1, 1, 'receipt_created', 'Receipt', 1, N'Lập phiếu sửa cho Wave Alpha', 'success', '2025-03-01 08:30:00'),
(1, 1, 'assignment_created', 'Assignment', 1, N'Gán Tuấn cho phiếu #1', 'success', '2025-03-01 08:35:00'),
(1, 1, 'order_started', 'Order', 1, N'Tuấn bắt đầu công việc', 'success', '2025-03-01 08:40:00'),
(1, 1, 'order_completed', 'Order', 1, N'Tuấn hoàn thành công việc', 'success', '2025-03-01 10:00:00'),
(1, 3, 'payment_received', 'Payment', 1, N'Ngân nhận tiền mặt 165.000 VND', 'success', '2025-03-01 10:05:00'),
(1, 3, 'receipt_created', 'Receipt', 3, N'Lập phiếu nạp bình cho Lead 125', 'success', '2025-04-01 10:00:00'),
(1, 1, 'order_started', 'Order', 3, N'Hùng bắt đầu nạp bình', 'success', '2025-04-01 10:10:00'),
-- Shop 2 activities
(2, 6, 'receipt_created', 'Receipt', 5, N'Lập phiếu thay lốc cho Lead 125', 'success', '2025-03-15 11:20:00'),
(2, 6, 'assignment_created', 'Assignment', 5, N'Gán Bách cho phiếu #5', 'success', '2025-03-15 11:25:00'),
(2, 6, 'order_completed', 'Order', 5, N'Bách hoàn thành thay lốc', 'success', '2025-03-15 13:00:00'),
(2, 7, 'payment_received', 'Payment', 5, N'Hương nhận tiền chuyển khoản 330.000 VND', 'success', '2025-03-15 13:05:00'),
(2, 6, 'receipt_created', 'Receipt', 6, N'Lập phiếu đại tu động cơ cho Raider', 'success', '2025-04-01 09:30:00'),
(2, 6, 'order_started', 'Order', 6, N'Bách bắt đầu đại tu động cơ', 'success', '2025-04-01 09:40:00');
GO

PRINT '✓ PMP Racing Updated Sample Data inserted successfully!';
GO