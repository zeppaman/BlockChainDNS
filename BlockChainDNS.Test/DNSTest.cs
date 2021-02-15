using BlockChainDNS.Client;
using BlockChainDNS.Model;
using DnsClient;
using DnsClient.Protocol;
using System;
using Xunit;
using System.Linq;
using System.Net;

namespace BlockChainDNS.Test
{
    public class DNSTest
    {
        ShamanDNSCLient cli;

        public DNSTest()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            cli = new ShamanDNSCLient(null);
        }

        [Fact]
        private  void TestDNSRecord()
        {
            // Add and read a TXT record
            cli.AddRecord(new DNSEntry
            {
                Domain = "xxx2.provina.it",
                Name = "XXX",
                Value = "Got it!",
                Type = "TXT"
            }).Wait();

            var lookup = new LookupClient();
            var result = lookup.QueryAsync("xxx2.provina.it", QueryType.TXT).Result;

            var record = result.Answers.Cast<TxtRecord>().FirstOrDefault();
            var ip = string.Join(" ", record?.Text);
            Console.WriteLine(ip);
        }


        [Fact]
        private void TestARecord()
        {
            // Add and check a A Record
            ShamanDNSCLient cli = new ShamanDNSCLient(null);
            cli.AddRecord(new DNSEntry
            {
                Domain = "xxx2.provina.it",
                Name = "XXX",
                Value = "127.0.0.2",
                Type = "A"
            }).Wait();

            //var addresses = Dns.GetHostEntry("xxx2.provina.it").AddressList;
            //var iplist = string.Join(' ', addresses.Select(x=>x.MapToIPv4().ToString()));
            //Console.WriteLine(iplist);
        
        }
    }
}
