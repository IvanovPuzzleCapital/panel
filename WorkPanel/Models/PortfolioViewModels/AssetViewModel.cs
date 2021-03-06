﻿using System;

namespace WorkPanel.Models.PortfolioViewModels
{
    public class AssetViewModel
    {
        public string Name { get; set; }

        public string ShortName { get; set; }

        public DateTime Date { get; set; }

        public double Quantity { get; set; }

        public double Price { get; set; }

        public double BTCPrice { get; set; }

        public bool HasBtc { get; set; }        

        public PurchaseType PurchaseType { get; set; }
    }

    public enum PurchaseType
    {
        USD,
        BTC
    }
}
