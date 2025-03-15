using Fitness_Gym_Club.Services;
using System.ComponentModel.DataAnnotations;

namespace Fitness_Gym_Club.Models
{
	public class SignUp
	{
		[Required(ErrorMessage = "First name is required")]
		[MaxLength(100)]
		public string? FirstName { get; set; }
		[Required(ErrorMessage = "Last name is required")]
		[MaxLength(100)]
		public string? LastName { get; set; }
		[Required(ErrorMessage = "Email is required")]
		[DataType(DataType.EmailAddress)]
		public string? Email { get; set; }
		[Required(ErrorMessage = "Please enter your password.")]
		[DataType(DataType.Password)]
		public string? Password { get; set; }

		[Required(ErrorMessage = "Please select your date of birth")]
		[DataType(DataType.Date)]
		public DateTime DateOfBirth { get; set; } = DateTime.Now;

		public Gender Gender { get; set; }
		public readonly string DefaultRole = "Customer";
	}
}
