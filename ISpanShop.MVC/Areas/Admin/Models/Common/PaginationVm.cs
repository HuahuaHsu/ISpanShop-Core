namespace ISpanShop.MVC.Areas.Admin.Models.Common;

public class PaginationVm
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    /// <summary>產生第 N 頁的連結 URL</summary>
    public Func<int, string> UrlForPage { get; set; } = null!;

    /// <summary>&lt;nav&gt; 的額外 CSS class，預設 "mt-4"</summary>
    public string NavClass { get; set; } = "mt-4";

    /// <summary>頁碼圓圈的寬高（px），預設 38</summary>
    public int CircleSize { get; set; } = 38;

    /// <summary>是否在分頁連結上加 shadow-sm，預設 false</summary>
    public bool ShadowLinks { get; set; } = false;
}
