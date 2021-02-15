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

          Task<DNSEntry> GetRecord(string host, string zone);

          Task<bool> AddRecord(DNSEntry entry);

         Action Init { get; set; }
    }
}