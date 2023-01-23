using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;
using SAPbobsCOM;
using SAP_ARInvoice.Connection;
using SAP_ARInvoice.Model.Setting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using SAP_ARInvoice.Model;
using Microsoft.Extensions.Logging;
using SAP_ARInvoice.Model.DTO;

namespace SAP_ARInvoice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController
    {

        public HomeController() {
         
        }


        [HttpGet]
        public async Task<string> GetAsync()
        {
            return "SAP B1 Background service";
        }


    }
}