using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Fitness_Gym_Club.Models
{
	public class Customer : User
	{
		public int ScheduleId { get; set; }

		public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
	}
}
