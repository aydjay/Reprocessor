using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reprocessor.Models;
using RestSharp;

namespace Reprocessor
{
    public class PriceCache
    {
        //EveCrest _api = new EveCrest();
        private static readonly int JitaId = 30000142;
        private static readonly int TheForgeRegionId = 10000002;

        //see: https://market.fuzzwork.co.uk/api/
        //string _marketDataUrl = https://market.fuzzwork.co.uk/aggregates/?station=60003760&types;
        //Item id vs price data as int (TBD) 
        public Dictionary<int, double> Cache = new Dictionary<int, double>();

        public void GetPrices(int itemId)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(itemId) == false)
                {
                    Cache.Add(itemId, GetMarketHistory(itemId).Price);
                }
            }
        }

        public void GetPrices(List<int> itemIdsForPriceCheck)
        {
            foreach (var itemId in itemIdsForPriceCheck)
            {
                GetPrices(itemId);
            }
        }

        private Order GetMarketHistory(int itemId)
        {
            var client = new RestClient("https://esi.tech.ccp.is/latest");

            var request = new RestRequest("markets/{regionId}/orders/?datasource=tranquility&order_type=sell&type_id={typeId}", Method.GET);

            request.AddUrlSegment("typeId", itemId); // adds to POST or URL querystring based on Method
            request.AddUrlSegment("regionId", TheForgeRegionId);

            var reponse = client.Execute<List<Order>>(request);
            if (reponse.IsSuccessful == false)
            {
                throw new NoMarketOrdersException();
            }

            var pageResponse = reponse.Headers.First(x => x.Name == "X-Pages");
            if (pageResponse.Value.ToString() != "1")
            {
                throw new NotSupportedException("We are only processing the first page at this time. We will need to iterate over the number of pages that it is responding with if we want all the data available");
            }
            var content = reponse.Content;
            
            if (reponse.ErrorException != null)
                throw reponse.ErrorException;
            
            if(reponse.Data.Any(x => x.IsBuyOrder))
                throw new NotSupportedException("We are not processing buy orders at the moment");

            if (reponse.Data.Count == 0)
                throw new NoMarketOrdersException();

            return reponse.Data.OrderByDescending(x => x.Price).Last();
        }
    }

    internal class NoMarketOrdersException : Exception
    {
    }
}