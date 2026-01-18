IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Customer_Email' 
      AND object_id = OBJECT_ID('Customer')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Customer_Email ON Customer(Email);
END

IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Product_CategoryID' 
      AND object_id = OBJECT_ID('Product')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Product_CategoryID ON Product(CategoryID);
END

IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Product_Name' 
      AND object_id = OBJECT_ID('Product')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Product_Name ON Product(Name);
END

IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Orders_CustomerID' 
      AND object_id = OBJECT_ID('Orders')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Orders_CustomerID ON Orders(CustomerID);
END

IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Orders_OrderDate' 
      AND object_id = OBJECT_ID('Orders')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Orders_OrderDate ON Orders(OrderDate DESC);
END

IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_OrderItem_OrderID' 
      AND object_id = OBJECT_ID('OrderItem')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_OrderItem_OrderID ON OrderItem(OrderID);
END

IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_OrderItem_ProductID' 
      AND object_id = OBJECT_ID('OrderItem')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_OrderItem_ProductID ON OrderItem(ProductID);
END

IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Payment_OrderID' 
      AND object_id = OBJECT_ID('Payment')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Payment_OrderID ON Payment(OrderID);
END

IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Delivery_OrderID' 
      AND object_id = OBJECT_ID('Delivery')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Delivery_OrderID ON Delivery(OrderID);
END
