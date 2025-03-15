using Fitness_Gym_Club.Services;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitness_Gym_Club.Models
{
	public class User : IdentityUser 
	{
		[Required(ErrorMessage = "First name is required")]
		[MaxLength(100)]
		public string? FirstName { get; set; }
		[Required(ErrorMessage = "Last name is required")]
		[MaxLength(100)]
		public string? LastName { get; set; }
		public DateTime DateOfBirth { get; set; } = DateTime.Now;
		public Gender Gender { get; set; }
	}
}
