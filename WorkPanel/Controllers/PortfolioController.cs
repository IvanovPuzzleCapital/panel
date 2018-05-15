using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WorkPanel.Data;
using WorkPanel.DataExchange;
using WorkPanel.DataExchange.Responses;
using WorkPanel.Models;
using WorkPanel.Models.PortfolioViewModels;

namespace WorkPanel.Controllers
{
    [Authorize]
    public class PortfolioController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        private PortfolioViewModel _viewModel;

        private readonly CoinApi coinApi;

        public PortfolioController(ApplicationDbContext context, IConfiguration configuration)
        {
            dbContext = context;
            coinApi = new CoinApi(configuration);
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
                var list = histories.Where(hi => hi.ShortName == asset.ShortName && hi.Type == TransactionType.Buy && hi.RealDate >= asset.RealDate).ToList();
                var purchasedSum = list.Sum(h => h.Price * h.Quantity * h.Rate);
                asset.AveragePrice = purchasedSum / list.Sum(h => h.Quantity);
                asset.Profit = (asset.Quantity * asset.Price - purchasedSum) / purchasedSum;
            }

            var totalInvested = investors.Sum(i => i.AmountInvested);
           
            var sum = assets.Sum(a => a.Price * a.Quantity);

            _viewModel = new PortfolioViewModel
            {
                Assets = assets.OrderByDescending(a => a.ShortName == "USD").ThenByDescending(a => a.ShortName == "BTC")
                    .ThenByDescending(a => (a.Price * a.Quantity) / sum).ToList(),
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
            model.HasBtc = dbContext.Assets.ToList().Exists(item => item.ShortName == "BTC");
            return PartialView("Info", model);
        }

        public IActionResult AddAsset()
        {
            return View(_viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> AddAsset([FromBody]AssetViewModel viewModel)
        {
            try
            {          
                var currency = dbContext.Currencies.FirstOrDefault(cur => cur.Name == viewModel.Name);
                if (currency == null || viewModel.Price <= 0)
                    return this.Json(new MetaResponse<object> { StatusCode = 200, ErrorCode = 1});

                var rate = await GetCurrencyRate(currency);
                if (rate == null)
                    return this.Json(new MetaResponse<object> { StatusCode = 200, ErrorCode = 3 });
                currency.Rate = rate.Rate;
                dbContext.Currencies.Update(currency);

                var usd = dbContext.Assets.First(a => a.ShortName == "USD");

                Asset btc = null;

                if (viewModel.PurchaseType == PurchaseType.USD)
                {
                    if (usd.Quantity - viewModel.Quantity * viewModel.Price < 0)
                        return this.Json(new MetaResponse<object> {StatusCode = 200, ErrorCode = 2});
                }
                else
                {
                    
                    btc = dbContext.Assets.First(a => a.ShortName == "BTC");
                    if (btc.Quantity - viewModel.Quantity * viewModel.Price < 0)
                        return this.Json(new MetaResponse<object> {StatusCode = 200, ErrorCode = 2});
                }

                var asset = dbContext.Assets.FirstOrDefault(a => a.ShortName == currency.ShortName);
                if (asset != null)
                {
                    asset.Quantity += viewModel.Quantity;                    
                    dbContext.Assets.Update(asset);
                }
                else
                {
                    asset = new Asset
                    {
                        Name = currency.Name,
                        Date = viewModel.Date,
                        RealDate = DateTime.Now,
                        ShortName = currency.ShortName,
                        Quantity = viewModel.Quantity,                        
                        Price = rate.Rate,
                    };
                    dbContext.Assets.Add(asset);
                }

                var history = new AssetHistory
                {
                    Quantity = viewModel.Quantity,
                    ShortName = asset.ShortName,
                    Price = viewModel.Price,
                    Rate = viewModel.PurchaseType == PurchaseType.USD ? 1 : btc.Price,
                    Type = TransactionType.Buy,
                    Date = viewModel.Date,
                    RealDate = DateTime.Now
                };
                dbContext.AssetHistories.Add(history);

                switch (viewModel.PurchaseType)
                {
                    case PurchaseType.USD:
                        usd.Quantity -= viewModel.Quantity * viewModel.Price;                        
                        dbContext.Assets.Update(usd);
                        break;
                    case PurchaseType.BTC:
                        btc.Quantity -= viewModel.Quantity * viewModel.Price;                        
                        dbContext.Assets.Update(btc);
                        break;
                }

                dbContext.SaveChanges();
                return this.Json(new MetaResponse<object> { StatusCode = 200 });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.Json(new MetaResponse<object> { StatusCode = 200 });
            }
        }

        [HttpPost]
        public ActionResult SellAsset([FromBody]AssetViewModel viewModel)
        {
            var asset = dbContext.Assets.FirstOrDefault(a => a.ShortName == viewModel.ShortName);
            if (asset != null)
            {
                if (asset.Quantity - viewModel.Quantity < 0)
                    return this.Json(new MetaResponse<object> { StatusCode = 200, ErrorCode = 2 });

                double rate = 1;
                switch (viewModel.PurchaseType)
                {
                    case PurchaseType.USD:
                        var usd = dbContext.Assets.FirstOrDefault(a => a.ShortName == "USD");
                        
                        usd.Quantity += viewModel.Quantity * viewModel.Price;
                        rate = 1;
                        dbContext.Assets.Update(usd);
                        break;
                    case PurchaseType.BTC:
                        var btc = dbContext.Assets.FirstOrDefault(a => a.ShortName == "BTC");
                        btc.Quantity += viewModel.Quantity * viewModel.Price;
                        rate = btc.Price;
                        dbContext.Assets.Update(btc);
                        break;
                }

                asset.Quantity -= viewModel.Quantity;                

                var sellHistory = new AssetHistory
                {
                    Quantity = viewModel.Quantity,
                    ShortName = asset.ShortName,
                    Date = viewModel.Date,
                    RealDate = DateTime.Now,
                    Price = viewModel.Price,
                    Rate = rate,
                    Type = TransactionType.Sell
                };

                if (asset.Quantity > 0)
                {
                    dbContext.Assets.Update(asset);
                }
                else
                {
                    dbContext.Assets.Remove(asset);
                }
                
                dbContext.AssetHistories.Add(sellHistory);
                dbContext.SaveChanges();
            }
            return this.Json(new MetaResponse<object> { StatusCode = 200 });
        }

        private async Task<CurrencyRate> GetCurrencyRate(Currency currency)
        {

            if (!dbContext.CurrencyRates.Any(cur => cur.Base == currency.ShortName))
            {
                var rate = await coinApi.GetCurrencyRateToUsd(currency.ShortName);
                if (rate == null)
                    return null;               
                dbContext.CurrencyRates.Add(rate);
                dbContext.SaveChanges();
            }
            var priceCurrency = dbContext.CurrencyRates.Last(cur => cur.Base == currency.ShortName);
            return priceCurrency;
        }
    }
}
