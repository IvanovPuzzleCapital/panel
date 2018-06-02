using System.Collections.Generic;

namespace WorkPanel.Models.PortfolioViewModels
{
    public class PortfolioViewModel
    {
        public List<Asset> Assets { get; set; }

        public bool HasBtc { get; set; }

        public double NetAssetValue { get; set; }

        public double Nav1W { get; set; }

        public double Nav1M { get; set; }

        public double Nav3M { get; set; }

        public double AssetsUnderManagement { get; set; }

        public double TotalInvested { get; set; }

        public double Acquisition { get; set; }        
    }
}