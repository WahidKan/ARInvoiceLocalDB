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
    public class CreditMemoController : Controller
    {
        private readonly ILogger _logger;
        private readonly SAP_Connection connection;

        public CreditMemoController(IOptions<Setting> setting, ILogger<HomeController> logger)
        {
            this.connection = new SAP_Connection(setting.Value);
            _logger = logger;
        }


        [HttpGet]
        public async Task<string> GetAsync()
        {

            if (connection.Connect() == 0)
            {
                Documents invoice = null;
                IDictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("@Date", DateTime.Now.ToString("yyyy/MM/dd"));
                List<Orders> invoices = InvoiceMapper(await connection.ArInvoice_SP<DataModel>("[dbo].[SP_AR_Invoice]", parameters));
                foreach (var singleInvoice in invoices)
                {
                    var userResponse = await CheckBussinessCustomer(singleInvoice.CustName);



                    if (!userResponse)
                    {
                        _logger.LogError("Unable to Create New User");
                        return "SAP B1 Background service";
                    }

                    var productResponse = await CheckIfArMemoExist(singleInvoice.OrderDetail);
                    if (!productResponse)
                    {
                        _logger.LogError("Unable to Create New Item");
                        return "SAP B1 Background service";
                    }

                    var invocieResponse = CheckIfInvoiceExist(singleInvoice.OrderCode);
                    if (invocieResponse)
                    {
                        _logger.LogError("Credit Memo Already Exist");
                        return "SAP B1 Background service";
                    }

                    invoice = connection.GetCompany().GetBusinessObject(BoObjectTypes.oCreditNotes);

                    invoice.CardCode = singleInvoice.CustName;
                    invoice.DocDueDate = DateTime.Now;
                    invoice.DocDate = DateTime.Now;
                    invoice.NumAtCard = singleInvoice.OrderCode;

                    foreach (var OrderItem in singleInvoice.OrderDetail)
                    {

                        invoice.Lines.ItemCode = OrderItem.ItemCode;
                        invoice.Lines.ItemDescription = OrderItem.ItemCode;
                        invoice.Lines.WarehouseCode = OrderItem.WareHouse;
                        invoice.Lines.Quantity = OrderItem.Quantity;
                        #region Expenses
                        SAPbobsCOM.Recordset expenseRecordSet = null;
                        expenseRecordSet = connection.GetCompany().GetBusinessObject(BoObjectTypes.BoRecordset);
                        expenseRecordSet.DoQuery($"SELECT T0.\"ExpnsCode\" FROM OEXD T0 WHERE Lower(\"ExpnsName\") = Lower('{OrderItem.TaxCode}') ");
                        if (expenseRecordSet.RecordCount != 0)
                        {
                            var expenseCode = expenseRecordSet.Fields.Item(0).Value;
                            invoice.Lines.Expenses.ExpenseCode = expenseCode;
                            invoice.Lines.Expenses.LineTotal = double.Parse(OrderItem.TaxAmount);

                            invoice.Lines.Expenses.Add();
                        }
                        #endregion

                        invoice.Lines.Add();

                    }
                    if (invoice.Add() == 0)
                    {
                        _logger.LogInformation($"Record added successfully");

                    }
                    else
                    {
                        var errCode = connection.GetCompany().GetLastErrorCode();
                        var response = connection.GetCompany().GetLastErrorDescription();
                        _logger.LogError($"{errCode}:{response}");
                    }
                    connection.GetCompany().Disconnect();
                }
            }
            else
            {
                _logger.LogError(connection.GetErrorCode() + ": " + connection.GetErrorMessage());
            }
            return "SAP B1 Background service";
        }


        private List<Orders> InvoiceMapper(List<DataModel> data)
        {

            List<Orders> orders = new List<Orders>();
            List<DataModel> resp = data.Select(x => new { x.CustName, x.OrderCode }).Distinct().Select(x => data.FirstOrDefault(r => r.CustName == x.CustName && r.OrderCode == x.OrderCode)).Distinct().ToList();
            foreach (var item in resp)
            {
                var orderDetail = data.Where(x => x.OrderCode == item.OrderCode && x.CustName == item.CustName).Select(x => new OrderDetail { ItemCode = x.ItemCode, Quantity = int.Parse(x.Quantity), WareHouse = x.WareHouse, BankDiscount = x.BankDiscount, CostCenter = x.CostCenter, TaxAmount = x.TaxAmount, TaxCode = x.TaxCode, Section = x.Section }).Distinct().ToList();
                orders.Add(new Orders() { CustName = item.CustName, OrderCode = item.OrderCode, OrderDate = item.OrderDate, OrderDetail = orderDetail });
            }

            return orders;
        }

        private async Task<bool> CheckIfArMemoExist(List<OrderDetail> orderDetail)
        {
            bool output = false;
            SAPbobsCOM.Items product = null;
            SAPbobsCOM.Recordset recordSet = null;
            recordSet = connection.GetCompany().GetBusinessObject(BoObjectTypes.BoRecordset);
            product = connection.GetCompany().GetBusinessObject(BoObjectTypes.oItems);

            foreach (var singleOrderDetail in orderDetail)
            {
                recordSet.DoQuery($"SELECT * FROM \"OITM\" WHERE \"ItemCode\"='{singleOrderDetail.ItemCode}'");
                if (recordSet.RecordCount == 0)
                {
                    IDictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@ItemCode", singleOrderDetail.ItemCode);
                    List<Item> items = await connection.ArInvoice_SP<Item>("GetItems", parameters);
                    foreach (var item in items)
                    {
                        product.ItemCode = item.ItemCode;
                        product.ItemName = item.ItemDescription;
                        product.PurchaseItemsPerUnit = Double.Parse(item.UnitPrice);

                        var resp = product.Add();
                        if (resp.Equals(0))
                        {
                            output = true;
                        }
                        else
                        {
                            output = false;
                        }

                    }

                }
                else
                {
                    output = true;
                }
            }


            return output;
        }

        private async Task<bool> CheckBussinessCustomer(string CustomerId)
        {
            bool output = false;
            SAPbobsCOM.Recordset recordSet = null;
            BusinessPartners businessPartners = null;
            recordSet = connection.GetCompany().GetBusinessObject(BoObjectTypes.BoRecordset);
            businessPartners = connection.GetCompany().GetBusinessObject(BoObjectTypes.oBusinessPartners);

            recordSet.DoQuery($"SELECT * FROM \"OCRD\" WHERE \"CardCode\"='{CustomerId}'");
            if (recordSet.RecordCount == 0)
            {
                IDictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("@CardCode", CustomerId);

                List<Customer> customer = await connection.ArInvoice_SP<Customer>("[dbo].[GetCustomer]", parameters);
                foreach (var item in customer)
                {
                    businessPartners.CardCode = item.CardCode;
                    businessPartners.CardName = item.CustName;
                    businessPartners.Phone1 = item.Phone;
                    businessPartners.CardType = BoCardTypes.cCustomer;
                    businessPartners.SubjectToWithholdingTax = (BoYesNoNoneEnum)BoYesNoEnum.tNO;
                    var response = businessPartners.Add();
                    if (response.Equals(0))
                    {
                        return true;

                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                output = true;
            }
            return output;
        }

        private bool CheckIfInvoiceExist(string orderCode)
        {
            bool output = false;
            SAPbobsCOM.Recordset recordSet = null;
            recordSet = connection.GetCompany().GetBusinessObject(BoObjectTypes.BoRecordset);
            recordSet.DoQuery($"SELECT * FROM \"ORIN\" WHERE \"NumAtCard\"='{orderCode}'");
            if (recordSet.RecordCount > 0)
            {
                output = true;
            }
            return output;

        }
    }
}
