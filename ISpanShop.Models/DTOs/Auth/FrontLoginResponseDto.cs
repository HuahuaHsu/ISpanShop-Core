namespace ISpanShop.Models.DTOs.Auth
{
    public class FrontLoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int MemberId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public int PointBalance { get; set; }
    }
}
