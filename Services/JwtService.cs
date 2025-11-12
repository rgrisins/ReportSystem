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

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        // Method to generate JWT access token
        public string GenerateAccessToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["JwtConfig:Key"]);
            var accessTokenMinutes = int.Parse(_config["JwtConfig:AccessTokenValidityMins"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(accessTokenMinutes),
                Issuer = _config["JwtConfig:Issuer"],
                Audience = _config["JwtConfig:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // 
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
