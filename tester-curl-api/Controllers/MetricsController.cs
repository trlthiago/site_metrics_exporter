using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace tester_curl_api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class MetricsController : ControllerBase
    {
        private ILogger<MetricsController> _logger;

        public MetricsController(ILogger<MetricsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get(string url)
        {
            var escapedArgs = "-i -L -S --stderr - http://" + url;

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "curl",
                    Arguments = escapedArgs,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit(61000);

            var obj = new Output();

            if (string.IsNullOrWhiteSpace(result))
                return obj.SetError("2", "completely empty response");

            var match = Regex.Match(result, @"(curl:)\s(\(\d+\))(.*)(\n)");
            if (match.Success)
                return obj.SetError("3", match.Value);

            //var headersEnd = GetIndexOfHeadersEnd(result);

            //string headers = headersEnd == -1 ? result : result.Substring(0, headersEnd);

            match = Regex.Match(result, @"HTTP\/.*\s(?<status>\d{3,3})\s(?<response>.*)", RegexOptions.RightToLeft); //I've been used to use the headers insted of result until now.
            //but thinking twice, i guess we will the same result;

            if (match.Success)
            {
                obj.SetStatusCode(match);
                EvaluateTitleCondition(obj, result);
                return obj.ToString();
            }
            else
            {
                return obj.SetError("3", "status code not found");
            }
        }

        public int GetIndexOfHeadersEnd(string result)
        {
            var headersEnd = result.IndexOf("\n\n\n");

            if (headersEnd == -1)
                headersEnd = result.IndexOf("\r\n\r\n");

            if (headersEnd == -1)
                headersEnd = result.IndexOf('<');

            return headersEnd;
        }

        public void EvaluateTitleCondition(Output obj, string result)
        {
            if (obj.IsSiteUp == "0")
            {
                var match = Regex.Match(result, @"(\<title.*\>)(?<title>(?<status>5\d\d).*)(\<)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var status = match.Groups["status"].Value;
                    var response = match.Groups["title"].Value;
                    obj.SetError(status, response);
                }

            }
        }
    }
}