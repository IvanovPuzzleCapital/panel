using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkPanel.Data;
using WorkPanel.Models.PanelViewModels;

namespace WorkPanel.ViewComponents
{
    public class InvestorsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext dbContext;

        public InvestorsViewComponent(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var investors = await dbContext.Investors.OrderBy(s => s.Status).ThenByDescending(s => s.Date).ToListAsync();
            var assets = await dbContext.Assets.ToListAsync();

            var viewModel = new InvestorsSectionViewModel()
            {
                Investors = investors,
                NetAssetValue = assets.Sum(a => a.Price * a.Quantity) / investors.Sum(investor => investor.SharesReceived)
            };
            return View(viewModel);
        }
    }
}