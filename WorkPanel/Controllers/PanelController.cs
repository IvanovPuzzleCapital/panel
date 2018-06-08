using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WorkPanel.Data;
using WorkPanel.DataExchange;
using WorkPanel.DataExchange.Responses;
using WorkPanel.Models;
using WorkPanel.Models.PanelViewModels;

namespace WorkPanel.Controllers
{
    [Authorize]
    public class PanelController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        private readonly CoinApi coinApi;

        public PanelController(ApplicationDbContext context, IConfiguration configuration)
        {
            dbContext = context;
            coinApi = new CoinApi(configuration);
            ViewBag.Selected = "Investors";
            ViewData["Section"] = "Investors";
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public IActionResult InvestorsVC()
        {
            return ViewComponent("Investors");
        }

        public IActionResult InvestedVC()
        {
            return ViewComponent("InvestedPanel");
        }      

        [HttpPost]
        public async Task<ActionResult> InsertInvestor([FromBody]InvestorViewModel viewModel)
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
                        var time = DateTime.Now;
                        var nav = new NavHistory
                        {
                            Date = time,
                            Value = usd.Quantity / investor.SharesReceived
                        };

                        var rate = await coinApi.GetCurrencyRateToUsd("BTC");

                        var btc = new BtcHistory
                        {
                            Date = time,
                            Value = rate.Rate
                        };

                        dbContext.NavHistories.Add(nav);
                        dbContext.BtcHistories.Add(btc);

                        //if (!dbContext.Assets.ToList().Exists(a => a.ShortName == "BTC"))
                        //{
                        //    try
                        //    {
                        //        var rate = await coinApi.GetCurrencyRateToUsd("BTC");                                
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
            return PartialView("AddInvestor");
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
                model.Date = investor.Date.ToString();
                model.DeactivateDate = investor.DeactivateDate.ToString();
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
        public ActionResult Activate([FromBody]InvestorViewModel model)
        {
            var investor = dbContext.Investors.FirstOrDefault(p => p.Id == model.Id);
            if (investor != null)
            {
                investor.Status = Status.Active;
                investor.Date = DateTime.ParseExact(model.Date, "d/M/yyyy", CultureInfo.InvariantCulture);
                //investor.DeactivateDate = DateTime.ParseExact(model.DeactivateDate, "d/M/yyyy", CultureInfo.InvariantCulture); ;
                investor.SharesBurned = 0;
                investor.AmountReturned = 0;
                investor.AmountInvested = model.AmountInvested;
                investor.SharesReceived = model.SharesReceived;               
                var list = JsonConvert.DeserializeObject<List<DateTime>>(investor.JsonDateList);
                list.Add(investor.Date);
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
            return this.Json(new MetaResponse<object> { StatusCode = 200 });
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