using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Data;
using ReportSystem.Enums;
using ReportSystem.Models;
using ReportSystem.Services;

namespace ReportSystem.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtService _jwtService;
        private readonly ReportSystemDbContext _context;

        public ProfileController(UserManager<User> userManager, SignInManager<User> signInManager, JwtService jwtService, ReportSystemDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var pendingRequest = await _context.RoleRequests
                .Where(r => r.UserId == user.Id && r.Status == RequestStatus.Pending)
                .FirstOrDefaultAsync();

            var vm = new UserProfileView
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                Role = user.Role.ToString(),
                CreationDate = user.CreationDate,
                HasPendingRoleRequest = pendingRequest != null
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestEditorRole(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason) || reason.Length < 10)
            {
                TempData["ErrorMessage"] = "Please provide a reason (minimum 10 characters).";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (user.Role == UserRole.Editor || user.Role == UserRole.Admin)
            {
                TempData["ErrorMessage"] = "You already have Editor or Admin role.";
                return RedirectToAction(nameof(Index));
            }

            var existingRequest = await _context.RoleRequests
                .Where(r => r.UserId == user.Id && r.Status == RequestStatus.Pending)
                .FirstOrDefaultAsync();

            if (existingRequest != null)
            {
                TempData["ErrorMessage"] = "You already have a pending role request.";
                return RedirectToAction(nameof(Index));
            }

            var roleRequest = new RoleRequest
            {
                UserId = user.Id,
                Reason = reason,
                Status = RequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.RoleRequests.Add(roleRequest);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your Editor role request has been submitted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind("Id,UserName,Email")] UserProfileView model)
        {
            ModelState.Remove(nameof(UserProfileView.CurrentPassword));
            ModelState.Remove(nameof(UserProfileView.ConfirmPassword));
            ModelState.Remove(nameof(UserProfileView.NewPassword));

            if (!ModelState.IsValid)
                return await ReturnIndexWithErrors(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var usernameChanged = !string.Equals(user.UserName, model.UserName, StringComparison.Ordinal);
            var emailChanged = !string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase);

            if (usernameChanged)
            {
                var nameResult = await _userManager.SetUserNameAsync(user, model.UserName);
                AddErrors(nameResult);
            }
            if (emailChanged)
            {
                var emailResult = await _userManager.SetEmailAsync(user, model.Email);
                AddErrors(emailResult);
            }

            if (!ModelState.IsValid)
                return await ReturnIndexWithErrors(model);

            await _signInManager.RefreshSignInAsync(user);

            var oldRefresh = Request.Cookies["refreshToken"];
            _jwtService.GenerateAuthCookies(HttpContext, user, oldRefresh);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([Bind("CurrentPassword,NewPassword,ConfirmPassword")] UserProfileView model)
        {
            ModelState.Remove(nameof(UserProfileView.UserName));
            ModelState.Remove(nameof(UserProfileView.Email));
            ModelState.Remove(nameof(UserProfileView.Id));

            if (!ModelState.IsValid)
                return await ReturnIndexWithErrors(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword!, model.NewPassword!);
            AddErrors(result);

            if (!ModelState.IsValid)
                return await ReturnIndexWithErrors(model);

            await _signInManager.RefreshSignInAsync(user);

            var oldRefresh = Request.Cookies["refreshToken"];
            _jwtService.GenerateAuthCookies(HttpContext, user, oldRefresh);

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> ReturnIndexWithErrors(UserProfileView model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var pendingRequest = await _context.RoleRequests
                .Where(r => r.UserId == user.Id && r.Status == RequestStatus.Pending)
                .FirstOrDefaultAsync();

            var vm = new UserProfileView
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                Role = user.Role.ToString(),
                CreationDate = user.CreationDate,
                CurrentPassword = model.CurrentPassword,
                NewPassword = model.NewPassword,
                ConfirmPassword = model.ConfirmPassword,
                HasPendingRoleRequest = pendingRequest != null
            };

            return View("Index", vm);
        }

        private void AddErrors(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
            }
        }
    }
}