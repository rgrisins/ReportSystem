using System.IdentityModel.Tokens.Jwt;

namespace ReportSystem.Services
{
    public class UserSessionService
    {
        private readonly string _cookieName = "accessToken";

        public bool IsAuthenticated(HttpContext httpContext)
        {
            var token = GetAccessTokenFromCookie(httpContext);
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                return jwtToken.ValidTo > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        public string? GetUserName(HttpContext httpContext)
        {
            var token = GetAccessTokenFromCookie(httpContext);
            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
        }

        public string? GetUserRole(HttpContext httpContext)
        {
            var token = GetAccessTokenFromCookie(httpContext);
            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        }

        private string? GetAccessTokenFromCookie(HttpContext httpContext)
        {
            if (httpContext.Request.Cookies.TryGetValue(_cookieName, out var token))
                return token;

            return null;
        }
    }
}
