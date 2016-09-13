using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Consul.Discovery.Example.Controllers
{
    [Route("api/values")]
    [Discoverable("values-service")]
    public class ValuesController : Controller
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
