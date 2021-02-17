using BlockChainDNS.Client;
using BlockChainDNS.Model;
using BlockChainDNS.Services;
using DnsClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Xunit;

namespace BlockChainDNS.Test
{
   public class Chain
    {
        Stopwatch timer = new Stopwatch();

        ShamanDNSCLient cli;

        public Chain()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

             cli = new ShamanDNSCLient(null);
        }

        [Fact]
        public void CRUD()
        {
            var domain = "dns-blockchain.io";
            LookupClient lookupClient = new LookupClient(IPAddress.Parse("127.0.0.1"), 53);
            IBlockChainService service = new BlockChainService(cli, lookupClient);
            var data = new JObject();
            data["date"] = DateTime.Now.ToLongDateString();
            data["text"] = Guid.NewGuid();
            data["message"] = "I love you";

            var node = new BlockChainNode();
            node.Data = data;

            Console.WriteLine(node.Key);
            node.TokenMap.ToList().ForEach((x) => { Console.WriteLine(x.Key + " | " + x.Value); });

            service.Add(node, 1,domain);
            Thread.Sleep(3000);
            var node2 = service.Get(node.Key, 1, domain);

            var node3 = service.New(node2, data);
            node3.Data["message"] = "Second Item in flow";
            service.Add(node3, 1, domain);

            timer.Start();
            var node4 = service.Get(node3.Key, 1, domain);
            timer.Stop();
            Console.WriteLine($"timing {timer.ElapsedMilliseconds}ms");


            var emptylist = service.GetAncerstor(node2, 1, domain);
            var parentList = service.GetAncerstor(node4, 1, domain);

           // var validation=
                service.Validate(node4.Data, node4.Key,1, domain);

        }

    }
}
