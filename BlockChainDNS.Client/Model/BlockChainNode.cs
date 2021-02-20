using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using BlockChainDNS.Services;
using BlockChainDNS.Client;
using BlockChainDNS.Client.Services;

namespace BlockChainDNS.Model
{
    public class BlockChainNode
    {
       
        public BlockChainNode()
        {
            
            this.History.CollectionChanged += History_CollectionChanged;
        }

        private void History_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Data["_history"] = JArray.FromObject(this.History);
        }


        private JObject _data = new JObject();
        public JObject Data
        {
            get { return _data; }
            set { _data = value; History_CollectionChanged(null, null); }
        }

        public  String DecryptKey { get;  set; }

        
        public string Hash { get
            {
                return GetHash(this.ToBase64());
            }
        } // computed as hashing of hashes



        public ObservableCollection<string> History { get; set; } = new ObservableCollection<string>();//First to last

        //public Dictionary<string, string> TokenMap
        //{
        //    get
        //    {

        //        var result = new Dictionary<string, string>();
        //        var base64 = this.ToBase64();
        //        var tokens = Tokenize(base64, 254);// max txt lenght -1
        //        foreach (var token in tokens)
        //        {
        //            result[token] = GetHash(token);
        //        }
        //        return result;
        //    }
        //}




        public List<string> Validate(string key, JObject body, List<string> hashes)
        {
            var errors = new List<string>();
            if (key != this.Hash) errors.Add("Key mismatch. This object is altered");
            //check the full hierarchy
            return errors;

        }


        
        

        public string ToBase64()
        {
            var content = UnicodeEncoding.Unicode.GetBytes(Data.ToString(Formatting.None));
           
            return Convert.ToBase64String(content);
        }


        

     

        public virtual string GetHash(string text)
        {
            using (var md5 = MD5.Create())
            {
                return Base32.ToBase32String(md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(text))).ToLower();                
            }
        }
    }
}
