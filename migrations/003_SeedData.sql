-- 003_SeedData.sql
-- Seed ONLY if database is empty (safe for real project)

IF EXISTS (SELECT 1 FROM Category)
BEGIN
    PRINT 'Seed data already exists. Skipping 003_SeedData.sql';
    RETURN;
END

-----------------------------------------------------------
-- КАТЕГОРІЇ
-----------------------------------------------------------
SET IDENTITY_INSERT Category ON;
INSERT INTO Category (CategoryID, Name) VALUES
(1, 'Magic Potions'), (2, 'Herbs & Ingredients'), (3, 'Crystals & Stones'),
(4, 'Amulets & Talismans'), (5, 'Candles & Incense'), (6, 'Spell Books'),
(7, 'Divination Tools'), (8, 'Ritual Supplies'), (9, 'Witch Apparel'),
(10, 'Decor & Symbols'), (11, 'Cursed items'), (12, 'Rare Artifacts'),
(13, 'Herbal Manuals'), (14, 'Ritual Guides');
SET IDENTITY_INSERT Category OFF;

-----------------------------------------------------------
-- КЛІЄНТИ
-----------------------------------------------------------
SET IDENTITY_INSERT Customer ON;
INSERT INTO Customer (CustomerID, Name, Email, Phone, Address) VALUES
(1, 'Alexander Ivanov', 'ivanov@gmail.com', '380671234567', 'Kyiv, Khreshchatyk St, 10'),
(2, 'Maria Petrenko', 'petrenko.m@gmail.com', '380931112233', 'Lviv, Doroshenka St, 5'),
(3, 'Ihor Kovalenko', 'kovalenko.igor@gmail.com', '380501234987', 'Kharkiv, Nauky Ave, 23'),
(4, 'Olena Shevchenko', 'shevchenko.olena@gmail.com', '380971234555', 'Dnipro, Polya St, 12'),
(5, 'Serhiy Bondar', 'bondar.sergiy@gmail.com', '380631234789', 'Odesa, Deribasivska St, 1'),
(6, 'Tetiana Melnyk', 'melnyk.tanya@gmail.com', '380991234111', 'Zaporizhzhia, Soborna St, 45'),
(7, 'Volodymyr Hnatyuk', 'hnatyuk.v@gmail.com', '380671111222', 'Chernivtsi, Holovna St, 100'),
(8, 'Natalia Romanyuk', 'romanyuk.n@gmail.com', '380933331234', 'Vinnytsia, Keletska St, 34'),
(9, 'Dmytro Sydorenko', 'sydorenko.d@gmail.com', '380501231231', 'Poltava, Yevropeiska St, 15'),
(10, 'Inna Lysenko', 'lysenko.inna@gmail.com', '380671212121', 'Kropyvnytskyi, Perspektyvna St, 8'),
(11, 'Andrii Marchenko', 'marchenko.andrii@gmail.com', '380631239999', 'Sumy, Kharkivska St, 22'),
(12, 'Larysa Danylko', 'larysa.danylko@gmail.com', '380991117777', 'Chernihiv, Myru Ave, 101'),
(13, 'Petro Kravets', 'kravets.petro@gmail.com', '380501235555', 'Rivne, Soborna St, 9'),
(14, 'Yulia Tkachenko', 'tkachenko.yulia@gmail.com', '380671236666', 'Lutsk, Lesi Ukrainky St, 3'),
(15, 'Viktor Andrusyak', 'andrusyak.v@gmail.com', '380931110000', 'Ivano-Frankivsk, Nezalezhnosti St, 18'),
(16, 'Iryna Kravchuk', 'iryna.kravchuk@gmail.com', '380671234321', 'Zhytomyr, Kyivska St, 24'),
(17, 'Kateryna Polischuk', 'katya.polischuk@gmail.com', '380991231010', 'Ternopil, Ruska St, 12'),
(18, 'Oleksandr Veres', 'veres.o@gmail.com', '380501239111', 'Uzhhorod, Shandora Petefi Sq, 4'),
(19, 'Halyna Boiko', 'boiko.halyna@gmail.com', '380931234444', 'Cherkasy, Shevchenka Blvd, 99'),
(20, 'Roman Doroshenko', 'roman.doro@gmail.com', '380631234555', 'Mykolaiv, Central Ave, 76');
SET IDENTITY_INSERT Customer OFF;

