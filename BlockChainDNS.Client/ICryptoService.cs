using System;
using System.Collections.Generic;
using System.Text;

namespace BlockChainDNS.Client
{
    public interface ICryptoService
    {
        byte[] EncodeKey(byte[] text, byte[] publicKey);

        byte[] DecodeKey(byte[] text, byte[] privateKey);


        byte[] EncodeData(byte[] text, byte[] sharedKey);

        byte[] DecodeData(byte[] text, byte[] sharedKey);


        byte[] Hash(byte[] text);
    }
}
