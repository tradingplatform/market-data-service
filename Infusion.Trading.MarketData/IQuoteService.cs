using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Infusion.Trading.MarketData
{
    [ContractClass(typeof(IQuoteServiceContract))]
    public interface IQuoteService
    {
        IObservable<Quote> GetQuotes(params string[] securityIds);

        Task<IReadOnlyCollection<Quote>> GetQuotesSnapshotAsync(params string[] securityIds);
    }

    [ContractClassFor(typeof(IQuoteService))]
    internal abstract class IQuoteServiceContract : IQuoteService
    {
        public IObservable<Quote> GetQuotes(params string[] securityIds)
        {
            Contract.Ensures(Contract.Result<IObservable<Quote>>() != null);
            return null;
        }

        public Task<IReadOnlyCollection<Quote>> GetQuotesSnapshotAsync(params string[] securityIds)
        {
            Contract.Ensures(Contract.Result<Task<IReadOnlyCollection<Quote>>>() != null);
            return null;
        }
    }
}
