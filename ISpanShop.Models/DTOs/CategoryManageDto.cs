using System.Collections.Generic;

namespace ISpanShop.Models.DTOs
{
    public class CategoryManageDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ImageUrl { get; set; }
        public string? NameEn { get; set; }
        public int ProductCount { get; set; }
        public int ChildCount { get; set; }
        public List<CategoryManageDto> Children { get; set; } = new();
    }
}
