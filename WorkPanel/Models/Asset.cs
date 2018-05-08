using System.ComponentModel.DataAnnotations.Schema;

namespace WorkPanel.Models
{
    public class Asset
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public double Quantity { get; set; }

        public double PurchasePrice { get; set; }

        public double Price { get; set; }

        [NotMapped]
        public double AveragePrice { get; set; }

        public double Exposure { get; set; }        

    }
}