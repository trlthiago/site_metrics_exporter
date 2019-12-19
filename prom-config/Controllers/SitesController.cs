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
        public void Post([FromBody] List<SitesModel> model) /*List<SitesModel>*/
        {

            Dictionary<string, SdModel> dic = new Dictionary<string, SdModel>();
            foreach (var item in model)
            {
                if (!dic.ContainsKey(item.Box))
                    dic.Add(item.Box, new SdModel { labels = new Labels { box = item.Box }, targets = new List<string>() });

                dic[item.Box].targets.Add(item.Identifier);
            }

            foreach (var server in dic)
            {
                SdModel[] array = new SdModel[1];
                array[0] = server.Value;
                var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(array);
                System.IO.File.WriteAllText($"{Startup.BasePath}/targets.{server.Key}.json", serialized);
            }
        }

        // DELETE: api/ApiWithActions/kamino
        [HttpDelete("{box}")]
        public void Delete(string box)
        {
        }
    }
}
