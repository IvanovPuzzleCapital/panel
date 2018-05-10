using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkPanel.Data;
using WorkPanel.DataExchange;
using WorkPanel.DataExchange.Responses;
using WorkPanel.Models;
using WorkPanel.Models.PortfolioViewModels;

namespace WorkPanel.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        private PortfolioViewModel _viewModel;

        private readonly CoinApi coinApi;

        public PortfolioController(ApplicationDbContext context)
        {
            dbContext = context;
            coinApi = new CoinApi();
            ViewBag.Selected = "Portfolio";
            ViewData["Section"] = "Portfolio";
        }

        public async Task<IActionResult> Index()
        {

            var investors = await dbContext.Investors.ToListAsync();
           
            var currencies = await GetCurrencies(true);

            var assets = dbContext.Assets
                .GroupBy(a => a.ShortName)
                .Select(ca => new Asset
                {
                    Name = ca.First().Name,
                    ShortName = ca.First().ShortName,
                    Quantity = ca.Sum(c=>c.Quantity),
                    Exposure = ca.Sum(c => c.Quantity) * ca.First().Price,
                    Price = ca.First().Price,
                    AveragePrice = ca.Sum(c=>c.PurchasePrice*c.PurchaseQuantity) / ca.Sum(c=>c.PurchaseQuantity)
                }).ToList();
            //currencies.FirstOrDefault(cur=>cur.ShortName == ca.First().ShortName).Rate
            var totalInvested = investors.Sum(i => i.AmountInvested);

            //foreach (var asset in assets)
            //{
            //    if (asset.ShortName == "USD") continue;                
            //    asset.AveragePrice = dbContext.CurrencyRates.Where(c => c.Base == asset.ShortName).Average(c => c.Rate);
            //}
           
            var sum = assets.Sum(a => a.Exposure);

            _viewModel = new PortfolioViewModel
            {
                Assets = assets.OrderByDescending(a => a.ShortName == "USD").ThenByDescending(a => a.ShortName == "BTC")
                    .ThenByDescending(a => a.Exposure / sum).ToList(),
                NetAssetValue = sum,
                Acquisition = totalInvested,
                TotalInvested = totalInvested,
                Currencies = currencies,
                HasBtc = assets.Exists(item => item.ShortName == "BTC")
            };
            

            return View(_viewModel);
        }

        private async Task<List<Currency>> GetCurrencies(bool onlyCrypto)
        {            
            var allCurrencies = dbContext.Currencies.ToList();

            if (allCurrencies.Count == 0)
            {
                allCurrencies = await coinApi.GetAllAssets();
                dbContext.Currencies.AddRange(allCurrencies);
                dbContext.SaveChanges();
            }

            var currencies = onlyCrypto ? allCurrencies.Where(c => c.IsCrypto).ToList() : allCurrencies.ToList();

            return currencies;
        }

        public IActionResult Info(long? id)
        {
            var model = new AssetViewModel();
            var asset = dbContext.Assets.FirstOrDefault(p => p.Id == id);
            model.Name = asset.Name;
            model.ShortName = asset.ShortName;
            model.Quantity = asset.Quantity;
            return PartialView("Info", model);
        }

        public IActionResult AddAsset()
        {
            return View(_viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> AddAsset(AssetViewModel viewModel)
        {
            try
            {                
                var currency = dbContext.Currencies.FirstOrDefault(cur => cur.Name == viewModel.Name);
                if (currency == null)
                    return this.Json(new MetaResponse<object> { StatusCode = 200 });

                var rate = await GetCurrencyRate(currency);
                currency.Rate = rate.Rate;

                var asset = new Asset
                {
                    Name = currency.Name,
                    ShortName = currency.ShortName,
                    Quantity = viewModel.Quantity,
                    Exposure = rate.Rate * viewModel.Quantity,
                    Price = rate.Rate,
                    PurchasePrice = viewModel.Price,
                    PurchaseQuantity = viewModel.Quantity
                };

                var usd = dbContext.Assets.First(a => a.ShortName == "USD");
                usd.Quantity -= viewModel.Quantity * viewModel.Price;
                usd.Exposure = usd.Quantity;
                usd.PurchaseQuantity = usd.Quantity;

                dbContext.Currencies.Update(currency);
                dbContext.Assets.Update(usd);
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

        private async Task<CurrencyRate> GetCurrencyRate(Currency currency)
        {

            if (!dbContext.CurrencyRates.Any(cur => cur.Base == currency.ShortName))
            {
                var rate = await coinApi.GetCurrencyRateToUsd(currency.ShortName);
                dbContext.CurrencyRates.Add(rate);
                dbContext.SaveChanges();
            }
            var priceCurrency = dbContext.CurrencyRates.Last(cur => cur.Base == currency.ShortName);
            return priceCurrency;
        }
    }
}
