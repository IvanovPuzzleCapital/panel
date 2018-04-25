using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkPanel.Models.PanelViewModels
{
    public class InvestorViewModel
    {
        public int Id { get; set; }

        public Status Status { get; set; }

        public string Name { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Field required")]
        public DateTime DeactivateDate { get; set; }

        public List<DateTime> HistoricalDateList { get; set; }

        public List<DateTime> HistoricalDeactivateDateList { get; set; }

        public string Agreement { get; set; }

        public double AmountInvested { get; set; }

        [Required(ErrorMessage = "Field required")]
        public double AmountReturned { get; set; }

        public int SharesReceived { get; set; }

        public int SharesBurned { get; set; }
    }
}