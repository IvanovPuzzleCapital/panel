using System;

namespace WorkPanel.Models.PortfolioViewModels
{
    public class AssetViewModel
    {

        public string Name { get; set; }

        public string ShortName { get; set; }

        public DateTime Date { get; set; }

        public double Quantity { get; set; }

        public double Price { get; set; }

        public bool HasBtc { get; set; }
    }
}
