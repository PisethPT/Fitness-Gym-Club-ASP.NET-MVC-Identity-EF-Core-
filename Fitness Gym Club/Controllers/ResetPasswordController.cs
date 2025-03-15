using Fitness_Gym_Club.Models;
using Fitness_Gym_Club.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Fitness_Gym_Club.Controllers
{
	[Authorize(Roles = "Senior Supervisor")]
	public class ResetPasswordController : Controller
    {
		private readonly UserManager<User> userManager;
		private readonly IUser userService;

		public ResetPasswordController(UserManager<User> userManager, IUser userService)
		{
			this.userManager = userManager;
			this.userService = userService;
		} 

		[HttpGet]
		public IActionResult Index()
		{
			ViewBag.Valid = false;
			return View(new UserRoles());
		}

		[HttpPost]
		public async Task<IActionResult> SearchUser(UserRoles model)
		{
			if (ModelState.IsValid)
			{
				var existingUser = await userManager.FindByEmailAsync(model.Email!);
				if (existingUser is not null)
				{
					model.UserId = existingUser.Id;
					model.UserName = $"{existingUser.FirstName} {existingUser.LastName}";
					model.Gender = existingUser.Gender == Services.Gender.Male ? "Male" : "Female";
					var userRoles = await userManager.GetRolesAsync(existingUser);
					ViewBag.Valid = true;
					return View("Index", model);
				}else
				{
					ViewBag.Valid = false;
					return View("Index", model);
				}
			}
			else
			{
				ViewBag.Valid = false;
				return View("Index", model);
			}	
		}

		[HttpPost]
		public async Task<IActionResult> SaveResetPassword(UserRoles model, string NewPassword)
		{
			var existingUser = await userManager.FindByEmailAsync(model.Email!);
			if (existingUser is not null) {
				var passwordHash = userManager.PasswordHasher.HashPassword(existingUser, NewPassword);
				existingUser.PasswordHash = passwordHash;
				var result = await userManager.UpdateAsync(existingUser);
				return RedirectToAction(await userService.SignOutAsync(), "Home");
			}
			else
			{
				ViewBag.Valid = false;
				return View("Index", model);
			}
		}
	}
}
