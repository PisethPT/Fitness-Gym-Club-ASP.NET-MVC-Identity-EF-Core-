using Fitness_Gym_Club.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Gym_Club.Services
{
	public class UserService : IUser
	{
		private readonly UserManager<User> userManager;
		private readonly IUserStore<User> userStore;
		private readonly SignInManager<User> signInManager;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly IUserEmailStore<User> emailStore;

		public UserService(UserManager<User> userManager, IUserStore<User> userStore, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
		{
			this.userManager = userManager;
			this.userStore = userStore;
			this.signInManager = signInManager;
			this.roleManager = roleManager;
			this.emailStore = GetEmailStore();
		}

		public async Task<string> DeleteUserAsync(string Id)
		{
			var user = await userManager.FindByIdAsync(Id);
			if(user is not null)
			{
				await userManager.DeleteAsync(user);
				return "Delete user.";
			}
			return  "User not found.";
		}

		public async Task<string> EditUserAsync(User user, string Id)
		{
			var existing = await userManager.FindByIdAsync(Id);
			if (existing is not null)
			{
				existing.UserName = user.Email;
				existing.Email = user.Email;
				existing.FirstName = user.FirstName;
				existing.LastName = user.LastName;
				existing.Gender = user.Gender;
				existing.DateOfBirth = user.DateOfBirth;
				await userManager.UpdateAsync(existing);
				return "Edit user.";
			}
			return "User not found.";
		}

		public async Task<List<dynamic>> GetUsersWithRolesAsync()
		{
			var userManage = new List<dynamic>();
			foreach (var user in await userManager.Users.ToListAsync())
			{
				userManage.Add(new
				{
					Id = user.Id,
					FullName = user.FirstName + " " + user.LastName,
					Email = user.Email,
					Gender = user.Gender == Services.Gender.Male ? "Male" : "Female",
					Roles = await userManager.GetRolesAsync(user)
				});
			}
			return userManage;
		}

		public Task<string> SearchUserAsync(string Email)
		{
			throw new NotImplementedException();
		}

		public async Task<UserException> SignUpAsync(SignUp model)
		{
			if (await userManager.FindByEmailAsync(model.Email!) is null)
			{
				var user = new User
				{
					Email = model.Email,
					UserName = model.Email,
					FirstName = model.FirstName,
					LastName = model.LastName,
					DateOfBirth = model.DateOfBirth,
					Gender = model.Gender
				};

				var existingRole = await roleManager.RoleExistsAsync(model.DefaultRole);
				if (!existingRole)
				{
					var role = new IdentityRole
					{
						Name = model.DefaultRole
					};
					var response = await roleManager.CreateAsync(role);
					if (!response.Succeeded)
					{
						return new UserException() { Message = string.Join("", response.Errors.Select(e => e.Description)), IsSuccess = false , StatusCode = 400 };
					}
				}

				var result = await userManager.CreateAsync(user, model.Password!);
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(user, model.DefaultRole);

					await emailStore.SetUserNameAsync(user, model.Email, CancellationToken.None);
					await emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
					await emailStore.SetEmailConfirmedAsync(user, true, CancellationToken.None);

					return new UserException() { Message = "SignIn", IsSuccess = true , StatusCode = 200 };
				}
				return new UserException() { Message = string.Join("", result.Errors.Select(e => e.Description)), IsSuccess = false, StatusCode = 400 };
			}
			return new UserException() { Message = "SignUp", IsSuccess = false , StatusCode = 404 };
		}

		public async Task<UserException> SignInAsync(SignIn model)
		{
			var user = await userManager.FindByEmailAsync(model.Email!);
			if (user != null)
			{
				var passwordCorrect = await userManager.CheckPasswordAsync(user, model.Password!);
				if (passwordCorrect)
				{
					var result = await signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.IsRemember, false);
					return new UserException() { Message = "Index", IsSuccess = true, StatusCode = 200 };
				}
				else
				{
					return new UserException() { Message = "Invalid password.", IsSuccess = false, StatusCode = 404 };
				}
			}
			else
			{
				return new UserException() { Message = "User not found.", IsSuccess = false, StatusCode = 404 };
			}
		}
				
			
		private IUserEmailStore<User> GetEmailStore()
		{
			if (!userManager.SupportsUserEmail)
			{
				throw new NotSupportedException("The default UI requires a user store with email support.");
			}
			return (IUserEmailStore<User>)userStore;
		}

		public async Task<User> GetUserByIdAsync(string Id) => await userManager.FindByIdAsync(Id) ?? null!;

		public async Task<string> SignOutAsync()
		{
			await signInManager.SignOutAsync();
			return "SignIn";
		}

		public async Task<User> AboutMe(string Email) => await userManager.FindByEmailAsync(Email) ?? new User();

		public async Task<UserException> UpdateAboutMe(User user, string CurrentPassword, string Password)
		{
			if(user == null)
				return new UserException() { IsSuccess = false, Message = "AboutMe" , StatusCode = 404 };
			else
			{
				var existingUser = await userManager.FindByEmailAsync(user.Email!);
				if(existingUser is not null)
				{
					var response = await userManager.ChangePasswordAsync(existingUser, CurrentPassword, Password);
					if (response.Succeeded)
						return new UserException() { IsSuccess = true, Message = "SignIn", StatusCode = 200 };
					else
						return new UserException() { IsSuccess = false, Message = string.Join("", response.Errors.Select(e => e.Description)), StatusCode = 400 };
				}
				else
					return new UserException() { IsSuccess = false, Message = "AboutMe", StatusCode = 404 };
			}
		}
	}
}
