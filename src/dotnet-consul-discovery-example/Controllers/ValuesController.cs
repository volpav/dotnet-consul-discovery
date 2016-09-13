using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Consul.Discovery.Example.Controllers
{
    // This will auto-register the service but keep in mind that the class gets instantiated for every request.
    [Discoverable("values-service")]
    [Route("api/values")]
    public class ValuesController : Controller
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
