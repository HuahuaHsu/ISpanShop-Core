using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs
{
	public class PointUpdateDTO
	{
		public int UserId { get; set; }
		public int ChangeAmount { get; set; } // 正值為增加，負值為折抵
		public string OrderNumber { get; set; }
		public string Description { get; set; }
	}
}
