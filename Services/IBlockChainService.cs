using BlockChainDNS.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlockChainDNS.Services
{
    public interface IBlockChainService
    {
        //id vs version?
        public BlockChainNode Get(string key, int db);

        public void Add(BlockChainNode nodetoadd, int db);

        public void Validate(JObject data, string key,  int db, string expectedKey=null);
        BlockChainNode New(BlockChainNode node2, JObject data=null);

        public List<BlockChainNode> GetAncerstor(BlockChainNode node, int db);
    }
}
