using BlockChainDNS.Client.Services;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace BlockChainDNS.UnitTest
{
    public class Crypto
    {
        [Fact]
        public void GenerateKeys()
        {
            var key=RSA.Create(4096);
            File.WriteAllBytes("./private.txt",  key.ExportRSAPrivateKey());
            File.WriteAllText("./public.txt", key.ToXmlString(false));

            var service = new CryptoService();           

        }

        [Fact]
        public void TestEncodeKeyMD5()
        {
            var randomData = MD5.Create().ComputeHash(Guid.NewGuid().ToByteArray());

            TestCryptoKey(randomData);
        }


        [Fact]
        public void TestEncodeKeySHA256()
        {
            var randomData = SHA256.Create().ComputeHash(Guid.NewGuid().ToByteArray());

            TestCryptoKey(randomData);
        }


        [Fact]
        public void TestEncodeKeySHA512()
        {
            var randomData = SHA512.Create().ComputeHash(Guid.NewGuid().ToByteArray());

            TestCryptoKey(randomData);
        }

        private static void TestCryptoKey(byte[] randomData)
        {
            var service = new CryptoService();

            var text = Base32.ToBase32String(randomData);
            var value = service.EncodeKey(UnicodeEncoding.Unicode.GetBytes(text),
             File.ReadAllBytes("./public.txt")
             );

            var valueToCompare = service.DecodeKey(value,
                File.ReadAllBytes("./private.txt")
                );

            Assert.Equal(text, UnicodeEncoding.Unicode.GetString(valueToCompare));
        }


        [Fact]
        public void TestDataEncoding()
        {
            var key = SymmetricAlgorithm.Create("Rijndael");

            key.GenerateKey();
            key.GenerateIV();
            var fullkey = new byte[key.Key.Length + key.IV.Length];

            Array.Copy(key.Key, fullkey, key.Key.Length);
            Array.Copy(key.IV, 0, fullkey, key.Key.Length, key.IV.Length);

            var stringTest = "ewAiAGQAYQB0AGUAIgA6ACIAcwBhAGIAYQB0AG8AIAAyADAAIABmAGUAYgBiAHIAYQBpAG8AIAAyADAAMgAxACIALAAiAHQAZQB4AHQAIgA6ACIAZAAxAGQAYgBmADcAYgBhAC0AMwBhADUAZgAtADQANgBkAGEALQA4AGIAMQA5AC0ANQA5ADQAMgBjADMAMwA0ADEAMwA1ADYAIgAsACIAbQBlAHMAcwBhAGcAZQAiADoAIgBJACAAbABvAHYAZQAgAHkAbwB1ACIALAAiAF8AaABpAHMAdABvAHIAeQAiADoAWwBdAH0A";
            var data = UnicodeEncoding.Unicode.GetBytes(stringTest);

            var service = new CryptoService();

            var encoded=service.EncodeData(data, fullkey);
            var decoded = service.DecodeData(encoded, fullkey);
            Assert.Equal(decoded, data);


        }
    }
}
