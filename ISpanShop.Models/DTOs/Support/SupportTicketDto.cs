using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs.Support
{
	public class SupportTicketDto
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string UserName { get; set; }
		public long? OrderId { get; set; }
		public byte Category { get; set; } // 0:訂單問題, 1:帳號問題, 2:檢舉
		public string Subject { get; set; }
		public string Description { get; set; }
		public string AttachmentUrl { get; set; }
		public byte? Status { get; set; } // 改為可為空
		public string AdminReply { get; set; }
		public DateTime? CreatedAt { get; set; } // 改為可為空
		public DateTime? ResolvedAt { get; set; }
	}
}
