using System;
using WorkPanel.Models.PanelViewModels;

namespace WorkPanel.Models
{
    public class Investor
    {
        public int Id { get; set; }

        public Status Status { get; set; }

        public string Name { get; set; }

        public DateTime Date { get; set; }

        public DateTime DeactivateDate { get; set; }

        public string Agreement { get; set; }

        public double AmountInvested { get; set; }

        public double AmountReturned{ get; set; }

        public int SharesReceived { get; set; }

        public int SharesBurned { get; set; }

        public Investor()
        {
            
        }

        public Investor(InvestorViewModel viewModel)
        {
            Id = viewModel.Id;
            Status = viewModel.Status;
            Name = viewModel.Name;
            Date = viewModel.Date;
            Agreement = viewModel.Agreement;
            AmountInvested = viewModel.AmountInvested;
            AmountReturned = 0;
            SharesReceived = viewModel.SharesReceived;
            SharesBurned = 0;
        }
    }

    public enum Status
    {
        Active,
        Inactive
    }
}
