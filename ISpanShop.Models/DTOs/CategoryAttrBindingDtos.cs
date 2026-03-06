using System.Collections.Generic;

namespace ISpanShop.Models.DTOs
{
    // ════════════════════════════════════════
    // Response DTOs
    // ════════════════════════════════════════

    /// <summary>分類已綁定屬性的完整詳情（含名稱、選項、排序）</summary>
    public class BoundSpecDetailDto
    {
        public int    SpecId       { get; set; }
        public string Name         { get; set; } = string.Empty;
        public string InputType    { get; set; } = string.Empty;
        public bool   IsRequired   { get; set; }
        public bool   IsFilterable { get; set; }
        public int    Sort         { get; set; }
        public List<string> Options { get; set; } = new();
    }

    // ════════════════════════════════════════
    // Request DTOs
    // ════════════════════════════════════════

    public class BindSpecDto
    {
        public int CategoryId { get; set; }
        public int SpecId     { get; set; }
    }

    public class UpdateBindingSortDto
    {
        public int        CategoryId      { get; set; }
        public List<int>  OrderedSpecIds  { get; set; } = new();
    }

    public class CreateSpecAjaxDto
    {
        public string       Name       { get; set; } = string.Empty;
        public string       InputType  { get; set; } = "text";
        public bool         IsRequired { get; set; }
        public int          SortOrder  { get; set; }
        public List<string>? Options   { get; set; }
    }

    public class EditSpecAjaxDto
    {
        public int          Id         { get; set; }
        public string       Name       { get; set; } = string.Empty;
        public string       InputType  { get; set; } = "text";
        public bool         IsRequired { get; set; }
        public int          SortOrder  { get; set; }
        public List<string>? Options   { get; set; }
    }
}
