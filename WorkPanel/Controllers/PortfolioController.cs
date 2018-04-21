using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WorkPanel.Data;

namespace WorkPanel.Controllers
{
    public class PortfolioController : Controller
    {
        private ApplicationDbContext dbContext;

        public PortfolioController(ApplicationDbContext context)
        {
            dbContext = context;
            ViewBag.Selected = "Portfolio";
            ViewData["Section"] = "Portfolio";
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public IActionResult AddAsset()
        {
            return View();
        }
    }
}
