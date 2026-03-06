namespace ISpanShop.WebAPI.DTOs
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
        public int    SortOrder    { get; set; }
        /// <summary>當 InputType 為 select/checkbox/radio 時才有值</summary>
        public List<string> Options { get; set; } = new();
    }
}
