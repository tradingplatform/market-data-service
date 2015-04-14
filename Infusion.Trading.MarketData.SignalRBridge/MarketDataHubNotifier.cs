using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Infusion.Trading.MarketData.SignalRBridge
{
    public class MarketDataHubNotifier
    {
        private readonly IMarketDataProxy _marketDataProxy;
        private readonly IHubConnectionContext<dynamic> _clients;

        public MarketDataHubNotifier() :
            this(RandomMarketDataProxy.Instance)
        {
        }

        public MarketDataHubNotifier(IMarketDataProxy marketDataProxy) :
            this(marketDataProxy, GlobalHost.ConnectionManager.GetHubContext<MarketDataHub>().Clients)
        {
        }

        public MarketDataHubNotifier(IMarketDataProxy marketDataProxy, IHubConnectionContext<dynamic> clients)
        {
            _marketDataProxy = marketDataProxy;
            _clients = clients;

            _marketDataProxy.MarketStateChanged += HandleMarketStateChanged;
            _marketDataProxy.MarketDataChanged += HandleMarketDataChanged;
        }

        private void HandleMarketStateChanged(object sender, MarketState marketState)
        {
            switch (marketState)
            {
                case MarketState.Open:
                    _clients.All.marketOpened();
                    break;
                case MarketState.Closed:
                    _clients.All.marketClosed();
                    break;
            }
        }

        private void HandleMarketDataChanged(object sender, Quote marketData)
        {
            _clients.All.updateMarketData(marketData);
        }
    }
}
