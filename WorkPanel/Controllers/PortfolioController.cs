using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkPanel.Data;
using WorkPanel.Models;
using WorkPanel.Models.PortfolioViewModels;

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
            var assets = dbContext.Assets.ToList();
            var investors = await dbContext.Investors.ToListAsync();
            var totalInvested = investors.Sum(i => i.AmountInvested);            
            if (assets.Count == 0)
            {
                var usdAsset = new Asset
                {
                    Name = "USD",
                    ShortName = "USD",
                    Quantity = totalInvested,
                    Exposure = totalInvested,
                    Price = 1
                };
                assets.Add(usdAsset);
                dbContext.SaveChanges();
            }

            var viewModel = new PortfolioViewModel
            {
                Assets = assets,
                NetAssetValue = assets.Sum(a => a.Exposure),
                Acquisition = totalInvested
            };


            return View(viewModel);
        }

        public IActionResult AddAsset()
        {
            return View();
        }
    }
}
