using ISpanShop.Models.EfModels.DTOs;

namespace ISpanShop.MVC.Models.Orders
{
	public class OrderIndexVm
	{
		public PagedResultDto<OrderDto> Orders { get; set; }
		public OrderSearchDto Criteria { get; set; }
	}
}
