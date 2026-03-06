USE [ISpanShopDB]
GO

SET NOCOUNT ON;

-- =============================================
-- 0. 確保基礎基礎資料存在 (Roles & MembershipLevels)
-- =============================================

-- 檢查並插入 Roles
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [RoleName] = 'Admin')
    INSERT INTO [dbo].[Roles] ([RoleName], [Description]) VALUES ('Admin', N'系統管理員');
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [RoleName] = 'Member')
    INSERT INTO [dbo].[Roles] ([RoleName], [Description]) VALUES ('Member', N'一般會員');
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [RoleName] = 'Seller')
    INSERT INTO [dbo].[Roles] ([RoleName], [Description]) VALUES ('Seller', N'賣家');

DECLARE @MemberRoleId INT = (SELECT TOP 1 Id FROM [dbo].[Roles] WHERE [RoleName] = 'Member');
DECLARE @SellerRoleId INT = (SELECT TOP 1 Id FROM [dbo].[Roles] WHERE [RoleName] = 'Seller');

-- 如果還是抓不到 RoleId (防呆)，就抓現有的第一個
IF @MemberRoleId IS NULL SET @MemberRoleId = (SELECT TOP 1 Id FROM [dbo].[Roles]);
IF @SellerRoleId IS NULL SET @SellerRoleId = (SELECT TOP 1 Id FROM [dbo].[Roles]);

-- 檢查並插入 MembershipLevels
IF NOT EXISTS (SELECT 1 FROM [dbo].[MembershipLevels])
    INSERT INTO [dbo].[MembershipLevels] ([LevelName], [MinSpending], [DiscountRate]) VALUES (N'一般會員', 0, 1.00);

DECLARE @DefaultLevelId INT = (SELECT TOP 1 Id FROM [dbo].[MembershipLevels]);

-- =============================================
-- 1. 生成 30 個假會員與賣家資料
-- =============================================
DECLARE @i INT = 1;
DECLARE @NewUserId INT;

WHILE @i <= 30
BEGIN
    DECLARE @IsSeller BIT = CASE WHEN @i % 2 = 0 THEN 1 ELSE 0 END;
    DECLARE @CurrentRoleId INT = CASE WHEN @IsSeller = 1 THEN @SellerRoleId ELSE @MemberRoleId END;
    DECLARE @Account VARCHAR(50) = 'seed_user_' + CAST(RAND(CHECKSUM(NEWID())) * 100000 AS VARCHAR(10));
    
    INSERT INTO [dbo].[Users] ([RoleId], [Account], [Password], [Email], [IsConfirmed], [IsBlacklisted], [CreatedAt], [IsSeller])
    VALUES (@CurrentRoleId, @Account, 'password123', @Account + '@example.com', 1, 0, DATEADD(DAY, -400, GETDATE()), @IsSeller);
    
    SET @NewUserId = SCOPE_IDENTITY();

    IF @NewUserId IS NOT NULL
    BEGIN
        -- 為每個 User 建立 MemberProfile
        INSERT INTO [dbo].[MemberProfiles] ([UserId], [LevelId], [FullName], [Gender], [DateOfBirth], [PhoneNumber], [TotalSpending], [PointBalance], [UpdatedAt])
        VALUES (@NewUserId, @DefaultLevelId, N'測試人' + CAST(@i AS NVARCHAR(5)), @i % 2 + 1, '1995-05-20', '09' + RIGHT('00000000' + CAST(CAST(RAND(CHECKSUM(NEWID())) * 100000000 AS INT) AS VARCHAR(10)), 8), 0, 100, GETDATE());

        -- 為賣家建立 Store
        IF @IsSeller = 1
        BEGIN
            INSERT INTO [dbo].[Stores] ([UserId], [StoreName], [Description], [IsVerified], [CreatedAt])
            VALUES (@NewUserId, N'測試商場_' + CAST(@i AS NVARCHAR(5)), N'這是一個自動生成的測試賣場。', 1, DATEADD(DAY, -390, GETDATE()));
        END
    END

    SET @i = @i + 1;
END

-- =============================================
-- 2. 生成 300 筆隨機訂單資料 (跨度一年)
-- =============================================
DECLARE @OrderCount INT = 1;
DECLARE @TotalOrderTarget INT = 300;

-- 獲取所有可用的買家與商店 ID
DECLARE @BuyerIds TABLE (Id INT);
INSERT INTO @BuyerIds SELECT Id FROM Users WHERE IsSeller = 0;

DECLARE @StoreIds TABLE (Id INT);
INSERT INTO @StoreIds SELECT Id FROM Stores;

-- 檢查是否有商品可用，若無商品則無法生成訂單明細
IF NOT EXISTS (SELECT 1 FROM Products) OR NOT EXISTS (SELECT 1 FROM ProductVariants)
BEGIN
    PRINT 'Error: No Products or Variants found in database. Please run product scripts first.';
    RETURN;
END

