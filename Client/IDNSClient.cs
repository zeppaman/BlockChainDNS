using BlockChainDNS.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChainDNS.Client
{
    public interface IDNSClient
    {

        public  Task<DNSEntry> GetRecord(string host, string zone);

        public  Task<bool> AddRecord(DNSEntry entry);

        public Action Init { get; set; }
    }
}