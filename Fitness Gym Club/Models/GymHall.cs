using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Fitness_Gym_Club.Models
{
	public class GymHall
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int GymHallId { get; set; }
		public string? Name { get; set; }
		public byte Floor { get; set; }
		public string? RoomNumber { get; set; }
		public int? Capacity { get; set; }
		public string? Facilities { get; set; }
	}
}
