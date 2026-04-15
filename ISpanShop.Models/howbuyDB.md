# howbuyDB — EfModels 資料庫結構（展開欄位與關聯）

說明:
- 每個實體對應一個資料表；Id 或命名為 Id 的屬性視為主鍵。
- 下列欄位格式：欄位名 : 類型 [可否為 NULL]
- 關聯使用箭頭標示（→）：單向或集合導向關聯。

EfModels/

---
Brand
- Id : int [PK]
- Name : string [NOT NULL]
- Description : string [NOT NULL]
- LogoUrl : string [NOT NULL]
- Sort : int? [NULL]
- IsVisible : bool? [NULL]
- IsDeleted : bool? [NULL]
關聯：
- Products → Product (1-to-many, Product.BrandId)

---
CategoryAttribute
- Id : int [PK]
- Name : string [NOT NULL]
- InputType : string [NOT NULL]
- IsRequired : bool [NOT NULL]
- IsActive : bool [NOT NULL]
- AllowCustomInput : bool [NOT NULL]
關聯：
- CategoryAttributeMappings → CategoryAttributeMapping (1-to-many)
- CategoryAttributeOptions → CategoryAttributeOption (1-to-many)

---
AdminLevel
- Id : int [PK]
- LevelName : string [NOT NULL]
- Description : string [NOT NULL]
關聯：
- Users → User (1-to-many, User.AdminLevelId)
- Permissions → Permission (many-to-many via join in DbContext)

---
Address
- Id : int [PK]
- UserId : int [FK -> User.Id]
- RecipientName : string [NOT NULL]
- RecipientPhone : string [NOT NULL]
- City : string [NOT NULL]
- Region : string [NOT NULL]
- Street : string [NOT NULL]
- IsDefault : bool? [NULL]
關聯：
- User ← User (many-to-1)

---
BlacklistRecord
- Id : int [PK]
- BlockedUserId : int [FK -> User.Id]
- AdminUserId : int [FK -> User.Id]
- Reason : string [NOT NULL]
- CreatedAt : DateTime? [NULL]
- UnblockAt : DateTime? [NULL]
關聯：
- AdminUser → User
- BlockedUser → User

---
Cart
- Id : int [PK]
- UserId : int [FK -> User.Id]
關聯：
- CartItems → CartItem (1-to-many)
- User ← User

---
CartItem
- Id : int [PK]
- CartId : int [FK -> Cart.Id]
- StoreId : int [FK -> Store.Id]
- ProductId : int [FK -> Product.Id]
- VariantId : int [FK -> ProductVariant.Id]
- Quantity : int [NOT NULL]
- UnitPrice : decimal? [NULL]
關聯：
- Cart ← Cart
- Product ← Product
- Store ← Store
- Variant ← ProductVariant

---
Category
- Id : int [PK]
- Name : string [NOT NULL]
- ParentId : int? [FK -> Category.Id]
- IconUrl : string [NOT NULL]
- Sort : int? [NULL]
- IsVisible : bool? [NULL]
- Icon : string [NOT NULL]
- NameEn : string [NOT NULL]
關聯：
- Products → Product (1-to-many)
- Parent ← Category (self-referencing)
- InverseParent → Category (children collection)
- CategoryAttributeMappings → CategoryAttributeMapping (1-to-many)

---
CategoryAttributeOption
- Id : int [PK]
- CategoryAttributeId : int [FK -> CategoryAttribute.Id]
- OptionName : string [NOT NULL]
- SortOrder : int [NOT NULL]
關聯：
- CategoryAttribute ← CategoryAttribute

---
CategoryAttributeMapping
- CategoryId : int [FK -> Category.Id]
- CategoryAttributeId : int [FK -> CategoryAttribute.Id]
- IsFilterable : bool [NOT NULL]
- Sort : int [NOT NULL]
關聯：
- Category ← Category
- CategoryAttribute ← CategoryAttribute

---
ChatMessage
- Id : long [PK]
- SessionId : Guid [NOT NULL]
- SenderId : int [FK -> User.Id]
- ReceiverId : int [FK -> User.Id]
- Content : string [NOT NULL]
- Type : byte [NOT NULL]
- IsRead : bool? [NULL]
- SentAt : DateTime? [NULL]
關聯：
- Sender → User
- Receiver → User

