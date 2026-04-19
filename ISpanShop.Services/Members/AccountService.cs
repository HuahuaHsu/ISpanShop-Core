using ISpanShop.Common.Helpers;
using ISpanShop.Models.DTOs.Auth;
using ISpanShop.Repositories.Members;

namespace ISpanShop.Services.Members
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;

        public AccountService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordDto dto)
        {
            // 1. 取得使用者
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                return (false, "找不到該使用者");
            }

            // 2. 驗證舊密碼
            if (!SecurityHelper.Verify(dto.OldPassword, user.Password))
            {
                return (false, "舊密碼錯誤");
            }

            // 3. 確認新舊密碼不同
            if (dto.OldPassword == dto.NewPassword)
            {
                return (false, "新密碼不能與舊密碼相同");
            }

            // 4. 更新密碼
            var newHash = SecurityHelper.ToBCrypt(dto.NewPassword);
            await _userRepository.UpdatePasswordHashAsync(dto.UserId, newHash);

            return (true, "密碼變更成功");
        }
    }
}
