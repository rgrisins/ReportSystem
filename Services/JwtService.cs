using Microsoft.IdentityModel.Tokens;
using ReportSystem.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ReportSystem.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private readonly SessionService _sessionService;

        public JwtService(IConfiguration config, SessionService sessionService)
        {
            _config = config;
            _sessionService = sessionService;
        }

        public string GenerateAccessToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["JwtConfig:Key"]);
            var accessTokenMinutes = int.Parse(_config["JwtConfig:AccessTokenValidityMins"]);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(accessTokenMinutes),
                Issuer = _config["JwtConfig:Issuer"],
                Audience = _config["JwtConfig:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        public void GenerateAccessTokenCookie(HttpContext httpContext, User user)
        {
            var token = GenerateAccessToken(user);
            var minutes = int.Parse(_config["JwtConfig:AccessTokenValidityMins"]);
            httpContext.Response.Cookies.Append("accessToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(minutes)
            });
        }

        public (string accessToken, string refreshToken) GenerateAuthCookies(HttpContext httpContext, User user, string oldRefreshToken = null!)
        {
            var accessToken = GenerateAccessToken(user);
            var minutes = int.Parse(_config["JwtConfig:AccessTokenValidityMins"]);
            httpContext.Response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(minutes)
            });

            if (!string.IsNullOrEmpty(oldRefreshToken))
            {
                _sessionService.DeleteRefreshToken(oldRefreshToken);
            }

            var refreshToken = GenerateRefreshToken();
            _sessionService.SaveRefreshToken(refreshToken, user.Id, GetRefreshTokenExpiry());

            httpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.Add(GetRefreshTokenExpiry())
            });

            return (accessToken, refreshToken);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public TimeSpan GetRefreshTokenExpiry()
        {
            var days = int.Parse(_config["JwtConfig:RefreshTokenValidityDays"]);
            return TimeSpan.FromDays(days);
        }
    }
}
