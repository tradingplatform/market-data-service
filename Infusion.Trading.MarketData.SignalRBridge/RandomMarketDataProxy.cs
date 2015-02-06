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

        private readonly ConcurrentDictionary<string, MarketData> _allMarketData = new ConcurrentDictionary<string, MarketData>();

        // Stock can go up or down by a percentage of this factor on each change
        private readonly double _rangePercent = 0.002;

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(250);
        private readonly Random _updateOrNotRandom = new Random();

        private readonly HashSet<string> filterBySecurityIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private Timer _timer;
        private volatile bool _updatingMarketDataPrices;
        private volatile MarketState _marketState;

        private RandomMarketDataProxy()
        {
            LoadDefaultMarketData();
            OpenMarket();
        }

        public event EventHandler<MarketData> MarketDataChanged;
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

        public IEnumerable<MarketData> GetAllMarketData(params string[] securityIds)
        {
            return securityIds == null || securityIds.Length == 0
                 ? _allMarketData.Values
                 : from symbol in securityIds
                   select _allMarketData[symbol];
        }

        public IEnumerable<string> FilterBySecurityIds(params string[] securityIds)
        {
            filterBySecurityIds.Clear();

            foreach (var id in from id in securityIds
                               where id != null
                               let trimmedId = id.Trim()
                               where 0 < trimmedId.Length && trimmedId.Length <= 5
                                  && trimmedId.All(char.IsLetter)
                               select trimmedId)
            {
                filterBySecurityIds.Add(id);

                yield return id;
            }
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

            List<MarketData> allMarketData =
                new List<MarketData>
                {
                    new MarketData { SecurityId = "KRX", Price = 75.86m },
                    new MarketData { SecurityId = "FB", Price = 77.83m },
                    new MarketData { SecurityId = "TWTR", Price = 37.10m },
                    new MarketData { SecurityId = "AMZN", Price = 307.32m },
                    new MarketData { SecurityId = "EBAY", Price = 55.77m },
                    new MarketData { SecurityId = "NFLX", Price = 334.48m },
                    new MarketData { SecurityId = "CI", Price = 101.73m },
                    new MarketData { SecurityId = "AET", Price = 87.30m },
                    new MarketData { SecurityId = "HUM", Price = 143.60m },
                    new MarketData { SecurityId = "UNH", Price = 98.76m },
                    new MarketData { SecurityId = "JPM", Price = 60.04m },
                    new MarketData { SecurityId = "FNMA", Price = 2.24m },
                    new MarketData { SecurityId = "MS", Price = 36.25m },
                    new MarketData { SecurityId = "WFC", Price = 53.70m },
                    new MarketData { SecurityId = "NKE", Price = 96.17m },
                    new MarketData { SecurityId = "BMW", Price = 87.988m },
                    new MarketData { SecurityId = "GM", Price = 31.57m },
                    new MarketData { SecurityId = "MSFT", Price = 46.95m },
                    new MarketData { SecurityId = "AAPL", Price = 109.73m },
                    new MarketData { SecurityId = "GOOG", Price = 518.66m }
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

        private bool TryUpdateMarketDataPrice(MarketData stock)
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

        private void OnMarketDataChanged(MarketData marketData)
        {
            if (MarketDataChanged != null)
            {
                MarketDataChanged(this, marketData);
            }
        }
    }
}
