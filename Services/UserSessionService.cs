using System.IdentityModel.Tokens.Jwt;

namespace ReportSystem.Services
{
    public class UserSessionService
    {
        private readonly SessionService _sessionService;

        // Constructor, that sets the SessionService dependency
        public UserSessionService(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        // Method to check if the user is authenticated based on the session_id cookie
        public bool IsAuthenticated(HttpContext httpContext)
        {
            var sessionId = httpContext.Request.Cookies["SessionId"];
            if (string.IsNullOrEmpty(sessionId)) return false;
            var token = _sessionService.GetToken(sessionId);
            return token != null;
        }

        // Method to get the username from the JWT token stored in the Redis session
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

        // Method to get the user role from the JWT token stored in the Redis session
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
