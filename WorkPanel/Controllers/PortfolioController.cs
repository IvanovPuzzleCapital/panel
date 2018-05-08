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
            var totalInvested = investors.Sum(i => i.AmountInvested);
            var currencies = await GetCurrencies(true);

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

        public IActionResult AddAsset()
        {
            return View(_viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> AddAsset(AssetViewModel viewModel)
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
                else
                {
                    var currency = dbContext.Currencies.FirstOrDefault(cur => cur.Name == viewModel.Name);
                    if (currency== null)
                        return this.Json(new MetaResponse<object> { StatusCode = 200 });

                    var rate = await GetCurrencyRate(currency);
                    asset = new Asset
                    {
                        Name = currency.Name,
                        ShortName = currency.ShortName,
                        Quantity = viewModel.Quantity,
                        Exposure = rate.Rate * viewModel.Quantity,
                        Price = rate.Rate
                    };
                }
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
