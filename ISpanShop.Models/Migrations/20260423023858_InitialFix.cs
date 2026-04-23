using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISpanShop.Models.Migrations
{
    /// <inheritdoc />
    public partial class InitialFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PasswordResetTokens",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSeller",
                table: "MemberProfiles",
                type: "bit",
                nullable: true,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Users__3214EC07B29557C9",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__SupportT__3214EC0789C8C0A4",
                table: "SupportTickets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Stores__3214EC074D4E53F2",
                table: "Stores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Sensitiv__3214EC078BD26FA1",
                table: "SensitiveWords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Sensitiv__3214EC077849F621",
                table: "SensitiveWordCategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Roles__3214EC070CF15786",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ReviewIm__3214EC072755974D",
                table: "ReviewImages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Promotio__3214EC07FD701303",
                table: "Promotions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Promotio__3214EC071864C6D6",
                table: "PromotionRules",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Promotio__3214EC079513EEFB",
                table: "PromotionItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ProductV__3214EC0758F430C3",
                table: "ProductVariants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Products__3214EC07B5C0B072",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ProductI__3214EC0754523EDC",
                table: "ProductImages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__PointHis__3214EC076C183EFD",
                table: "PointHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__PaymentL__3214EC07F3990609",
                table: "PaymentLogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Orders__3214EC079627FFBC",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__OrderRev__3214EC077AD35710",
                table: "OrderReviews",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__OrderDet__3214EC07B159CF56",
                table: "OrderDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Membersh__3214EC073BF07D38",
                table: "MembershipLevels",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__MemberPr__3214EC0733EF43BE",
                table: "MemberProfiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__LoginHis__3214EC075C3F60F3",
                table: "LoginHistories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ChatMess__3214EC078C3DDCA1",
                table: "ChatMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Category__3214EC072C2D6978",
                table: "CategoryAttributes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Category__3214EC0745CE6B6D",
                table: "CategoryAttributeOptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Category__2B1402DCADA82BB2",
                table: "CategoryAttributeMappings",
                columns: new[] { "CategoryId", "CategoryAttributeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK__Categori__3214EC076E37F147",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Carts__3214EC07AD165691",
                table: "Carts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__CartItem__3214EC079F46FDB3",
                table: "CartItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Brands__3214EC07EA4B2CF7",
                table: "Brands",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Blacklis__3214EC071F47C438",
                table: "BlacklistRecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Addresse__3214EC07CCB52BF0",
                table: "Addresses",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK__Users__3214EC07B29557C9",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK__SupportT__3214EC0789C8C0A4",
                table: "SupportTickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Stores__3214EC074D4E53F2",
                table: "Stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Sensitiv__3214EC078BD26FA1",
                table: "SensitiveWords");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Sensitiv__3214EC077849F621",
                table: "SensitiveWordCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Roles__3214EC070CF15786",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ReviewIm__3214EC072755974D",
                table: "ReviewImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Promotio__3214EC07FD701303",
                table: "Promotions");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Promotio__3214EC071864C6D6",
                table: "PromotionRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Promotio__3214EC079513EEFB",
                table: "PromotionItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ProductV__3214EC0758F430C3",
                table: "ProductVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Products__3214EC07B5C0B072",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ProductI__3214EC0754523EDC",
                table: "ProductImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK__PointHis__3214EC076C183EFD",
                table: "PointHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK__PaymentL__3214EC07F3990609",
                table: "PaymentLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Orders__3214EC079627FFBC",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK__OrderRev__3214EC077AD35710",
                table: "OrderReviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK__OrderDet__3214EC07B159CF56",
                table: "OrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Membersh__3214EC073BF07D38",
                table: "MembershipLevels");

            migrationBuilder.DropPrimaryKey(
                name: "PK__MemberPr__3214EC0733EF43BE",
                table: "MemberProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK__LoginHis__3214EC075C3F60F3",
                table: "LoginHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ChatMess__3214EC078C3DDCA1",
                table: "ChatMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Category__3214EC072C2D6978",
                table: "CategoryAttributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Category__3214EC0745CE6B6D",
                table: "CategoryAttributeOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Category__2B1402DCADA82BB2",
                table: "CategoryAttributeMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Categori__3214EC076E37F147",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Carts__3214EC07AD165691",
                table: "Carts");

            migrationBuilder.DropPrimaryKey(
                name: "PK__CartItem__3214EC079F46FDB3",
                table: "CartItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Brands__3214EC07EA4B2CF7",
                table: "Brands");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Blacklis__3214EC071F47C438",
                table: "BlacklistRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Addresse__3214EC07CCB52BF0",
                table: "Addresses");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PasswordResetTokens",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSeller",
                table: "MemberProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: false);

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
        }
    }
}
