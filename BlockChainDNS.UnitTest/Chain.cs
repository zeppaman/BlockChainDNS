using BlockChainDNS.Client;
using BlockChainDNS.Client.Model;
using BlockChainDNS.Client.Services;
using BlockChainDNS.Model;
using BlockChainDNS.Services;
using DnsClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace BlockChainDNS.Test
{
   public class Chain
    {
        Stopwatch timer = new Stopwatch();

        ShamanDNSCLient cli;
        private ITestOutputHelper OutputHelper { get; }

        public Chain(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;      
        
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

             cli = new ShamanDNSCLient(null);
        }

        [Fact]
        public void WriteDecryptKey()
        {
            var domain = "dns-blockchain.io";
            var privateKey = File.ReadAllBytes("./private.txt");
            LookupClient lookupClient = new LookupClient(IPAddress.Parse("127.0.0.1"), 53);
            ICryptoService cryptoService = new CryptoService();
            IBlockChainService service = new BlockChainService(cli, lookupClient, cryptoService);
            DecriptKey decriptKey = new DecriptKey();
            decriptKey.Storage = KeyStorage.DNS;
            decriptKey.Value = Convert.ToBase64String(privateKey);
            service.CreateDatabase(1, domain, decriptKey);
      
            var key= service.GetDecryptKey(1, domain);
            Assert.Equal(key.Value, Convert.ToBase64String(privateKey));
            Assert.Equal(key.Key, privateKey);
        }

        [Fact]
        public void CRUD()
        {
            var domain = "dns-blockchain.io";
            LookupClient lookupClient = new LookupClient(IPAddress.Parse("127.0.0.1"), 53);
            ICryptoService cryptoService = new CryptoService();
            IBlockChainService service = new BlockChainService(cli, lookupClient, cryptoService);

            // private for reader, public for writer
            var publicKey= File.ReadAllBytes("./public.txt");
            var privateKey = File.ReadAllBytes("./private.txt");

            this.OutputHelper.WriteLine($"public key: {publicKey}");
            this.OutputHelper.WriteLine($"private key: {privateKey}");


            var data = new JObject();
            data["date"] = DateTime.Now.ToLongDateString();
            data["text"] = Guid.NewGuid();
            data["message"] = "I love you";

            this.OutputHelper.WriteLine($"object: {data.ToString()}");

            var node = new BlockChainNode();
            node.Data = data;

            this.OutputHelper.WriteLine($"Node Hash: {node.Hash}");
            //node.TokenMap.ToList().ForEach((x) => { Console.WriteLine(x.Key + " | " + x.Value); });
            this.OutputHelper.WriteLine($"Domain: {domain}");
            this.OutputHelper.WriteLine($"Public Key: {publicKey}");
            service.Add(node, 1,domain, publicKey);
            Thread.Sleep(3000);
            var node2 = service.Get(node.Hash, 1, domain, privateKey);

            this.OutputHelper.WriteLine($"Result Hash: {node2.Hash}");
            this.OutputHelper.WriteLine($"Result Data: {node2.Data.ToString()}");

            var node3 = service.New(node2,publicKey, data);
            node3.Data["message"] = "Second Item in flow";
            service.Add(node3, 1, domain, publicKey);

            timer.Start();
            var node4 = service.Get(node3.Hash, 1, domain, privateKey);
            timer.Stop();
            Console.WriteLine($"timing {timer.ElapsedMilliseconds}ms");


            var emptylist = service.GetAncerstor(node2, 1, domain, privateKey);
            var parentList = service.GetAncerstor(node4, 1, domain, privateKey);

        
           service.Validate(node4.Data, node4.Hash,1, domain, privateKey);

        }

    }
}
