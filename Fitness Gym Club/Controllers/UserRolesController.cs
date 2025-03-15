using Fitness_Gym_Club.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Gym_Club.Controllers
{
	[Authorize(Roles = "Senior Supervisor")]
	public class UserRolesController : Controller
    {
		private readonly UserManager<User> userManager;
		private readonly RoleManager<IdentityRole> roleManager;

		public UserRolesController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
		{
			this.userManager = userManager;
			this.roleManager = roleManager;
		}

		[HttpGet]
		public IActionResult Index() => View(new UserRoles());

        [HttpPost]
        public async Task<IActionResult> SearchUser(UserRoles model)
        {
			if (ModelState.IsValid)
			{
				var existingUser = await userManager.FindByEmailAsync(model.Email!);
				if(existingUser is not null)
				{
					model.UserId = existingUser.Id;
					model.UserName = $"{existingUser.FirstName} {existingUser.LastName}";
					model.Gender = existingUser.Gender == Services.Gender.Male ? "Male" : "Female";
					var userRoles = await userManager.GetRolesAsync(existingUser);
					model.Roles = await roleManager.Roles.Select(role => new Roles
					{
						Id = role.Id,
						Name = role.Name,
						IsChecked = userRoles.Contains(role.Name!)
					}).ToListAsync();
				}
				return View("Index", model);
			}else
				return View("Index", model);
		}

		[HttpPost]
		public async Task<JsonResult> SaveUserRole(string userId, string roleName, bool isChecked)
		{
			var user = await userManager.FindByIdAsync(userId);
			if(user is not null)
			{
				if (isChecked)
					await userManager.AddToRoleAsync(user, roleName);
				else
					await userManager.RemoveFromRoleAsync(user, roleName);
			}
			return Json(new { success = true, statusCode = 200, message = "OK" });
		}

		[HttpPost]
		public async Task<IActionResult> CreateRole(UserRoles model)
		{
			if (!string.IsNullOrEmpty(model.RoleName))
			{
				var response = await roleManager.RoleExistsAsync(model.RoleName);
				if(!response)
				{
					var role = new IdentityRole
					{
						Name = model.RoleName
					};
					await roleManager.CreateAsync(role);
					return View("Index", model);
				}
			}
			return View("Index", model);
		}
    }
}