---
LoginHistory
- Id : int [PK]
- UserId : int? [FK -> User.Id]
- AttemptedAccount : string [NOT NULL]
- Ipaddress : string [NOT NULL]
- LoginTime : DateTime [NOT NULL]
- IsSuccess : bool [NOT NULL]
關聯：
- User ← User

---
MemberProfile
- Id : int [PK]
- UserId : int [FK -> User.Id]
- LevelId : int [FK -> MembershipLevel.Id]
- FullName : string [NOT NULL]
- Gender : byte? [NULL]
- DateOfBirth : DateOnly? [NULL]
- PhoneNumber : string [NOT NULL]
- TotalSpending : decimal? [NULL]
- PointBalance : int? [NULL]
- EmailNotification : bool? [NULL]
- UpdatedAt : DateTime? [NULL]
- IsSeller : bool [NOT NULL]
關聯：
- Level → MembershipLevel
- User ← User

---
MembershipLevel
- Id : int [PK]
- LevelName : string [NOT NULL]
- MinSpending : decimal [NOT NULL]
- DiscountRate : decimal [NOT NULL]
關聯：
- MemberProfiles → MemberProfile (1-to-many)

---
Order
- Id : long [PK]
- OrderNumber : string [NOT NULL]
- UserId : int [FK -> User.Id]
- StoreId : int [FK -> Store.Id]
- TotalAmount : decimal [NOT NULL]
- ShippingFee : decimal? [NULL]
- PointDiscount : int? [NULL]
- DiscountAmount : decimal? [NULL]
- FinalAmount : decimal [NOT NULL]
- Status : byte? [NULL]
- PaymentDate : DateTime? [NULL]
- RecipientName : string [NOT NULL]
- RecipientPhone : string [NOT NULL]
- RecipientAddress : string [NOT NULL]
- Note : string [NOT NULL]
- CreatedAt : DateTime? [NULL]
- CompletedAt : DateTime? [NULL]
關聯：
- OrderDetails → OrderDetail (1-to-many)
- OrderReviews → OrderReview (1-to-many)
- PaymentLogs → PaymentLog (1-to-many)
- ReturnRequests → ReturnRequest (1-to-many)
- SupportTickets → SupportTicket (1-to-many)
- Store ← Store
- User ← User

---
OrderDetail
- Id : long [PK]
- OrderId : long [FK -> Order.Id]
- ProductId : int [FK -> Product.Id]
- VariantId : int? [FK -> ProductVariant.Id]
- ProductName : string [NOT NULL]
- VariantName : string [NOT NULL]
- SkuCode : string [NOT NULL]
- CoverImage : string [NOT NULL]
- Price : decimal? [NULL]
- Quantity : int [NOT NULL]
關聯：
- Order ← Order
- Product ← Product
- Variant ← ProductVariant

---
OrderReview
- Id : int [PK]
- UserId : int [FK -> User.Id]
- OrderId : long [FK -> Order.Id]
- Rating : byte [NOT NULL]
- Comment : string [NOT NULL]
- StoreReply : string [NOT NULL]
- IsHidden : bool? [NULL]
- CreatedAt : DateTime? [NULL]
關聯：
- Order ← Order
- User ← User
- ReviewImages → ReviewImage (1-to-many)

---
PaymentLog
- Id : long [PK]
- OrderId : long [FK -> Order.Id]
- MerchantTradeNo : string [NOT NULL]
- TradeNo : string [NOT NULL]
- RtnCode : int? [NULL]
- RtnMsg : string [NOT NULL]
- TradeAmt : decimal? [NULL]
- PaymentType : string [NOT NULL]
- PaymentDate : DateTime? [NULL]
- CreatedAt : DateTime? [NULL]
關聯：
- Order ← Order

---
PointHistory
- Id : long [PK]
- UserId : int [FK -> User.Id]
- OrderNumber : string [NOT NULL]
- ChangeAmount : int [NOT NULL]
- BalanceAfter : int [NOT NULL]
- Description : string [NOT NULL]
- CreatedAt : DateTime? [NULL]
關聯：
- User ← User

---
Permission
- Id : int [PK]
- PermissionKey : string [NOT NULL]
- DisplayName : string [NOT NULL]
- Description : string [NOT NULL]
關聯：
- AdminLevels → AdminLevel (many-to-many, via DbContext configuration)