-----------------------------------------------------------
-- ПРОДУКТИ
-----------------------------------------------------------
SET IDENTITY_INSERT Product ON;
INSERT INTO Product (ProductID, Name, Description, Price, Stock, CategoryID) VALUES
(1, 'Love Elixir', 'Potion to make someone love you.', 499.00, 25, 1),
(2, 'Nightshade Essence', 'Potion for protection or to scare bad spirits.', 899.00, 10, 1),
(3, 'Dried Mandrake Root', 'Root that makes magic stronger.', 699.00, 15, 2),
(4, 'Moonstone Crystal', 'Crystal for better dreams and intuition.', 299.00, 30, 3),
(5, 'Protection Amulet', 'Talisman to stop bad things from happening to you.', 399.00, 20, 4),
(6, 'Black Candle Set', 'Candles for dark rituals and banishing.', 249.00, 50, 5),
(7, 'Book of Shadows', 'Notebook to write your spells.', 599.00, 12, 6),
(8, 'Tarot Deck: The Witch’s Path', 'Deck for fortune telling and advice.', 899.00, 8, 7),
(9, 'Silver Ritual Dagger', 'Dagger to use in magic ceremonies.', 1499.00, 5, 8),
(10, 'Witch’s Cloak', 'Cloak with runes to protect you.', 1999.00, 7, 9),
(11, 'Healing Potion', 'Potion that heals you quickly.', 599.00, 20, 1),
(12, 'Dragonroot', 'Herb that helps make stronger magic.', 799.00, 10, 2),
(13, 'Amethyst Crystal', 'Crystal for spiritual energy.', 399.00, 25, 3),
(14, 'Talisman of Luck', 'Pendant to bring you luck.', 499.00, 15, 4),
(15, 'White Candle Set', 'Candles for blessings and protection.', 199.00, 40, 5),
(16, 'Oracle Deck', 'Cards to see the future a little.', 899.00, 7, 7),
(17, 'Ritual Knife', 'Knife to use in magic rituals.', 999.00, 6, 8),
(18, 'Cursed Ring', 'Ring that brings bad luck to those who wear it.', 1299.00, 3, 11),
(19, 'Spell Book Advanced', 'Book with harder spells for practice.', 699.00, 10, 6),
(20, 'Witch Hat', 'Hat that helps you focus in magic.', 299.00, 12, 9);
SET IDENTITY_INSERT Product OFF;

-----------------------------------------------------------
-- ЗАМОВЛЕННЯ
-----------------------------------------------------------
SET IDENTITY_INSERT Orders ON;
INSERT INTO Orders (OrderID, CustomerID, OrderDate, Status) VALUES
(1, 1, GETDATE(), 'Payment on Delivery'),
(2, 2, GETDATE(), 'Paid'),
(3, 3, GETDATE(), 'Cancelled'),
(4, 4, GETDATE(), 'Paid'),
(5, 5, GETDATE(), 'Payment on Delivery'),
(6, 6, GETDATE(), 'Paid'),
(7, 7, GETDATE(), 'Paid'),
(8, 8, GETDATE(), 'Pending Payment'),
(9, 9, GETDATE(), 'Paid'),
(10, 10, GETDATE(), 'Pending Payment'),
(11, 1, GETDATE(), 'Payment on Delivery'),
(12, 1, GETDATE(), 'Paid'),
(13, 2, GETDATE(), 'Paid'),
(14, 3, GETDATE(), 'Cancelled'),
(15, 4, GETDATE(), 'Paid'),
(16, 5, GETDATE(), 'Paid'),
(17, 6, GETDATE(), 'Payment on Delivery'),
(18, 7, GETDATE(), 'Paid'),
(19, 8, GETDATE(), 'Pending Payment'),
(20, 9, GETDATE(), 'Refunded');
SET IDENTITY_INSERT Orders OFF;

