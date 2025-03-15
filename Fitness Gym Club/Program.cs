using Fitness_Gym_Club.Data;
using Fitness_Gym_Club.Models;
using Fitness_Gym_Club.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add dbcontex
builder.Services.AddDbContext<AppDbContext>(optionsAction => optionsAction.UseSqlServer(builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Connection string 'Default' is not found.")));

// Add identity
builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

// Add identity options
builder.Services.Configure<IdentityOptions>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireNonAlphanumeric = false; 
	options.Password.RequiredLength = 8; 
	options.Password.RequiredUniqueChars = 1; 
});

// Add application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.LoginPath = "/Home/SignIn";
});

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireClaim("Admin", ClaimTypes.Name).RequireRole("Senior Supervisor");
    });

    options.AddPolicy("CoachPolicy", policy =>
    {
        policy.RequireClaim("Coach", "Senior Coach").RequireRole("CoachMember");
    });

});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add others services
builder.Services.AddScoped<IUser, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
