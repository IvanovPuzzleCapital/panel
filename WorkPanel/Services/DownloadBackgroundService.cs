using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkPanel.Data;
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

                await Task.Delay(10000, stoppingToken);
            }
        }

        private async Task DoWork()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Debug.WriteLine("====================================" + "BACKGROUND TASK EXECUTING" + "====================================");
                Debug.WriteLine("====================================" + DateTime.Now + "====================================");
                var inv = new Investor
                {
                    Name = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                    Agreement = "",
                    AmountInvested = 1000,
                    SharesReceived = 10
                };
                context.Investors.Add(inv);
                await context.SaveChangesAsync();
            }
        }
    }
}
