namespace WorkPanel.Models
{
    public class Asset
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public double Quantity { get; set; }

        public double Price { get; set; }

        public double PreviousPrice { get; set; }

        public double Exposure { get; set; }

        public double Currency { get; set; }

        public double PreviousCurrency { get; set; }

        public double Weight { get; set; }
    }
}