using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISpanShop.Models.Migrations
{
    /// <inheritdoc />
    public partial class CreateAllTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Sort = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    IsVisible = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Brands__3214EC07E55E295C", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Sort = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    IsVisible = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    Icon = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Categori__3214EC070CB4C273", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Parent",
                        column: x => x.ParentId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CategoryAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InputType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "text"),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AllowCustomInput = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Category__3214EC07F7BA15E9", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MembershipLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MinSpending = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(3,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Membersh__3214EC0707631A52", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionKey = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__3214EC071F8E1E55", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SensitiveWordCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Sensitiv__3214EC074B794DCC", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryAttributeMappings",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CategoryAttributeId = table.Column<int>(type: "int", nullable: false),
                    IsFilterable = table.Column<bool>(type: "bit", nullable: false),
                    Sort = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Category__2B1402DC3B5C54D4", x => new { x.CategoryId, x.CategoryAttributeId });
                    table.ForeignKey(
                        name: "FK_AttributeMappings_Attributes",
                        column: x => x.CategoryAttributeId,
                        principalTable: "CategoryAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttributeMappings_Categories",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryAttributeOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryAttributeId = table.Column<int>(type: "int", nullable: false),
                    OptionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Category__3214EC0713F45311", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeOptions_Attributes",
                        column: x => x.CategoryAttributeId,
                        principalTable: "CategoryAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdminLevelPermissions",
                columns: table => new
                {
                    AdminLevelId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminLevelPermissions", x => new { x.AdminLevelId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_ALP_AdminLevels",
                        column: x => x.AdminLevelId,
                        principalTable: "AdminLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ALP_Permissions",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    Account = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Provider = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    ProviderId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    ConfirmCode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    IsBlacklisted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    AdminLevelId = table.Column<int>(type: "int", nullable: true),
                    IsFirstLogin = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__3214EC078B3C986A", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_AdminLevels",
                        column: x => x.AdminLevelId,
                        principalTable: "AdminLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Roles",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SensitiveWords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Word = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Sensitiv__3214EC0717CC74C4", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensitiveWords_Categories",
                        column: x => x.CategoryId,
                        principalTable: "SensitiveWordCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecipientPhone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    City = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Region = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Addresse__3214EC0796443C33", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BlacklistRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlockedUserId = table.Column<int>(type: "int", nullable: false),
                    AdminUserId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UnblockAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Blacklis__3214EC07D9FCC9A1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlacklistRecords_AdminUser",
                        column: x => x.AdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BlacklistRecords_BlockedUser",
                        column: x => x.BlockedUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Carts__3214EC07BC7F87DF", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    SentAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChatMess__3214EC07AE07539C", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Receiver",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatMessages_Sender",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LoginHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    AttemptedAccount = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LoginHis__3214EC07B4209083", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoginHistories_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MemberProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LevelId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Gender = table.Column<byte>(type: "tinyint", nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    TotalSpending = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    PointBalance = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    EmailNotification = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsSeller = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MemberPr__3214EC078A2BBE5A", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberProfiles_MembershipLevels",
                        column: x => x.LevelId,
                        principalTable: "MembershipLevels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MemberProfiles_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PointHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrderNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ChangeAmount = table.Column<int>(type: "int", nullable: false),
                    BalanceAfter = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PointHis__3214EC072BA1E7E1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointHistory_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PromotionType = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Promotio__3214EC07FA43E96D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Promotions_Users_Reviewer",
                        column: x => x.ReviewedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Promotions_Users_Seller",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StoreName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    StoreStatus = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Stores__3214EC076399D263", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stores_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PromotionRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromotionId = table.Column<int>(type: "int", nullable: false),
                    RuleType = table.Column<int>(type: "int", nullable: false),
                    Threshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Promotio__3214EC073713AF6F", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionRules_Promotions",
                        column: x => x.PromotionId,
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    PointDiscount = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: true, defaultValue: (byte)0),
                    PaymentDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RecipientName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecipientPhone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    RecipientAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CompletedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Orders__3214EC07D594D25D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Stores",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SpecDefinitionJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalSales = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ViewCount = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    Status = table.Column<byte>(type: "tinyint", nullable: true, defaultValue: (byte)1),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ReviewStatus = table.Column<int>(type: "int", nullable: false),
                    ReviewedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ForceOffShelfReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ForceOffShelfDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ForceOffShelfBy = table.Column<int>(type: "int", nullable: true),
                    ReApplyDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Products__3214EC0764C681F5", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Brands",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Categories",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Stores",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Users_ForceOffShelf",
                        column: x => x.ForceOffShelfBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Rating = table.Column<byte>(type: "tinyint", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StoreReply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsHidden = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OrderRev__3214EC076CFA0E27", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderReviews_Orders",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderReviews_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    MerchantTradeNo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    TradeNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    RtnCode = table.Column<int>(type: "int", nullable: true),
                    RtnMsg = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TradeAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentL__3214EC072F2D813F", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentLogs_Orders",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReturnRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ReasonCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReasonDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    AdminRemark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnRequests_Orders",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupportTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: true),
                    Category = table.Column<byte>(type: "tinyint", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: true, defaultValue: (byte)0),
                    AdminReply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    ResolvedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SupportT__3214EC07F5320F20", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Orders",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SkuCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VariantName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SpecValueJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    SafetyStock = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ProductV__3214EC072470832B", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PromotionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromotionId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiscountPercent = table.Column<int>(type: "int", nullable: true),
                    QuantityLimit = table.Column<int>(type: "int", nullable: true),
                    StockLimit = table.Column<int>(type: "int", nullable: true),
                    SoldCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Promotio__3214EC077A262F89", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionItems_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromotionItems_Promotions",
                        column: x => x.PromotionId,
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ReviewIm__3214EC07843137EA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewImages_OrderReviews",
                        column: x => x.ReviewId,
                        principalTable: "OrderReviews",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReturnRequestImages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnRequestId = table.Column<long>(type: "bigint", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequestImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnRequestImages_ReturnRequests",
                        column: x => x.ReturnRequestId,
                        principalTable: "ReturnRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CartItem__3214EC077FC82690", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_ProductVariants",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CartItems_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CartItems_Stores",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VariantName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SkuCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CoverImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OrderDet__3214EC07E57B0E42", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderDetails_ProductVariants",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderDetails_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    SortOrder = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ProductI__3214EC0773B4F88E", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_ProductVariants",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductImages_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminLevelPermissions_PermissionId",
                table: "AdminLevelPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "UQ_AdminLevels_Name",
                table: "AdminLevels",
                column: "LevelName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistRecords_AdminUserId",
                table: "BlacklistRecords",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistRecords_BlockedUserId",
                table: "BlacklistRecords",
                column: "BlockedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_StoreId",
                table: "CartItems",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_VariantId",
                table: "CartItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "UQ_Carts_UserId",
                table: "Carts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                table: "Categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryAttributeMappings_CategoryAttributeId",
                table: "CategoryAttributeMappings",
                column: "CategoryAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryAttributeOptions_CategoryAttributeId",
                table: "CategoryAttributeOptions",
                column: "CategoryAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReceiverId",
                table: "ChatMessages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                table: "ChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistories_UserId",
                table: "LoginHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberProfiles_LevelId",
                table: "MemberProfiles",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "UQ_MemberProfiles_UserId",
                table: "MemberProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_VariantId",
                table: "OrderDetails",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReviews_OrderId",
                table: "OrderReviews",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReviews_UserId",
                table: "OrderReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StoreId",
                table: "Orders",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_Orders_Number",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLogs_OrderId",
                table: "PaymentLogs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "UQ_Permissions_Key",
                table: "Permissions",
                column: "PermissionKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointHistory_UserId",
                table: "PointHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_VariantId",
                table: "ProductImages",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ForceOffShelfBy",
                table: "Products",
                column: "ForceOffShelfBy");

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreId",
                table: "Products",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "UQ_ProductVariants_SkuCode",
                table: "ProductVariants",
                column: "SkuCode",
                unique: true,
                filter: "[SkuCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionItems_ProductId",
                table: "PromotionItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionItems_PromotionId",
                table: "PromotionItems",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRules_PromotionId",
                table: "PromotionRules",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_ReviewedBy",
                table: "Promotions",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_SellerId",
                table: "Promotions",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequestImages_ReturnRequestId",
                table: "ReturnRequestImages",
                column: "ReturnRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_OrderId",
                table: "ReturnRequests",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewImages_ReviewId",
                table: "ReviewImages",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_SensitiveWords_CategoryId",
                table: "SensitiveWords",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_UserId",
                table: "Stores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_OrderId",
                table: "SupportTickets",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_UserId",
                table: "SupportTickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AdminLevelId",
                table: "Users",
                column: "AdminLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ_Users_Account",
                table: "Users",
                column: "Account",
                unique: true,
                filter: "[Account] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "AdminLevelPermissions");

            migrationBuilder.DropTable(
                name: "BlacklistRecords");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "CategoryAttributeMappings");

            migrationBuilder.DropTable(
                name: "CategoryAttributeOptions");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "LoginHistories");

            migrationBuilder.DropTable(
                name: "MemberProfiles");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "PaymentLogs");

            migrationBuilder.DropTable(
                name: "PointHistory");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "PromotionItems");

            migrationBuilder.DropTable(
                name: "PromotionRules");

            migrationBuilder.DropTable(
                name: "ReturnRequestImages");

            migrationBuilder.DropTable(
                name: "ReviewImages");

            migrationBuilder.DropTable(
                name: "SensitiveWords");

            migrationBuilder.DropTable(
                name: "SupportTickets");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "CategoryAttributes");

            migrationBuilder.DropTable(
                name: "MembershipLevels");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "ReturnRequests");

            migrationBuilder.DropTable(
                name: "OrderReviews");

            migrationBuilder.DropTable(
                name: "SensitiveWordCategories");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AdminLevels");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
