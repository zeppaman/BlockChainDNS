﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Collections.ObjectModel;

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

        
        public string Key { get
            {
                return ComputeKey(History,TokenMap.Values);
            }
        } // computed as hashing of hashes

        private string ComputeKey(IEnumerable<string> history, IEnumerable<string> values )
        {
            var key = string.Join('-', history) + "+";
            var tokenshash = string.Join('-', values);
            return Hash(key + tokenshash);
        }

        public ObservableCollection<string> History { get; set; } = new ObservableCollection<string>();//First to last

        public Dictionary<string, string> TokenMap
        {
            get
            {
                
                var result = new Dictionary<string, string>();
                var base32 = this.ToBase32();
                var tokens = Tokenize(base32, 254);// max txt lenght -1
                foreach (var token in tokens)
                {
                    result[token] = Hash(token);
                }
                return result;
            }
        }


       

        public List<string> Validate(string key, JObject body, List<string> hashes)
        {
            var errors = new List<string>();
            if (key != this.Key) errors.Add("Key mismatch. This object is altered");
            //check the full hierarchy
            return errors;

        }


        
        

        public string ToBase32()
        {
            return Base32.ToBase32String(UTF8Encoding.UTF8.GetBytes(Data.ToString()));
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
                tokens.Add(str.Substring(str.Length-mod, mod));
            }

            return tokens;
        }

        public string Hash(string text)
        {
            using (var md5 = MD5.Create())
            {
                return Base32.ToBase32String(md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(text)));                
            }
        }
    }
}
