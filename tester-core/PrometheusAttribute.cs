using System;

namespace tester_core
{
    public class PrometheusAttribute : Attribute
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
