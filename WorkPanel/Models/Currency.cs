using Newtonsoft.Json;

namespace WorkPanel.Models
{
    public class Currency
    {
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("asset_id")]
        public string ShortName { get; set; }

        [JsonProperty("type_is_crypto")]
        public bool IsCrypto { get; set; }

        public double Rate { get; set; }
    }
}