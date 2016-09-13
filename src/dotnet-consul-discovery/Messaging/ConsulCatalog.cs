using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Consul.Discovery.Messaging
{
    /// <summary>
    /// Represents Consul catalog.
    /// </summary>
    public class ConsulCatalog
    {
        private readonly ConsulHttpClient _client;

        /// <summary>
        /// Gets the Consul HTTP client used to communicate with Consul.
        /// </summary>
        protected ConsulHttpClient Client
        {
            get { return _client; }
        }

        /// <summary>
        /// Initializes a new instance of an object.
        /// </summary>
        /// <param name="baseUrl">Base URL to Consul API.</param>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="client" /> is null.</exception> 
        public ConsulCatalog(ConsulHttpClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            _client = client;
        }

        /// <summary>
        /// Returns a list of all available service nodes.
        /// </summary>
        /// <param name="name">Service name.</param>
        /// <returns>A task which, when completes, returns a list of all the service nodes associated with the service.</returns>
        /// <exception cref="ArgumentException">Occurs when <paramref name="name" /> is null or empty string.</exception> 
        public async Task<IEnumerable<string>> GetServiceNodes(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Service name must be specified.", "name");
            }

            // Loading all nodes.
            var nodes = Client.DeserializeContent<List<dynamic>>(
                await Client.Get(string.Concat("catalog/service/", name, "?near=_agent"))
            );

            return nodes.Any() ? nodes.Select(n =>
            {
                string address = n["ServiceAddress"];

                // If the service didn't provide an address, taking agent's address.
                if (string.IsNullOrWhiteSpace(address))
                {
                    address = n["Address"];
                }

                // Appending port.
                address = string.Concat(address, ":", n["ServicePort"]);

                return address;
            }) : Enumerable.Empty<string>();
            
        }

        /// <summary>
        /// Registers the service with the given name and network address.
        /// </summary>
        /// <param name="name">Service name.</param>
        /// <param name="address">Service network address. For example, "127.0.0.1:8080".</param>
        /// <exception cref="ArgumentException">Occurs when either <paramref name="name" /> or <paramref name="address" /> are null or empty strings.</exception> 
        /// <returns>A task which, when completes, indicates of a successful registration.</returns>
        public async Task Register(string name, string address)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Service name must be specified.", "name");
            }
                
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Service address (host/port) must be specified.", "address");
            }

            // Making sure we can parse URL by appending "http://" as default scheme (we're don't actually need it).
            var uri = new Uri(address.Contains("://") ? address : string.Concat("http://", address), UriKind.Absolute);

            await Client.Put(
                "agent/service/register", 
                new
                {
                    ID = name,
                    Name = name,
                    Address = uri.Host,
                    Port = uri.Port
                }
            );
        }

        /// <summary>
        /// Unregisters the service with the given name.
        /// </summary>
        /// <param name="name">Service name.</param>
        /// <returns>A task which, when completes, indicates of a successful unregistration.</returns>
        /// <exception cref="ArgumentException">Occurs when <paramref name="name" /> is null or empty string.</exception> 
        public async Task Unregister(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Service name must be specified.", "name");
            }

            await Client.Put(string.Concat("agent/service/deregister/", name), requestBody: null);
        }
    }
}