---
Promotion
- Id : int [PK]
- Name : string [NOT NULL]
- Description : string [NOT NULL]
- PromotionType : int [NOT NULL]
- StartTime : DateTime [NOT NULL]
- EndTime : DateTime [NOT NULL]
- Status : int [NOT NULL]
- SellerId : int [FK -> User.Id]
- ReviewedBy : int? [FK -> User.Id]
- ReviewedAt : DateTime? [NULL]
- RejectReason : string [NOT NULL]
- CreatedAt : DateTime [NOT NULL]
- UpdatedAt : DateTime? [NULL]
- IsDeleted : bool [NOT NULL]
關聯：
- PromotionItems → PromotionItem (1-to-many)
- PromotionRules → PromotionRule (1-to-many)
- Seller → User
- ReviewedByNavigation → User

---
PromotionItem
- Id : int [PK]
- PromotionId : int [FK -> Promotion.Id]
- ProductId : int [FK -> Product.Id]
- OriginalPrice : decimal [NOT NULL]
- DiscountPrice : decimal? [NULL]
- DiscountPercent : int? [NULL]
- QuantityLimit : int? [NULL]
- StockLimit : int? [NULL]
- SoldCount : int [NOT NULL]
關聯：
- Product ← Product
- Promotion ← Promotion

---
PromotionRule
- Id : int [PK]
- PromotionId : int [FK -> Promotion.Id]
- RuleType : int [NOT NULL]
- Threshold : decimal [NOT NULL]
- DiscountType : int [NOT NULL]
- DiscountValue : decimal [NOT NULL]
關聯：
- Promotion ← Promotion

---
Product
- Id : int [PK]
- StoreId : int [FK -> Store.Id]
- CategoryId : int [FK -> Category.Id]
- BrandId : int? [FK -> Brand.Id]
- Name : string [NOT NULL]
- Description : string [NOT NULL]
- VideoUrl : string [NOT NULL]
- SpecDefinitionJson : string [NOT NULL]
- MinPrice : decimal? [NULL]
- MaxPrice : decimal? [NULL]
- TotalSales : int? [NULL]
- ViewCount : int? [NULL]
- Status : byte? [NULL]
- CreatedAt : DateTime? [NULL]
- UpdatedAt : DateTime? [NULL]
- RejectReason : string [NOT NULL]
- IsDeleted : bool [NOT NULL]
- ReviewStatus : int [NOT NULL]
- ReviewedBy : string [NOT NULL]
- ReviewDate : DateTime? [NULL]
- ForceOffShelfReason : string [NOT NULL]
- ForceOffShelfDate : DateTime? [NULL]
- ForceOffShelfBy : int? [FK -> User.Id]
- ReApplyDate : DateTime? [NULL]
關聯：
- Brand ← Brand
- CartItems → CartItem (1-to-many)
- Category ← Category
- ForceOffShelfByNavigation → User
- OrderDetails → OrderDetail (1-to-many)
- ProductImages → ProductImage (1-to-many)
- ProductVariants → ProductVariant (1-to-many)
- PromotionItems → PromotionItem (1-to-many)
- Store ← Store

---
ProductImage
- Id : int [PK]
- ProductId : int [FK -> Product.Id]
- VariantId : int? [FK -> ProductVariant.Id]
- ImageUrl : string [NOT NULL]
- IsMain : bool? [NULL]
- SortOrder : int? [NULL]
關聯：
- Product ← Product
- Variant ← ProductVariant

---
ProductVariant
- Id : int [PK]
- ProductId : int [FK -> Product.Id]
- SkuCode : string [NOT NULL]
- VariantName : string [NOT NULL]
- SpecValueJson : string [NOT NULL]
- Price : decimal [NOT NULL]
- Stock : int? [NULL]
- SafetyStock : int? [NULL]
- IsDeleted : bool? [NULL]
關聯：
- CartItems → CartItem (1-to-many)
- OrderDetails → OrderDetail (1-to-many)
- Product ← Product
- ProductImages → ProductImage (1-to-many)

---
ReviewImage
- Id : int [PK]
- ReviewId : int [FK -> OrderReview.Id]
- ImageUrl : string [NOT NULL]
關聯：
- Review ← OrderReview

