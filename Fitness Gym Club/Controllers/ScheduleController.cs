using Fitness_Gym_Club.Data;
using Fitness_Gym_Club.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Fitness_Gym_Club.Controllers
{
    public class ScheduleController : Controller
    {
		private readonly AppDbContext context;
		private readonly UserManager<User> userManager;

		public ScheduleController(AppDbContext context, UserManager<User> userManager)
		{
			this.context = context;
			this.userManager = userManager;
		}

		[Authorize(Roles = "Senior Supervisor")]
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var schedules = await context.Schedule
				.GroupBy(s => new { s.Type, s.CoachId, s.StartTime, s.EndTime })
				.Select(s => new
				{
					Type = s.Key.Type,
					CoachId = s.Key.CoachId,
					CoachName = userManager.Users
						.Where(u => u.Id == s.Key.CoachId)
						.Select(u => string.Concat(u.FirstName, " ", u.LastName))
						.FirstOrDefault(),
					GymHallId = s.Select(s => s.GymHallId).FirstOrDefault(),
					GymHallName = context.GymHall
						.Where(g => g.GymHallId == s.Select(s => s.GymHallId).FirstOrDefault())
						.Select(g => g.RoomNumber)
						.FirstOrDefault(),
					Customers = s.Select(s => s.CustomerId).Count(),
					StartTime = DateTime.Parse(s.Key.StartTime.ToString()),
					EndTime = DateTime.Parse(s.Key.EndTime.ToString()),
				})
				.ToListAsync();
			return View(schedules);
		}
		
		[Authorize(Roles = "Customer ,Coach, Senior Coach, Gym Hall Supervisor")] // for all roles except Senior Supervisor
		public async Task<IActionResult> UserSchedules()
		{
			var userEmail = User.Identity!.Name;
			var user = userManager.Users.FirstOrDefault(u => u.Email == userEmail);
			if (user == null)
				return RedirectToAction("SignIn", "Home");
			else
			{
				var schedules = await context.Schedule
					.Where(s => s.CustomerId == user.Id)
					.Select(s => new
					{
						Type = s.Type,
						CoachName = userManager.Users
							.Where(u => u.Id == s.CoachId)
							.Select(u => string.Concat(u.FirstName, " ", u.LastName))
							.FirstOrDefault(),
						GymHallName = context.GymHall
							.Where(g => g.GymHallId == s.GymHallId)
							.Select(g => g.RoomNumber)
							.FirstOrDefault(),
						StartTime = DateTime.Parse(s.StartTime.ToString()),
						EndTime = DateTime.Parse(s.EndTime.ToString()),
					})
					.ToListAsync();
				return View(schedules);
			}
		}

		[Authorize(Roles = "Senior Supervisor")]
		[HttpGet]
		public async Task<IActionResult> CreateSchedule()
		{
			var model = new Schedule();
			var coachs = await userManager.GetUsersInRoleAsync("Coach");
			var seniorCoachs = await userManager.GetUsersInRoleAsync("Senior Coach");
			var combinedCoachs = coachs.Concat(seniorCoachs).ToList();

			foreach (var user in combinedCoachs)
			{
				var roles = await userManager.GetRolesAsync(user);
				string roleNames = string.Join(", ", roles.Where(r => r.Equals("Coach") || r.Equals("Senior Coach")));

				model.SelectCoachListItem.Add(new SelectListItem
				{
					Value = user.Id,
					Text = $"{user.FirstName} {user.LastName} ({roleNames})"
				});
			}

			var customers = await userManager.GetUsersInRoleAsync("Customer");
			foreach (var user in customers)
			{
				var roles = await userManager.GetRolesAsync(user);
				string roleNames = string.Join(", ", roles.Where(r => r.Equals("Customer")));

				model.SelectCustomerListItem.Add(new SelectListItem
				{
					Value = user.Id,
					Text = $"{user.FirstName} {user.LastName} ({roleNames})"
				});
			}

			var gymHalls = await context.GymHall.Select(g => new
			{
				GymHallId = g.GymHallId,
				Name = g.Name,
				RoomNumber = g.RoomNumber,
				Floor = g.Floor
			}).ToListAsync();

			foreach (var gymHall in gymHalls)
			{
				model.SelectGymHallListItem.Add(new SelectListItem
				{
					Value = gymHall.GymHallId.ToString(),
					Text = $"{gymHall.Name} - Room: { gymHall.RoomNumber} (Floor: {gymHall.Floor})"
				});
			}

			return View(model);
		}

		[Authorize(Roles = "Senior Supervisor")]
		[HttpPost]
		public async Task<IActionResult> SaveCreateSchedule(Schedule model)
		{
			var customes = model.CustomersId.GroupBy(c => c).Select(c => new { customerId = c.Key }).ToList();
			var existingSchedule = await context.Schedule.FirstOrDefaultAsync(s => s.CoachId == model.CoachId
																				&& s.GymHallId == model.GymHallId
																				&& s.Type == model.Type
																				&& s.StartTime.Hours == model.StartTime.Hours
																				&& s.StartTime.Minutes == model.StartTime.Minutes
																				&& s.EndTime.Hours == model.EndTime.Hours
																				&& s.EndTime.Minutes == model.EndTime.Minutes);
			if (existingSchedule is null)
			{
				foreach(var customer in customes)
				{
					var schedule = new Schedule()
					{ 
						Type = model.Type, 
						CoachId = model.CoachId,
						CustomerId = customer.customerId,
						GymHallId = model.GymHallId,
						StartTime = model.StartTime,
						EndTime = model.EndTime
					};

					await context.Schedule.AddAsync(schedule);
				}
				await context.SaveChangesAsync();
			}

			var schedules = await context.Schedule
				.GroupBy(s => new { s.Type, s.CoachId, s.StartTime, s.EndTime })
				.Select(s => new
				{
					Type = s.Key.Type,
					CoachId = s.Key.CoachId,
					CoachName = userManager.Users
						.Where(u => u.Id == s.Key.CoachId)
						.Select(u => string.Concat(u.FirstName, " ", u.LastName))
						.FirstOrDefault(),
					GymHallId = s.Select(s => s.GymHallId).FirstOrDefault(),
					GymHallName = context.GymHall
						.Where(g => g.GymHallId == s.Select(s => s.GymHallId).FirstOrDefault())
						.Select(g => g.RoomNumber)
						.FirstOrDefault(),
					Customers = s.Select(s => s.CustomerId).Count(),
					StartTime = DateTime.Parse(s.Key.StartTime.ToString()),
					EndTime = DateTime.Parse(s.Key.EndTime.ToString()),
				})
				.ToListAsync();

			return View("Index", schedules);
		}

		[Authorize(Roles = "Senior Supervisor")]
		[HttpPost]
		public async Task<IActionResult> EditSchedule(Schedule model)
		{
			var existingSchedule = await context.Schedule.Where(s => s.CoachId == model.CoachId
																  && s.GymHallId == model.GymHallId
																  && s.Type == model.Type
																  && s.StartTime.Hours == model.StartTime.Hours
																  && s.StartTime.Minutes == model.StartTime.Minutes
																  && s.EndTime.Hours == model.EndTime.Hours
																  && s.EndTime.Minutes == model.EndTime.Minutes)
														 .Select(s => new { customerId = s.CustomerId!.ToString() })
														 .ToListAsync();

			if (existingSchedule.Any())
			{
				model.CustomersId.AddRange(existingSchedule.Select(id => id.customerId).ToList());
				var coachs = await userManager.GetUsersInRoleAsync("Coach");
				var seniorCoachs = await userManager.GetUsersInRoleAsync("Senior Coach");
				var combinedCoachs = coachs.Concat(seniorCoachs).ToList();

				foreach (var user in combinedCoachs)
				{
					var roles = await userManager.GetRolesAsync(user);
					string roleNames = string.Join(", ", roles.Where(r => r.Equals("Coach") || r.Equals("Senior Coach")));

					model.SelectCoachListItem.Add(new SelectListItem
					{
						Value = user.Id,
						Text = $"{user.FirstName} {user.LastName} ({roleNames})"
					});
				}

				var customers = await userManager.GetUsersInRoleAsync("Customer");
				foreach (var user in customers)
				{
					var roles = await userManager.GetRolesAsync(user);
					string roleNames = string.Join(", ", roles.Where(r => r.Equals("Customer")));

					model.SelectCustomerListItem.Add(new SelectListItem
					{
						Value = user.Id,
						Text = $"{user.FirstName} {user.LastName} ({roleNames})"
					});
				}
				return View(model);
			}
			else
			{
				var schedules = await context.Schedule
							.GroupBy(s => new { s.Type, s.CoachId, s.StartTime, s.EndTime })
							.Select(s => new
							{
								Type = s.Key.Type,
								CoachId = s.Key.CoachId,
								CoachName = userManager.Users
									.Where(u => u.Id == s.Key.CoachId)
									.Select(u => string.Concat(u.FirstName, " ", u.LastName))
									.FirstOrDefault(),
								GymHallId = s.Select(s => s.GymHallId).FirstOrDefault(),
								GymHallName = context.GymHall
									.Where(g => g.GymHallId == s.Select(s => s.GymHallId).FirstOrDefault())
									.Select(g => g.RoomNumber)
									.FirstOrDefault(),
								Customers = s.Select(s => s.CustomerId).Count(),
								StartTime = DateTime.Parse(s.Key.StartTime.ToString()),
								EndTime = DateTime.Parse(s.Key.EndTime.ToString()),
							})
							.ToListAsync();

				return View("Index", schedules);
			}
		}

		[Authorize(Roles = "Senior Supervisor")]
		[HttpPost]
		public async Task<IActionResult> SaveEditScheduleAsync(Schedule model)
		{
			var customers = model.CustomersId.Distinct().ToList();
			var customersGroupBy = model.CustomersId.GroupBy(c => c).Select(c => new { customerId = c.Key }).ToList();

			var existingSchedule = await context.Schedule
				.Where(s => s.CoachId == model.CoachId
							&& s.GymHallId == model.GymHallId
							&& s.Type == model.Type
							&& s.StartTime.Hours == model.StartTime.Hours
							&& s.StartTime.Minutes == model.StartTime.Minutes
							&& s.EndTime.Hours == model.EndTime.Hours
							&& s.EndTime.Minutes == model.EndTime.Minutes)
				.Select(s => new { s.CustomerId })
				.ToListAsync();

			if (existingSchedule.Any())
			{
				var existingCustomerIds = existingSchedule.Select(s => s.CustomerId).ToList();
				var customersToRemove = existingCustomerIds.Except(customers).ToList();

				foreach (var customerId in customersToRemove)
				{
					var scheduleToRemove = await context.Schedule
						.FirstOrDefaultAsync(s => s.CustomerId == customerId
												  && s.CoachId == model.CoachId
												  && s.GymHallId == model.GymHallId
												  && s.Type == model.Type
												  && s.StartTime.Hours == model.StartTime.Hours
												  && s.StartTime.Minutes == model.StartTime.Minutes
												  && s.EndTime.Hours == model.EndTime.Hours
												  && s.EndTime.Minutes == model.EndTime.Minutes);

					if (scheduleToRemove != null)
					{
						context.Schedule.Remove(scheduleToRemove);
					}
				}

				await context.SaveChangesAsync();
			}

			foreach (var customer in customersGroupBy)
			{
				if (!existingSchedule.Any(s => s.CustomerId == customer.customerId))
				{
					var schedule = new Schedule()
					{
						Type = model.Type,
						CoachId = model.CoachId,
						CustomerId = customer.customerId,
						GymHallId = model.GymHallId,
						StartTime = model.StartTime,
						EndTime = model.EndTime
					};

					await context.Schedule.AddAsync(schedule);
				}
			}
			await context.SaveChangesAsync();


			var schedules = await context.Schedule
				.GroupBy(s => new { s.Type, s.CoachId, s.StartTime, s.EndTime })
				.Select(s => new
				{
					Type = s.Key.Type,
					CoachId = s.Key.CoachId,
					CoachName = userManager.Users
						.Where(u => u.Id == s.Key.CoachId)
						.Select(u => string.Concat(u.FirstName, " ", u.LastName))
						.FirstOrDefault(),
					GymHallId = s.Select(s => s.GymHallId).FirstOrDefault(),
					GymHallName = context.GymHall
						.Where(g => g.GymHallId == s.Select(s => s.GymHallId).FirstOrDefault())
						.Select(g => g.RoomNumber)
						.FirstOrDefault(),
					Customers = s.Select(s => s.CustomerId).Count(),
					StartTime = DateTime.Parse(s.Key.StartTime.ToString()),
					EndTime = DateTime.Parse(s.Key.EndTime.ToString()),
				})
				.ToListAsync();

			return View("Index", schedules);
		}

		[Authorize(Roles = "Senior Supervisor")]
		[HttpPost]
		public async Task<JsonResult> GetCustomers(string indexOf)
		{
			var model = new Schedule();
			string selected = string.Empty;
			var customers = await userManager.GetUsersInRoleAsync("Customer");
			foreach (var user in customers)
			{
				var roles = await userManager.GetRolesAsync(user);
				string roleNames = string.Join(", ", roles.Where(r => r.Equals("Customer")));

				model.SelectCustomerListItem.Add(new SelectListItem
				{
					Value = user.Id,
					Text = $"{user.FirstName} {user.LastName} ({roleNames})"
				});
			}
			if (!string.IsNullOrEmpty(indexOf))
			{
				selected = model.SelectCustomerListItem[int.Parse(indexOf)].Value;
			}

			if (model.SelectCustomerListItem.Count > 0)
				return Json(new { status = true , message = "Ok.", statusCode = 200 , data = model.SelectCustomerListItem, selectedIndex = selected });
			else
				return Json(new { status = false, message = "Bad request.", statusCode = 400 , data = new List<SelectListItem>() });
		}
		
		public async IAsyncEnumerator<dynamic> SchedulesAsync()
		{
			var schedules = await context.Schedule
				.GroupBy(s => new { s.Type, s.CoachId, s.StartTime, s.EndTime })
				.Select(s => new
				{
					Type = s.Key.Type == "1" ? "Individual" : "Group Training",
					CoachId = s.Key.CoachId,
					CoachName = userManager.Users
						.Where(u => u.Id == s.Key.CoachId)
						.Select(u => string.Concat(u.FirstName, " ", u.LastName))
						.FirstOrDefault(),
					GymHallName = context.GymHall
						.Where(g => g.GymHallId == s.Select(s => s.GymHallId).FirstOrDefault())
						.Select(g => g.RoomNumber)
						.FirstOrDefault(),
					Customers = s.Select(s => s.CustomerId).Count(),
					StartTime = DateTime.Parse(s.Key.StartTime.ToString()),
					EndTime = DateTime.Parse(s.Key.EndTime.ToString()),
				})
				.ToListAsync();
			yield return schedules;
		}
	}
}
