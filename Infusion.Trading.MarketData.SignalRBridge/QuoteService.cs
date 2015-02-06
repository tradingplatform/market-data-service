using Infusion.Trading.MarketData.SignalRBridge.Properties;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Infusion.Trading.MarketData.SignalRBridge
{
    internal sealed class QuoteService : IQuoteService
    {
        private static readonly string pluginsDirectory = Path.Combine(Environment.CurrentDirectory, Settings.Default.PluginsDirectory);
        private static readonly Lazy<List<Assembly>> assemblies = new Lazy<List<Assembly>>(() =>
            Directory.EnumerateFiles(pluginsDirectory, Settings.Default.PluginAssemblyPattern)
                     .Select(Assembly.LoadFrom)
                     .ToList());

        [Import]
        private IQuoteService instance { get; set; }

        public void Load()
        {
            var config = new ContainerConfiguration().WithAssemblies(assemblies.Value);

            using (var host = config.CreateContainer())
            {
                host.SatisfyImports(this);
            }
        }

        public IObservable<Quote> GetQuotes(params string[] securityIds)
        {
            return instance.GetQuotes(securityIds);
        }

        public Task<IReadOnlyCollection<Quote>> GetQuotesSnapshotAsync(params string[] securityIds)
        {
            return instance.GetQuotesSnapshotAsync(securityIds);
        }
    }
}