-----------------------------------------------------------
-- ДОСТАВКА
-----------------------------------------------------------
INSERT INTO Delivery (OrderID, Type, Cost, Status) VALUES
(1, 'Courier', 150.00, 'Pending Shipment'),
(2, 'Pickup', 0.00, 'Delivered'),
(3, 'Courier', 150.00, 'Cancelled'),
(4, 'Courier', 150.00, 'In Transit'),
(5, 'Courier', 150.00, 'In Transit'),
(6, 'Courier', 150.00, 'Delivered'),
(7, 'Pickup', 0.00, 'Ready for Pickup'),
(8, 'Courier', 150.00, 'Pending Shipment'),
(9, 'Courier', 150.00, 'Customs Clearance'),
(10, 'Pickup', 0.00, 'Pending Shipment'),
(11, 'Pickup', 150.00, 'Ready for Pickup'),
(12, 'Pickup', 0.00, 'Delivered'),
(13, 'Courier', 150.00, 'Pending Shipment'),
(14, 'Courier', 150.00, 'Cancelled'),
(15, 'Pickup', 0.00, 'Customs Clearance'),
(16, 'Courier', 150.00, 'In Transit'),
(17, 'Courier', 150.00, 'Delivered'),
(18, 'Pickup', 0.00, 'Customs Clearance'),
(19, 'Courier', 150.00, 'Pending Shipment'),
(20, 'Courier', 150.00, 'Returned');

-----------------------------------------------------------
-- ЗАМОВЛЕНІ ТОВАРИ
-----------------------------------------------------------
INSERT INTO OrderItem (OrderID, ProductID, Amount, Price) VALUES
(1, 1, 1, 499.00), (1, 7, 1, 599.00), (2, 2, 1, 899.00), (3, 3, 1, 699.00),
(4, 5, 1, 399.00), (5, 9, 1, 1499.00), (6, 6, 1, 249.00), (7, 10, 1, 1999.00),
(8, 8, 2, 1798.00), (9, 4, 1, 299.00), (11, 1, 2, 998.00), (11, 3, 1, 699.00),
(12, 2, 3, 2697.00), (12, 5, 1, 399.00), (13, 4, 2, 598.00), (14, 7, 1, 599.00),
(15, 8, 1, 899.00), (16, 9, 1, 1499.00), (17, 10, 2, 3998.00), (18, 6, 1, 249.00),
(19, 3, 1, 699.00), (20, 5, 2, 798.00);

-----------------------------------------------------------
-- ОПЛАТИ
-----------------------------------------------------------
WITH PaymentData AS (
    SELECT o.OrderID,
    CASE o.OrderID
        WHEN 1 THEN 'Apple Pay' WHEN 2 THEN 'Cash' WHEN 3 THEN 'Card'
        WHEN 4 THEN 'Card' WHEN 5 THEN 'Card' WHEN 6 THEN 'Google Pay'
        WHEN 7 THEN 'Cash' WHEN 8 THEN 'Apple Pay' WHEN 9 THEN 'Card'
        WHEN 10 THEN 'Card' WHEN 11 THEN 'Card' WHEN 12 THEN 'Cash'
        WHEN 13 THEN 'Apple Pay' WHEN 14 THEN 'Google Pay' WHEN 15 THEN 'Card'
        WHEN 16 THEN 'Card' WHEN 17 THEN 'Cash' WHEN 18 THEN 'Google Pay'
        WHEN 19 THEN 'Card' WHEN 20 THEN 'Card'
    END AS Method
    FROM Orders o
)
INSERT INTO Payment (OrderID, Amount, PaymentDate, Method)
SELECT pd.OrderID, SUM(oi.Price * oi.Amount), GETDATE(), pd.Method
FROM PaymentData pd
JOIN OrderItem oi ON oi.OrderID = pd.OrderID
GROUP BY pd.OrderID, pd.Method;
