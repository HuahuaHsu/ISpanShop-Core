namespace ISpanShop.Models.DTOs.Categories
{
    /// <summary>分類屬性（規格定義）回應 DTO</summary>
    public class CategoryAttributeResponse
    {
        public int    Id           { get; set; }
        public string Name         { get; set; } = string.Empty;
        /// <summary>text | select | checkbox | radio</summary>
        public string InputType    { get; set; } = string.Empty;
        public bool   IsRequired   { get; set; }
        public bool   IsFilterable { get; set; }
        /// <summary>是否為多選（checkbox 為 true，radio/select 為 false）</summary>
        public bool   IsMultiple   { get; set; }
        /// <summary>最多可選幾個（僅多選時有效）</summary>
        public int?   MaxSelect    { get; set; }
        /// <summary>允許賣家自填選項（可輸入不在清單中的選項）</summary>
        public bool   AllowCustom  { get; set; }
        /// <summary>當 InputType 為 select/checkbox/radio 時才有值</summary>
        public List<AttributeOptionDto> Options { get; set; } = new();
    }

    /// <summary>屬性選項 DTO</summary>
    public class AttributeOptionDto
    {
        public int    Id    { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>API 統一回應格式</summary>
    public class ApiResponse<T>
    {
        public bool   Success { get; set; }
        public T?     Data    { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