WHILE @OrderCount <= @TotalOrderTarget
BEGIN
    DECLARE @RandomBuyer INT = (SELECT TOP 1 Id FROM @BuyerIds ORDER BY NEWID());
    DECLARE @RandomStore INT = (SELECT TOP 1 Id FROM @StoreIds ORDER BY NEWID());
    
    -- 隨機日期：今天往前推 365 天
    DECLARE @OrderDate DATETIME = DATEADD(SECOND, ABS(CHECKSUM(NEWID()) % 86400), DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 365), GETDATE()));
    DECLARE @Status TINYINT = ABS(CHECKSUM(NEWID()) % 5); -- 0:Pending, 1:Processing, 2:Shipped, 3:Completed, 4:Cancelled
    DECLARE @OrderNo VARCHAR(30) = 'ORD' + CONVERT(VARCHAR(8), @OrderDate, 112) + RIGHT('00000' + CAST(@OrderCount AS VARCHAR(10)), 6);
    
    DECLARE @PaymentDate DATETIME = CASE WHEN @Status >= 1 THEN DATEADD(MINUTE, ABS(CHECKSUM(NEWID()) % 1440), @OrderDate) ELSE NULL END;
    DECLARE @CompletedAt DATETIME = CASE WHEN @Status = 3 THEN DATEADD(DAY, ABS(CHECKSUM(NEWID()) % 5) + 1, @PaymentDate) ELSE NULL END;

    -- 插入訂單主檔
    INSERT INTO [dbo].[Orders] 
    ([OrderNumber], [UserId], [StoreId], [TotalAmount], [ShippingFee], [FinalAmount], [Status], [CreatedAt], [PaymentDate], [CompletedAt], [RecipientName], [RecipientPhone], [RecipientAddress])
    VALUES 
    (@OrderNo, @RandomBuyer, @RandomStore, 0, 60, 0, @Status, @OrderDate, @PaymentDate, @CompletedAt, N'測試收件人' + CAST(@OrderCount AS NVARCHAR(5)), '09' + RIGHT('00000000' + CAST(CAST(RAND(CHECKSUM(NEWID())) * 100000000 AS INT) AS VARCHAR(10)), 8), N'台灣某個城市的測試路' + CAST(@OrderCount AS NVARCHAR(5)) + N'號');

    DECLARE @CurrentOrderId BIGINT = SCOPE_IDENTITY();

    IF @CurrentOrderId IS NOT NULL
    BEGIN
        -- 為每筆訂單生成 1~3 筆明細
        DECLARE @DetailIdx INT = 1;
        DECLARE @DetailLimit INT = (ABS(CHECKSUM(NEWID()) % 3) + 1);
        DECLARE @OrderSum DECIMAL(18,2) = 0;

        WHILE @DetailIdx <= @DetailLimit
        BEGIN
            DECLARE @P_Id INT, @V_Id INT, @P_Name NVARCHAR(200), @V_Name NVARCHAR(100), @V_Sku NVARCHAR(100), @V_Price DECIMAL(18,2);
            
            -- 優先找該商店的產品
            SELECT TOP 1 @P_Id = p.Id, @V_Id = v.Id, @P_Name = p.Name, @V_Name = v.VariantName, @V_Sku = v.SkuCode, @V_Price = v.Price
            FROM [dbo].[Products] p
            JOIN [dbo].[ProductVariants] v ON p.Id = v.ProductId
            WHERE p.StoreId = @RandomStore
            ORDER BY NEWID();

            -- 如果該店沒產品，隨機抓全站產品 (模擬跨店或數據補齊)
            IF @P_Id IS NULL
            BEGIN
                SELECT TOP 1 @P_Id = p.Id, @V_Id = v.Id, @P_Name = p.Name, @V_Name = v.VariantName, @V_Sku = v.SkuCode, @V_Price = v.Price
                FROM [dbo].[Products] p
                JOIN [dbo].[ProductVariants] v ON p.Id = v.ProductId
                ORDER BY NEWID();
            END

            IF @P_Id IS NOT NULL
            BEGIN
                DECLARE @Q INT = (ABS(CHECKSUM(NEWID()) % 3) + 1);

                INSERT INTO [dbo].[OrderDetails] ([OrderId], [ProductId], [VariantId], [ProductName], [VariantName], [SkuCode], [Price], [Quantity])
                VALUES (@CurrentOrderId, @P_Id, @V_Id, @P_Name, @V_Name, @V_Sku, @V_Price, @Q);

                SET @OrderSum = @OrderSum + (@V_Price * @Q);
            END
            SET @DetailIdx = @DetailIdx + 1;
        END

        -- 更新訂單總額
        UPDATE [dbo].[Orders] 
        SET [TotalAmount] = @OrderSum, 
            [FinalAmount] = @OrderSum + ISNULL([ShippingFee], 0)
        WHERE [Id] = @CurrentOrderId;
    END

    SET @OrderCount = @OrderCount + 1;
END

PRINT 'Seed data generation completed successfully: 30 Users/Profiles/Stores and 300 Orders added.';
GO
