USE OnlineStore;
GO

-- 1. Prevent deletion of customers with existing orders
IF OBJECT_ID('trg_PreventDeleteCustomerWithOrders', 'TR') IS NOT NULL
    DROP TRIGGER trg_PreventDeleteCustomerWithOrders;
GO

EXEC('
CREATE TRIGGER trg_PreventDeleteCustomerWithOrders 
ON Customer 
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 
        FROM Orders 
        WHERE CustomerID IN (SELECT CustomerID FROM deleted)
    )
    BEGIN
        RAISERROR(''Cannot delete a customer with existing orders!'', 16, 1);
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        RETURN;
    END

    DELETE FROM Customer
    WHERE CustomerID IN (SELECT CustomerID FROM deleted);
END');
GO

-- 2. Decrement product stock after inserting order items
IF OBJECT_ID('trg_DecrementStock', 'TR') IS NOT NULL
    DROP TRIGGER trg_DecrementStock;
GO

EXEC('
CREATE TRIGGER trg_DecrementStock
ON OrderItem
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.Stock = p.Stock - i.Amount
    FROM Product p
    INNER JOIN inserted i ON p.ProductID = i.ProductID;
END');
GO

-- 3. Set delivery status to "Customs Clearance" if item price exceeds 1000
IF OBJECT_ID('trg_LuxuryCustomsCheck', 'TR') IS NOT NULL
    DROP TRIGGER trg_LuxuryCustomsCheck;
GO

EXEC('
CREATE TRIGGER trg_LuxuryCustomsCheck
ON OrderItem
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE d
    SET d.Status = ''Customs Clearance''
    FROM Delivery d
    INNER JOIN inserted i ON d.OrderID = i.OrderID
    WHERE i.Price > 1000;
END');
GO

-- 4. Automatically create a payment after inserting order items
IF OBJECT_ID('trg_AutoCreatePayment', 'TR') IS NOT NULL
    DROP TRIGGER trg_AutoCreatePayment;
GO

EXEC('
CREATE TRIGGER trg_AutoCreatePayment
ON OrderItem
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Payment (OrderID, Amount, PaymentDate, Method)
    SELECT
        i.OrderID,
        i.Amount * i.Price,
        SYSUTCDATETIME(),
        ''Card''
    FROM inserted i;
END');
GO
