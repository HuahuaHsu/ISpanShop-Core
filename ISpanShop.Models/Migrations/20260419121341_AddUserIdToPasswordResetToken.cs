using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISpanShop.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToPasswordResetToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK__Users__3214EC078B3C986A",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK__SupportT__3214EC07F5320F20",
                table: "SupportTickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Stores__3214EC076399D263",
                table: "Stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Sensitiv__3214EC0717CC74C4",
                table: "SensitiveWords");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Sensitiv__3214EC074B794DCC",
                table: "SensitiveWordCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Roles__3214EC071F8E1E55",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ReviewIm__3214EC07843137EA",
                table: "ReviewImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Promotio__3214EC07FA43E96D",
                table: "Promotions");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Promotio__3214EC073713AF6F",
                table: "PromotionRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Promotio__3214EC077A262F89",
                table: "PromotionItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ProductV__3214EC072470832B",
                table: "ProductVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Products__3214EC0764C681F5",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ProductI__3214EC0773B4F88E",
                table: "ProductImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK__PointHis__3214EC072BA1E7E1",
                table: "PointHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK__PaymentL__3214EC072F2D813F",
                table: "PaymentLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Orders__3214EC07D594D25D",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK__OrderRev__3214EC076CFA0E27",
                table: "OrderReviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK__OrderDet__3214EC07E57B0E42",
                table: "OrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Membersh__3214EC0707631A52",
                table: "MembershipLevels");

            migrationBuilder.DropPrimaryKey(
                name: "PK__MemberPr__3214EC078A2BBE5A",
                table: "MemberProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK__LoginHis__3214EC07B4209083",
                table: "LoginHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ChatMess__3214EC07AE07539C",
                table: "ChatMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Category__3214EC07F7BA15E9",
                table: "CategoryAttributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Category__3214EC0713F45311",
                table: "CategoryAttributeOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Category__2B1402DC3B5C54D4",
                table: "CategoryAttributeMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Categori__3214EC070CB4C273",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Carts__3214EC07BC7F87DF",
                table: "Carts");

            migrationBuilder.DropPrimaryKey(
                name: "PK__CartItem__3214EC077FC82690",
                table: "CartItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Brands__3214EC07E55E295C",
                table: "Brands");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Blacklis__3214EC07D9FCC9A1",
                table: "BlacklistRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Addresse__3214EC0796443C33",
                table: "Addresses");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Stores",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CouponId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AllocatedDiscountAmount",
                table: "OrderDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "MemberProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__Users__3214EC071CE4420D",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__SupportT__3214EC075D10031D",
                table: "SupportTickets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Stores__3214EC078D01D42D",
                table: "Stores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Sensitiv__3214EC079DF73360",
                table: "SensitiveWords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Sensitiv__3214EC07156B6588",
                table: "SensitiveWordCategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Roles__3214EC07D7442CDF",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ReviewIm__3214EC074C21816B",
                table: "ReviewImages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Promotio__3214EC07CCFDDD24",
                table: "Promotions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Promotio__3214EC07269B7A44",
                table: "PromotionRules",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Promotio__3214EC07C9CDD985",
                table: "PromotionItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ProductV__3214EC073A6575F7",
                table: "ProductVariants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Products__3214EC079295A641",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ProductI__3214EC0772EEB958",
                table: "ProductImages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__PointHis__3214EC0748BD63DC",
                table: "PointHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__PaymentL__3214EC076A05C45F",
                table: "PaymentLogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Orders__3214EC078F47CCEE",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__OrderRev__3214EC07EDA47055",
                table: "OrderReviews",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__OrderDet__3214EC07D8EF013B",
                table: "OrderDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Membersh__3214EC077D283FA0",
                table: "MembershipLevels",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__MemberPr__3214EC07CBF77DA4",
                table: "MemberProfiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__LoginHis__3214EC07A95A3CD4",
                table: "LoginHistories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ChatMess__3214EC072C0A50B3",
                table: "ChatMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Category__3214EC07FB7FECCB",
                table: "CategoryAttributes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Category__3214EC0795733785",
                table: "CategoryAttributeOptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Category__2B1402DCE86C9054",
                table: "CategoryAttributeMappings",
                columns: new[] { "CategoryId", "CategoryAttributeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK__Categori__3214EC07B271DEB5",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Carts__3214EC07ADAFED9C",
                table: "Carts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__CartItem__3214EC073D89D384",
                table: "CartItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Brands__3214EC07EBF8F95E",
                table: "Brands",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Blacklis__3214EC07CC1F5DE7",
                table: "BlacklistRecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Addresse__3214EC0720517029",
                table: "Addresses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    CouponCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DistributionType = table.Column<byte>(type: "tinyint", nullable: false),
                    CouponType = table.Column<byte>(type: "tinyint", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumSpend = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaximumDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    TotalQuantity = table.Column<int>(type: "int", nullable: false),
                    UsedQuantity = table.Column<int>(type: "int", nullable: false),
                    PerUserLimit = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ApplyToAll = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsExclusive = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coupons_Stores",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Coupons_Users",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CouponCategories",
                columns: table => new
                {
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCategories", x => new { x.CouponId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_CouponCategories_Categories",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CouponCategories_Coupons",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CouponProducts",
                columns: table => new
                {
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponProducts", x => new { x.CouponId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_CouponProducts_Coupons",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CouponProducts_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MemberCoupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    UsageStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UsedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberCoupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberCoupons_Coupons",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MemberCoupons_Orders",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MemberCoupons_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CouponId",
                table: "Orders",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCategories_CategoryId",
                table: "CouponCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponProducts_ProductId",
                table: "CouponProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_SellerId",
                table: "Coupons",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_StoreId",
                table: "Coupons",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberCoupons_CouponId",
                table: "MemberCoupons",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberCoupons_OrderId",
                table: "MemberCoupons",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberCoupons_UserId",
                table: "MemberCoupons",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Coupons",
                table: "Orders",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Coupons",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "CouponCategories");

            migrationBuilder.DropTable(
                name: "CouponProducts");

            migrationBuilder.DropTable(
                name: "MemberCoupons");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Users__3214EC071CE4420D",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK__SupportT__3214EC075D10031D",
                table: "SupportTickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Stores__3214EC078D01D42D",
                table: "Stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Sensitiv__3214EC079DF73360",
                table: "SensitiveWords");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Sensitiv__3214EC07156B6588",
                table: "SensitiveWordCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Roles__3214EC07D7442CDF",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ReviewIm__3214EC074C21816B",
                table: "ReviewImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Promotio__3214EC07CCFDDD24",
                table: "Promotions");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Promotio__3214EC07269B7A44",
                table: "PromotionRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Promotio__3214EC07C9CDD985",
                table: "PromotionItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ProductV__3214EC073A6575F7",
                table: "ProductVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Products__3214EC079295A641",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ProductI__3214EC0772EEB958",
                table: "ProductImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK__PointHis__3214EC0748BD63DC",
                table: "PointHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK__PaymentL__3214EC076A05C45F",
                table: "PaymentLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Orders__3214EC078F47CCEE",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CouponId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK__OrderRev__3214EC07EDA47055",
                table: "OrderReviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK__OrderDet__3214EC07D8EF013B",
                table: "OrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Membersh__3214EC077D283FA0",
                table: "MembershipLevels");

            migrationBuilder.DropPrimaryKey(
                name: "PK__MemberPr__3214EC07CBF77DA4",
                table: "MemberProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK__LoginHis__3214EC07A95A3CD4",
                table: "LoginHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ChatMess__3214EC072C0A50B3",
                table: "ChatMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Category__3214EC07FB7FECCB",
                table: "CategoryAttributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Category__3214EC0795733785",
                table: "CategoryAttributeOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Category__2B1402DCE86C9054",
                table: "CategoryAttributeMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Categori__3214EC07B271DEB5",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Carts__3214EC07ADAFED9C",
                table: "Carts");

            migrationBuilder.DropPrimaryKey(
                name: "PK__CartItem__3214EC073D89D384",
                table: "CartItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Brands__3214EC07EBF8F95E",
                table: "Brands");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Blacklis__3214EC07CC1F5DE7",
                table: "BlacklistRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Addresse__3214EC0720517029",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "CouponId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AllocatedDiscountAmount",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "MemberProfiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Users__3214EC078B3C986A",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__SupportT__3214EC07F5320F20",
                table: "SupportTickets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Stores__3214EC076399D263",
                table: "Stores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Sensitiv__3214EC0717CC74C4",
                table: "SensitiveWords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Sensitiv__3214EC074B794DCC",
                table: "SensitiveWordCategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Roles__3214EC071F8E1E55",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ReviewIm__3214EC07843137EA",
                table: "ReviewImages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Promotio__3214EC07FA43E96D",
                table: "Promotions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Promotio__3214EC073713AF6F",
                table: "PromotionRules",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Promotio__3214EC077A262F89",
                table: "PromotionItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ProductV__3214EC072470832B",
                table: "ProductVariants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Products__3214EC0764C681F5",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ProductI__3214EC0773B4F88E",
                table: "ProductImages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__PointHis__3214EC072BA1E7E1",
                table: "PointHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__PaymentL__3214EC072F2D813F",
                table: "PaymentLogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Orders__3214EC07D594D25D",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__OrderRev__3214EC076CFA0E27",
                table: "OrderReviews",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__OrderDet__3214EC07E57B0E42",
                table: "OrderDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Membersh__3214EC0707631A52",
                table: "MembershipLevels",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__MemberPr__3214EC078A2BBE5A",
                table: "MemberProfiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__LoginHis__3214EC07B4209083",
                table: "LoginHistories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ChatMess__3214EC07AE07539C",
                table: "ChatMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Category__3214EC07F7BA15E9",
                table: "CategoryAttributes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Category__3214EC0713F45311",
                table: "CategoryAttributeOptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Category__2B1402DC3B5C54D4",
                table: "CategoryAttributeMappings",
                columns: new[] { "CategoryId", "CategoryAttributeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK__Categori__3214EC070CB4C273",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Carts__3214EC07BC7F87DF",
                table: "Carts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__CartItem__3214EC077FC82690",
                table: "CartItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Brands__3214EC07E55E295C",
                table: "Brands",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Blacklis__3214EC07D9FCC9A1",
                table: "BlacklistRecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Addresse__3214EC0796443C33",
                table: "Addresses",
                column: "Id");
        }
    }
}
