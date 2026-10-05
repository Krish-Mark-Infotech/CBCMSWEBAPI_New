using CBCMSWEBAPI;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ExtExmDtls
{
    public class EXTAPIClient
    {
        private readonly HttpClient _client;
        WSDataAccessLayer wSData;
        String SeqNo = String.Empty;
        public EXTAPIClient()
        {
            //Added proxy for UAT
            string host = "10.185.6.192";
            int portnew = 443;
            HttpClientHandler handler = new HttpClientHandler()
            {
                Proxy = new System.Net.WebProxy(host, portnew),
                UseProxy = true,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
                   System.Security.Authentication.SslProtocols.Tls13
            };
            //End proxy for UAT


            _client = new HttpClient(handler);
            //_client = new HttpClient();
            _client.BaseAddress = new System.Uri("https://cms.krishmark.com/ExamCntr/api/Exam");
            wSData = new WSDataAccessLayer();
		}

        public List<ExamCenter> CallAPI(string name)
        {
            string error = string.Empty;
            var json = @"{
            ""insurerID"": ""GIJ24T9649"",
            ""extendedExamCentreName"": """ + name + @""",
            ""showPastExamsAlso"": false
            }";
            List<ExamCenter> result = new List<ExamCenter>();
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _client.PostAsync("", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    result = JsonConvert.DeserializeObject<List<ExamCenter>>(jsonResponse);
                }
               
            }
            catch (Exception ex)
            {
                error = $"Error: {ex.ToString()}";
            }
            return result;
        }
        private DataTable ConvertToDataTable( string Name, string Flag)
        {
            // get exam center list
            List<ExamCenter> examCenters = CallAPI(Name);  

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ExtendedExamCentreName", typeof(string));
            dataTable.Columns.Add("Address", typeof(string));
            dataTable.Columns.Add("RegistrationStartDate", typeof(string)); // Date as string
            dataTable.Columns.Add("RegistrationEndDate", typeof(string)); // Date as string
            dataTable.Columns.Add("ExamDate", typeof(string));// Date as string
            dataTable.Columns.Add("Error", typeof(string));
            // check the list is not empty
            if (examCenters != null && examCenters.Count > 0)
            {
                foreach (var examCenter in examCenters)
                {
                    var str = examCenter.RegistrationEndDate.ToString("dd/MM/yyyy");
                    if (Flag=="RegChk" && CalculateDateDifference(str).Days >= 2)
                    {
                        dataTable.Rows.Add(examCenter.ExtendedExamCentreName,
                            examCenter.Address,
                            examCenter.RegistrationStartDate.ToString("dd/MM/yyyy"),
                            examCenter.RegistrationEndDate.ToString("dd/MM/yyyy"),
                            examCenter.ExamDate.ToString("dd/MM/yyyy"),
                            string.Empty);
                    }else if ((Flag == "UrnChk" || Flag == "QCeChk") && CalculateDateDifference(str).Days >= 1)
                    {
                        dataTable.Rows.Add(examCenter.ExtendedExamCentreName,
                            examCenter.Address,
                            examCenter.RegistrationStartDate.ToString("dd/MM/yyyy"),
                            examCenter.RegistrationEndDate.ToString("dd/MM/yyyy"),
                            examCenter.ExamDate.ToString("dd/MM/yyyy"),
                            string.Empty);
                    }
                    else if ((Flag == "ExmChk" || Flag == "PreChk") && CalculateDateDifference(examCenter.ExamDate.ToString("dd/MM/yyyy")).Days >= 2)
                    {
                        dataTable.Rows.Add(examCenter.ExtendedExamCentreName,
                            examCenter.Address,
                            examCenter.RegistrationStartDate.ToString("dd/MM/yyyy"),
                            examCenter.RegistrationEndDate.ToString("dd/MM/yyyy"),
                            examCenter.ExamDate.ToString("dd/MM/yyyy"),
                            string.Empty);
                    }
                    else
                    {
                        dataTable.Rows.Add(Name, string.Empty, string.Empty, string.Empty, string.Empty, "Error: Not Found");
                    }
                }
            }
            else
            {
                dataTable.Rows.Add(Name, string.Empty, string.Empty, string.Empty, string.Empty, "Error: Not Found");
            }

            // Select distinct rows based on all columns
            var distinctRows = dataTable.AsEnumerable().Distinct(DataRowComparer.Default);

            // Create a new DataTable with the same schema
            DataTable distinctDataTable = dataTable.Clone();

            // Add distinct rows to the new DataTable
            foreach (DataRow row in distinctRows)
            {
                distinctDataTable.ImportRow(row);
            }

            return distinctDataTable;
        }

        // Assuming RegistrationEndDate is a string in format "DD/MM/YYYY"
        public TimeSpan CalculateDateDifference(string ExtDate)
        {
            DateTime endDate = DateTime.ParseExact(ExtDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime currentDate = DateTime.Today.Date;

            return endDate - currentDate;
        }

        public DataTable ExtChkReg(string extname, string flag)
        {
            return ConvertToDataTable(extname,flag);
        }

        public string ExtChkURN(string extname, string flag)
        {
            var result = ConvertToDataTable(extname, flag);
            if (result.Rows.Count >= 1)
            {
                if(String.IsNullOrEmpty(result.Rows[0]["Error"].ToString()))
                {
                    return "Y";
                }
                else
                {
                    return "N";
                }
            }
            else
            {
                return "N";
            }
        }
        
        public string ExtChkQC(string extname, string flag, string AppNo)
        {
            string res = string.Empty;
            var result = ConvertToDataTable(extname, flag);
            if (result.Rows.Count >= 1)
            {
                if(String.IsNullOrEmpty(result.Rows[0]["Error"].ToString()))
                {
                    res = "Y";
                }
                else
                {
                    res = "N";
                }
            }
            else
            {
                res = "N";
            }
            try
            {
                Hashtable hashtable = new Hashtable();
                hashtable.Clear();
                hashtable.Add("@AppNo", AppNo);
                hashtable.Add("@Flag", flag);
                hashtable.Add("@Result", res);
                wSData.selectFromDatabase("Prc_UpdateExtCndStatus",hashtable,"ARTL");
            }
            catch (Exception e)
            {
                e.ToString();
            }
            return res;
        }
        
       
    }

    public class ExamCenter
    {
        public string ExtendedExamCentreName { get; set; }
        public string Address { get; set; }
        public DateTime RegistrationStartDate { get; set; }
        public DateTime RegistrationEndDate { get; set; }
        public DateTime ExamDate { get; set; }
    }

}
