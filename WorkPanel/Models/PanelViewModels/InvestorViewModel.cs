using System;
using System.Collections.Generic;

namespace WorkPanel.Models.PanelViewModels
{
    public class InvestorViewModel
    {
        public int Id { get; set; }

        public Status Status { get; set; }

        public string Name { get; set; }

        public string Date { get; set; }

        public string DeactivateDate { get; set; }

        public List<DateTime> HistoricalDateList { get; set; }

        public List<DateTime> HistoricalDeactivateDateList { get; set; }

        public string Agreement { get; set; }

        public double AmountInvested { get; set; }
        
        public double AmountReturned { get; set; }

        public int SharesReceived { get; set; }

        public int SharesBurned { get; set; }
    }

    public class DeactivateViewModel
    {
        public int Id { get; set; }

        public string DeactivateDate { get; set; }

        public double AmountReturned { get; set; }
    }
}