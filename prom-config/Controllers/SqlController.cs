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
        [Consumes("text/plain")]
        public void Post([FromBody]string body)
        {
            Dictionary<string, SdModel> dic = new Dictionary<string, SdModel>();
            
            var lines = body.Replace("\r", "").Split('\n');
            foreach (var line in lines)
            {
                var parts = line.Split('\t');
                var domain = parts[0].Trim();
                var server = parts[1].Trim(); //server also can mean a box (ex: tatooine)
                
                if (!dic.ContainsKey(server))
                    dic.Add(server, new SdModel { labels = new Labels { box = server }, targets = new List<string>() });

                dic[server].targets.Add(domain);
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
