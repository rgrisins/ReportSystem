using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReportSystem.Enums;
using ReportSystem.Models;
using ReportSystem.Services;

public class AuthController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtService _jwtService;
    private readonly SessionService _sessionService;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        JwtService jwtService,
        SessionService sessionService,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _sessionService = sessionService;
        _config = config;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("", "Email is already in use.");
            return View(request);
        }

        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            Role = UserRole.User
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(request);
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        _sessionService.SaveRefreshToken(refreshToken, user.Id.ToString(), _jwtService.GetRefreshTokenExpiry());

        Response.Cookies.Append("accessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtConfig:AccessTokenValidityMins"]))
        });

        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.Add(_jwtService.GetRefreshTokenExpiry())
        });

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found");
            return View(request);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            ModelState.AddModelError("", "Invalid password");
            return View(request);
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        _sessionService.SaveRefreshToken(refreshToken, user.Id.ToString(), _jwtService.GetRefreshTokenExpiry());

        Response.Cookies.Append("accessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtConfig:AccessTokenValidityMins"]))
        });

        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.Add(_jwtService.GetRefreshTokenExpiry())
        });

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            _sessionService.DeleteRefreshToken(refreshToken);
            Response.Cookies.Delete("refreshToken");
        }

        if (Request.Cookies.TryGetValue("accessToken", out var accessToken))
        {
            Response.Cookies.Delete("accessToken");
        }

        await _signInManager.SignOutAsync();

        return RedirectToAction("Login");
    }

    [HttpPost]
    public IActionResult Refresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return Unauthorized();

        var userId = _sessionService.GetUserIdByRefreshToken(refreshToken);
        if (userId == null)
            return Unauthorized();

        var user = _userManager.FindByIdAsync(userId).Result;
        if (user == null)
            return Unauthorized();

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        _sessionService.DeleteRefreshToken(refreshToken);
        _sessionService.SaveRefreshToken(newRefreshToken, user.Id.ToString(), _jwtService.GetRefreshTokenExpiry());

        Response.Cookies.Append("accessToken", newAccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtConfig:AccessTokenValidityMins"]))
        });

        Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.Add(_jwtService.GetRefreshTokenExpiry())
        });

        return Ok(new { AccessToken = newAccessToken });
    }
}
