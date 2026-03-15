using System.Collections.Generic;

namespace ISpanShop.Models.DTOs.Admins
{
    public class AdminLoginClaimsDto
	{
        public int AdminId { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public List<string> PermissionKeys { get; set; } = new List<string>(); // 所有擁有的 PermissionKey 清單
    }
}
