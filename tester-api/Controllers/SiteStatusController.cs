using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tester_core;
using tester_core.Models;

namespace tester_api.Controllers
{
    [Route("[controller]")]
    public class SiteStatusController : Controller
    {
        private readonly Services _services;

        public SiteStatusController(Services services)
        {
            _services = services;
        }

        public async Task<IActionResult> IndexAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Json("");

            var site = await _services.AccessPage(url.NormalizeUrl());
            
            StringBuilder sb = new StringBuilder();
            var name = "site_up";
            sb.AppendLine($"# HELP {name} TRL");
            sb.AppendLine($"# TYPE {name} gauge");
            sb.AppendLine($"{name} {site.IsSiteUp}");

            name = "site_response_status";
            sb.AppendLine($"# HELP {name} TRL");
            sb.AppendLine($"# TYPE {name} gauge");
            sb.AppendLine($"{name} {site.SiteStatus}");

            return Ok(sb.ToString());
        }
    }
}