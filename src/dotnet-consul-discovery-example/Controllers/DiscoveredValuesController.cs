using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Newtonsoft.Json;

namespace Consul.Discovery.Example.Controllers.Controllers
{
    // This is a controller that consumes the service by taking address from service discovery.
    [Route("api/discovered/values")]
    public class DiscoveredValuesController
    {
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            // Step 1: Getting actual service address (base URL) via service discovery.
            string url = await DiscoveryClient.Default.GetServiceUrl("values-service");

            Console.WriteLine("*** client: " + DiscoveryClient.Default != null);

            // Step 2: Making a remote call to the service and reading the output.
            var response = await new HttpClient().GetAsync(new Uri(new Uri("http://" + url), "/api/values"));
            var json = await response.Content.ReadAsStringAsync();

            // Step 3: Deserializing JSON and returning data.
            return JsonConvert.DeserializeObject<string[]>(json);
        }
    }
}
