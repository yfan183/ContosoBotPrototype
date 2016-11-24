using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoBotPrototype.DataModels
{
    public class myTable
    {
        [JsonProperty(PropertyName = "Id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string userName { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string passWord { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public double Balance { get; set; }
        
    }
}