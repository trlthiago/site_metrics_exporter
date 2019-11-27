using System.Text.RegularExpressions;
using System.Text;

namespace tester_curl_api.Controllers
{
    public class Output
    {
        public string SiteUp { get; set; }
        public string Response { get; set; }
        public string SiteStatusCode { get; private set; }
        public string IsSiteUp => int.Parse(SiteStatusCode) >= 200 && int.Parse(SiteStatusCode) < 300 ? "1" : "0";

        public void SetStatusCode(Match match)
        {
            Response = match.Groups["response"].Value;
            SiteStatusCode = match.Groups["status"].Value;
        }

        public void SetOtherStatusBasedOnStatusCode()
        {
            int status = GetStatusCode();
            SiteUp = status >= 200 && status < 300 ? "1" : "0";
            Response = status >= 200 && status < 300 ? "OK" : "NOK";
        }

        public string SetError(string statusCode, string response)
        {
            SiteUp = "0";
            Response = response;
            SiteStatusCode = statusCode;
            return ToString();
        }

        public int GetStatusCode()
        {
            return int.Parse(SiteStatusCode);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"# TYPE site_up gauge");
            sb.AppendLine($"site_up{{response=\"{Response.Replace("\n", "").Trim()}\"}} " + IsSiteUp);

            sb.AppendLine($"# TYPE site_up_raw gauge");
            sb.AppendLine($"site_up_raw " + IsSiteUp);

            sb.AppendLine($"# TYPE site_status_code gauge");
            sb.AppendLine($"site_status_code " + SiteStatusCode);

            return sb.ToString();
        }
    }
}