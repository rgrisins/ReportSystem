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
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        JwtService jwtService,
        SessionService sessionService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _sessionService = sessionService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [IgnoreAntiforgeryToken]
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

        _jwtService.GenerateAuthCookies(HttpContext, user);

        ClearAntiforgeryCookies();

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [IgnoreAntiforgeryToken]
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

        _jwtService.GenerateAuthCookies(HttpContext, user);

        ClearAntiforgeryCookies();

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

        ClearAntiforgeryCookies();

        return RedirectToAction("Login");
    }

    private void ClearAntiforgeryCookies()
    {
        var antiforgeryCookies = Request.Cookies.Keys
            .Where(k => k.StartsWith(".AspNetCore.Antiforgery"))
            .ToList();

        foreach (var cookie in antiforgeryCookies)
        {
            Response.Cookies.Delete(cookie);
        }
    }
}