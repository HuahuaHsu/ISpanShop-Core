using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs.ContentModeration
{
	public class SensitiveWordDto
	{
		public int Id { get; set; }
		public string Word { get; set; }

		public int? CategoryId { get; set; }
		public string? Category { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedTime { get; set; }
	}
}
