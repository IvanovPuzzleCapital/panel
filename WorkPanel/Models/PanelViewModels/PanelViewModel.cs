using System.Collections.Generic;

namespace WorkPanel.Models.PanelViewModels
{
    public class PanelViewModel
    {
        public List<Investor> Investors { get; set; }

        public double TotalInvested { get; set; }

        public double TotalShares { get; set; }

        public double Profit { get; set; }

        public double CurrentShare { get; set; }
    }
}
