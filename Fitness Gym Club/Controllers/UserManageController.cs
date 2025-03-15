using Fitness_Gym_Club.Models;
using Fitness_Gym_Club.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fitness_Gym_Club.Controllers
{
	[Authorize(Roles = "Senior Supervisor")]
	public class UserManageController : Controller
    {
		private readonly IUser userService;

		public UserManageController(IUser userService) => this.userService = userService;

		[HttpGet]
		public async Task<IActionResult> Index() => View(await userService.GetUsersWithRolesAsync());

		[HttpGet]
		public IActionResult AddNewUser() => View(new SignUp());

		[HttpPost]
		public async Task<IActionResult> SaveAddNewUser(SignUp model)
		{
			if (ModelState.IsValid)
			{
				var response = await userService.SignUpAsync(model);
				if (response.IsSuccess)
					return View("Index", await userService.GetUsersWithRolesAsync());
				else
					return View("AddNewUser", model);
			}
			return View("AddNewUser", model);
		}

		[HttpPost]
		public async Task<IActionResult> EditUser(string Id)
		{
			var model = await userService.GetUserByIdAsync(Id);
			if (model is null)
				return View("Index", await userService.GetUsersWithRolesAsync());
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> SaveEditUser(User model)
		{
			if (ModelState.IsValid)
			{
				await userService.EditUserAsync(model, model.Id);
				return View("Index", await userService.GetUsersWithRolesAsync());
			}
			return View("EditUser", model);
		}

		[HttpPost]
		public async Task<IActionResult> DeleteUser(string Id)
		{
			await userService.DeleteUserAsync(Id);
			return View("Index", await userService.GetUsersWithRolesAsync());
		}
	}
}
