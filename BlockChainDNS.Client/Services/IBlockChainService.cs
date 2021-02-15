﻿using BlockChainDNS.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlockChainDNS.Services
{
    public interface IBlockChainService
    {
        //id vs version?
         BlockChainNode Get(string key, int db);

         void Add(BlockChainNode nodetoadd, int db);

         void Validate(JObject data, string key,  int db, string expectedKey=null);
        BlockChainNode New(BlockChainNode node2, JObject data=null);

         List<BlockChainNode> GetAncerstor(BlockChainNode node, int db);
    }
}
