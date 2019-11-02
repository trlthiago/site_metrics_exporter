using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tester_core;
using tester_core.Models;

namespace tester_api.Controllers
{
    [Route("[controller]")]
    public class MonitorController : Controller
    {
        private readonly Services _services;

        public MonitorController(Services services)
        {
            _services = services;
        }

        public async Task<IActionResult> IndexAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Json("");

            var siteMetrics = await _services.AccessPageAndTakeScreenshot(url.NormalizeUrl(), true);

            StringBuilder sb = new StringBuilder();

            var name = "site_up";
            sb.AppendLine($"# HELP {name} TRL");
            sb.AppendLine($"# TYPE {name} gauge");
            sb.AppendLine($"{name}{{response=\"{siteMetrics.SiteStatus}\",screenshot=\"{siteMetrics.ScreenShotPath}\"}} {siteMetrics.IsSiteUp}");

            if (siteMetrics.Assets != null)
                foreach (var metric in siteMetrics.Assets)
                {
                    var fields = typeof(AssetPerformance).GetProperties()
                                                         .Where(methodInfo => methodInfo.GetCustomAttributes(typeof(PrometheusAttribute), true).Length > 0)
                                                         .ToList();
                    foreach (var field in fields)
                    {
                        name = field.GetCustomAttributes(typeof(PrometheusAttribute), true)[0].ToString();

                        sb.AppendLine($"# HELP {name} (SystemDriverResidentBytes)");
                        sb.AppendLine($"# TYPE {name} gauge");
                        sb.AppendLine($"{name}{{asset=\"{metric.name}\",type=\"{metric.entryType}\"}} {field.GetValue(metric)}");
                    }
                }
            return Ok(sb.ToString());
        }
    }
}