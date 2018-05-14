using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkPanel.Data;
using WorkPanel.DataExchange;

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
    }
}
