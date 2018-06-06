using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkPanel.Data;
using WorkPanel.DataExchange;
using WorkPanel.Extensions;
using WorkPanel.Models;

namespace WorkPanel.Services
{
    public class DownloadBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public DownloadBackgroundService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _loggerFactory = loggerFactory;            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateInfoFromApi();
                await UpdateDatabase();
                await UpdateCurrencyList();
                int time;
                var res = int.TryParse(_configuration.GetSection("BackgroundRefreshTime").Value, out time);
                if (!res)
                {
                    time = 60;
                }
                await Task.Delay(60000 * time, stoppingToken);
            }
        }

        private async Task UpdateInfoFromApi()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                _loggerFactory.AddFile(Path.Combine(Directory.GetCurrentDirectory(), "logger.txt"));
                var logger = _loggerFactory.CreateLogger("FileLogger");

                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var coinApi = new CoinApi(_configuration);
                var assets = context.Assets.Where(a => a.Name != "USD").GroupBy(al=>al.ShortName).Select(an=>an.First()).ToList();
                foreach (var asset in assets)
                {
                    try
                    {
                        var rate = await coinApi.GetCurrencyRateToUsd(asset.ShortName);
                        asset.Price = rate.Rate;
                        context.Assets.Update(asset);
                        var currency = context.Currencies.FirstOrDefault(c => c.ShortName == asset.ShortName);
                        if (currency != null)
                        {
                            currency.Rate = rate.Rate;
                            context.Currencies.Update(currency);
                        }
                        context.CurrencyRates.Add(rate);
                        logger.LogInformation(String.Format("========= {0} =========== {1} =========", DateTime.Now,
                            "Update rate for " + currency.ShortName));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        logger.LogInformation(String.Format("========= {0} =========== {1} =========", DateTime.Now, "Failed to update rates!"));                        
                    }
                    
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
                var coinApi = new CoinApi(_configuration);
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
                if (investors.Count == 0) return;

                var sum = assets.Sum(a => a.Price * a.Quantity);
                var totalShares = investors.Sum(investor => investor.SharesReceived);
                var time = DateTime.Now;
                

                var nav = new NavHistory
                {
                    Date = time,
                    Value = sum / totalShares
                };

                var rate = await coinApi.GetCurrencyRateToUsd("BTC");

                var btc = new BtcHistory
                {
                    Date = time,
                    Value = rate.Rate
                };

                dbContext.NavHistories.Add(nav);
                dbContext.BtcHistories.Add(btc);
                dbContext.SaveChanges();

                //if (!assets.Exists(a => a.ShortName == "BTC"))
                //{
                //    try
                //    {


                //        var currency = dbContext.Currencies.FirstOrDefault(c => c.ShortName == "BTC");
                //        if (currency != null)
                //        {
                //            currency.Rate = rate.Rate;
                //            dbContext.Currencies.Update(currency);
                //        }
                //        dbContext.CurrencyRates.Add(rate);                        
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine(e);                        
                //    }
                //}
            }
        }
    }
}
