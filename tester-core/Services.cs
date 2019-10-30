using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tester_core.Models;

namespace tester_core
{
    public class Services : IDisposable
    {

        public static string ChromePath = string.Empty;

        //https://automationrhapsody.com/performance-testing-in-the-browser/

        private Browser _browser { get; set; }

        private Browser Browser
        {
            get
            {
                if (_browser == null)
                {
                    var options = new LaunchOptions { Headless = true, Devtools = false, ExecutablePath = ChromePath };
                    if (string.IsNullOrWhiteSpace(ChromePath))
                        options.Args = new string[] { "--no-sandbox" };
                    _browser = Puppeteer.LaunchAsync(options).Result;
                }
                return _browser;
            }
        }

        public Services()
        {
            if (string.IsNullOrWhiteSpace(ChromePath))
                DownloadChromeAsync().Wait();
        }

        private async Task DownloadChromeAsync()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
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

        public async Task<Dictionary<string, decimal>> AccessPageAndGetMetrics(string url)
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
                throw e;
            }

            var metrics2 = await page.MetricsAsync();

            Console.WriteLine("Status Code: " + response.Status);

            //await page.ScreenshotAsync("D:\\screens\\" + new Uri(url).Host + ".png");

            Console.WriteLine("Trying close the " + url);
            await page.CloseAsync();

            return metrics2;
        }

        public async Task<Dictionary<string, decimal>> AccessPageAndGetMetricsByTrl(string url)
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
                var metrics2 = await MetricsAsync(cpd);
                System.Threading.Thread.Sleep(10000);
                var metrics3 = await MetricsAsync(cpd);

                foreach (var metric1 in metrics1)
                {
                    var m2 = metrics2[metric1.Key];
                    var m3 = metrics3[metric1.Key];
                    Console.WriteLine($"{metric1.Key,30} | {metric1.Value,15} | {m2,15} | {m3,15}");
                }
                return metrics2;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                await page.CloseAsync();
            }
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

        public async Task<List<AssetPerformance>> AccessPageAndGetTiming(string url)
        {
            var page = await Browser.NewPageAsync();

            try
            {
                Response response;

                response = await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

                if (response == null)
                    throw new Exception("Null Response for " + url);

                //https://github.com/GoogleChrome/puppeteer/issues/417
                //var entries2 = await page.EvaluateExpressionAsync<object>("performance.getEntries().filter(e => e.entryType === 'resource').map(e => {e.name})");
                //var entries2 = await page.EvaluateExpressionAsync<object>("performance.getEntries().filter(e => e.entryType !== 'resource').map(e => e.name)");
                //var entries1 = await page.EvaluateExpressionAsync<string[]>("performance.getEntries().map(e=> JSON.stringify(e, null, 2))");
                var entries2 = await page.EvaluateExpressionAsync<string>("JSON.stringify(performance.timing)");

                var metric = Newtonsoft.Json.JsonConvert.DeserializeObject<PerformanceMetrics>(entries2);


                //var entries2 = await page.EvaluateExpressionAsync<object>("performance.getEntries().filter(e => e.entryType !== 'resource').map(e => e.name)");
                var entries1 = await page.EvaluateExpressionAsync<string[]>("performance.getEntries().filter(e => e.entryType === 'navigation').map(e=> JSON.stringify(e, null, 2))");

                List<AssetPerformance> assetsPerf = new List<AssetPerformance>();

                foreach (var entry in entries1)
                {
                    var b = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetPerformance>(entry);
                    assetsPerf.Add(b);
                }


                //List<AssetPerformance> assetsPerf = new List<AssetPerformance>();

                //foreach (var entry in entries1)
                //{
                //    var b = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetPerformance>(entry);
                //    assetsPerf.Add(b);
                //}

                return assetsPerf;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public async Task<List<AssetPerformance>> AccessPageAndGetResources(string url)
        {
            var page = await Browser.NewPageAsync();

            try
            {
                Response response;

                response = await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

                // await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle0 }, Timeout = 60000 });

                if (response == null)
                    throw new Exception("Null Response for " + url);

                //https://github.com/GoogleChrome/puppeteer/issues/417
                //var entries2 = await page.EvaluateExpressionAsync<object>("performance.getEntries().filter(e => e.entryType === 'resource').map(e => {e.name})");
                //var entries2 = await page.EvaluateExpressionAsync<object>("performance.getEntries().filter(e => e.entryType !== 'resource').map(e => e.name)");
                var entries1 = await page.EvaluateExpressionAsync<string[]>("performance.getEntries().map(e=> JSON.stringify(e, null, 2))");

                List<AssetPerformance> assetsPerf = new List<AssetPerformance>();

                foreach (var entry in entries1)
                {
                    var b = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetPerformance>(entry);
                    assetsPerf.Add(b);
                }

                return assetsPerf;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public async Task<List<AssetPerformance>> AccessPageAndGetResourcesFull(string url)
        {
            var page = await Browser.NewPageAsync();

            try
            {
                CDPSession cpd = await page.Target.CreateCDPSessionAsync();

                await cpd.SendAsync("Performance.enable");

                Response response;

                response = await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

                if (response == null)
                    throw new Exception("Null Response for " + url);

                //https://github.com/GoogleChrome/puppeteer/issues/417
                //var entries2 = await page.EvaluateExpressionAsync<object>("performance.getEntries().filter(e => e.entryType === 'resource').map(e => {e.name})");
                //var entries2 = await page.EvaluateExpressionAsync<object>("performance.getEntries().filter(e => e.entryType !== 'resource').map(e => e.name)");
                var entries1 = await page.EvaluateExpressionAsync<string[]>("performance.getEntries().map(e=> JSON.stringify(e, null, 2))");

                var metrics2 = await MetricsAsync(cpd);

                foreach (var metric1 in metrics2)
                {
                    var m2 = metrics2[metric1.Key];
                    Console.WriteLine($"{metric1.Key,30} | {metric1.Value,15} | {m2,15}");
                }

                List<AssetPerformance> assetsPerf = new List<AssetPerformance>();

                foreach (var entry in entries1)
                {
                    var b = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetPerformance>(entry);
                    assetsPerf.Add(b);
                }

                return assetsPerf;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                await page.CloseAsync();
            }
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
