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
        private static DiscoveryClient _default;

        /// <summary>
        /// Gets or sets the global flag indicating whether to enable auto-registration of services on application start.
        /// </summary>
        public static bool EnableAutoRegistration
        {
            get { return _enableAutoRegistration; }
            set { _enableAutoRegistration = value; }
        }

        /// <summary>
        /// Gets or sets the default 
        /// </summary>
        public static DiscoveryClient Default
        {
            get { return _default; }
            set { _default = value; }
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
        /// Returns the URL of the registered service.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <returns>A task which, when completes, returns the URL.</returns>
        /// <exception cref="ArgumentException">Occurs when <paramref name="serviceName" /> is either null or an empty string.</exception> 
        public async Task<string> GetServiceUrl(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentException("Service name cannot be an empty string.", "serviceName");
            }

            // Returning the first available node.
            return (await _catalog.GetServiceNodes(serviceName)).FirstOrDefault();
        }
    }
}
