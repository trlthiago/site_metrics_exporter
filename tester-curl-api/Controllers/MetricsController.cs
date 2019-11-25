using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;

namespace tester_curl_api.Controllers
{
    public class Output
    {
        public string SiteUp { get; set; }
        public string SiteUpRaw { get; set; }
        public string SiteStatusCode { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class MetricsController : ControllerBase
    {
        private ILogger<MetricsController> _logger;

        public MetricsController(ILogger<MetricsController> logger)
        {
            _logger = logger;
        }

        private string FormatSiteError(Match match)
        {
            int status = int.Parse(match.Groups["status"].Value);

            StringBuilder sb = new StringBuilder();
            var name = "site_up_raw";
            sb.AppendLine($"# TYPE {name} gauge");
            sb.AppendLine($"{name} {(status > 299 ? "0" : "1")}");

            name = "site_status_code";
            sb.AppendLine($"# TYPE {name} gauge");
            sb.AppendLine($"{name} {status}");

            return sb.ToString();
        }

        private string FormatNetworkError(string stdErr)
        {
            StringBuilder sb = new StringBuilder();

            var name = "site_up";
            sb.AppendLine($"# TYPE {name} gauge");
            sb.AppendLine($"{name}{{response=\"{stdErr.Replace("\n", "").Trim()}\"}} 0");

            name = "site_up_raw";
            sb.AppendLine($"# TYPE {name} gauge");
            sb.AppendLine($"{name} 0");

            name = "site_status_code";
            sb.AppendLine($"# TYPE {name} gauge");
            sb.AppendLine($"{name} 2");

            return sb.ToString();
        }

        private string FormatUnknowError()
        {
            StringBuilder sb = new StringBuilder();

            var name = "site_up";
            sb.AppendLine($"# TYPE {name} gauge");
            sb.AppendLine($"{name}{{response=\"unknow error\"}} 0");

            name = "site_up_raw";
            sb.AppendLine($"# TYPE {name} gauge");
            sb.AppendLine($"{name} 0");

            name = "site_status_code";
            sb.AppendLine($"# TYPE {name} gauge");
            sb.AppendLine($"{name} 3");

            return sb.ToString();
        }

        [HttpGet]
        public string Get(string url)
        {
            var escapedArgs = "-i -L -S --stderr - http://" + url;

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "curl",
                    Arguments = escapedArgs,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit(61000);
            System.Threading.Thread.Sleep(2000);
            string error1 = process.StandardError.ReadToEnd();

            bool flag_error = string.IsNullOrWhiteSpace(result);

            if (flag_error)
                return FormatUnknowError();

            var match = Regex.Match(result, @"(curl:)\s(\(\d+\))(.*)(\n)");
            if (match.Success)
                return FormatNetworkError(match.Value);

            var headersEnd = result.IndexOf("\n\n\n");
            if (headersEnd == -1)
            {
                headersEnd = result.IndexOf('<');
                if (headersEnd == -1)
                    flag_error = true;
            }

            string headers = result.Substring(0, headersEnd);

            match = Regex.Match(headers, @"HTTP\/(.*)\s(?<status>\d{3,3})", RegexOptions.RightToLeft);

            StringBuilder sb = new StringBuilder();
            var name = "site_up_raw";
            //sb.AppendLine($"# HELP {name} TRL");
            sb.AppendLine($"# TYPE {name} gauge");

            if (match.Success)
            {
                int status = int.Parse(match.Groups["status"].Value);
                sb.AppendLine($"{name} {(status > 299 ? "0" : "1")}");

                name = "site_status_code";
                sb.AppendLine($"# TYPE {name} gauge");
                sb.AppendLine($"{name} {status}");
            }
            else
            {
                sb.AppendLine($"{name} 2");
            }

            return sb.ToString();
        }
    }
}

//var statuses = Regex.Matches(headers, @"HTTP\/(.*)\s(?<status>\d{3,3})");
//statuses[statuses.Count - 1].Groups["status"].Value