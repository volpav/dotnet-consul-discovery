using Consul.Discovery.Messaging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.Discovery
{
    /// <summary>
    /// Represents a discovery client.
    /// </summary>
    public sealed class DiscoveryClient
    {
        private ConsulCatalog _catalog;

        private static bool _enableAutoRegistration = true;
        internal static DiscoveryClient _default;

        /// <summary>
        /// Gets or sets the global flag indicating whether to enable auto-registration of services on application start.
        /// </summary>
        public static bool EnableAutoRegistration
        {
            get { return _enableAutoRegistration; }
            set { _enableAutoRegistration = value; }
        }

        /// <summary>
        /// Gets or sets the default client instance.
        /// </summary>
        public static DiscoveryClient Default
        {
            get { return _default; }
            set { _default = value; }
        }

        /// <summary>
        /// Gets the catalog.
        /// </summary>
        public ConsulCatalog Catalog
        {
            get { return _catalog; }
        }

        /// <summary>
        /// Initializes a new instance of an object.
        /// </summary>
        /// <param name="catalog">Service catalog.</param>
        /// <exception cref="ArgumentNullException">Occurs when catalog is null.</exception>
        public DiscoveryClient(ConsulCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException("catalog");
            }

            _catalog = catalog;
        }

        /// <summary>
        /// Registers the service with the given name and network address.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <param name="serviceAddress">Service network address. For example, "127.0.0.1:8080".</param>
        /// <returns>A task which, when completes, indicates of a successful registration.</returns>
        public async Task Register(string serviceName, string serviceAddress)
        {
            await _catalog.Register(serviceName, serviceAddress);
        }

        /// <summary>
        /// Unregisters the service with the given name.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <returns>A task which, when completes, indicates of a successful unregistration.</returns>
        public async Task Unregister(string serviceName)
        {
            await _catalog.Unregister(serviceName);
        }

        /// <summary>
        /// Returns the URL of the registered service.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <returns>A task which, when completes, returns the URL.</returns>
        public async Task<string> GetServiceUrl(string serviceName)
        {
            // Returning the first available node.
            return (await _catalog.GetServiceNodes(serviceName)).FirstOrDefault();
        }
    }
}
