using System.IdentityModel.Tokens.Jwt;

namespace ReportSystem.Services
{
    public class UserSessionService
    {
        private readonly SessionService _sessionService;

        public UserSessionService(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public bool IsAuthenticated(HttpContext httpContext)
        {
            var sessionId = httpContext.Request.Cookies["SessionId"];
            if (string.IsNullOrEmpty(sessionId)) return false;
            var token = _sessionService.GetToken(sessionId);
            return token != null;
        }

        public string? GetUserName(HttpContext httpContext)
        {
            var sessionId = httpContext.Request.Cookies["SessionId"];
            if (string.IsNullOrEmpty(sessionId))
                return null;

            var token = _sessionService.GetToken(sessionId);
            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;

            return userName;
        }

        public string? GetUserRole(HttpContext httpContext)
        {
            var sessionId = httpContext.Request.Cookies["SessionId"];
            if (string.IsNullOrEmpty(sessionId))
                return null;
            var token = _sessionService.GetToken(sessionId);
            if (string.IsNullOrEmpty(token))
                return null;
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userRole = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            return userRole;
        }
    }
}
