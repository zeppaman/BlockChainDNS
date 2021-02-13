using BlockChainDNS.Client;
using BlockChainDNS.Model;
using DnsClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;

namespace BlockChainDNS.Services
{
    public class BlockChainService : IBlockChainService
    {

        IDNSClient client;
        string domain;
        LookupClient lookup = new LookupClient( IPAddress.Parse("127.0.0.1") ,53);
        public BlockChainService(IDNSClient client, string domain)
        {
            this.client = client;
            this.domain = domain;
            this.client.Init?.Invoke();

        }
   

        public void Add(BlockChainNode nodetoadd, int db)
        {

            //list of tokens that contains info. Each token is signed
            var tokens = nodetoadd.TokenMap;
            var key = nodetoadd.Key; // id of the record


            var itemUrl = ComputeRecordUrl(null, db, key);
            var keyDomain = lookup.QueryAsync(itemUrl, QueryType.TXT).Result;
            if (keyDomain == null || keyDomain.Questions.Count == 0)
            {
                client.AddRecord(new DNSEntry()
                {
                    Domain = itemUrl,
                    Value = tokens.Count.ToString()
                });
            }


            int i = 0;
            foreach (var token in tokens)
            {
                    this.client.AddRecord(new DNSEntry()
                    {
                        Domain = ComputeFragmentUrl(nodetoadd, db, key, i),
                        Type="TXT",
                        Value= token.Key,
                        Name="XX"                        
                    });

                //this.client.AddRecord(new DNSEntry()
                //{
                //    Domain = ComputeFragmentUrl(nodetoadd, db, key, i),
                //    Type = "A",
                //    Value = "12.0.0.1",
                //    Name = "XX"
                //});

                i++;
            }
        }


        public BlockChainNode Get(string key, int db)
        {          

            List<string> fragments = new List<string>();

            for (int i = 0; i < 100; i++)
            {
                var fragmentUrl = ComputeFragmentUrl(null, db, key, i);

                var result = lookup.QueryAsync(fragmentUrl,QueryType.TXT).Result;
                if (result!=null && !result.HasError && result.Answers?.Count > 0)
                {
                    fragments.Add(result.Answers.TxtRecords().FirstOrDefault()?.EscapedText.FirstOrDefault());
                }
                else
                {
                     result = lookup.QueryAsync(fragmentUrl, QueryType.A).Result;
                    break;
                }              

            }

            var base32=string.Join("", fragments);
            var item=FromBase32(base32);
            if(key!=item.Key)
            {
                throw new Exception("Invalid content. Key mismatch");
            }
            return item;
        }

        public  BlockChainNode FromBase32(string text32)
        {
            var obj = JObject.Parse(UTF8Encoding.UTF8.GetString(Base32.FromBase32String(text32)));
            BlockChainNode bc = new BlockChainNode();
            bc.Data = (JObject)obj.DeepClone();//othewise reactive field history override history
            obj["_history"]?.ToObject<List<string>>().ForEach((x) => { bc.History.Add(x); });
            return bc;            
        }


        public void Validate(JObject data, string key, int db, string expectedKey = null)
        {
            //ValidateBase coerenza tra chiave e valori.

            //ValidateHierarchy: tutti i nodi dichiarati devono esistere. La gerarchia dichiarata dever coincidere con quella reale.

            //var commonAnchestor = anchestors.IndexOf(parent.Key);
            //for (int k = commonAnchestor + 1; k < anchestors.Count; k++)
            //{
            //    if (anchestors[k] !=)
            //    }
        }

        private string ComputeDBUrl(BlockChainNode node, int db)
        {
            return $"{db}.{domain}".Replace("=","-").ToLower();
        }

        private string ComputeRecordUrl(BlockChainNode node, int db, string key)
        {
            var bas = ComputeDBUrl(node, db);
            return $"{key}.{bas}".Replace("=", "-").ToLower();
        }

        private string ComputeFragmentUrl(BlockChainNode node, int db, string key, int index)
        {
            var bas = ComputeRecordUrl(node, db, key);
            return $"{index}.{bas}".Replace("=", "-").ToLower();
        }

        public BlockChainNode New(BlockChainNode node2, JObject data=null)
        {
            BlockChainNode newnode = new BlockChainNode();
            
            if (data != null)
            {
                newnode.Data = data;
            }

            newnode.History.Add(node2.Key);
            return newnode;
        }

        public List<BlockChainNode> GetAncerstor(BlockChainNode node, int db)
        {
            var result = new List<BlockChainNode>();
            var anchestors = node.History;
            for (int i = 0; i < anchestors.Count; i++)
            {
                var parent = this.Get(anchestors[i], db);
                result.Add(parent);              
            }

            return result;
        }
    }
}
