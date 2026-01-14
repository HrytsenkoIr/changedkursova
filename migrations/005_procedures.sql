-- 005_procedures.sql

-- 1. sp_PlaceOrder
IF OBJECT_ID('sp_PlaceOrder', 'P') IS NOT NULL DROP PROCEDURE sp_PlaceOrder;
EXEC('
CREATE PROCEDURE sp_PlaceOrder
    @CustomerID INT,
    @ProductID INT,
    @Amount INT,
    @DeliveryType NVARCHAR(50),
    @NewOrderID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @CurrentStock INT, @UnitPrice DECIMAL(10,2);
    SELECT @CurrentStock = Stock, @UnitPrice = Price FROM Product WHERE ProductID = @ProductID;
    
    IF @CurrentStock < @Amount
    BEGIN
        RAISERROR(''Not enough stock!'', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO Orders (CustomerID, OrderDate, Status)
        VALUES (@CustomerID, SYSUTCDATETIME(), ''Pending'');
        
        SET @NewOrderID = SCOPE_IDENTITY();
        
        INSERT INTO OrderItem (OrderID, ProductID, Amount, Price)
        VALUES (@NewOrderID, @ProductID, @Amount, @UnitPrice);
        
        UPDATE Product SET Stock = Stock - @Amount WHERE ProductID = @ProductID;
        
        INSERT INTO Delivery (OrderID, Type, Cost, Status)
        VALUES (@NewOrderID, @DeliveryType, CASE WHEN @DeliveryType = ''Courier'' THEN 150.00 ELSE 0.00 END, ''Processing'');
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END');

-- 2. sp_CancelOrder
IF OBJECT_ID('sp_CancelOrder', 'P') IS NOT NULL DROP PROCEDURE sp_CancelOrder;
EXEC('
CREATE PROCEDURE sp_CancelOrder @OrderID INT
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE Orders SET Status = ''Cancelled'' WHERE OrderID = @OrderID;
        UPDATE Delivery SET Status = ''Cancelled'' WHERE OrderID = @OrderID;
        UPDATE p SET p.Stock = p.Stock + oi.Amount
        FROM Product p
        INNER JOIN OrderItem oi ON p.ProductID = oi.ProductID
        WHERE oi.OrderID = @OrderID;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END');

-- 3. sp_GetBestSellers
IF OBJECT_ID('sp_GetBestSellers', 'P') IS NOT NULL DROP PROCEDURE sp_GetBestSellers;
EXEC('
CREATE PROCEDURE sp_GetBestSellers @TopCount INT = 5
AS
BEGIN
    SELECT TOP (@TopCount) p.ProductID, p.Name, SUM(oi.Amount) AS TotalSold, SUM(oi.Amount * oi.Price) AS TotalRevenue
    FROM Product p
    INNER JOIN OrderItem oi ON p.ProductID = oi.ProductID
    GROUP BY p.ProductID, p.Name
    ORDER BY TotalSold DESC;
END');