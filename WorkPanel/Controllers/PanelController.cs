﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WorkPanel.Data;
using WorkPanel.DataExchange.Responses;
using WorkPanel.Models;
using WorkPanel.Models.PanelViewModels;

namespace WorkPanel.Controllers
{
    [Authorize]
    public class PanelController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public PanelController(ApplicationDbContext context)
        {
            dbContext = context;
            ViewBag.Selected = "Investors";
            ViewData["Section"] = "Investors";
        }

        public async Task<IActionResult> Index()
        {
            var investors = await dbContext.Investors.OrderBy(s => s.Status).ThenByDescending(s => s.Date).ToListAsync();
            var assets = await dbContext.Assets.ToListAsync();

            var viewModel = new PanelViewModel
            {
                Investors = investors,
                TotalInvested = investors.Sum(investor => investor.AmountInvested),
                TotalShares = investors.Sum(investor => investor.SharesReceived),
                AssetsUnderManagement = assets.Sum(a => a.Price * a.Quantity),
                NetAssetValue = assets.Sum(a => a.Price * a.Quantity) / investors.Sum(investor => investor.SharesReceived)
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult InsertInvestor(InvestorViewModel viewModel)
        {
            try
            {
                var investor = new Investor(viewModel);
                var invCnt = dbContext.Investors.Count();
                dbContext.Investors.Add(investor);
                var usd = dbContext.Assets.FirstOrDefault(a => a.Name == "USD");
                if (usd != null)
                {
                    usd.Quantity += investor.AmountInvested;                                    
                    dbContext.Assets.Update(usd);

                    if (invCnt == 0)
                    {
                        var nav = new NavHistory
                        {
                            Date = DateTime.Now,
                            Value = usd.Quantity / investor.SharesReceived
                        };

                        dbContext.NavHistories.Add(nav);
                    }
                }

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
                model.HistoricalDateList = JsonConvert.DeserializeObject<List<DateTime>>(investor.JsonDateList);
                model.HistoricalDeactivateDateList = JsonConvert.DeserializeObject<List<DateTime>>(investor.JsonDeactivateDateList);                
                model.Agreement = investor.Agreement;
                model.AmountInvested = investor.AmountInvested;
                model.SharesReceived = investor.SharesReceived;
            }
            return PartialView("Info", model);
        }

        [HttpPost]        
        public ActionResult DeactivateInvestor([FromBody]DeactivateViewModel model)
        {
            if (model == null)
                return this.Json(new MetaResponse<object> { StatusCode = 200, ErrorMessage = "model is null"});
            var usd = dbContext.Assets.FirstOrDefault(a => a.Name == "USD");
            if (usd != null && usd.Quantity - model.AmountReturned < 0)
            {
                return this.Json(new MetaResponse<object> { StatusCode = 200, ErrorCode = 5 });
                //ModelState.AddModelError("AmountReturned", "Incorrect amount value. Max value is " + usd.Quantity +"$");                
            }

            var investor = dbContext.Investors.FirstOrDefault(p => p.Id == model.Id);
            if (investor != null)
            {
                investor.Status = Status.Inactive;
                investor.DeactivateDate = DateTime.ParseExact(model.DeactivateDate, "d/M/yyyy", CultureInfo.InvariantCulture);
                investor.SharesBurned = investor.SharesReceived;
                investor.AmountReturned = model.AmountReturned;
                investor.AmountInvested = 0;
                investor.SharesReceived = 0;
                var list = JsonConvert.DeserializeObject<List<DateTime>>(investor.JsonDeactivateDateList);
                list.Add(DateTime.ParseExact(model.DeactivateDate, "d/M/yyyy", CultureInfo.InvariantCulture));
                investor.JsonDeactivateDateList = JsonConvert.SerializeObject(list);
                dbContext.Investors.Update(investor);

                usd.Quantity -= investor.AmountReturned;
                dbContext.Assets.Update(usd);

                dbContext.SaveChanges();
            }

            return this.Json(new MetaResponse<object> { StatusCode = 200 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                var list = JsonConvert.DeserializeObject<List<DateTime>>(investor.JsonDateList);
                list.Add(model.Date);
                investor.JsonDateList = JsonConvert.SerializeObject(list);
                dbContext.Investors.Update(investor);

                var usd = dbContext.Assets.FirstOrDefault(a => a.Name == "USD");
                if (usd != null)
                {
                    usd.Quantity += investor.AmountInvested;                    
                    dbContext.Assets.Update(usd);
                }

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

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(PanelController.Index), "Panel");
            }
        }

        #endregion
    }
}