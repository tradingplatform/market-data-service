using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;

namespace Infusion.Trading.MarketData.SignalRBridge
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var proxy = new MarketDataProxy())
                {
                    GlobalHost.DependencyResolver.Register(typeof(MarketDataHub), () => new MarketDataHub(proxy));

                    Console.WriteLine("Enter a uri or hit enter (i.e. http://+:80)");
                    string input = Console.ReadLine();

                    // This will *ONLY* bind to localhost, if you want to bind to all addresses
                    // use http://*:8080 to bind to all addresses. 
                    // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
                    // for more information.
                    string url = "http://*:80";
                    if (!string.IsNullOrEmpty(input))
                    {
                        url = input;
                    }

                    using (WebApp.Start(url))
                    {
                        Console.WriteLine("Server running on {0}", url);
                        MarketDataHubNotifier notifier = new MarketDataHubNotifier(proxy);
                        Console.ReadLine();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR(new HubConfiguration { EnableDetailedErrors = true });
        }
    }
}
