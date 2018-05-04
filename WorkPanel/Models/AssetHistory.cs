using System;

namespace WorkPanel.Models
{
    public class AssetHistory
    {
        public int Id { get; set; }

        public virtual Asset Asset { get; set; }

        public DateTime Date { get; set; }

        public double Value { get; set; }
    }
}