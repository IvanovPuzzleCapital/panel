using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkPanel.Data;
using WorkPanel.Models.PanelViewModels;

namespace WorkPanel.ViewComponents
{
    public class InvestedPanelViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext dbContext;

        public InvestedPanelViewComponent(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var investors = await dbContext.Investors.ToListAsync();
            var assets = await dbContext.Assets.ToListAsync();

            var viewModel = new PanelInfoSectionViewModel
            {
                TotalInvested = investors.Sum(investor => investor.AmountInvested),
                TotalShares = investors.Sum(investor => investor.SharesReceived),
                AssetsUnderManagement = assets.Sum(a => a.Price * a.Quantity),
                NetAssetValue = assets.Sum(a => a.Price * a.Quantity) / investors.Sum(investor => investor.SharesReceived)
            };
            return View(viewModel);
        }
    }
}