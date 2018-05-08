using System;
using Newtonsoft.Json;

namespace WorkPanel.Models
{
    public class CurrencyRate
    {
        public int Id { get; set; }

        [JsonProperty("time")]
        public DateTime Date { get; set; }

        [JsonProperty("asset_id_base")]
        public string Base { get; set; }

        [JsonProperty("asset_id_quote")]
        public string Quote { get; set; }

        [JsonProperty("rate")]
        public double Rate { get; set; }
    }
}