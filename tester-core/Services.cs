using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using tester_core.Models;

namespace tester_core
{
    public class Services : IDisposable
    {
        //https://automationrhapsody.com/performance-testing-in-the-browser/

        private Browser _browser { get; set; }

        private Browser Browser
        {
            get
            {
                if (_browser == null)
                    _browser = Puppeteer.LaunchAsync(new LaunchOptions { Headless = true, Devtools=false, ExecutablePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" }).Result;
                return _browser;
            }
        }

        public Services()
        {

        }

        public async void AccessPage(string url)
        {
            var page = await Browser.NewPageAsync();

            Response response;

            try
            {
                response = await page.GoToAsync(url);
                if (response == null)
                    throw new Exception("Null Response for " + url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await page.CloseAsync();
                return;
            }

            Console.WriteLine("Status Code: " + response.Status);

            await page.ScreenshotAsync("D:\\screens\\" + new Uri(url).Host + ".png");

            Console.WriteLine("Trying close the " + url);

            await page.CloseAsync();
        }

        public async void AccessPageAndGetMetrics(string url)
        {
            var page = await Browser.NewPageAsync();

            var metrics1 = await page.MetricsAsync();

            Response response;

            try
            {
                response = await page.GoToAsync(url);
                if (response == null)
                    throw new Exception("Null Response for " + url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await page.CloseAsync();
                return;
            }

            var metrics2 = await page.MetricsAsync();

            Console.WriteLine("Status Code: " + response.Status);

            await page.ScreenshotAsync("D:\\screens\\" + new Uri(url).Host + ".png");

            Console.WriteLine("Trying close the " + url);
            await page.CloseAsync();
        }

        public async void AccessPageAndGetTiming(string url)
        {
            var page = await Browser.NewPageAsync();

            CDPSession cpd = await page.Target.CreateCDPSessionAsync();

            await cpd.SendAsync("Performance.enable");

            var metrics1 = await MetricsAsync(cpd);

            Response response;

            try
            {
                response = await page.GoToAsync(url);
                if (response == null)
                    throw new Exception("Null Response for " + url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await page.CloseAsync();
                return;
            }

            var metrics2 = await MetricsAsync(cpd);

            foreach (var metric1 in metrics1)
            {
                var m2 = metrics2[metric1.Key];
                Console.WriteLine($"{metric1.Key,30} | {metric1.Value,15} | {m2,15}");
            }

            //await page.ScreenshotAsync("D:\\screens\\" + new Uri(url).Host + ".png");

            Console.WriteLine("Trying close the " + url);
            await page.CloseAsync();
        }

        public async void AccessPageAndTrace(string url)
        {
            var page = await Browser.NewPageAsync();

            await page.Tracing.StartAsync(new TracingOptions { Path = "D:\\screens\\trace.json" });

            Response response;

            try
            {
                response = await page.GoToAsync(url);
                if (response == null)
                    throw new Exception("Null Response for " + url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await page.CloseAsync();
                return;
            }

            await page.Tracing.StopAsync();

            Console.WriteLine("Trying close the " + url);
            await page.CloseAsync();
        }

        public async Task<List<AssetPerformance>> AccessPageAndGetResources(string url)
        {
            var page = await Browser.NewPageAsync();

            Response response;

            try
            {
                response = await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

                if (response == null)
                    throw new Exception("Null Response for " + url);
            }
            catch (Exception e)
            {
                await page.CloseAsync();
                throw e;
            }

            //https://github.com/GoogleChrome/puppeteer/issues/417
            //var entries = await page.EvaluateExpressionAsync("performance.getEntries()");
            //var entries2 = await page.EvaluateExpressionAsync<object>("JSON.parse(performance.getEntries())");
            //var entries2 = await page.EvaluateExpressionAsync<object>("performance.getEntries().filter(e => e.entryType === 'resource').map(e => {e.name})");
            //var entries2 = await page.EvaluateExpressionAsync<object>("performance.getEntries().filter(e => e.entryType !== 'resource').map(e => e.name)");
            var entries1 = await page.EvaluateExpressionAsync<string[]>("performance.getEntries().map(e=> JSON.stringify(e, null, 2))");

            List<AssetPerformance> assetsPerf = new List<AssetPerformance>();

            foreach (var entry in entries1)
            {
                var b = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetPerformance>(entry);
                assetsPerf.Add(b);
            }

            await page.CloseAsync();

            return assetsPerf;
        }

        private bool IsInFailure(Page page, Response response)
        {
            if (page.GetTitleAsync().Result.StartsWith("503"))
                return true;

            if ((int)response.Status >= 300)
                return true;

            return false;
        }

        private async Task<Dictionary<string, decimal>> MetricsAsync(CDPSession cdp)
        {
            var response = await cdp.SendAsync<PerformanceGetMetricsResponse>("Performance.getMetrics");
            return BuildMetricsObject(response.Metrics);
        }

        private Dictionary<string, decimal> BuildMetricsObject(List<KeyValue> metrics)
        {
            var result = new Dictionary<string, decimal>();

            foreach (var item in metrics)
            {
                result.Add(item.Name, item.Value);
            }

            return result;
        }

        public async void Dispose()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser.Dispose();
            }
        }
    }
}
