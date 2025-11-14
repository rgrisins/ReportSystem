using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Enums;
using ReportSystem.Models;

namespace ReportSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;

        public AdminController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // GET: Admin
        public IActionResult Index(string userRole, string searchString, DateTime? dateFrom, DateTime? dateTo)
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

        // GET: Admin/Details/5
        [HttpGet]
        public IActionResult Details(string id)
        {
            var user = GetUserByID(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: Admin/Delete/5
        [HttpGet]
        public IActionResult Delete(string id)
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
                return RedirectToAction(nameof(Index));
            }

            var user = GetUserByID(id);
            if (user == null)
                return NotFound();

            var result = _userManager.DeleteAsync(user).Result;
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
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
        public IActionResult Edit(string id)
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

                    user.CreationDate = editedUser.CreationDate;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
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
