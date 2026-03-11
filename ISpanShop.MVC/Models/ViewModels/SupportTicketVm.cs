using System.ComponentModel.DataAnnotations;

namespace ISpanShop.MVC.Models.ViewModels
{
	public class SupportTicketVm
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public string Subject { get; set; }
		public string CategoryName => Category switch { 0 => "訂單", 1 => "帳號", 2 => "檢舉", _ => "其他" };
		public byte Category { get; set; }

		[Display(Name = "工單狀態")]
		public byte Status { get; set; }

		[Required(ErrorMessage = "回覆內容不能為空")]
		[Display(Name = "管理員回覆")]
		public string AdminReply { get; set; }

		public DateTime CreatedAt { get; set; }

		public string Description { get; set; }
		public string AttachmentUrl { get; set; }
		public long? OrderId { get; set; }
	}
}