using System;
using System.Collections.Generic;

namespace prom_config.Models
{
    public class SdModel
    {
        public List<string> targets { get; set; }
        //public List<Tuple<string,string>> labels { get; set; }
        public Labels labels { get; set; }
    }

    public class Labels
    {
        public string box { get; set; }
    }

    public class RootObject
    {
        public Labels labels { get; set; }
        public List<string> targets { get; set; }
    }
}
