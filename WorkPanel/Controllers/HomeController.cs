using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WorkPanel.Models;

namespace WorkPanel.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return Redirect("Portfolio");
            }

            return Redirect("Account/Login");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
