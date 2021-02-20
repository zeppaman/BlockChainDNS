using BlockChainDNS.Client;
using BlockChainDNS.Model;
using DnsClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Diagnostics;
using BlockChainDNS.Client.Model;

namespace BlockChainDNS.Services
{
    public class BlockChainService : IBlockChainService
    {

        IDNSClient client;

        LookupClient lookup;

        ICryptoService cryptoService;
        public BlockChainService(IDNSClient client, LookupClient lookup, ICryptoService cryptoService)
        {
            this.lookup = lookup;
            this.client = client;
            this.cryptoService = cryptoService;
            this.client.Init?.Invoke();

        }
   

        public void Add(BlockChainNode nodetoadd, int db, string domain, byte[] publicKey)
        {
            var password = SHA512.Create().ComputeHash(Guid.NewGuid().ToByteArray());

            WriteLog($"password: {Convert.ToBase64String(password)}");
            var decriptkey = this.cryptoService.EncodeKey(password, publicKey);
            var key64 = Convert.ToBase64String(decriptkey);
            WriteLog($"key64: {key64}");


            WriteLog($"node 64: { nodetoadd.ToBase64()}");
            var dataToEncode = UnicodeEncoding.Unicode.GetBytes(nodetoadd.Data.ToString(Formatting.None));
            WriteLog($"node 64: { dataToEncode.Length}");
            WriteLog($"datatoencode: {Convert.ToBase64String(dataToEncode)}");
            WriteLogByteArray("decript key", password);
            WriteLogByteArray("decript key encoded", decriptkey);
            var encoded = this.cryptoService.EncodeData(dataToEncode, password);
            WriteLogByteArray($"encoded data:", encoded);
            WriteLog($"encoded: {encoded.Length}");
            WriteLog($"encoded 64: {Convert.ToBase64String(encoded)}");
            WriteLogByteArray("data encoded", encoded);
            var data64 = Convert.ToBase64String(encoded); //TODO is neede the double base 64 encoding?
            WriteLog($"data64 encoded: {data64}");
            var fullObj = new JObject();
            fullObj["key"] = key64;
            fullObj["data"] = data64;
            var fullObj64 = Convert.ToBase64String(UnicodeEncoding.Unicode.GetBytes(fullObj.ToString(Formatting.None)));
            WriteLog($"fullObj64: {fullObj64}");
            

            var itemUrl = ComputeRecordUrl(db, nodetoadd.Hash, domain);
            var keyDomain = lookup.QueryAsync(itemUrl, QueryType.TXT).Result;
            //token count
            if (keyDomain == null || keyDomain.Questions.Count == 0)
            {
                client.AddRecord(new DNSEntry()
                {
                    Domain = itemUrl,
                    Value = fullObj64.Length.ToString()
                });
            }         


            WriteDNSFragmentedText(itemUrl, fullObj64, 254);
           
        }

        public static void WriteLog(string v)
        {
            Debug.WriteLine(v);
        }

        private string GenerateDectriptKey(BlockChainNode nodetoadd, byte[] publicKey)
        {
            var keyBytes = this.cryptoService.EncodeKey(ASCIIEncoding.ASCII.GetBytes(nodetoadd.Hash), publicKey); // id of the record
            var keyStr = Base32.ToBase32String(keyBytes);
            return keyStr;
        }

        public BlockChainNode Get(string key, int db, string domain, byte[] privateKey)
        {

            var base64 = ReadDNSFragmentedText(ComputeRecordUrl(db, key, domain));
           
            WriteLog($"fullObj64: {base64}");
            var fullObj = JObject.Parse(UnicodeEncoding.Unicode.GetString(Convert.FromBase64String(base64)));
            WriteLog($"decriptKey base64: {fullObj["key"].Value<string>()}");

            var decriptKeyEncoded = Convert.FromBase64String(fullObj["key"].Value<string>());            
            WriteLogByteArray("decript key encoded", decriptKeyEncoded);
            var decriptKey = this.cryptoService.DecodeKey(decriptKeyEncoded, privateKey);
            WriteLogByteArray("decript key decoded", decriptKey);
            var decodedKey = Convert.ToBase64String(decriptKey);
            WriteLog($"decriptKey decoded: {decodedKey}");
            var data64 = fullObj["data"].Value<string>();
            WriteLog($"data64: {data64}");
            var data = Convert.FromBase64String(fullObj["data"].Value<string>());
            WriteLog($"data ecoded  len: {data.Length}");
            WriteLogByteArray("data encoded", data);
            WriteLogByteArray("decript key", decriptKey);
            WriteLogByteArray($"encoded data:", data);
            var decoded = this.cryptoService.DecodeData(data, decriptKey);
            var decodedStr = UnicodeEncoding.Unicode.GetString(decoded);
            WriteLog($"decoded str: {decodedStr}");
            var item=FromJSonString(decodedStr);
            if(key!=item.Hash)
            {
                throw new Exception("Invalid content. Key mismatch");
            }
            return item;
        }

        private string ReadDNSFragmentedText(string domain)
        {
            List<string> fragments = new List<string>();

            for (int i = 0; i < 1000; i++) //TODO: canghe to a infinite loop?
            {
                var fragmentUrl = $"{i}.{domain}";

                var result = ReadDNSTxtResult(fragmentUrl);
                if (result == null) break;// otherwise parent domain value will be added

                fragments.Add(result);

            }

            return string.Join("", fragments);
        }

