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
        { //todo: разнести все из индекста по секциям

            var investors = await dbContext.Investors.ToListAsync();            

            var assets = dbContext.Assets.ToList();

            CalculateAveragePrices(assets);

            var totalInvested = investors.Sum(i => i.AmountInvested);

            var sum = assets.Sum(a => a.Price * a.Quantity);

            var totalShares = investors.Sum(investor => investor.SharesReceived);

            CalculateNavPercents(out var nav1WValue, out var nav1MValue, out var nav3MValue);

            //SetChart();
          //  GenerateLineChart();

            _viewModel = new PortfolioViewModel
            {
                Assets = assets.OrderByDescending(a => a.ShortName == "USD").ThenByDescending(a => a.ShortName == "BTC")
                    .ThenByDescending(a => (a.Price * a.Quantity) / sum).ToList(),
                AssetsUnderManagement = sum,
                Acquisition = totalInvested,
                TotalInvested = totalInvested,
                NetAssetValue = sum / totalShares,
                Nav1W = nav1WValue,
                Nav1M = nav1MValue,
                Nav3M = nav3MValue,                
                HasBtc = assets.Exists(item => item.ShortName == "BTC")
            };

            return View(_viewModel);
        }

        private void CalculateNavPercents(out double nav1WValue, out double nav1MValue, out double nav3MValue)
        {
            var navhistory = dbContext.NavHistories.ToList();
            var weekAgo = DateTime.Now - TimeSpan.FromDays(7);
            var nav1W = navhistory.Where(n => n.Date <= weekAgo).OrderByDescending(d => d.Date).FirstOrDefault();
            nav1WValue = nav1W?.Value ?? 0;
            var monthAgo = DateTime.Now - TimeSpan.FromDays(30);
            var nav1M = navhistory.Where(n => n.Date <= monthAgo).OrderByDescending(d => d.Date).FirstOrDefault(); ;
            nav1MValue = nav1M?.Value ?? 0;
            var month3Ago = DateTime.Now - TimeSpan.FromDays(90);
            var nav3M = navhistory.Where(n => n.Date <= month3Ago).OrderByDescending(d => d.Date).FirstOrDefault(); ;
            nav3MValue = nav3M?.Value ?? 0;
        }

        public ActionResult GetChartData(string period)
        {
            var timeAgo = new DateTime();
            switch (period)
            {
                case "day":
                    timeAgo = DateTime.Now - TimeSpan.FromDays(1);
                    break;
                case "week":
                    timeAgo = DateTime.Now - TimeSpan.FromDays(7);
                    break;
                case "month":
                    timeAgo = DateTime.Now - TimeSpan.FromDays(30);
                    break;
                case "month3":
                    timeAgo = DateTime.Now - TimeSpan.FromDays(90);
                    break;
                case "year":
                    timeAgo = DateTime.Now - TimeSpan.FromDays(365);
                    break;
                case "ytd":
                    timeAgo = new DateTime(DateTime.Now.Year, 1, 1); ;
                    break;
                case "all":
                    timeAgo = DateTime.MinValue;
                    break;
            }

            var btcRates = new List<BtcHistory>();
            foreach (var rate in dbContext.BtcHistories.Where(b => b.Date >= timeAgo).OrderByDescending(b=>b.Date))
            {
                rate.Value = Math.Round(rate.Value, 2, MidpointRounding.AwayFromZero);
                btcRates.Add(rate);
            }
            var navRates = new List<NavHistory>();
            foreach (var rate in dbContext.NavHistories.Where(b => b.Date >= timeAgo).OrderByDescending(b => b.Date))
            {
                rate.Value = Math.Round(rate.Value, 2, MidpointRounding.AwayFromZero);
                navRates.Add(rate);
            }

            var result = new {btc = btcRates, nav = navRates};

            return this.Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Assets()
        {
            var assets = dbContext.Assets.ToList();

            CalculateAveragePrices(assets);

            var sum = assets.Sum(a => a.Price * a.Quantity);

            _viewModel = new PortfolioViewModel
            {
                Assets = assets.OrderByDescending(a => a.ShortName == "USD").ThenByDescending(a => a.ShortName == "BTC")
                   .ThenByDescending(a => (a.Price * a.Quantity) / sum).ToList(),
                AssetsUnderManagement = sum,
                HasBtc = assets.Exists(item => item.ShortName == "BTC")
            };

            return PartialView(_viewModel);
        }

        public IActionResult ChartSection()
        {
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> InfoSection()
        {
            var investors = await dbContext.Investors.ToListAsync();            

            var assets = dbContext.Assets.ToList();            

            var totalInvested = investors.Sum(i => i.AmountInvested);

            var sum = assets.Sum(a => a.Price * a.Quantity);

            var totalShares = investors.Sum(investor => investor.SharesReceived);

            CalculateNavPercents(out var nav1WValue, out var nav1MValue, out var nav3MValue);

            _viewModel = new PortfolioViewModel
            {
                Assets = assets.OrderByDescending(a => a.ShortName == "USD").ThenByDescending(a => a.ShortName == "BTC")
                    .ThenByDescending(a => (a.Price * a.Quantity) / sum).ToList(),
                AssetsUnderManagement = sum,
                Acquisition = totalInvested,
                TotalInvested = totalInvested,
                NetAssetValue = sum / totalShares,
                Nav1W = nav1WValue,
                Nav1M = nav1MValue,
                Nav3M = nav3MValue,                
                HasBtc = assets.Exists(item => item.ShortName == "BTC")
            };

            return PartialView(_viewModel);
        }

        private void CalculateAveragePrices(List<Asset> assets)
        {
            var histories = dbContext.AssetHistories.ToList();
            foreach (var asset in assets.Where(a => a.ShortName != "USD"))
            {
                List<AssetHistory> list;
                if (asset.ShortName == "BTC")
                {
                    list = new List<AssetHistory>();
                    foreach (var hi in histories)
                    {
                        if (hi.ShortName == asset.ShortName && hi.Type == TransactionType.Buy &&
                            hi.RealDate >= asset.RealDate)
                            list.Add(hi);
                        if (hi.Type == TransactionType.Sell && hi.BuyShortName == asset.ShortName &&
                            hi.RealDate >= asset.RealDate)
                        {
                            hi.Quantity = hi.Price * hi.Quantity;
                            list.Add(hi);
                        }
                    }
                }
                else
                    list = histories.Where(hi => hi.ShortName == asset.ShortName && hi.Type == TransactionType.Buy &&
                                                 hi.RealDate >= asset.RealDate).ToList();
                double purchasedSum = 0;
                foreach (var h in list)
                {
                    if (h.Type == TransactionType.Buy)
                        purchasedSum += h.Price * h.Quantity * h.Rate;
                    else
                    {
                        purchasedSum += h.Quantity * h.Rate;
                    }
                }
                asset.AveragePrice = purchasedSum / list.Sum(h => h.Quantity);
                asset.Profit = asset.Quantity * asset.Price / (asset.Quantity * asset.AveragePrice) - 1;
            }
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

        [HttpGet]
        public async Task<ActionResult> AutocompleteCurrency(string term)
        {
            if (string.IsNullOrEmpty(term))
                return this.Json(new MetaResponse<object> {StatusCode = 200, ResponseObject = ""});
            var result = await dbContext.Currencies.Where(c => c.Name.Contains(term)).Take(10).Select(s=>s.Name).Distinct().ToListAsync();
            return this.Json(result);
        }

        [HttpPost]
        public async Task<ActionResult> AddAsset([FromBody]AssetViewModel viewModel)
        {
            try
            {
                return await BuyAsset(viewModel);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.Json(new MetaResponse<object> { StatusCode = 200 });
            }
        }

        [HttpPost]
        public async Task<ActionResult> SellAsset([FromBody]AssetViewModel viewModel)
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
                        if (btc == null)
                        {
                            var btcRate = await GetCurrencyRate(new Currency { ShortName = "BTC" });
                            btc = new Asset
                            {
                                Name = "Bitcoin",
                                Date = viewModel.Date,
                                RealDate = DateTime.Now,
                                ShortName = "BTC",
                                Quantity = viewModel.Quantity * viewModel.Price,
                                Price = btcRate.Rate
                            };
                            dbContext.Assets.Add(btc);
                        }
                        else
                        {
                            btc.Quantity += viewModel.Quantity * viewModel.Price;
                            rate = viewModel.BTCPrice;
                            dbContext.Assets.Update(btc);
                        }
                      
                        break;
                }

                asset.Quantity -= viewModel.Quantity;                

                var sellHistory = new AssetHistory
                {
                    Quantity = viewModel.Quantity,
                    ShortName = asset.ShortName,
                    BuyShortName = viewModel.PurchaseType == PurchaseType.USD ? "USD" : "BTC",
                    Date = viewModel.Date,
                    RealDate = DateTime.Now,
                    Price = viewModel.Price,
                    Rate = viewModel.PurchaseType == PurchaseType.USD ? rate : viewModel.BTCPrice,
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

        private async Task<JsonResult> BuyAsset(AssetViewModel viewModel)
        {
            var currency = dbContext.Currencies.FirstOrDefault(cur => cur.Name == viewModel.Name);
            if (currency == null || viewModel.Price <= 0)
                return this.Json(new MetaResponse<object> { StatusCode = 200, ErrorCode = 1 });

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
                    return this.Json(new MetaResponse<object> { StatusCode = 200, ErrorCode = 2 });
            }
            else
            {
                btc = dbContext.Assets.First(a => a.ShortName == "BTC");
                if (btc.Quantity - viewModel.Quantity * viewModel.Price < 0)
                    return this.Json(new MetaResponse<object> { StatusCode = 200, ErrorCode = 2 });
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
                BuyShortName = "USD",
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
