using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Infusion.Trading.MarketData.SignalRBridge
{
    public class RandomMarketDataProxy : IMarketDataProxy
    {
        // Singleton instance
        private readonly static RandomMarketDataProxy _instance = new RandomMarketDataProxy();

        private readonly object _marketStateLock = new object();
        private readonly object _updateMarketDataLock = new object();

        private readonly ConcurrentDictionary<string, Quote> _allMarketData = new ConcurrentDictionary<string, Quote>();

        // Stock can go up or down by a percentage of this factor on each change
        private readonly double _rangePercent = 0.002;

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(250);
        private readonly Random _updateOrNotRandom = new Random();

        private readonly SecurityIdCollection filterBySecurityIds = new SecurityIdCollection();

        private Timer _timer;
        private volatile bool _updatingMarketDataPrices;
        private volatile MarketState _marketState;

        private RandomMarketDataProxy()
        {
            LoadDefaultMarketData();
            OpenMarket();
        }

        public event EventHandler<Quote> MarketDataChanged;
        public event EventHandler<MarketState> MarketStateChanged;

        public static RandomMarketDataProxy Instance
        {
            get { return _instance; }
        }

        public MarketState MarketState
        {
            get { return _marketState; }
            private set { _marketState = value; }
        }

        public IEnumerable<Quote> GetAllMarketData(params string[] securityIds)
        {
            return securityIds == null || securityIds.Length == 0
                 ? _allMarketData.Values
                 : from symbol in securityIds
                   select _allMarketData[symbol];
        }

        public IEnumerable<string> FilterBySecurityIds(params string[] securityIds)
        {
            filterBySecurityIds.Clear();

            return filterBySecurityIds.TryAddRange(securityIds);
        }

        public void OpenMarket()
        {
            lock (_marketStateLock)
            {
                if (MarketState != MarketState.Open)
                {
                    _timer = new Timer(UpdateMarketDataPrices, null, _updateInterval, _updateInterval);

                    MarketState = MarketState.Open;

                    OnMarketStateChanged(MarketState.Open);
                }
            }
        }

        public void CloseMarket()
        {
            lock (_marketStateLock)
            {
                if (MarketState == MarketState.Open)
                {
                    if (_timer != null)
                    {
                        _timer.Dispose();
                    }

                    MarketState = MarketState.Closed;

                    OnMarketStateChanged(MarketState.Closed);
                }
            }
        }

        private void LoadDefaultMarketData()
        {
            _allMarketData.Clear();

            List<Quote> allMarketData =
                new List<Quote>
                {
                    new Quote { SecurityId = "KRX", Price = 75.86m },
                    new Quote { SecurityId = "FB", Price = 77.83m },
                    new Quote { SecurityId = "TWTR", Price = 37.10m },
                    new Quote { SecurityId = "AMZN", Price = 307.32m },
                    new Quote { SecurityId = "EBAY", Price = 55.77m },
                    new Quote { SecurityId = "NFLX", Price = 334.48m },
                    new Quote { SecurityId = "CI", Price = 101.73m },
                    new Quote { SecurityId = "AET", Price = 87.30m },
                    new Quote { SecurityId = "HUM", Price = 143.60m },
                    new Quote { SecurityId = "UNH", Price = 98.76m },
                    new Quote { SecurityId = "JPM", Price = 60.04m },
                    new Quote { SecurityId = "FNMA", Price = 2.24m },
                    new Quote { SecurityId = "MS", Price = 36.25m },
                    new Quote { SecurityId = "WFC", Price = 53.70m },
                    new Quote { SecurityId = "NKE", Price = 96.17m },
                    new Quote { SecurityId = "BMW", Price = 87.988m },
                    new Quote { SecurityId = "GM", Price = 31.57m },
                    new Quote { SecurityId = "MSFT", Price = 46.95m },
                    new Quote { SecurityId = "AAPL", Price = 109.73m },
                    new Quote { SecurityId = "GOOG", Price = 518.66m }
                };

            allMarketData.ForEach(marketData => _allMarketData.TryAdd(marketData.SecurityId, marketData));
        }

        private void UpdateMarketDataPrices(object state)
        {
            // This function must be re-entrant as it's running as a timer interval handler
            lock (_updateMarketDataLock)
            {
                if (!_updatingMarketDataPrices)
                {
                    _updatingMarketDataPrices = true;

                    foreach (var marketData in _allMarketData.Values)
                    {
                        if (TryUpdateMarketDataPrice(marketData)
                            && (filterBySecurityIds.Count == 0 || filterBySecurityIds.Contains(marketData.SecurityId)))
                        {
                            OnMarketDataChanged(marketData);
                        }
                    }

                    _updatingMarketDataPrices = false;
                }
            }
        }

        private bool TryUpdateMarketDataPrice(Quote stock)
        {
            // Randomly choose whether to update
            double r = _updateOrNotRandom.NextDouble();
            if (r > 0.1)
            {
                return false;
            }

            // Update the price by a random factor of the range percent
            Random random = new Random((int)Math.Floor(stock.Price));
            double percentChange = random.NextDouble() * _rangePercent;
            bool isPositiveChange = random.NextDouble() > 0.51;
            decimal change = Math.Round(stock.Price * (decimal)percentChange, 2);
            change = isPositiveChange ? change : -change;

            stock.Price += change;
            return true;
        }

        private void OnMarketStateChanged(MarketState marketState)
        {
            if (MarketStateChanged != null)
            {
                MarketStateChanged(this, marketState);
            }
        }

        private void OnMarketDataChanged(Quote marketData)
        {
            if (MarketDataChanged != null)
            {
                MarketDataChanged(this, marketData);
            }
        }

        public void Dispose()
        {
        }
    }
}
