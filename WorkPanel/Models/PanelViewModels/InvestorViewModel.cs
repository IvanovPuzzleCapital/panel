using System;

namespace WorkPanel.Models.PanelViewModels
{
    public class InvestorViewModel
    {
        public int Id { get; set; }

        public Status Status { get; set; }

        public string Name { get; set; }

        public DateTime Date { get; set; }

        public string Agreement { get; set; }

        public double AmountInvested { get; set; }

        public int SharesReceived { get; set; }
    }
}