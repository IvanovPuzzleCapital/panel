using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkPanel.Data;
using WorkPanel.DataExchange.Responses;
using WorkPanel.Models;
using WorkPanel.Models.PortfolioViewModels;

namespace WorkPanel.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        private PortfolioViewModel _viewModel;

        public List<Currency> Currencies =
            new List<Currency>
            {
                new Currency {Name = "Bitcoin", ShortName = "BTC"},
                new Currency {Name = "Ethereum", ShortName = "ETH"},
                new Currency {Name = "Ripple", ShortName = "XRP"},
                new Currency {Name = "Cardano", ShortName = "ADA"},
                new Currency {Name = "Litecoin", ShortName = "LTC"},
                new Currency {Name = "Stellar", ShortName = "XLM"},
            };

        public PortfolioController(ApplicationDbContext context)
        {
            dbContext = context;
            ViewBag.Selected = "Portfolio";
            ViewData["Section"] = "Portfolio";
        }

        public async Task<IActionResult> Index()
        {
            
            var investors = await dbContext.Investors.ToListAsync();
            var totalInvested = investors.Sum(i => i.AmountInvested);

            var assets = dbContext.Assets.ToList();
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
                dbContext.Assets.Add(usdAsset);
                dbContext.SaveChanges();
            }

            _viewModel = new PortfolioViewModel
            {
                Assets = assets,
                NetAssetValue = assets.Sum(a => a.Exposure),
                Acquisition = totalInvested,
                Currencies = Currencies,
                HasBtc = assets.Exists(item=>item.ShortName == "BTC")
            };


            return View(_viewModel);
        }

        public IActionResult AddAsset()
        {
            return View(_viewModel);
        }

        [HttpPost]
        public ActionResult AddAsset(AssetViewModel viewModel)
        {
            try
            {
                var asset = new Asset();
                var assets = dbContext.Assets.ToList();
                if (assets.Exists(a => a.Name == viewModel.Name))
                {
                    asset = assets.Find(a => a.Name == viewModel.Name);
                    asset.Quantity += viewModel.Quantity;
                }
                asset = new Asset
                {
                    Name = "USD",
                    ShortName = "USD",
                    Quantity = 1,
                    Exposure = 1,
                    Price = 1
                };
                dbContext.Assets.Add(asset);
                dbContext.SaveChanges();
                return this.Json(new MetaResponse<object> { StatusCode = 200 });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.Json(new MetaResponse<object> { StatusCode = 200 });
            }
        }
    }
}
