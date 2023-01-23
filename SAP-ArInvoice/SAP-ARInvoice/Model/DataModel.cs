using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAP_ARInvoice.Model
{
    public class DataModel
    {
        public string CustName { get; set; }
        public string OrderCode { get; set; }
        public string OrderDate { get; set; }
        public string ItemCode { get; set; }
        public string Quantity { get; set; }
        public string WareHouse { get; set; }
        public string CostCenter { get; set; }
        public string BankDiscount { get; set; }
        public string TaxCode { get; set; }
        public string TaxAmount { get; set; }
        public string Section { get; set; }
        public string UnitPrice { get; set; }
    }

   
}
