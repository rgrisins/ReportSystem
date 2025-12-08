using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Data;
using ReportSystem.Enums;
using ReportSystem.Models;
using ReportSystem.Services;

namespace ReportSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtService _jwtService;
        private readonly ReportSystemDbContext _context;

        public AdminController(UserManager<User> userManager, JwtService jwtService, ReportSystemDbContext context)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _context = context;
        }

        // GET: Admin - Main dashboard
        public IActionResult Index()
        {
            return View();
        }

        // GET: Admin/Users
        public IActionResult Users(string userRole, string searchString, DateTime? dateFrom, DateTime? dateTo)
        {
            var filteredUsers = FilterUsersByDate(SearchUsers(FilterUsersByRole(GetUsers(), userRole), searchString), dateFrom, dateTo);

            var viewModel = new UserViewModel
            {
                Users = filteredUsers.ToList(),
                Roles = new SelectList(Enum.GetValues(typeof(UserRole))),
                RoleCounts = GetRoleCounts(filteredUsers),
                TotalCount = GetFilteredUserCount(filteredUsers),
                UserRole = userRole,
                SearchString = searchString,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            return View(viewModel);
        }

        // GET: Admin/RoleRequests
        public async Task<IActionResult> RoleRequests(string status)
        {
            var query = _context.RoleRequests
                .Include(r => r.User)
                .Include(r => r.ReviewedByUser)
                .OrderByDescending(r => r.RequestedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RequestStatus>(status, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }

            var requests = await query.ToListAsync();
            ViewData["StatusFilter"] = status;

            return View(requests);
        }

        // POST: Admin/ApproveRoleRequest/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRoleRequest(int id, string? reviewNote)
        {
            var request = await _context.RoleRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(request.UserId!);
            if (user == null)
                return NotFound();

            user.Role = UserRole.Editor;
            await _userManager.UpdateAsync(user);

            request.Status = RequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedBy = _userManager.GetUserId(User);
            request.ReviewNote = reviewNote;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Role request approved. {user.UserName} is now an Editor.";
            return RedirectToAction(nameof(RoleRequests));
        }

        // POST: Admin/DeclineRoleRequest/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineRoleRequest(int id, string? reviewNote)
        {
            var request = await _context.RoleRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            request.Status = RequestStatus.Denied;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedBy = _userManager.GetUserId(User);
            request.ReviewNote = reviewNote;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Role request has been denied.";
            return RedirectToAction(nameof(RoleRequests));
        }

        // GET: Admin/Details/5
        [HttpGet]
        public IActionResult UserDetails(string id)
        {
            var user = GetUserByID(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: Admin/Delete/5
        [HttpGet]
        public IActionResult UserDelete(string id)
        {
            var user = GetUserByID(id);
            if (user == null)
                return NotFound();
            return View(user);
        }

        // POST: Admin/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id, bool notUsed)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (id == currentUserId)
            {
                return RedirectToAction(nameof(Users));
            }

            var user = GetUserByID(id);
            if (user == null)
                return NotFound();

            var result = _userManager.DeleteAsync(user).Result;
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Users));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Delete", user);
            }
        }

        // GET: Admin/Edit/5
        public IActionResult UserEdit(string id)
        {
            var user = GetUserByID(id);
            if (user == null)
                return NotFound();
            return View(user);
        }

        // POST: Admin/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,Email,Role,CreationDate")] User editedUser)
        {
            if (id != editedUser.Id)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    user.UserName = editedUser.UserName;
                    user.Email = editedUser.Email;

                    if (user.Role != UserRole.Admin && id != currentUserId)
                    {
                        user.Role = editedUser.Role;
                    }

                    user.CreationDate = DateTime.SpecifyKind(editedUser.CreationDate, DateTimeKind.Utc);

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        if (id == currentUserId)
                        {
                            var oldRefresh = Request.Cookies["refreshToken"];
                            _jwtService.GenerateAuthCookies(HttpContext, user, oldRefresh);
                        }
                        return RedirectToAction(nameof(Users));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }

            return View(editedUser);
        }

        public User GetUserByID(string id)
        {
            return _userManager.FindByIdAsync(id).Result;
        }

        public IQueryable<User> GetUsers()
        {
            return _userManager.Users.OrderBy(u => u.UserName);
        }

        private IQueryable<User> FilterUsersByRole(IQueryable<User> users, string role)
        {
            if (string.IsNullOrEmpty(role))
                return users;
            return users.Where(u => u.Role.ToString() == role);
        }

        private IQueryable<User> FilterUsersByDate(IQueryable<User> users, DateTime? dateFrom, DateTime? dateTo)
        {
            if (dateFrom.HasValue)
            {
                users = users.Where(u => u.CreationDate >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                users = users.Where(u => u.CreationDate <= dateTo.Value);
            }

            return users;
        }

        private IQueryable<User> SearchUsers(IQueryable<User> users, string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return users;

            searchString = searchString.ToLower();

            return users.Where(u =>
                u.UserName.ToLower().Contains(searchString) ||
                u.Email.ToLower().Contains(searchString));
        }

        private int GetFilteredUserCount(IQueryable<User> users)
        {
            return users.Count();
        }


        private Dictionary<UserRole, int> GetRoleCounts(IQueryable<User> users)
        {
            var roleCounts = new Dictionary<UserRole, int>();
            foreach (UserRole status in Enum.GetValues(typeof(UserRole)))
            {
                roleCounts[status] = users.Count(u => u.Role == status);
            }
            return roleCounts;
        }
    }
}
