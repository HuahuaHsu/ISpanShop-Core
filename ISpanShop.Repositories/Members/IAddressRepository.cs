using ISpanShop.Models.EfModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.Members
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetAllByUserIdAsync(int userId);
        Task<Address> GetByIdAsync(int id, int userId);
        Task AddAsync(Address address);
        Task UpdateAsync(Address address);
        Task DeleteAsync(int id, int userId);
        Task SetDefaultAsync(int id, int userId);
    }
}
