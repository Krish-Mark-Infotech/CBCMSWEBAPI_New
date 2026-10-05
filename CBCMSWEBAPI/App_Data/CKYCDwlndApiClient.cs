using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using System.Data;
using System.Collections;
using CBCMSWEBAPI;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Summary description for CKYCDwlndApiClient
/// </summary>
public class CKYCDwlndApiClient
{
    private readonly HttpClient _client;
    DataSet dstemp = new DataSet();
    Hashtable htParam = new Hashtable();
    WSDataAccessLayer objDAL = new WSDataAccessLayer();
    string SeqNo;
    public CKYCDwlndApiClient()
    {
        string host = "10.185.6.192";
        int portnew = 443;
        HttpClientHandler handler = new HttpClientHandler()
        {
            //Proxy = new System.Net.WebProxy(host, portnew),
            //UseProxy = true,
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
                   System.Security.Authentication.SslProtocols.Tls13
        };
        //End proxy for UAT


        _client = new HttpClient(handler);
        //_client = new HttpClient();
        _client.BaseAddress = new System.Uri("https://integration.brobotinsurance.com/CKYCApi/api/CKYCService/Verify_CKYC"); //UAT
        //_client.BaseAddress = new System.Uri("http://ckycpapp.brobot.com/api/CKYCService/Verify_CKYC"); //PROD

    }

    public string VerifyCKYC(string strSapCode, string PAN, string dob, string UNIQUEID)
    {
        try
        {
            var jsonRequest = new JObject
            {
                { "PAN", PAN }, 
                { "DOB", dob }, 
                { "CKYC", "" }, 
                { "MOBILE", "" }, 
                { "PINCODE", "" },
                { "BIRTHYEAR", "" },    
                { "ReturnURL", "rrrrr" },    
                { "UNIQUEID", UNIQUEID },
                { "CIN", "" },    
                { "VOTERID", "" },
                { "DL_No", "" },  
                { "PASSPORT", "" }, 
                { "SubcriptionId", "ICM" }, 
                { "GSTIN", "" },
                { "UDYOG", "" },
                { "UDYAM", "" },
                { "PASSPORT_FILENO", "" }
            };

            string jsonString = jsonRequest.ToString();

            SeqNo = ServerTrnReqLog(jsonString, "CKYCDwlndApiClient", strSapCode, "", "CKYCAPI");
            //var jsonRequest = JsonConvert.SerializeObject(requestData);

            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpResponseMessage response = _client.PostAsync("", content).Result;

            // Check if response is successful
            if (response.IsSuccessStatusCode)
            {
                updateResponse(response.Content.ReadAsStringAsync().Result, SeqNo);
                return response.Content.ReadAsStringAsync().Result; // Return raw JSON response
            }
            else
            {
                updateResponse("error: " + response.StatusCode, SeqNo);
                return $"{{\"error\": \"{response.StatusCode}\", \"message\": \"Failed to verify CKYC\"}}";
            }
        }
        catch (Exception ex)
        {
            updateResponse("error: " + ex.Message, SeqNo);
            return $"{{\"error\": \"Exception\", \"message\": \"{ex.Message}\"}}";
        }
    }

    public String ServerTrnReqLog(String Param, String Method, String UserId, String AppNo, String AppName)
    {
        
        dstemp = null;
        
        htParam.Clear();
        htParam.Add("@UserId", UserId);
        htParam.Add("@AppNo", AppNo);
        htParam.Add("@Request", Param);
        htParam.Add("@RequestDtime", String.Empty);
        htParam.Add("@ErrDesc", String.Empty);
        htParam.Add("@MethodName", Method);
        htParam.Add("@AppName", AppName);

        dstemp = objDAL.selectFromDatabase("prc_insServerTrnReqLog", htParam, "USRMGMT");

        if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
        {
            return Convert.ToString(dstemp.Tables[0].Rows[0]["SeqNo"]);
        }
        else
        {
            return "";
        }
    }

    public void updateResponse(String response, String SeqNo)
    {
        dstemp = null;
        htParam.Clear();
        htParam.Add("@SeqNo", SeqNo);
        htParam.Add("@Response", response);
        htParam.Add("@ResponseDtime", String.Empty);

        dstemp = objDAL.selectFromDatabase("prc_updtServerTrnReqLog", htParam, "USRMGMT");
    }
}