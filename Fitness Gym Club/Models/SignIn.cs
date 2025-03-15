using System.ComponentModel.DataAnnotations;

namespace Fitness_Gym_Club.Models
{
	public class SignIn
	{
		[Required(ErrorMessage = "Email must be provided.")]
		[DataType(DataType.EmailAddress)]
		public string? Email { get; set; }
		[Required(ErrorMessage = "Password must be provided.")]
		[DataType(DataType.Password)]
		public string? Password { get; set; }
		public bool IsRemember { get; set; }
	}
}
