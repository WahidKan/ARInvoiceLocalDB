using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAP_ARInvoice.Model.DTO
{
    public class Orders
    {
        public string CustName { get; set; }
        public string OrderCode { get; set; }
        public string OrderDate { get; set; }
        public List<OrderDetail> OrderDetail { get; set; }
    }
    public class OrderDetail
    {
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public string WareHouse { get; set; }
        public string CostCenter { get; set; }
        public string BankDiscount { get; set; }
        public string TaxCode { get; set; }
        public string TaxAmount { get; set; }
        public string Section { get; set; }
        public double UnitPrice { get; set; }
    }
}