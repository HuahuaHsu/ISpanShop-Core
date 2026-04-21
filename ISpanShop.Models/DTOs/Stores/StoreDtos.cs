using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs.Stores
{
    public class StoreDto
    {
        public int StoreId { get; set; }
        public int UserId { get; set; }
        public string OwnerAccount { get; set; } // Users.Account（店主帳號）
        public string StoreName { get; set; } // Stores.StoreName
        public string? Description { get; set; } // Stores.Description
        public bool? IsVerified { get; set; } // Stores.IsVerified
        public bool IsBlacklisted { get; set; } // Users.IsBlacklisted
        public int StoreStatus { get; set; } // Stores.StoreStatus (1/2/3)
        public string? StoreStatusName { get; set; } // 顯示名稱
        public string? StoreStatusBadge { get; set; } // Badge CSS class
        public DateTime? CreatedAt { get; set; } // Stores.CreatedAt
        public int ProductCount { get; set; } // 商品數量
    }

    /// <summary>前台公開賣場資訊（任何人可看）</summary>
    public class StorePublicProfileDto
    {
        public int       Id            { get; set; }
        public string    Name          { get; set; } = string.Empty;
        public string?   Description   { get; set; }
        public string?   LogoUrl       { get; set; }
        /// <summary>平均評分（目前無評分機制，回傳 null）</summary>
        public decimal?  Rating        { get; set; }
        public int       ProductCount  { get; set; }
        /// <summary>粉絲數（目前無追蹤機制，回傳 0）</summary>
        public int       FollowerCount { get; set; }
        public DateTime? CreatedAt     { get; set; }
    }

    public class StoreDetailDto
    {
        public int StoreId { get; set; }
        public int UserId { get; set; }
        public string OwnerAccount { get; set; }
        public string StoreName { get; set; }
        public string? Description { get; set; }
        public bool? IsVerified { get; set; }
        public bool IsBlacklisted { get; set; }
        public int StoreStatus { get; set; } // Stores.StoreStatus (1/2/3)
        public string? StoreStatusName { get; set; } // 顯示名稱
        public string? StoreStatusBadge { get; set; } // Badge CSS class
        public DateTime? CreatedAt { get; set; }
        public int ProductCount { get; set; } // 商品總數（IsDeleted = 0）
        public int ActiveProductCount { get; set; } // 上架中商品數（Status = 1）
        public int TotalSales { get; set; } // 所有商品 TotalSales 加總
    }
}
