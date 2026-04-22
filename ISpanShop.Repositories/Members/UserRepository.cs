using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Repositories.Members
{
    public class UserRepository : IUserRepository
    {
        private readonly ISpanShopDBContext _db;

        public UserRepository(ISpanShopDBContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByEmailOrAccountAsync(string emailOrAccount)
        {
            return await _db.Users
                .Include(u => u.MemberProfile)
                    .ThenInclude(mp => mp.Level)
                .Include(u => u.Stores)
                .FirstOrDefaultAsync(u => u.Email == emailOrAccount || u.Account == emailOrAccount);
        }

        public async Task<bool> ExistsAsync(string email, string account)
        {
            return await _db.Users.AnyAsync(u => u.Email == email || u.Account == account);
        }

        public async Task CreateAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _db.Users
                .Include(u => u.MemberProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> UpdatePasswordHashAsync(int userId, string newHash)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                user.Password = newHash;
                user.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
