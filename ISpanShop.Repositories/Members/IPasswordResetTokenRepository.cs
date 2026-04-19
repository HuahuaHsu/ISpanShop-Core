using ISpanShop.Models.EfModels;

namespace ISpanShop.Repositories.Members
{
	public interface IPasswordResetTokenRepository
	{
		Task CreateAsync(PasswordResetToken token);
		Task<PasswordResetToken?> GetByTokenAsync(string token);
		Task DeleteByUserIdAsync(int userId);      // ¡ö Email §ï¬° UserId
		Task DeleteExpiredTokensAsync();
	}
}