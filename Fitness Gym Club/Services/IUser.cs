using Fitness_Gym_Club.Models;

namespace Fitness_Gym_Club.Services
{
	public interface IUser
	{
		Task<List<dynamic>> GetUsersWithRolesAsync();
		Task<UserException> SignUpAsync(SignUp model);
		Task<UserException> SignInAsync(SignIn model);
		Task<string> SignOutAsync();
		Task<string> EditUserAsync(User user, string Id);
		Task<User> GetUserByIdAsync(string Id);
		Task<string> DeleteUserAsync(string Id);
		Task<string> SearchUserAsync(string Email);
		Task<User> AboutMe(string Email);
		Task<UserException> UpdateAboutMe(User user, string CurrentPassword, string Password);
	}
}
