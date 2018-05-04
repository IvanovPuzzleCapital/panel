using System.Collections.Generic;

namespace WorkPanel.Models.PortfolioViewModels
{
    public class PortfolioViewModel
    {
        public List<Asset> Assets { get; set; }

        public double NetAssetValue { get; set; }

        public double Acquisition { get; set; }

        public List<string> Currencies { get; set; }
    }
}