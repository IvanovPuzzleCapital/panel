using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using WorkPanel.Models;

namespace WorkPanel.DataExchange
{
    public class CoinApi
    {
        private const string endpoint = "https://rest.coinapi.io/v1";

        private readonly string apiKey = "AB38DB62-BD91-457B-B508-C49C25CA1951";

        public CoinApi(IConfiguration configuration)
        {
            var key = configuration.GetSection("CoinApiKey").Value;
            if (key != null)
                apiKey = key;
        }

        public async Task<List<Currency>> GetAllAssets()
        {
            var client = new RestClient(endpoint+"/assets");
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-CoinAPI-Key", apiKey);
            var response = await client.ExecuteTaskAsync(request);
            if (!response.IsSuccessful) return null;
            var list = JsonConvert.DeserializeObject<List<Currency>>(response.Content);
            return list;
        }

        public async Task<CurrencyRate> GetCurrencyRateToUsd(string shortName)
        {            
            var client = new RestClient(endpoint + "/exchangerate/" + shortName + "/USD");
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-CoinAPI-Key", apiKey);
            var response = await client.ExecuteTaskAsync(request);
            if (!response.IsSuccessful) return null;
            var rate = JsonConvert.DeserializeObject<CurrencyRate>(response.Content);
            rate.Date = DateTime.Now;
            return rate;
        }
    }
}
