using BlockChainDNS.Client.ShamanClient;
using BlockChainDNS.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlockChainDNS.Client
{

    public class ShamanDNSCLient: IDNSClient
    {

        HttpClient client;
        string domain = "http://localhost:1632";
        public ShamanDNSCLient(string domain=null)
        {
            this.domain = domain??this.domain;
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                }
            };
            
             client = new HttpClient(handler);
        }

        public Action Init { get; set; }

        public async Task<bool> AddRecord(DNSEntry entry)
        {
            ShamanDomain domain = new ShamanDomain();
            domain.domain = entry.Domain;
            domain.records = new List<Record>();
            domain.records.Add(new Record()
            {
                address = entry.Value,
                type = entry.Type,
                _class = "IN"
            });


            var contentRequest = new StringContent(domain.ToJson());
            contentRequest.Headers.Add("X-AUTH-TOKEN", "xxx");
            var result = await client.PostAsync($"{this.domain}/records", contentRequest);

            if (result.StatusCode == HttpStatusCode.OK) return true;
            return false;
        }



        public async Task<DNSEntry> GetRecord(string host, string zone)
        {
            var result = await client.GetAsync($"{this.domain}/records/{host}");
            if (result.StatusCode == HttpStatusCode.NotFound) return null;
            var body = await result.Content.ReadAsStringAsync();
            var data = JObject.Parse(body);
            return new DNSEntry()
            {

            };
        }

    }
}
