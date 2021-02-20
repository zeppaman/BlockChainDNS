using BlockChainDNS.Client.Services;
using BlockChainDNS.Services;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BlockChainDNS.Client.Services
{
    public class CryptoService: ICryptoService
    {
      

        //https://www.technical-recipes.com/2013/using-rsa-to-encrypt-large-data-files-in-c/
        public byte[] DecodeKey(byte[] text, byte[] privateKey)
        {
   
           
           
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096))
            {
                rsa.ImportRSAPrivateKey(privateKey, out int p);

                return rsa.Decrypt(text, true);
            }
        }

      

        public byte[] EncodeKey(byte[] text, byte[] publicKey)
        {        

          
           
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096))
            {
                rsa.FromXmlString(UnicodeEncoding.UTF8.GetString(publicKey));
              
                return rsa.Encrypt(text,true);
            }
                         
        }


        public byte[] EncodeData(byte[] text, byte[] sharedKey)
        {

            using (SymmetricAlgorithm algorithm =  SymmetricAlgorithm.Create("Rijndael"))
            {
                algorithm.GenerateIV();
                algorithm.GenerateKey();
                //algorithm.Padding = PaddingMode.Zeros;
                //algorithm.Mode = CipherMode.CBC;
                var key = new byte[algorithm.Key.Length];
                var iv = new byte[algorithm.IV.Length];

                Array.Copy(sharedKey, 0, key, 0, key.Length);
                Array.Copy(sharedKey, key.Length, iv, 0, iv.Length);

                BlockChainService.WriteLogByteArray("Crypte:Encode:key", key);
                BlockChainService.WriteLogByteArray("Crypte:Encode:iv", iv);

                using (var cryptoTransform = algorithm.CreateEncryptor(key, iv))
                {
                   return  Transform(text, cryptoTransform);
                }
            }
        }


        public byte[] DecodeData(byte[] text, byte[] sharedKey)
        {
            using (SymmetricAlgorithm algorithm = SymmetricAlgorithm.Create("Rijndael"))
            {
                algorithm.GenerateIV();
                algorithm.GenerateKey();
                //algorithm.Padding = PaddingMode.Zeros;
                //algorithm.Mode = CipherMode.CBC;
                var key = new byte[algorithm.Key.Length];               
                var iv = new byte[algorithm.IV.Length];
                Array.Copy(sharedKey, 0, key, 0, key.Length);
                Array.Copy(sharedKey, key.Length, iv, 0, iv.Length);


                BlockChainService.WriteLogByteArray("Crypte:Decode:key", key);
                BlockChainService.WriteLogByteArray("Crypte:Decode:iv", iv);

                using (var cryptoTransform = algorithm.CreateDecryptor(key,iv))
                {
                    return  Transform(text, cryptoTransform);
                }
            }
        }
        public byte[] Hash(byte[] text)
        {
            // for future implementation
            return null;
        }
        private  byte[] Transform(byte[] input, ICryptoTransform cryptoTransform)
        {
            MemoryStream stream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Write);          

            cryptoStream.Write(input, 0, input.Length);
            cryptoStream.FlushFinalBlock();

            return stream.ToArray();
        }


    }
}
