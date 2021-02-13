using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlockChainDNS.Client.ShamanClient
{
  
        public class ShamanDomain
        {
            public string domain { get; set; }
            public List<Record> records { get; set; }

            public string ToJson()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public class Record
        {
            public int ttl { get; set; }
            public string _class { get; set; }
            public string type { get; set; }
            public string address { get; set; }
        }

    
}
