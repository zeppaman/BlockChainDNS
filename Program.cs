using BlockChainDNS.Client;
using BlockChainDNS.Model;
using DnsClient;
using DnsClient.Protocol;
using System;
using System.Net;
using System.Linq;
using BlockChainDNS.Services;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace BlockChainDNS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            
            ShamanDNSCLient cli = new ShamanDNSCLient(null);
            // TestARecord();
             TestDNSRecord(cli);


            IBlockChainService service = new BlockChainService(cli, "dns-blockchain.io");
            var data = new JObject();
            data["date"] = DateTime.Now.ToLongDateString();
            data["text"] = Guid.NewGuid();
            data["message"] = "I love you";

            var node = new BlockChainNode();
            node.Data = data;

            Console.WriteLine(node.Key);
            node.TokenMap.ToList().ForEach((x) => { Console.WriteLine(x.Key+" | "+ x.Value);});

            service.Add(node, 1);
            Thread.Sleep(3000);
            var node2 = service.Get(node.Key, 1);

            var node3 = service.New(node2, data);
            node3.Data["message"] = "Second Item in flow";
            service.Add(node3,1);

            var node4 = service.Get(node3.Key, 1);


           var emptylist= service.GetAncerstor(node2,1);
           var  parentList = service.GetAncerstor(node4,1);

            Console.ReadLine();
        }

        private static void TestDNSRecord(ShamanDNSCLient cli)
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
            var ip = string.Join(' ', record?.Text);
            Console.WriteLine(ip);
        }

        private static ShamanDNSCLient TestARecord()
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
            return cli;
        }
    }
}
