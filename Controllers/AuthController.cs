using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Data;
using ReportSystem.Enums;
using ReportSystem.Models;
using ReportSystem.Services;

public class AuthController : Controller
{
    private readonly ReportSystemContext _context;
    private readonly JwtService _jwtService;
    private readonly SessionService _sessionService;

    // Constructor that sets the ReportSystemContext, JwtService and SessionService dependencies
    public AuthController(ReportSystemContext context, JwtService jwtService, SessionService sessionService)
    {
        _context = context;
        _jwtService = jwtService;
        _sessionService = sessionService;
    }

    // GET Register page
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST Register form
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        if (_context.User.Any(u => u.Email == request.Email))
        {
            ModelState.AddModelError("", "Selected Email is already in use.");
            return View(request);
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Role = UserRole.User,
        };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        _context.User.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);
        var sessionId = Guid.NewGuid().ToString();
        _sessionService.SaveSession(sessionId, token, TimeSpan.FromHours(2));

        Response.Cookies.Append("SessionId", sessionId);

        return RedirectToAction("Index", "Home");
    }

    // GET Login page
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST Login form
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            ModelState.AddModelError("", "User with the specified email does not exist");
            return View(request);
        }

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Invalid credentials");
            return View(request);
        }

        var token = _jwtService.GenerateToken(user);
        var sessionId = Guid.NewGuid().ToString();
        _sessionService.SaveSession(sessionId, token, TimeSpan.FromHours(2));

        Response.Cookies.Append("SessionId", sessionId);

        return RedirectToAction("Index", "Home");
    }

    // POST Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        if (Request.Cookies.TryGetValue("SessionId", out var sessionId))
        {
            _sessionService.DeleteSession(sessionId);
            Response.Cookies.Delete("SessionId");
        }

        return RedirectToAction("Login");
    }
}
