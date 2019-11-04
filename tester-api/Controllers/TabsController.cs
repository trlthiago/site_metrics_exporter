using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tester_core;

namespace tester_api.Controllers
{
    [Route("[controller]")]
    public class TabsController : Controller
    {
        private readonly Services _services;

        public TabsController(Services services)
        {
            _services = services;
        }

        [Route("index")]
        public async Task<IActionResult> IndexAsync()
        {
            var pages = await _services.GetTabsAsync();

            StringBuilder sb = new StringBuilder();
            foreach (var page in pages)
            {
                sb.AppendLine($"IsClosed={page.IsClosed};url={page.Url}");
            }

            return Ok(sb.ToString());
        }

        [Route("killall")]
        public async Task<IActionResult> KillAllAsync()
        {
            var pages = await _services.GetTabsAsync();

            StringBuilder sb = new StringBuilder();
            foreach (var page in pages)
            {
                sb.AppendLine($"IsClosed={page.IsClosed};url={page.Url}");
            }

            return Ok(sb.ToString());
        }
    }
}
