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

        public string Agreement { get; set; }

        public double AmountInvested { get; set; }

        public int SharesReceived { get; set; }

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
            SharesReceived = viewModel.SharesReceived;
        }
    }

    public enum Status
    {
        Active,
        Inactive
    }
}
