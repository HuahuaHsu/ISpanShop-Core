using ISpanShop.Models.EfModels;

namespace ISpanShop.Repositories.Members
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailOrAccountAsync(string emailOrAccount);
        Task<bool> ExistsAsync(string email, string account);
        Task CreateAsync(User user);
        Task<User?> GetByIdAsync(int userId);
        Task<bool> UpdatePasswordHashAsync(int userId, string newHash);
    }
}
