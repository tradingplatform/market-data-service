using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive.Disposables;

namespace Infusion.Trading.MarketData.SignalRBridge
{
    internal sealed class MarketDataProxy : IMarketDataProxy
    {
        public MarketState MarketState
        {
            get
            {
                return state;
            }
        }

        public event EventHandler<Quote> MarketDataChanged;
        public event EventHandler<MarketState> MarketStateChanged;

        private readonly QuoteService quoteService = new QuoteService();
        private readonly SerialDisposable subscription = new SerialDisposable();
        private readonly SecurityIdCollection filterBySecurityIds = new SecurityIdCollection();
        private MarketState state;

        public MarketDataProxy()
        {
            // TODO: Refactor IMarketDataProxy to expose a Load/Initialize method. It's better for callers' exceptations than having a constructor throwing an exception.
            quoteService.Load();

            subscription.Disposable = quoteService.GetQuotes().Subscribe(OnMarketDataChanged);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(quoteService != null);
        }

        public IEnumerable<Quote> GetAllMarketData(params string[] securityIds)
        {
            // TODO: Refactor IMarketDataProxy to be async so that we don't have to block here (.Result) and potentially cause dead locks.
            return quoteService.GetQuotesSnapshotAsync(securityIds).Result;
        }

        public IEnumerable<string> FilterBySecurityIds(params string[] securityIds)
        {
            subscription.Disposable = quoteService.GetQuotes(securityIds).Subscribe(OnMarketDataChanged);

            filterBySecurityIds.Clear();

            return filterBySecurityIds.TryAddRange(securityIds);
        }

        public void OpenMarket()
        {
            state = MarketState.Open;

            OnMarketStateChanged(state);
        }

        public void CloseMarket()
        {
            state = MarketState.Closed;

            OnMarketStateChanged(state);
        }

        private void OnMarketDataChanged(Quote e)
        {
            var handler = MarketDataChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnMarketStateChanged(MarketState e)
        {
            var handler = MarketStateChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Dispose()
        {
            subscription.Dispose();
        }
    }
}
