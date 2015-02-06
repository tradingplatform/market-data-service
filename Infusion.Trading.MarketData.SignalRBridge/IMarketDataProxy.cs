using System;
using System.Collections.Generic;

namespace Infusion.Trading.MarketData.SignalRBridge
{
    public interface IMarketDataProxy
    {
        event EventHandler<Quote> MarketDataChanged;
        event EventHandler<MarketState> MarketStateChanged;

        IEnumerable<Quote> GetAllMarketData(params string[] securityIds);

        IEnumerable<string> FilterBySecurityIds(params string[] securityIds);

        MarketState MarketState { get; }

        void OpenMarket();
        void CloseMarket();
    }
}
