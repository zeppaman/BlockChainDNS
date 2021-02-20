using System;
using System.Collections.Generic;
using System.Text;

namespace BlockChainDNS.Client.Model
{
    public enum KeyStorage
    {
        HTTP,
        DNS,
        MANUAL
    }
    public class DecriptKey
    {
        public KeyStorage Storage { get; set; }
        public string Value { get; set; }

        public byte[] Key { get; set; }

    }
}
