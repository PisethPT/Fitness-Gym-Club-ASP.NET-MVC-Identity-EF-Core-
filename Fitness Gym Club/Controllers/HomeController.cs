using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Fitness_Gym_Club.Models;
using Microsoft.AspNetCore.Authorization;
using Fitness_Gym_Club.Services;
using System.Threading.Tasks;

namespace Fitness_Gym_Club.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
	private readonly IUser userService;

	public HomeController(ILogger<HomeController> logger, IUser userService)
    {
        _logger = logger;
		this.userService = userService;
	}

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult SignUp() => View(new SignUp());

    [HttpPost]
    public async Task<IActionResult> SignUp(SignUp model)
    {
        if (ModelState.IsValid)
        {
            var response = await userService.SignUpAsync(model);
			if (response.IsSuccess)
				return RedirectToAction(response.Message);
            else
                return View("SignUp", model);
		}
        return View(model);
    }

    [HttpGet]
    public IActionResult SignIn() => View(new SignIn());

    [HttpPost]
    public async Task<IActionResult> SignIn(SignIn model)
    {
        if (ModelState.IsValid)
        {
            var response = await userService.SignInAsync(model);
            if (response.IsSuccess)
            {
                return RedirectToAction(response.Message);
            }
            ViewBag.Error = response.Message;
			ModelState.AddModelError("SignIn", "Cannot login.");
			return View(model);
		}
        return View(model);
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    [HttpGet]
    public async Task<IActionResult> UserSignOut() => RedirectToAction(await userService.SignOutAsync());

    [HttpGet]
	public async Task<IActionResult> AboutMe()
    {
        var userEmail = User.Identity!.Name;
        var user = await userService.AboutMe(userEmail!);
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAboutMe(User user, string CurrentPassword, string Password)
    {
		var response = await userService.UpdateAboutMe(user, CurrentPassword, Password);
		if (!response.IsSuccess && response.StatusCode == 404)
			return RedirectToAction(response.Message);
        else if(!response.IsSuccess && response.StatusCode == 400)
        {
			ModelState.AddModelError("UpdateAboutMe", response.Message!);
			return View("AboutMe", user);
		}
		return RedirectToAction(await userService.SignOutAsync());
	}

    public IActionResult Contact() => View();

	[Authorize(Roles = "Senior Supervisor")]
	public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}