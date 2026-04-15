using ISpanShop.Models.DTOs.Auth;
using ISpanShop.Models.EfModels;
using ISpanShop.Common.Helpers;
using ISpanShop.Common.Enums;
using ISpanShop.Repositories.Members;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ISpanShop.Services.Auth
{
    public class FrontAuthService : IFrontAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;

        public FrontAuthService(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _config = config;
        }

        public async Task<FrontLoginResponseDto?> LoginAsync(FrontLoginRequestDto request)
        {
            // 1. 查詢使用者 (透過 Repository)
            var user = await _userRepository.GetByEmailOrAccountAsync(request.Account);

            if (user == null || !SecurityHelper.Verify(request.Password, user.Password))
            {
                throw new Exception("帳號或密碼錯誤");
            }

            // 2. 簽發 JWT
            var token = GenerateJwtToken(user);

            return new FrontLoginResponseDto
            {
                Token = token,
                MemberId = user.Id,
                Email = user.Email,
                Account = user.Account,
                MemberName = user.MemberProfile?.FullName ?? user.Account,
                LevelName = user.MemberProfile?.Level?.LevelName ?? "一般會員",
                PointBalance = user.MemberProfile?.PointBalance ?? 0
            };
        }

        public async Task<bool> RegisterAsync(FrontRegisterRequestDto request)
        {
            // 1. 驗證 Email/Account 是否重複 (透過 Repository)
            if (await _userRepository.ExistsAsync(request.Email, request.Account))
            {
                throw new Exception("Email 或 帳號已存在");
            }

            // 2. 建立新 User (使用 Enum)
            var user = new User
            {
                Account = request.Account,
                Password = SecurityHelper.ToBCrypt(request.Password),
                Email = request.Email,
                RoleId = (int)RoleEnum.Member,
                CreatedAt = DateTime.Now,
                IsConfirmed = true
            };

            // 3. 建立 MemberProfile (使用 Enum)
            user.MemberProfile = new MemberProfile
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                LevelId = (int)MemberLevelEnum.Normal,
                UpdatedAt = DateTime.Now,
                IsSeller = false
            };

            await _userRepository.CreateAsync(user);

            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.MemberProfile?.FullName ?? user.Account),
                new Claim("RoleId", user.RoleId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
