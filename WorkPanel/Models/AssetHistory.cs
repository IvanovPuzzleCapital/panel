using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPanel.Models
{
    public class AssetHistory
    {
        public int Id { get; set; }

        public virtual Asset Asset { get; set; }

        public DateTime Date { get; set; }

        public TransactionType Type { get; set; }

        public double Price { get; set; }

        public double Quantity { get; set; }
    }

    public enum TransactionType
    {
        Buy,
        Sell
    }
}
