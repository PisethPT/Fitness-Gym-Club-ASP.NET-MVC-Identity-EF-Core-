using Fitness_Gym_Club.Services;
using Microsoft.AspNetCore.Identity;

namespace Fitness_Gym_Club.Models
{
	public class UserRoles
	{
		public string? UserId { get; set; }
		public string? Email { get; set; }
		public string? UserName { get; set; }
		public string? Gender { get; set; }
		public string? RoleName { get; set; }
		public bool IsChecked { get; set; }
		public List<Roles>? Roles { get; set; } = new List<Roles>();
	}
}
