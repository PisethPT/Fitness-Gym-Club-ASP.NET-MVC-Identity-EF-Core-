namespace Fitness_Gym_Club.Models
{
	public class Coach : User
	{
		public TimeSpan WorkingHours { get; set; }
		public int StatusId { get; set; }
		public int SpecializationId { get; set; }
		public int ScheduleId { get; set; }

		public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
	}
}
