using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkPanel.Data;
using WorkPanel.DataExchange.Responses;
using WorkPanel.Models;
using WorkPanel.Models.PanelViewModels;

namespace WorkPanel.Controllers
{
    [Authorize]
    public class PanelController : Controller
    {
        private ApplicationDbContext dbContext;

        public PanelController(ApplicationDbContext context)
        {
            dbContext = context;
            ViewBag.Selected = "Investors";
            ViewData["Section"] = "Investors";
        }

        public async Task<IActionResult> Index()
        {
            var investors = await dbContext.Investors.OrderBy(s => s.Status).ThenByDescending(s => s.Date).ToListAsync();
            var viewModel = new PanelViewModel
            {
                Investors = investors,
                TotalInvested = investors.Sum(investor => investor.AmountInvested),
                TotalShares = investors.Sum(investor => investor.SharesReceived)
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult InsertInvestor(InvestorViewModel viewModel)
        {
            try
            {
                var newInvestor = new Investor(viewModel);
                dbContext.Investors.Add(newInvestor);
                dbContext.SaveChanges();
                return this.Json(new MetaResponse<object> { StatusCode = 200 });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.Json(new MetaResponse<object> { StatusCode = 200 });
            }
        }

        public IActionResult AddInvestor()
        {
            return View();
        }

        public IActionResult Info(long? id)
        {
            var model = new InvestorViewModel();
            var investor = dbContext.Investors.FirstOrDefault(p => p.Id == id);
            if (investor != null)
            {
                model.Id = investor.Id;
                model.Status = investor.Status;
                model.Name = investor.Name;
                model.Date = investor.Date;
                model.DeactivateDate = investor.DeactivateDate;
                model.Agreement = investor.Agreement;
                model.AmountInvested = investor.AmountInvested;
                model.SharesReceived = investor.SharesReceived;
            }
            return PartialView("Info", model);
        }

        public IActionResult DeactivateInvestor(InvestorViewModel model)
        {
            var investor = dbContext.Investors.FirstOrDefault(p => p.Id == model.Id);
            if (investor != null)
            {
                investor.Status = Status.Inactive;
                investor.DeactivateDate = model.DeactivateDate;
                investor.SharesBurned = investor.SharesReceived;
                investor.AmountReturned = model.AmountReturned;
                investor.AmountInvested = 0;
                investor.SharesReceived = 0;
                dbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult ActivateInvestor(InvestorViewModel model)
        {
            var investor = dbContext.Investors.FirstOrDefault(p => p.Id == model.Id);
            if (investor != null)
            {
                investor.Status = Status.Active;
                investor.Date = model.Date;
                investor.DeactivateDate = model.DeactivateDate;
                investor.SharesBurned = 0;
                investor.AmountReturned = 0;
                investor.AmountInvested = model.AmountInvested;
                investor.SharesReceived = model.SharesReceived;
                dbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult RemoveInvestor(int id)
        {
            var investor = dbContext.Investors.FirstOrDefault(p => p.Id == id);
            if (investor != null)
            {
                dbContext.Investors.Remove(investor);
                dbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}