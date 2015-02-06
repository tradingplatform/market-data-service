using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Generic;

namespace Infusion.Trading.MarketData.SignalRBridge
{
    [HubName("marketData")]
    public class MarketDataHub : Hub
    {
        private readonly IMarketDataProxy _marketDataProxy;

        public MarketDataHub() :
            this(RandomMarketDataProxy.Instance)
        {
        }

        public MarketDataHub(IMarketDataProxy marketDataProxy)
        {
            _marketDataProxy = marketDataProxy;
        }

        public IEnumerable<Quote> GetAllMarketData(params string[] securityIds)
        {
            return _marketDataProxy.GetAllMarketData(securityIds);
        }

        public IEnumerable<string> FilterBySecurityIds(params string[] securityIds)
        {
            return _marketDataProxy.FilterBySecurityIds(securityIds);
        }

        public string GetMarketState()
        {
            return _marketDataProxy.MarketState.ToString();
        }
    }
}
