using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Common.Enums
{
	public enum OrderStatus : byte
	{
		Pending = 0,    // 待處理
		Processing = 1, // 處理中
		Shipped = 2,    // 已出貨
		Completed = 3,  // 已完成
		Cancelled = 4   // 已取消
	}
}
