using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using Consul.Discovery.Messaging;

namespace Consul.Discovery
{
    /// <summary>
    /// Represents registration type.
    /// </summary>
    internal enum RegistrationType
    {
        /// <summary>
        /// Register a service.
        /// </summary>
        Register = 1,

        /// <summary>
        /// Unregister a service.
        /// </summary>
        Unregister = 2
    }

    /// <summary>
    /// Represents registration event arguments.
    /// </summary>
    internal sealed class RegistrationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the service name.
        /// </summary>
        public string ServiceName { get; private set; }

        /// <summary>
        /// Gets the registration type.
        /// </summary>
        public RegistrationType RegistrationType { get; private set; }

        /// <summary>
        /// Initializes a new instance of an object.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <param name="registrationType">Registration type.</param>
        /// <exception cref="ArgumentException">Occurs when service name is either null or an empty string.</exception>
        public RegistrationEventArgs(string serviceName, RegistrationType registrationType)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentException("Service name must be specified.", "serviceName");
            }

            ServiceName = serviceName;
            RegistrationType = registrationType;
        }
    }

    /// <summary>
    /// Represents a registry for the purpose of auto-registration.
    /// </summary>
    internal static class Registry
    {
        private static readonly ConcurrentQueue<Tuple<string, RegistrationType>> _items;
        private static readonly Timer _timer;

        /// <summary>
        /// Occurs when registry queue is processing the next item.
        /// </summary>
        public static event EventHandler<RegistrationEventArgs> Processing;

        /// <summary>
        /// Initializes a new instance of an object.
        /// </summary>
        static Registry()
        {
            int ms = 5000;

            _items = new ConcurrentQueue<Tuple<string, RegistrationType>>();
            _timer = new Timer((state) => ProcessNextItem(), null, ms, ms);
            
            ConfigureAutoRegistration();
        }

        /// <summary>
        /// Adds new item to the registry.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <param name="registrationType">Registration type.</param>
        public static void Add(string serviceName, RegistrationType registrationType = RegistrationType.Register)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentException("Service name must be specified.", "serviceName");
            }

            _items.Enqueue(new Tuple<string, RegistrationType>(serviceName, registrationType));
        }

        /// <summary>
        /// Processes next item.
        /// </summary>
        private static void ProcessNextItem()
        {
            Tuple<string, RegistrationType> nextItem;

            if (DiscoveryClient.EnableAutoRegistration && _items.TryDequeue(out nextItem))
            {
                OnProcessing(new RegistrationEventArgs(
                    serviceName: nextItem.Item1, 
                    registrationType: nextItem.Item2
                ));
            }
        }

        /// <summary>
        /// Raises "Processing" event.
        /// </summary>
        /// <param name="args">Arguments.</param>
        private static void OnProcessing(RegistrationEventArgs args)
        {
            Processing?.Invoke(null, args);
        }

        /// <summary>
        /// Configures auto-registration.
        /// </summary>
        private static void ConfigureAutoRegistration()
        {
            string key = "discovery-server-origin";

            // Extracting discovery server origin either from command line argument or from environment variable.
            string origin = Environment.GetCommandLineArgs()
                .SkipWhile(arg => arg.IndexOf(string.Concat("--", key, "="), StringComparison.OrdinalIgnoreCase) != 0)
                .Take(1).FirstOrDefault()?.Split('=')[1] ?? 
                Environment.GetEnvironmentVariable(key.ToUpperInvariant().Replace('-', '_'));

            Interlocked.CompareExchange(
                ref DiscoveryClient._default,
                !string.IsNullOrWhiteSpace(origin) ? new DiscoveryClient(
                    new ConsulCatalog(
                        new ConsulHttpClient(origin)
                    )
                ) : null,
                null
            );

            DiscoveryClient client = DiscoveryClient.Default;

            if (client != null)
            {
                Processing += async (sender, args) =>
                {
                    switch (args.RegistrationType)
                    {
                        case RegistrationType.Register:
                            // FIXME: Find a good way of exposing this.
                            await client.Catalog.Register(args.ServiceName, "127.0.0.1:4321");
                            break;
                        case RegistrationType.Unregister:
                            await client.Catalog.Unregister(args.ServiceName);
                            break;
                    }
                };
            }
        }
    }
}
