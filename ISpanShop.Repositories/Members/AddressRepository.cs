using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.Members
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ISpanShopDBContext _context;

        public AddressRepository(ISpanShopDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Address>> GetAllByUserIdAsync(int userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.Id)
                .ToListAsync();
        }

        public async Task<Address> GetByIdAsync(int id, int userId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        }

        public async Task AddAsync(Address address)
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Address address)
        {
            _context.Entry(address).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var address = await GetByIdAsync(id, userId);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetDefaultAsync(int id, int userId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            foreach (var address in addresses)
            {
                address.IsDefault = (address.Id == id);
            }

            await _context.SaveChangesAsync();
        }
    }
}
