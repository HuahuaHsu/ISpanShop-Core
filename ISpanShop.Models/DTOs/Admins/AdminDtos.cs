using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace ISpanShop.Models.DTOs.Admins
{
	/// <summary>
	/// 管理員資料傳輸物件 - 用於列表、詳細資訊及編輯
	/// </summary>
	public class AdminDto
	{
		[Display(Name = "管理員ID")]
		public int UserId { get; set; }

		[Display(Name = "帳號")]
		public string Account { get; set; }

		[Display(Name = "電子信箱")]
		public string Email { get; set; }

		[Display(Name = "角色ID")]
		public int RoleId { get; set; }

		[Display(Name = "角色名稱")]
		public string RoleName { get; set; }

		[Display(Name = "管理員身分ID")]
		public int? AdminLevelId { get; set; }

		[Display(Name = "管理員身分名稱")]
		public string AdminLevelName { get; set; }

		[Display(Name = "是否停權")]
		public bool IsBlacklisted { get; set; }

		[Display(Name = "是否首次登入")]
		public bool IsFirstLogin { get; set; }

		[Display(Name = "建立時間")]
		public DateTime CreatedAt { get; set; }

		[Display(Name = "更新時間")]
		public DateTime? UpdatedAt { get; set; }

		[JsonIgnore]
		public string PasswordHash { get; set; }

		[Display(Name = "實際權限")]	// 來源：AdminLevelPermissions（依 AdminLevelId 查詢）
		public List<PermissionDto> ActualPermissions { get; set; } = new List<PermissionDto>();
	
	}

	/// <summary>
	/// 權限資料傳輸物件
	/// </summary>
	public class PermissionDto
	{
		[Display(Name = "權限ID")]
		public int PermissionId { get; set; }

		[Display(Name = "權限鍵值")]
		public string PermissionKey { get; set; }

		[Display(Name = "顯示名稱")]
		public string DisplayName { get; set; }

		[Display(Name = "描述")]
		public string Description { get; set; }
	}

	/// <summary>
	/// 管理員等級資料傳輸物件 - 用於 Modal 下拉選單來源
	/// </summary>
	public class AdminLevelDto
	{
		public int AdminLevelId { get; set; }
		public string LevelName { get; set; }
		public string Description { get; set; }

		[Display(Name = "預設權限")]
		public List<PermissionDto> DefaultPermissions { get; set; } = new List<PermissionDto>();
	}

	/// <summary>
	/// 新增管理員 DTO
	/// </summary>
	public class AdminCreateDto
	{
			
		[Required(ErrorMessage = "{0}為必填")]
		[Display(Name = "初始密碼")]
		public string Password { get; set; }

		[Required(ErrorMessage = "{0}為必填")]
		[Display(Name = "管理員等級")]
		public int AdminLevelId { get; set; }
	}

	/// <summary>
	/// 修改管理員密碼 DTO
	/// </summary>
	public class AdminChangePasswordDto
	{
		public int UserId { get; set; }

		[Required(ErrorMessage = "{0}為必填")]
		[DataType(DataType.Password)]
		[Display(Name = "新密碼")]
		public string NewPassword { get; set; }

		[Required(ErrorMessage = "{0}為必填")]
		[DataType(DataType.Password)]
		[Compare("NewPassword", ErrorMessage = "兩次密碼輸入不一致")]
		[Display(Name = "確認新密碼")]
		public string ConfirmPassword { get; set; }
	}

	/// <summary>
	/// 角色資料傳輸物件 - 用於下拉選單
	/// </summary>
	public class AdminPermissionDto
	{
		[Display(Name = "角色ID")]
		public int PermissionId { get; set; }

		[Display(Name = "角色名稱")]
		public string PermissionKey { get; set; }

		[Display(Name = "描述")]
		public string Description { get; set; }
	}

	/// <summary>
	/// 編輯管理員身分 DTO
	/// </summary>
	public class AdminUpdateDto
	{
		[Required]
		[Display(Name = "管理員ID")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "{0}為必填")]
		[Display(Name = "管理員等級")]
		public int AdminLevelId { get; set; }

		[Display(Name = "狀態")]
		public bool IsBlacklisted { get; set; }
	}

	/// <summary>
	/// 新增管理員等級 DTO
	/// </summary>
	public class AdminLevelCreateDto
	{
		[Required(ErrorMessage = "{0}為必填")]
		[StringLength(100, MinimumLength = 2, ErrorMessage = "{0}長度必須在2-100個字符之間")]
		[Display(Name = "等級名稱")]
		public string LevelName { get; set; }

		[StringLength(500)]
		[Display(Name = "描述")]
		public string Description { get; set; }

		[Required(ErrorMessage = "至少選擇一個權限")]
		[Display(Name = "權限列表")]
		public List<int> PermissionIds { get; set; } = new List<int>();
	}

	/// <summary>
	/// 編輯管理員等級 DTO
	/// </summary>
	public class AdminLevelUpdateDto
	{
		[Required]
		[Display(Name = "等級ID")]
		public int AdminLevelId { get; set; }

		[Required(ErrorMessage = "{0}為必填")]
		[StringLength(100, MinimumLength = 2, ErrorMessage = "{0}長度必須在2-100個字符之間")]
		[Display(Name = "等級名稱")]
		public string LevelName { get; set; }

		[StringLength(500)]
		[Display(Name = "描述")]
		public string Description { get; set; }

		[Required(ErrorMessage = "至少選擇一個權限")]
		[Display(Name = "權限列表")]
		public List<int> PermissionIds { get; set; } = new List<int>();
	}

	/// <summary>
	/// 重設管理員密碼 DTO
	/// </summary>
	public class AdminResetPasswordDto
	{
		public int UserId { get; set; }

		[Required(ErrorMessage = "{0}為必填")]
		[Display(Name = "臨時密碼")]
		public string NewPassword { get; set; }
	}
}