        private string ReadDNSTxtResult(string fragmentUrl)
        {
            if (!fragmentUrl.EndsWith("."))
            {
                fragmentUrl = fragmentUrl + ".";
            }
            var result = lookup.QueryAsync(fragmentUrl, QueryType.TXT).Result;
            if (result != null && !result.HasError && result.Answers?.Count > 0 )
            {
                var resultDomain = result.Answers.FirstOrDefault().DomainName.Value;
                if (resultDomain == fragmentUrl)
                {
                  return result.Answers.TxtRecords().FirstOrDefault()?.EscapedText.FirstOrDefault();
                }
            }
            return null;
        }

        public static void WriteLogByteArray(string v, byte[] data)
        {
            string line = v;
            foreach (var b in data)
            {
                line += b.ToString() + " ";
            }
            Debug.WriteLine(line);
        }

        public  BlockChainNode FromJSonString(string text)
        {

            
            var obj = JObject.Parse(text);
            BlockChainNode bc = new BlockChainNode();
            bc.Data = (JObject)obj.DeepClone();//othewise reactive field history override history
            obj["_history"]?.ToObject<List<string>>().ForEach((x) => { bc.History.Add(x.ToLower()); });
            return bc;            
        }


        public IEnumerable<string> Tokenize(string str, int chunkSize)
        {
            int chunks = str.Length / chunkSize;
            int mod = str.Length % chunkSize;
            List<string> tokens = new List<string>();
            for (int i = 0; i < chunks; i++)
            {
                tokens.Add(str.Substring(i * chunkSize, chunkSize));
            }

            if (mod > 0)
            {
                tokens.Add(str.Substring(str.Length - mod, mod));
            }

            return tokens;
        }
        public void Validate(JObject data, string key, int db, string domain, byte[] privateKey, string expectedKey = null)
        {
            //ValidateBase coerenza tra chiave e valori.

            //ValidateHierarchy: tutti i nodi dichiarati devono esistere. La gerarchia dichiarata dever coincidere con quella reale.

            //var commonAnchestor = anchestors.IndexOf(parent.Key);
            //for (int k = commonAnchestor + 1; k < anchestors.Count; k++)
            //{
            //    if (anchestors[k] !=)
            //    }
        }

        private string ComputeDBUrl(int db, string domain)
        {
            return $"{db}.{domain}".Replace("=","-").ToLower();
        }

        private string ComputeRecordUrl( int db, string key, string domain)
        {
            var bas = ComputeDBUrl( db,  domain);
            return $"{key}.{bas}".Replace("=", "-").ToLower();
        }

        private string ComputeFragmentUrl( int db, string key, int index, string domain)
        {
            var bas = ComputeRecordUrl( db, key,  domain);
            return $"{index}.{bas}".Replace("=", "-").ToLower();
        }

        public BlockChainNode New(BlockChainNode node2,byte[] publicKey, JObject data=null)
        {
            BlockChainNode newnode = new BlockChainNode();
            
            if (data != null)
            {
                newnode.Data = data;
            }

            newnode.History.Add(node2.Hash);
            return newnode;
        }

        public List<BlockChainNode> GetAncerstor(BlockChainNode node, int db, string domain, byte[] privateKey)
        {
            var result = new List<BlockChainNode>();
            var anchestors = node.History;
            for (int i = 0; i < anchestors.Count; i++)
            {
                var parent = this.Get(anchestors[i], db, domain, privateKey);
                result.Add(parent);              
            }

            return result;
        }



        public DecriptKey GetDecryptKey(int db, string domain)
        {
            var privateKey = new DecriptKey();
            var dbUrl = ComputeDBUrl(db, domain);
            var type = ReadDNSTxtResult(dbUrl);
            privateKey.Storage = (KeyStorage)Enum.Parse(typeof(KeyStorage), type);
            switch (privateKey.Storage)
            {
                case KeyStorage.HTTP:
                    var httpUrl = $"http.{dbUrl}";
                    privateKey.Value= ReadDNSTxtResult(httpUrl);
                    WebClient wc = new WebClient();
                    privateKey.Key = wc.DownloadData(httpUrl);
                    break;
                case KeyStorage.DNS:
                    var baseUrl = $"pk.{dbUrl}";
                    privateKey.Value = ReadDNSFragmentedText(baseUrl);
                    privateKey.Key = Convert.FromBase64String(privateKey.Value);
                    break;
                default:
                    //MANUAL DO NOTHING
                    break;
            }

            return privateKey;
        }

        public void CreateDatabase( int db, string domain, DecriptKey privateKey)
        {
            if (privateKey.Storage == KeyStorage.HTTP && !privateKey.Value.StartsWith("https", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("Only https address allowed");
            }

            var dbUrl = ComputeDBUrl(db, domain);
            var dbItems = lookup.QueryAsync(dbUrl, QueryType.TXT).Result;
            if (dbItems == null || dbItems.Answers.Count == 0)
            {
                WriteDNSRecord(dbUrl, "TXT", privateKey.Storage.ToString());               
            }
            switch (privateKey.Storage)
            {
                case KeyStorage.HTTP:
                    var httpUrl = $"http.{dbUrl}";
                    WriteDNSRecord(httpUrl, "TXT", privateKey.Value);
                break;
                case KeyStorage.DNS:
                    var baseUrl = $"pk.{dbUrl}";
                    WriteDNSFragmentedText(baseUrl, privateKey.Value,254);
                break;
                default:
                    //MANUAL DO NOTHING
                break;
            }
        }

        private int WriteDNSFragmentedText(string baseUrl, string value, int size)
        {
            var tokens = Tokenize(value, size).ToList();
            int i = 0;
            foreach (var token in tokens)
            {


                WriteDNSRecord($"{i}.{baseUrl}", "TXT", token);

                i++;
            }
            return i ;
        }

        private void WriteDNSRecord(string domain, string type, string value)
        {
            this.client.AddRecord(new DNSEntry()
            {
                Domain = domain,
                Type = type,
                Value = value
            });
        }
    }
}
