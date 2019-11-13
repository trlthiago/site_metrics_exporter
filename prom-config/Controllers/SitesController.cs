using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using prom_config.Models;

namespace prom_config.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SitesController : ControllerBase
    {
        public SitesController()
        {
            if (!System.IO.Directory.Exists(Startup.BasePath))
                System.IO.Directory.CreateDirectory(Startup.BasePath);
        }

        // GET: api/Sites
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var files = System.IO.Directory.EnumerateFiles(Startup.BasePath, "*.json");
            return files;
        }

        // GET: api/Sites/kamino
        [HttpGet("{box}", Name = "Get")]
        public string Get(string box)
        {
            var content = System.IO.File.ReadAllText($"{Startup.BasePath}/targets.{box}.json");
            //SdModel model = Newtonsoft.Json.JsonConvert.DeserializeObject<SdModel>(content);
            return content;
        }

        // POST: api/Sites
        [HttpPost]
        public void Post([FromBody] List<SitesModel> model)
        {
            var groups = model.GroupBy(x => x.Box);

            var configs = new List<SdModel>();

            foreach (var box in groups)
            {
                var config = new SdModel
                {
                    //labels = new List<Tuple<string, string>> { new Tuple<string, string>("box", box.Key) }
                    labels = new Labels { box = box.Key }
                };

            foreach (var site in box)
            {
                config.targets.Add(site.Domain);
            }

            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(config);
            System.IO.File.WriteAllText($"{Startup.BasePath}/targets.{box.Key}.json", serialized);
        }
    }

    // DELETE: api/ApiWithActions/kamino
    [HttpDelete("{box}")]
    public void Delete(string box)
    {
    }
}
}
