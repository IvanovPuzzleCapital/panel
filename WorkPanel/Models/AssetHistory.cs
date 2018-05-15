using System;

namespace WorkPanel.Models
{
    public class AssetHistory
    {
        public int Id { get; set; }

        public string ShortName { get; set; }

        public DateTime Date { get; set; }

        public DateTime RealDate { get; set; }

        public TransactionType Type { get; set; }

        public double Price { get; set; }

        public double Quantity { get; set; }        

        public double Rate { get; set; }
    }

    public enum TransactionType
    {
        Buy,
        Sell
    }
}