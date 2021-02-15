using BlockChainDNS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockChainDNS.Web.Models
{
    public class ValidationResult
    { 
        public BlockChainNode Result { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
        public List<BlockChainNode> Hierarchy { get; set; } = new List<BlockChainNode>();
    }
}
