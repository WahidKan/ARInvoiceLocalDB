using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SAP_ARInvoice.Model;
using SAP_ARInvoice.Model.Setting;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace SAP_ARInvoice.Connection
{
    public class SAP_Connection
    {
        private SAPbobsCOM.Company company = new SAPbobsCOM.Company();
        private int connectionResult;
        private int errorCode = 0;
        private string errorMessage = "";
        private Setting _setting;

        public SAP_Connection(Setting setting) {
            _setting = setting;
        }

        public int Connect()
        {

            company.Server = _setting.Server;
            company.CompanyDB = _setting.CompanyDB;
            company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB;
            company.DbUserName = _setting.DbUserName;
            company.DbPassword = _setting.DbPassword;
            company.UserName = _setting.UserName;
            company.Password = _setting.Password;
            company.language = SAPbobsCOM.BoSuppLangs.ln_English;
            company.UseTrusted = _setting.UseTrusted;
            company.LicenseServer = _setting.LicenseServer;

            connectionResult = company.Connect();

            if (connectionResult != 0)
            {
                company.GetLastError(out errorCode, out errorMessage);
            }

            return connectionResult;
        }
        public SAPbobsCOM.Company GetCompany()
        {
            return this.company;
        }

        public int GetErrorCode()
        {
            return this.errorCode;
        }

        public String GetErrorMessage()
        {
            return this.errorMessage;
        }


        public async Task<List<T>> ArInvoice_SP<T>(string SpName,IDictionary<string,string> parameters)
        {
            List<T> dataModel = new List<T>();
            try
            {
                string ConnectionString = _setting.DbConnection;
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(SpName, connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    foreach (var parameter in parameters)
                    {
                        cmd.Parameters.AddWithValue(parameter.Key,parameter.Value);
                    }
                       
                    connection.Open();
                    SqlDataReader sdr = cmd.ExecuteReader();

                    T obj = default(T);

                    while (sdr.Read())
                    {
                        obj = Activator.CreateInstance<T>();
                        foreach (PropertyInfo prop in obj.GetType().GetProperties())
                        {
                            if (!object.Equals(sdr[prop.Name], DBNull.Value))
                            {
                                prop.SetValue(obj, sdr[prop.Name].ToString(), null);
                            }
                        }
                        dataModel.Add(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Occurred: {ex.Message}");
            }


            return dataModel;
        }

        public List<T> ArInvoice_API<T>(string baseURI)
        {
            List<T> modelResponse = new List<T>();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(baseURI);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync("").Result;
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;

                modelResponse = JsonConvert.DeserializeObject<List<T>>(data);
            
            }

            return modelResponse;
        }
    }
}
