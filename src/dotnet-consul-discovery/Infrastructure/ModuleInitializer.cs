using System;
using System.Linq;
using Consul.Discovery.Messaging;

namespace Consul.Discovery.Infrastructure
{
    /// <summary>
    /// Represents module initializer. This class is for internal use only and is not meant to be called from your code.
    /// </summary>
    public static class ModuleInitializer
    {
        private static bool _initialized;

        /// <summary>
        /// Initializes the module.
        /// </summary>
        public static void Initialize()
        {
            if (!_initialized && DiscoveryClient.EnableAutoRegistration)
            {
                // Extracting discovery server origin either from command line argument or from environment variable.
                string origin = Environment.GetCommandLineArgs()
                    .SkipWhile(arg => arg.IndexOf("--discovery-server-origin=", StringComparison.OrdinalIgnoreCase) != 0)
                    .Take(1).FirstOrDefault()?.Split('=')[1] ?? Environment.GetEnvironmentVariable("DISCOVERY_SERVER_ORIGIN");

                if (!string.IsNullOrWhiteSpace(origin))
                {
                    // Hard-wiring the catalog.
                    var catalog = new ConsulCatalog(new ConsulHttpClient(origin));

                    // Hard-wiring discovery client.
                    DiscoveryClient.Default = new DiscoveryClient(catalog);

                    RegistryQueue.Current.Processing += async (sender, args) =>
                    {
                        switch (args.RegistrationType)
                        {
                            case RegistrationType.Register:
                                // Hard-wiring the address of the service.
                                await catalog.Register(args.ServiceName, "0.0.0.0:4321");
                                break;
                            case RegistrationType.Unregister:
                                await catalog.Unregister(args.ServiceName);
                                break;
                        }
                    };

                    _initialized = true;
                }
            }
        }
    }
}
