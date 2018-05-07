using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using WorkPanel.Models;

namespace WorkPanel.DataExchange.Responses
{
    public class CoinApi
    {
        private string endpoint = "https://rest.coinapi.io/v1";

        private string apiKey = "AB38DB62-BD91-457B-B508-C49C25CA1951";

        public CoinApi()
        {

        }

        public async Task<List<Currency>> GetAllAssets()
        {
            var client = new RestClient(endpoint+"/assets");
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-CoinAPI-Key", apiKey);
            IRestResponse response = await client.ExecuteTaskAsync(request);


            return null;
        }

    }
}
