namespace Fitness_Gym_Club.Models
{
	public class UserException
	{
		public string? Message { get; set; }
		public bool IsSuccess { get; set; }
		public int StatusCode { get; set; }
	}
}
