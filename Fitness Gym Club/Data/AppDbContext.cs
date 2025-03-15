using Fitness_Gym_Club.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Gym_Club.Data
{
	public class AppDbContext : IdentityDbContext<User>
	{
		public DbSet<User> User => Set<User>();
		public DbSet<Schedule> Schedule => Set<Schedule>();
		public DbSet<GymHall> GymHall => Set<GymHall>();
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}
	}
}
