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

            var histories = dbContext.AssetHistories.ToList();

            var assets = dbContext.Assets.ToList();

            foreach (var asset in assets.Where(a=>a.ShortName!= "USD"))
            {
                var list = histories.Where(hi => hi.Asset == asset && hi.Type == TransactionType.Buy).ToList();
                asset.AveragePrice = list.Sum(h => h.Price * h.Quantity) / list.Sum(h => h.Quantity);
            }

            var totalInvested = investors.Sum(i => i.AmountInvested);
           
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
                dbContext.Currencies.Update(currency);

                var asset = dbContext.Assets.FirstOrDefault(a => a.ShortName == currency.ShortName);
                if (asset != null)
                {
                    asset.Quantity += viewModel.Quantity;
                    asset.Exposure = rate.Rate * asset.Quantity;
                    dbContext.Assets.Update(asset);
                }
                else
                {
                    asset = new Asset
                    {
                        Name = currency.Name,
                        ShortName = currency.ShortName,
                        Quantity = viewModel.Quantity,
                        Exposure = rate.Rate * viewModel.Quantity,
                        Price = rate.Rate,
                    };
                    dbContext.Assets.Add(asset);
                }

                var history = new AssetHistory
                {
                    Quantity = viewModel.Quantity,
                    Asset = asset,
                    Price = viewModel.Price,
                    Type = TransactionType.Buy,
                    Date = viewModel.Date
                };
                dbContext.AssetHistories.Add(history);

                var usd = dbContext.Assets.First(a => a.ShortName == "USD");
                usd.Quantity -= viewModel.Quantity * viewModel.Price;
                usd.Exposure = usd.Quantity;
                dbContext.Assets.Update(usd);

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
