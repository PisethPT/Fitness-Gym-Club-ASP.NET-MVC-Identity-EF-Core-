	using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitness_Gym_Club.Models
{
	public class Schedule
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ScheduleId { get; set; }
		[MaxLength(500)]
		public string? Type { get; set; }
		public string? CustomerId { get; set; }
		public string? CoachId { get; set; }
		public int GymHallId { get; set; }
		public TimeSpan StartTime { get; set; } = DateTime.Now.TimeOfDay;
		public TimeSpan EndTime { get; set; } = DateTime.Now.TimeOfDay;

		public virtual ICollection<GymHall> GymHalls { get; set; } = new List<GymHall>();
		public virtual Coach? Coach { get; set; }
		public virtual Customer? Customer { get; set; }

		[NotMapped]
		public List<SelectListItem> SelectCoachListItem = new List<SelectListItem>();
		[NotMapped]
		public List<SelectListItem> SelectCustomerListItem = new List<SelectListItem>();
		[NotMapped]
		public List<SelectListItem> SelectGymHallListItem = new List<SelectListItem>();
		[NotMapped]
		public List<string> CustomersId { get; set; } = new List<string>();
	}
}
