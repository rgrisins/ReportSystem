using Microsoft.AspNetCore.Identity;
using ReportSystem.Models;
using ReportSystem.Services;
using System.IdentityModel.Tokens.Jwt;

namespace ReportSystem.Middlewares
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenRefreshMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            JwtService jwtService,
            SessionService sessionService,
            UserManager<User> userManager)
        {
            var accessToken = context.Request.Cookies["accessToken"];
            var refreshToken = context.Request.Cookies["refreshToken"];

            bool tokenValid = false;

            if (!string.IsNullOrEmpty(accessToken) && !context.Request.Headers.ContainsKey("Authorization"))
            {
                var handler = new JwtSecurityTokenHandler();
                try
                {
                    var jwt = handler.ReadJwtToken(accessToken);
                    if (jwt.ValidTo > DateTime.UtcNow)
                        tokenValid = true;
                }
                catch
                {
                    tokenValid = false;
                }
            }

            if (!tokenValid && !string.IsNullOrEmpty(refreshToken))
            {
                var userId = sessionService.GetUserIdByRefreshToken(refreshToken);
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var newAccessToken = jwtService.GenerateAccessToken(user);
                        var newRefreshToken = jwtService.GenerateRefreshToken();
                        var expiry = jwtService.GetRefreshTokenExpiry();

                        sessionService.DeleteRefreshToken(refreshToken);
                        sessionService.SaveRefreshToken(newRefreshToken, user.Id, expiry);

                        context.Response.Cookies.Append("accessToken", newAccessToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict
                        });

                        context.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict
                        });

                        context.Request.Headers["Authorization"] = $"Bearer {newAccessToken}";
                    }
                }
            }

            await _next(context);
        }
    }
}
