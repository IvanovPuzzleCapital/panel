using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkPanel.Data;
using WorkPanel.DataExchange;
using WorkPanel.Models;

namespace WorkPanel.Services
{
    public class DownloadBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DownloadBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await DoWork();

                await Task.Delay(60000 * 30, stoppingToken);
            }
        }

        private async Task DoWork()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var coinApi = new CoinApi();
                Debug.WriteLine("====================================" + "BACKGROUND TASK EXECUTING" + "====================================");
                Debug.WriteLine("====================================" + DateTime.Now + "====================================");
                var assets = context.Assets.Where(a => a.Name != "USD").ToList();
                foreach (var asset in assets)
                {
                    var rate = await coinApi.GetCurrencyRateToUsd(asset.ShortName);
                    asset.Price = rate.Rate;
                    context.CurrencyRates.Add(rate);
                }
                context.SaveChanges();
            }
        }
    }
}
