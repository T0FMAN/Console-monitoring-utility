using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_monitoring_utility.Data
{
    public class ParamsCheck
    {
        [JsonProperty("StringsConnection")]
        public List<string> StringsConnection { get; set; }
        [JsonProperty("Sites")]
        public List<string> Sites { get; set; }
        public string DateTime { get; set; }
    }
}
