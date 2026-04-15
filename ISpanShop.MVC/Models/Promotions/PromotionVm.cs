namespace ISpanShop.MVC.Models.Promotions
{
    // ── 狀態常數（對應 DB Promotion.Status）──
    // 0 = 待審核  1 = 已核准  2 = 已拒絕  3 = 已結束
    // 活動類型：1=限時特賣  2=滿額折扣  3=限量搶購
    // 規則類型：1=滿額  2=滿件
    // 折扣類型：1=折抵金額  2=折扣百分比

    // ────────────────────────────────
    // 列表頁
    // ────────────────────────────────
    public class PromotionIndexVm
    {
        public List<PromotionListItemVm> Items { get; set; } = new();
        public int CurrentPage   { get; set; } = 1;
        public int TotalPages    { get; set; }
        public int TotalCount    { get; set; }  // 篩選後的總筆數
        public int PageSize      { get; set; } = 10;
        public string? Keyword       { get; set; }
        public string? StatusFilter  { get; set; }
        public int?    TypeFilter    { get; set; }

        // 統計卡片（未經篩選的總筆數）
        public int AllCount          { get; set; }  // 資料庫真正的總筆數（未經篩選）
        public int PendingCount      { get; set; }
        public int ActiveCount       { get; set; }
        public int UpcomingCount     { get; set; }
        public int EndedCount        { get; set; }
        public int ReSubmittedCount  { get; set; }  // Status=4：重新申請審核
        public int RejectedCount     { get; set; }  // Status=2：已拒絕
    }

    public class PromotionListItemVm
    {
        public int      Id            { get; set; }
        public string   Name          { get; set; } = "";
        public int      PromotionType { get; set; }
        public DateTime StartTime     { get; set; }
        public DateTime EndTime       { get; set; }
        public int      Status        { get; set; }
        public string   SellerName    { get; set; } = "";
        public int      ItemCount     { get; set; }

        // 計算屬性
        public string TypeName => PromotionType switch
        {
            1 => "限時特賣", 2 => "滿額折扣", 3 => "限量搶購", _ => "未知"
        };
        public string TypeBadgeClass => PromotionType switch
        {
            1 => "bg-label-warning", 2 => "bg-label-info", 3 => "bg-label-danger", _ => "bg-label-secondary"
        };

        public string DisplayStatus
        {
            get
            {
                if (Status == 0) return "待審核";
                if (Status == 2) return "已拒絕";
                if (Status == 3) return "已結束";
                if (Status == 4) return "重新送審";
                var now = DateTime.Now;
                if (now < StartTime) return "即將開始";
                if (now > EndTime)   return "已結束";
                return "進行中";
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (Status == 0) return "bg-label-warning";
                if (Status == 2) return "bg-label-danger";
                if (Status == 3) return "bg-label-secondary";
                if (Status == 4) return "bg-label-info";
                var now = DateTime.Now;
                if (now < StartTime) return "bg-label-primary";
                if (now > EndTime)   return "bg-label-secondary";
                return "bg-label-success";
            }
        }

        public bool IsPending => Status == 0 || Status == 4;
    }

    // ────────────────────────────────
    // 建立 / 編輯表單
    // ────────────────────────────────
    public class PromotionFormVm
    {
        public int      Id            { get; set; }
        public string   Name          { get; set; } = "";
        public string?  Description   { get; set; }
        public int      PromotionType { get; set; } = 1;
        public DateTime StartTime     { get; set; } = DateTime.Now.AddDays(1);
        public DateTime EndTime       { get; set; } = DateTime.Now.AddDays(8);
        public int      SellerId      { get; set; }

        public List<PromotionItemFormVm> Items { get; set; } = new();
        public List<PromotionRuleFormVm> Rules { get; set; } = new();

        // 下拉選單資料（GET 用）
        public List<MockSellerVm> Sellers { get; set; } = new();
    }

    public class PromotionItemFormVm
    {
        public int     ProductId     { get; set; }
        public string  ProductName   { get; set; } = "";
        public decimal OriginalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int?    QuantityLimit { get; set; }
        public int?    StockLimit    { get; set; }
    }

    public class PromotionRuleFormVm
    {
        public int     RuleType      { get; set; } = 1;
        public decimal Threshold     { get; set; }
        public int     DiscountType  { get; set; } = 1;
        public decimal DiscountValue { get; set; }
    }

    // ────────────────────────────────
    // 詳情頁
    // ────────────────────────────────
    public class PromotionDetailVm
    {
        public int      Id            { get; set; }
        public string   Name          { get; set; } = "";
        public string?  Description   { get; set; }
        public int      PromotionType { get; set; }
        public DateTime StartTime     { get; set; }
        public DateTime EndTime       { get; set; }
        public int      Status        { get; set; }
        public string   SellerName    { get; set; } = "";
        public string?  RejectReason  { get; set; }
        public DateTime? ReviewedAt   { get; set; }
        public DateTime CreatedAt     { get; set; }

        // 賣家資訊（供審核參考）
        public double SellerRating       { get; set; }
        public int    SellerApprovalRate { get; set; }  // 0–100 (%)
        public int    SellerViolations   { get; set; }
        public int    SellerTotalPromos  { get; set; }

        public List<PromotionItemDetailVm> Items { get; set; } = new();
        public List<PromotionRuleDetailVm> Rules { get; set; } = new();

        public string TypeName => PromotionType switch
        {
            1 => "限時特賣", 2 => "滿額折扣", 3 => "限量搶購", _ => "未知"
        };
        public string TypeBadgeClass => PromotionType switch
        {
            1 => "bg-label-warning", 2 => "bg-label-info", 3 => "bg-label-danger", _ => "bg-label-secondary"
        };

        public string DisplayStatus
        {
            get
            {
                if (Status == 0) return "待審核";
                if (Status == 2) return "已拒絕";
                if (Status == 3) return "已結束";
                if (Status == 4) return "重新送審";
                var now = DateTime.Now;
                if (now < StartTime) return "即將開始";
                if (now > EndTime)   return "已結束";
                return "進行中";
            }
        }
        public string StatusBadgeClass
        {
            get
            {
                if (Status == 0) return "bg-label-warning";
                if (Status == 2) return "bg-label-danger";
                if (Status == 3) return "bg-label-secondary";
                if (Status == 4) return "bg-label-info";
                var now = DateTime.Now;
                if (now < StartTime) return "bg-label-primary";
                if (now > EndTime)   return "bg-label-secondary";
                return "bg-label-success";
            }
        }

        public bool IsPending => Status == 0 || Status == 4;
        public bool CanEdit   => Status == 0 || (Status == 1 && DateTime.Now < StartTime);
        public bool IsActiveOnly => Status == 1 && DateTime.Now >= StartTime && DateTime.Now <= EndTime;

        // 銷售成效（進行中或已結束才填入，其他狀態為 null）
        public PromotionSalesStatsVm? SalesStats { get; set; }
        public bool ShowSalesStats => SalesStats != null;

        public double ProgressPercent
        {
            get
            {
                var now = DateTime.Now;
                if (now <= StartTime) return 0;
                if (now >= EndTime)   return 100;
                var total   = (EndTime - StartTime).TotalSeconds;
                var elapsed = (now - StartTime).TotalSeconds;
                return Math.Round(elapsed / total * 100, 1);
            }
        }
    }

    public class PromotionItemDetailVm
    {
        public int     ProductId     { get; set; }
        public string  ProductName   { get; set; } = "";
        public decimal OriginalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int?    QuantityLimit { get; set; }
        public int?    StockLimit    { get; set; }
        public int     SoldCount     { get; set; }
    }

    public class PromotionRuleDetailVm
    {
        public int     RuleType      { get; set; }
        public decimal Threshold     { get; set; }
        public int     DiscountType  { get; set; }
        public decimal DiscountValue { get; set; }

        public string RuleTypeName    => RuleType switch    { 1 => "滿額", 2 => "滿件", _ => "未知" };
        public string DiscountTypeName => DiscountType switch { 1 => "折抵", 2 => "折扣", _ => "未知" };

        public string DisplayText => RuleType == 1
            ? $"消費滿 {Threshold:N0} 元，{(DiscountType == 1 ? $"折抵 {DiscountValue:N0} 元" : $"打 {DiscountValue}% 折")}"
            : $"購買滿 {Threshold:N0} 件，{(DiscountType == 1 ? $"折抵 {DiscountValue:N0} 元" : $"打 {DiscountValue}% 折")}";
    }

    // ────────────────────────────────
    // 輔助 / Mock 用
    // ────────────────────────────────
    public class MockSellerVm
    {
        public int    Id              { get; set; }
        public string Name            { get; set; } = "";
        public double Rating          { get; set; }   // 0.0 – 5.0
        public int    ApprovalRate    { get; set; }   // 0 – 100 (%)
        public int    ViolationCount  { get; set; }
        public int    TotalPromotions { get; set; }
    }

    public class MockProductVm
    {
        public int     Id           { get; set; }
        public string  Name         { get; set; } = "";
        public decimal Price        { get; set; }
        public string  CategoryName { get; set; } = "";
        public string  ImageUrl     { get; set; } = "";
    }

    // ────────────────────────────────
    // 銷售成效統計（進行中 / 已結束才有值）
    // ────────────────────────────────
    public class PromotionSalesStatsVm
    {
        public int     OrderCount       { get; set; }   // 參與訂單數
        public int?    TotalSoldQty     { get; set; }   // 總銷售數量（滿額折扣類型不適用 → null）
        public decimal TotalSalesAmount { get; set; }   // 活動商品總銷售金額
        public decimal TotalDiscount    { get; set; }   // 優惠總折抵金額
    }

    // ── 內部靜態 store 結構（模擬資料庫）──
    public class PromotionStoredVm
    {
        public int      Id            { get; set; }
        public string   Name          { get; set; } = "";
        public string?  Description   { get; set; }
        public int      PromotionType { get; set; }
        public DateTime StartTime     { get; set; }
        public DateTime EndTime       { get; set; }
        public int      Status        { get; set; }
        public int      SellerId      { get; set; }
        public string   SellerName    { get; set; } = "";
        public string?  RejectReason  { get; set; }
        public DateTime? ReviewedAt   { get; set; }
        public DateTime CreatedAt     { get; set; }
        public bool     IsDeleted     { get; set; }
        public List<PromotionItemDetailVm> Items { get; set; } = new();
        public List<PromotionRuleDetailVm> Rules { get; set; } = new();

        // Mock 銷售資料：訂單數（所有類型通用）
        public int     MockOrderCount { get; set; }
        // Mock 銷售資料：滿額折扣（Type 2）專用，無法從商品維度算
        public decimal MockTotalSalesAmount { get; set; }
        public decimal MockTotalDiscount    { get; set; }
    }
}
