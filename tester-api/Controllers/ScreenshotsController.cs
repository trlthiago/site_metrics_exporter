using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tester_core;

namespace tester_api.Controllers
{
    [Route("[controller]")]
    public class ScreenshotsController : Controller
    {
        private readonly Services _services;

        public ScreenshotsController(Services services)
        {
            _services = services;
        }

        public async Task<IActionResult> IndexAsync(string url)
        {
            if (!System.IO.Directory.Exists("screens"))
                System.IO.Directory.CreateDirectory("screens");

            StringBuilder sb = new StringBuilder();

            var files = System.IO.Directory.GetFiles("screens");

            sb.AppendLine("<h1>Files:</H1>");

            foreach (var file in files)
            {
                sb.AppendLine($"<a href=\"{file}\" target=\"_BLANK\">{file}</a>");
            }

            return new ContentResult
            {
                ContentType = "text/html",
                Content = sb.ToString().Replace("\n", "<br />")
            };
        }

        [Route("delete")]
        public async Task<IActionResult> DeleteAsync(string url)
        {
            if (!System.IO.Directory.Exists("screens"))
            {
                System.IO.Directory.CreateDirectory("screens");
            }
            else
            {
                System.IO.Directory.Delete("screens", true);
                System.Threading.Thread.Sleep(1000);
                System.IO.Directory.CreateDirectory("screens");
            }
            return Ok();
        }
    }
}