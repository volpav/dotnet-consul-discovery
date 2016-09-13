using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Consul.Discovery.Infrastructure;

namespace Consul.Discovery.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ModuleInitializer.Initialize();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls("http://0.0.0.0:4321")
                .Build();

            host.Run();
        }
    }
}
