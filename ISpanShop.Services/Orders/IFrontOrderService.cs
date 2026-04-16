using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Orders;

namespace ISpanShop.Services.Orders
{
    public interface IFrontOrderService
    {
        Task<List<FrontOrderListDto>> GetMemberOrdersAsync(int memberId);
        Task<FrontOrderDetailDto> GetOrderDetailAsync(long orderId, int memberId);
    }
}
