using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Repositories.Members
{
	public class PasswordResetTokenRepository : IPasswordResetTokenRepository
	{
		private readonly ISpanShopDBContext _db;

		public PasswordResetTokenRepository(ISpanShopDBContext db)
		{
			_db = db;
		}

		public async Task CreateAsync(PasswordResetToken token)
		{
			_db.PasswordResetTokens.Add(token);
			await _db.SaveChangesAsync();
		}

		public async Task<PasswordResetToken?> GetByTokenAsync(string token)
		{
			return await _db.PasswordResetTokens
				.FirstOrDefaultAsync(t => t.Token == token);
		}

		// ¡ö Email §ï¬° UserId
		public async Task DeleteByUserIdAsync(int userId)
		{
			var tokens = await _db.PasswordResetTokens
				.Where(t => t.UserId == userId)
				.ToListAsync();

			if (tokens.Any())
			{
				_db.PasswordResetTokens.RemoveRange(tokens);
				await _db.SaveChangesAsync();
			}
		}

		public async Task DeleteExpiredTokensAsync()
		{
			var now = DateTime.Now;
			var expiredTokens = await _db.PasswordResetTokens
				.Where(t => t.ExpiryDate < now)
				.ToListAsync();

			if (expiredTokens.Any())
			{
				_db.PasswordResetTokens.RemoveRange(expiredTokens);
				await _db.SaveChangesAsync();
			}
		}
	}
}