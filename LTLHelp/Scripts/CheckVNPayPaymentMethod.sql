-- Script để kiểm tra và tạo PaymentMethod VNPay nếu chưa có
-- Chạy script này trong SQL Server Management Studio

-- 1. Kiểm tra PaymentMethods hiện có
SELECT 
    PaymentMethodId,
    Name,
    Code,
    Description,
    IsActive
FROM PaymentMethods;

-- 2. Kiểm tra xem đã có VNPay chưa
SELECT 
    PaymentMethodId,
    Name,
    Code,
    Description,
    IsActive
FROM PaymentMethods
WHERE Code = 'VNPAY' OR Code = 'vnpay' OR Code = 'VnPay';

-- 3. Tạo PaymentMethod VNPay nếu chưa có
-- Lưu ý: Thay đổi PaymentMethodId nếu cần (tránh trùng với ID đã có)
IF NOT EXISTS (SELECT 1 FROM PaymentMethods WHERE Code = 'VNPAY' OR Code = 'vnpay' OR Code = 'VnPay')
BEGIN
    INSERT INTO PaymentMethods (Name, Code, Description, IsActive)
    VALUES ('VNPay', 'VNPAY', 'Thanh toán qua cổng VNPay', 1);
    
    PRINT 'PaymentMethod VNPay đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT 'PaymentMethod VNPay đã tồn tại!';
    
    -- Cập nhật để đảm bảo Code = 'VNPAY' (chữ hoa)
    UPDATE PaymentMethods
    SET Code = 'VNPAY',
        IsActive = 1
    WHERE Code = 'vnpay' OR Code = 'VnPay';
    
    PRINT 'Đã cập nhật Code thành VNPAY (chữ hoa)';
END

-- 4. Kiểm tra lại sau khi tạo/cập nhật
SELECT 
    PaymentMethodId,
    Name,
    Code,
    Description,
    IsActive
FROM PaymentMethods
WHERE Code = 'VNPAY';



