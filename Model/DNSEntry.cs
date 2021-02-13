using System;
using System.Collections.Generic;
using System.Text;

namespace BlockChainDNS.Model
{
    public class DNSEntry
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Domain { get; set; }
    }
}