---
ReturnRequest
- Id : long [PK]
- OrderId : long [FK -> Order.Id]
- ReasonCategory : string [NOT NULL]
- ReasonDescription : string [NOT NULL]
- RefundAmount : decimal [NOT NULL]
- Status : byte [NOT NULL]
- AdminRemark : string [NOT NULL]
- CreatedAt : DateTime [NOT NULL]
- UpdatedAt : DateTime? [NULL]
關聯：
- Order ← Order
- ReturnRequestImages → ReturnRequestImage (1-to-many)

---
ReturnRequestImage
- Id : long [PK]
- ReturnRequestId : long [FK -> ReturnRequest.Id]
- ImageUrl : string [NOT NULL]
- CreatedAt : DateTime [NOT NULL]
關聯：
- ReturnRequest ← ReturnRequest

---
Role
- Id : int [PK]
- RoleName : string [NOT NULL]
- Description : string [NOT NULL]
關聯：
- Users → User (1-to-many)

---
SensitiveWordCategory
- Id : int [PK]
- Name : string [NOT NULL]
關聯：
- SensitiveWords → SensitiveWord (1-to-many)

---
SensitiveWord
- Id : int [PK]
- Word : string [NOT NULL]
- Category : string [NOT NULL]
- IsActive : bool? [NULL]
- CreatedTime : DateTime? [NULL]
- CategoryId : int? [FK -> SensitiveWordCategory.Id]
關聯：
- CategoryNavigation → SensitiveWordCategory

---
Store
- Id : int [PK]
- UserId : int [FK -> User.Id]
- StoreName : string [NOT NULL]
- Description : string [NOT NULL]
- IsVerified : bool? [NULL]
- CreatedAt : DateTime? [NULL]
- StoreStatus : byte [NOT NULL]
關聯：
- CartItems → CartItem (1-to-many)
- Orders → Order (1-to-many)
- Products → Product (1-to-many)
- User ← User

---
User
- Id : int [PK]
- RoleId : int [FK -> Role.Id]
- Account : string [NOT NULL]
- Password : string [NOT NULL]
- Email : string [NOT NULL]
- Provider : string [NOT NULL]
- ProviderId : string [NOT NULL]
- IsConfirmed : bool? [NULL]
- ConfirmCode : string [NOT NULL]
- IsBlacklisted : bool? [NULL]
- CreatedAt : DateTime? [NULL]
- UpdatedAt : DateTime? [NULL]
- AdminLevelId : int? [FK -> AdminLevel.Id]
- IsFirstLogin : bool [NOT NULL]
關聯：
- Addresses → Address (1-to-many)
- AdminLevel ← AdminLevel
- BlacklistRecordAdminUsers → BlacklistRecord (as AdminUser)
- BlacklistRecordBlockedUsers → BlacklistRecord (as BlockedUser)
- Cart ← Cart (1-to-1)
- ChatMessageReceivers → ChatMessage (1-to-many)
- ChatMessageSenders → ChatMessage (1-to-many)
- LoginHistories → LoginHistory (1-to-many)
- MemberProfile ← MemberProfile (1-to-1)
- OrderReviews → OrderReview (1-to-many)
- Orders → Order (1-to-many)
- PointHistories → PointHistory (1-to-many)
- Products → Product (1-to-many)
- PromotionReviewedByNavigations → Promotion (1-to-many)
- PromotionSellers → Promotion (1-to-many)
- Role ← Role
- Stores → Store (1-to-many)
- SupportTickets → SupportTicket (1-to-many)

---
SupportTicket
- Id : int [PK]
- UserId : int [FK -> User.Id]
- OrderId : long? [FK -> Order.Id]
- Category : byte [NOT NULL]
- Subject : string [NOT NULL]
- Description : string [NOT NULL]
- AttachmentUrl : string [NOT NULL]
- Status : byte? [NULL]
- AdminReply : string [NOT NULL]
- CreatedAt : DateTime? [NULL]
- ResolvedAt : DateTime? [NULL]
關聯：
- Order ← Order
- User ← User

---
LoginHistory (備註：已列於上方)

ISpanShopDBContext.cs
- 包含 DbSet<T> 宣告與 Fluent API / 關聯設定（由 EF Core Power Tools 產生）。

---
備註：
- 若需輸出更詳細的索引、FK 名稱、或 DbContext 中的關聯設定，請回覆「列出 DbContext 關聯細節」，將解析 ISpanShopDBContext.cs 的 OnModelCreating 配置。