using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tester_core;
using tester_core.Models;

namespace tester_api.Controllers
{
    [Route("[controller]")]
    public class MetricsController : Controller
    {
        private readonly Services _services;

        public MetricsController(Services services)
        {
            _services = services;
        }

        public async Task<IActionResult> IndexAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Json("");

            var metrics = await _services.AccessPageAndGetResources(url.NormalizeUrl());

            StringBuilder sb = new StringBuilder();
            foreach (var metric in metrics)
            {
                var fields = typeof(AssetPerformance).GetProperties()
                                                     .Where(methodInfo => methodInfo.GetCustomAttributes(typeof(PrometheusAttribute), true).Length > 0)
                                                     .ToList();
                foreach (var field in fields)
                {
                    var name = field.GetCustomAttributes(typeof(PrometheusAttribute), true)[0].ToString();

                    sb.AppendLine($"# HELP {name} (SystemDriverResidentBytes)");
                    sb.AppendLine($"# TYPE {name} gauge");
                    sb.AppendLine($"{name}{{asset=\"{metric.name}\",type=\"{metric.entryType}\"}} {field.GetValue(metric)}");
                }

                //sb.AppendLine($"# HELP duration (SystemDriverResidentBytes)");
                //sb.AppendLine($"# TYPE duration gauge");
                //sb.AppendLine($"duration{{url=\"{url}\",asset=\"{metric.name}\"}} {metric.duration}");
            }

            return Ok(sb.ToString());

            //return Json(metrics);
        }
    }
}