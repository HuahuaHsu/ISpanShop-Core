using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISpanShop.Models.EfModels
{
	public class PasswordResetToken
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int UserId { get; set; }  // FK πÔ¿≥ Users.Id

		[Required]
		[MaxLength(100)]
		public string Token { get; set; }

		[Required]
		public DateTime ExpiryDate { get; set; }

		public bool IsUsed { get; set; } = false;

		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}