using BlockChainDNS.Services;
using BlockChainDNS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BlockChainDNS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlockChainService _blockChain;

        public HomeController(ILogger<HomeController> logger, IBlockChainService blockChain)
        {
            _logger = logger;
            _blockChain = blockChain;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Validate(string key, string json)
        {
            return View();
        }

        public IActionResult Open(string urlTxt)
        {

            var tokens = urlTxt.Split(".");
            var domain = tokens[tokens.Length - 2] + "." + tokens[tokens.Length - 1];
            var db = int.Parse(tokens[tokens.Length - 3]);
            var key = tokens[tokens.Length - 4];

            var decriptKey = _blockChain.GetDecryptKey(db, domain);
            //TODO: check token lenght, data integrity. Now an exeption will notify user about malformed url
            var item = _blockChain.Get(key, db, domain, decriptKey.Key);

            var result = new ValidationResult();
            result.ExpectedKey = key;
            result.RequestedURL = urlTxt;
            result.Hierarchy = _blockChain.GetAncerstor(item, db, domain,decriptKey.Key);
            result.Result = item;
            //TODO: validate
            return View(result); ;
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
