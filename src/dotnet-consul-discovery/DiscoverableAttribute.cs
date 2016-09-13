using System;

namespace Consul.Discovery
{
    /// <summary>
    /// Allows services to be registered within service catalog.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class DiscoverableAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of an object.
        /// </summary>
        /// <param name="name">Service name.</param>
        /// <exception cref="ArgumentException">Occurs when name is null or an empty string.</exception>
        public DiscoverableAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Service name must be specified.", "name");
            }

            Name = name;

            Console.WriteLine("here");
            // Adding to the registry in case of auto-registration.
            Registry.Add(Name);
        }
    }
}
