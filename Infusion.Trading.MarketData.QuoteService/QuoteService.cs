using Infusion.Trading.MarketData.QuoteService.Properties;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Infusion.Trading.MarketData.QuoteService
{
    [Export(typeof(IQuoteService))]
    internal sealed class QuoteService : IQuoteService
    {
        private const string exchange = "NASDAQ";

        // TODO: Don't use Google Finance - it's no longer officially supported and it doesn't appear to have all of the data we need anyway.
        // See example response file in the Solution Items folder: GoogleFinanceResponse-Example.json
        private static readonly Uri baseUrl = new Uri("http://finance.google.com/finance/");

        public async Task<IReadOnlyCollection<Quote>> GetQuotesSnapshotAsync(params string[] securityIds)
        {
            var parsedIds = SecurityIdCollection.TryParse(securityIds).ToList();

            if (parsedIds.Count == 0)
            {
                return new List<Quote>(0);
            }

            using (var client = new HttpClient()
            {
                BaseAddress = baseUrl
            })
            {
                var symbols = string.Join(",", parsedIds);

                var results = await client.GetStringAsync("info?client=ig&q=" + exchange + ":" + symbols).ConfigureAwait(false);

                var obj = (dynamic)Newtonsoft.Json.JsonConvert.DeserializeObject(results.Substring(4));

                var quotes = new List<Quote>(obj.Count);

                foreach (var quote in obj)
                {
                    quotes.Add(new Quote((string)quote.t, (decimal)quote.l, (decimal)quote.c));
                }

                return quotes;
            }
        }

        public IObservable<Quote> GetQuotes(params string[] securityIds)
        {
            return Observable.Timer(TimeSpan.Zero, Settings.Default.PollInterval)
                             .SelectMany(_ => GetQuotesSnapshotAsync(securityIds))
                             .SelectMany(quotes => quotes);
        }
    }
}
