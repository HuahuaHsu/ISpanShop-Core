using ISpanShop.Models.DTOs.Members;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISpanShop.Services.Members
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetAddressListAsync(int userId);
        Task<AddressDto> CreateAddressAsync(int userId, CreateAddressDto dto);
        Task<bool> UpdateAddressAsync(int userId, UpdateAddressDto dto);
        Task<bool> DeleteAddressAsync(int userId, int addressId);
        Task<bool> SetDefaultAddressAsync(int userId, int addressId);
    }
}
