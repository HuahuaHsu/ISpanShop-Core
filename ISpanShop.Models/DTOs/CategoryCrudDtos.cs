namespace ISpanShop.Models.DTOs
{
    public class CategoryCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; } = 0;
        public string? ImageUrl { get; set; }
    }

    public class CategoryUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class IdDto
    {
        public int Id { get; set; }
    }

    public class UpdateSortDto
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
    }

    public class ToggleActiveDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
}
