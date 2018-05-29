using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkPanel.Data;
using WorkPanel.DataExchange;
using WorkPanel.Models;

namespace WorkPanel.Services
{
    public class DownloadBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public DownloadBackgroundService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateInfoFromApi();
                await UpdateDatabase();
                await UpdateCurrencyList();

                await Task.Delay(60000 * 60, stoppingToken);
            }
        }

        private async Task UpdateInfoFromApi()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var coinApi = new CoinApi(_configuration);
                var assets = context.Assets.Where(a => a.Name != "USD").GroupBy(al=>al.ShortName).Select(an=>an.First()).ToList();
                foreach (var asset in assets)
                {
                    var rate = await coinApi.GetCurrencyRateToUsd(asset.ShortName);
                    asset.Price = rate.Rate;
                    var currency = context.Currencies.FirstOrDefault(c => c.ShortName == asset.ShortName);
                    if (currency != null)
                    {
                        currency.Rate = rate.Rate;
                        context.Currencies.Update(currency);
                    }
                    context.CurrencyRates.Add(rate);
                }
                context.SaveChanges();
            }
        }

        private async Task UpdateCurrencyList()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var coinApi = new CoinApi(_configuration);

                var allCurrencies = dbContext.Currencies.ToList();

                if (allCurrencies.Count == 0)
                {
                    allCurrencies = await coinApi.GetAllAssets();
                    dbContext.Currencies.AddRange(allCurrencies);
                    dbContext.SaveChanges();
                }
                else
                {
                    var lastDate = allCurrencies.First().Date;
                    if (DateTime.Now - lastDate < TimeSpan.FromDays(3))
                    {
                        allCurrencies = await coinApi.GetAllAssets();
                        dbContext.Currencies.UpdateRange(allCurrencies);
                        dbContext.SaveChanges();
                    }
                }
            }
        }

        private async Task UpdateDatabase()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var navs = dbContext.NavHistories.ToList();

                if (navs.Count > 0)
                {
                    var latestNav = navs.Last();
                    var timeDiff = latestNav.Date - DateTime.Now;
                    if (Math.Abs(timeDiff.Hours) < 3)
                    {
                        return;
                    }
                }

                var assets = dbContext.Assets.ToList();
                var investors = await dbContext.Investors.ToListAsync();
                var sum = assets.Sum(a => a.Price * a.Quantity);
                var totalShares = investors.Sum(investor => investor.SharesReceived);

                var nav = new NavHistory
                {
                    Date = DateTime.Now,
                    Value = sum / totalShares
                };


                dbContext.NavHistories.Add(nav);
                dbContext.SaveChanges();
            }
        }
    }
}
