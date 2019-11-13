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
    public class SqlController : ControllerBase
    {
        public SqlController()
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
        [HttpGet("{box}")]
        public string Get(string box)
        {
            var content = System.IO.File.ReadAllText($"{Startup.BasePath}/targets.{box}.json");
            //SdModel model = Newtonsoft.Json.JsonConvert.DeserializeObject<SdModel>(content);
            return content;
        }

        // POST: api/Sites
        [HttpPost]
        public void Post([FromBody] string body)
        {
            Dictionary<string, SdModel> dic = new Dictionary<string, SdModel>();
            dic.Add("tatooine", new SdModel { labels = new Labels { box = "tatooine" }, targets = new List<string>() });
            dic.Add("naboo", new SdModel { labels = new Labels { box = "naboo" }, targets = new List<string>() });
            dic.Add("kamino", new SdModel { labels = new Labels { box = "kamino" }, targets = new List<string>() });
            dic.Add("geonosis", new SdModel { labels = new Labels { box = "geonosis" }, targets = new List<string>() });

            var lines = body.Replace("\r", "").Split('\n');
            foreach (var line in lines)
            {
                var parts = line.Split('\t');
                var domain = parts[0].Trim();
                var box = parts[1].Trim();
                if (dic.ContainsKey(box))
                {
                    dic[box].targets.Add(domain);
                }
            }

            foreach (var box in dic)
            {
                SdModel[] array = new SdModel[1];
                array[0] = box.Value;
                var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(array);
                System.IO.File.WriteAllText($"{Startup.BasePath}/targets.{box.Key}.json", serialized);
            }

            //foreach (var box in groups)
            //{
            //    var config = new SdModel
            //    {
            //        labels = new List<Tuple<string, string>> { new Tuple<string, string>("box", box.Key) }
            //    };

            //    foreach (var site in box)
            //    {
            //        config.targets.Add(site.Domain);
            //    }

            //    var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(config);
            //    System.IO.File.WriteAllText($"{Startup.BasePath}/targets.{box.Key}.json", serialized);
            //}
        }

        // DELETE: api/ApiWithActions/kamino
        [HttpDelete("{box}")]
        public void Delete(string box)
        {
        }
    }
}
