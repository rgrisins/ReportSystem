using Microsoft.IdentityModel.Tokens;
using ReportSystem.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ReportSystem.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private readonly TimeSpan _expiry = TimeSpan.FromHours(2);

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["JwtConfig:Key"]);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_expiry),
                Issuer = _config["JwtConfig:Issuer"],
                Audience = _config["JwtConfig:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
