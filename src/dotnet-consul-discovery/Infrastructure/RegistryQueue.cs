using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Consul.Discovery.Infrastructure
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
    /// Represents a registry queue.
    /// </summary>
    internal sealed class RegistryQueue
    {
        private readonly ConcurrentQueue<Tuple<string, RegistrationType>> _items = 
            new ConcurrentQueue<Tuple<string, RegistrationType>>();

        private readonly Timer _timer;
        
        private static readonly RegistryQueue _current = new RegistryQueue();

        /// <summary>
        /// Occurs when registry queue is processing the next item.
        /// </summary>
        public event EventHandler<RegistrationEventArgs> Processing;

        /// <summary>
        /// Gets the current registry queue.
        /// </summary>
        public static RegistryQueue Current
        {
            get { return _current; }
        }

        /// <summary>
        /// Initializes a new instance of an object.
        /// </summary>
        public RegistryQueue()
        {
            int ms = 5000;

            // Starting in [ms] milliseconds, running every [ms] milliseconds.
            _timer = new Timer((state) => ProcessNextItem(), null, ms, ms);
        }

        /// <summary>
        /// Adds new item to the queue.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <param name="registrationType">Registration type.</param>
        public void Add(string serviceName, RegistrationType registrationType = RegistrationType.Register)
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
        private void ProcessNextItem()
        {
            Tuple<string, RegistrationType> nextItem;

            if (_items.TryDequeue(out nextItem))
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
        private void OnProcessing(RegistrationEventArgs args)
        {
            Processing?.Invoke(this, args);
        }
    }
}
