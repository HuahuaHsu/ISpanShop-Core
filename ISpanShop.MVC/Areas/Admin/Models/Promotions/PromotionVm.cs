namespace ISpanShop.MVC.Areas.Admin.Models.Promotions
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
        public int TotalCount    { get; set; }
        public int PageSize      { get; set; } = 10;
        public string? Keyword       { get; set; }
        public string? StatusFilter  { get; set; }
        public int?    TypeFilter    { get; set; }

        // 統計卡片
        public int PendingCount  { get; set; }
        public int ActiveCount   { get; set; }
        public int UpcomingCount { get; set; }
        public int EndedCount    { get; set; }
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
                var now = DateTime.Now;
                if (now < StartTime) return "bg-label-primary";
                if (now > EndTime)   return "bg-label-secondary";
                return "bg-label-success";
            }
        }

        public bool IsPending => Status == 0;
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
                var now = DateTime.Now;
                if (now < StartTime) return "bg-label-primary";
                if (now > EndTime)   return "bg-label-secondary";
                return "bg-label-success";
            }
        }

        public bool IsPending => Status == 0;
        public bool CanEdit   => Status == 0 || (Status == 1 && DateTime.Now < StartTime);
        public bool IsActiveOnly => Status == 1 && DateTime.Now >= StartTime && DateTime.Now <= EndTime;

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
        public int    Id   { get; set; }
        public string Name { get; set; } = "";
    }

    public class MockProductVm
    {
        public int     Id           { get; set; }
        public string  Name         { get; set; } = "";
        public decimal Price        { get; set; }
        public string  CategoryName { get; set; } = "";
        public string  ImageUrl     { get; set; } = "";
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
    }
}
