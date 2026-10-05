using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Data;
using System.Net.Mail;
using System.Net.PeerToPeer;
using System.Net;
using System.Runtime.Remoting.Messaging;
using static System.Net.WebRequestMethods;
using System.Web;
using Grpc.Core;
using System.Text;
using System.Configuration;
using System.Net.Http;
using WindowsServiceDwnlzZip;

namespace CBCMSWEBAPI
{
    public class BitlyUtil
    {
        DataSet objDaset = new DataSet();
        Hashtable htable = new Hashtable();
        WSDataAccessLayer objDAL = new WSDataAccessLayer();
        EncryptDecrypt encryptDecrypt = new EncryptDecrypt();
        string JSonString = "";
        //string emailhost = ConfigurationManager.AppSettings["EmailHost"].ToString();

        //string emailfrom = ConfigurationManager.AppSettings["EmailFrom"].ToString();

        //string emailpass = ConfigurationManager.AppSettings["EmailPass"].ToString();


        string emailhost ="";
        string emailfrom ="";
        string emailpass = "";
        public BitlyUtil() {
        }

        public string CreateBitlyLink(string strAppNo, string MobileNo, string EmailID, string VerURL, string OTP, string Source)
        {
            try
            {
                objDaset.Clear();
                htable.Clear();

                htable.Add("@AppNo", strAppNo);
                htable.Add("@OTP", OTP);
                htable.Add("@VerURL", VerURL);
                htable.Add("@Source", Source);
                htable.Add("@MobileNo", MobileNo);
                htable.Add("@EmailID", EmailID);

                objDaset = objDAL.selectFromDatabase("Prc_GetDetailsForURL", htable, "IsysARTL");

                if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                {
                    string shoturl = objDaset.Tables[0].Rows[0]["WebLink"].ToString();

                    if (string.IsNullOrWhiteSpace(shoturl))
                    {
                        //string url = "http://kmidev.centralus.cloudapp.azure.com/CBCMSRESP/Application/ISys/Recruit/CndReg.aspx?"
                        string url = "http://mservices.reliancegeneral.co.in/CBCMSRES/Index.aspx?"
                        + "ProspectId=" + HttpUtility.UrlEncode(encryptDecrypt.EncryptAndEncode(strAppNo))
                        + "&Type=" + HttpUtility.UrlEncode(encryptDecrypt.EncryptAndEncode("E"))
                        + "&ACT=" + HttpUtility.UrlEncode(encryptDecrypt.EncryptAndEncode("PR"))
                        + "&ModuleID=" + HttpUtility.UrlEncode(encryptDecrypt.EncryptAndEncode(objDaset.Tables[0].Rows[0]["ModuleId"].ToString()))
                        + "&Code=" + HttpUtility.UrlEncode(encryptDecrypt.EncryptAndEncode("Spon"))
                        + "&AgtType=" + HttpUtility.UrlEncode(encryptDecrypt.EncryptAndEncode(objDaset.Tables[0].Rows[0]["AgtType"].ToString()))
                        + "&RecruitEmp=" + HttpUtility.UrlEncode(encryptDecrypt.EncryptAndEncode(objDaset.Tables[0].Rows[0]["SapCode"].ToString()));


                        JObject tokenJobject = new JObject(
                                                new JProperty("group_guid", "Bkah5tv2LAU"),
                                                new JProperty("domain", "bit.ly"),
                                                new JProperty("long_url", url));

                        var client = new RestClient("https://api-ssl.bitly.com/v4/shorten");
                        var request = new RestRequest();
                        request.AddHeader("Authorization", "Bearer 058f9e2f4bcd3ee57962d3f83c31e208b9000601");
                        request.AddJsonBody(tokenJobject.ToString(), false, ContentType.Json);

                        var response = client.ExecutePost(request);

                        if (response.IsSuccessful)
                        {
                            var jdata = response.Content.ToString();

                            var data = JsonConvert.DeserializeObject<UrlShorter>(jdata);

                            JSonString = "{\"Table\": [ {\"ResponseCode\": \"0\","
                                + "\"WebLink\":\"" + data.link + "\","
                                + "\"AppLink\":\"" + objDaset.Tables[0].Rows[0]["AppLink"].ToString() + "\"}]}";

                            objDaset.Clear();
                            htable.Clear();

                            htable.Add("@AppNo", strAppNo);
                            htable.Add("@MobileNo", MobileNo);
                            htable.Add("@EmailID", EmailID);
                            htable.Add("@WebLink", data.link);
                            //htable.Add("@AppLink", objDaset.Tables[0].Rows[0]["AppLink"].ToString());

                            objDaset = objDAL.selectFromDatabase("Prc_UpdateBitly", htable, "IsysARTL");

                        }
                        else
                        {
                            JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\"," +
                                "\"Response\": \"" + response.StatusDescription + "\"," +
                                "\"ErrorDescription\":\"Unable to create short URL\"}]}";
                        }
                    }
                    else
                    {
                        JSonString = "{\"Table\": [ {\"ResponseCode\": \"0\","
                                + "\"WebLink\":\"" + objDaset.Tables[0].Rows[0]["WebLink"].ToString() + "\","
                                + "\"AppLink\":\"" + objDaset.Tables[0].Rows[0]["AppLink"].ToString() + "\"}]}";
                    }

                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Invalid details\"}]}";
                }
            } catch (Exception e)
            {
                LogAPIError("CreateBitlyLink", e.ToString());
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + e.Message.ToString() + "\"}]}";
            }

            return JSonString;
        }

        public string ShareLink(string strAppNo, string MobileNo, string EmailID, string VerURL, string OTP, string Source)
        {
            try
            {
                //objDaset.Clear();
                //htable.Clear();

                //htable.Add("@AppNo", strAppNo);
                //htable.Add("@OTP", OTP);
                //htable.Add("@VerURL", VerURL);
                //htable.Add("@Source", Source);
                //htable.Add("@MobileNo", MobileNo);
                //htable.Add("@EmailID", EmailID);

                // objDaset = objDAL.selectFromDatabase("Prc_GetDetailsForURL", htable, "IsysARTL");
                //objDaset = objDAL.selectFromDatabase("Prc_InsOTPNumber", htable, "IsysARTL");

                if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                {
                    //string strBody = string.Format("Hi {0},<br /><br />Please complete your application using below link {1}.<br />Please do not reply to this email<br /><br />Thank You.",
                      //  "Candidate", objDaset.Tables[0].Rows[0]["WebLink"].ToString(), objDaset.Tables[0].Rows[0]["AppLink"].ToString());
                    string subject = objDaset.Tables[0].Rows[0]["Subject"].ToString();
                    string senderMail = objDaset.Tables[0].Rows[0]["NotificationFrom"].ToString();
                    string mailBody = objDaset.Tables[0].Rows[0]["MailBody"].ToString();
                    //string reciverMailid = objDaset.Tables[0].Rows[0]["EmailID"].ToString();
                    string strbody = objDaset.Tables[0].Rows[0]["SMSBody"].ToString();//

                    //string strCallerRefId = objDaset.Tables[0].Rows[0]["mRefNo"].ToString();
                    string strSMobileTo = MobileNo;//objDaset.Tables[0].Rows[0]["MobileNo"].ToString();
                    string reciverMailid = EmailID;                                                         
                    string StrAppId = "ISYS";

                    SendAutoCreateRdlc SACR = new SendAutoCreateRdlc();
                    SACR.SendMail(strAppNo, senderMail, reciverMailid, null, subject, mailBody);


                    SACR.SendSMS(StrAppId, strAppNo, "8793453602", strSMobileTo, 0, strbody, "SMSSch");
                    // SACR.SendMail(strAppNo, senderMail, reciverMailid, null, subject, mailBody);
                    //SACR.SendSMS(strAppNo, strCallerRefId, "8793453602", strSMSTo, 0, strBody, "SYS");


                    //FuncSmsSend(objDaset.Tables[0].Rows[0]["MobileNo"].ToString(), strBody, objDaset.Tables[0].Rows[0]["mRefNo"].ToString());

                    //FunMailSend(objDaset.Tables[0].Rows[0]["EmailID"].ToString(), strBody);

                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"0\",\"Message\":\"We have sent the link.\"}]}";
                }
            }
            catch (Exception e)
            {
                LogAPIError("ShareLink", e.ToString());
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + e.Message.ToString() + "\"}]}";
            }

            return JSonString;
        }

        public void FunMailSend(string EmailID, string strBody)
        {
            try
            {

                MailMessage mm = new MailMessage(emailfrom, EmailID);
                mm.Subject = "Application Pending";
                mm.Body = strBody;
                mm.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = emailhost;
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential();
                NetworkCred.UserName = emailfrom;
                NetworkCred.Password = emailpass;
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.EnableSsl = false;
                smtp.Port = 25;
                smtp.Send(mm);
            } catch (Exception e)
            {
                LogAPIError("FunMailSend", e.ToString());
            }
        }

        public void FuncSmsSend(String strSMSTo, String strSMSContent, String strSMSRefNo)
        {
            string strResponse = "", status = "", sourceRequestId = "";
            try
            {
                string jsonData = "";

                Random rnd = new Random();
                int sourceId = rnd.Next(100000, 999999);

                sourceRequestId = DateTime.Now.ToString("yyyyMMddHHmmssFFF") + sourceId.ToString();

                SingleSmsReq AzureSingleReq = new SingleSmsReq();
                AzureSingleReq.departmentId = Convert.ToInt32("19");
                //  AzureSingleReq.mobileNumber = Convert.ToString(Dr["MobileNo"]);
                // AzureSingleReq.mobileNumber = "8080019090";
                //AzureSingleReq.mobileNumber = mobileNumber;
                AzureSingleReq.mobileNumber = strSMSTo;
                //AzureSingleReq.textMessage = "Dear Customer, SR 30021223 is registered and would be resolved by 26 Jul 2023. Kindly quote the same for future interaction. Click on http://bit.ly/1YPK5S5 to know the status of your service request instantly. To know more about the ownership transfer process, click  https://youtu.be/gDRKzw4iFQw Reliance General Ins.";
                AzureSingleReq.textMessage = strSMSContent;
                AzureSingleReq.appProcess = "OPS";
                AzureSingleReq.smsEvent = "srcreation";
                AzureSingleReq.refName = "msgid";
                AzureSingleReq.refValue = strSMSRefNo;
                AzureSingleReq.sourceRequestId = sourceRequestId.ToString();

                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                ////System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;


                //Uri requestUri = new Uri(ConfigurationManager.AppSettings["Serviceurl"]);
                Uri requestUri = new Uri(ConfigurationManager.AppSettings["SMSURL"]);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUri);

                request.Headers.Add("x-functions-key", "Z8QTzjzpZoliA3qeVzt1CGsBYVq5Kv/J5VylLc8nnjsL5L3w4h3FKg==");
                request.Headers.Add("username", "CHMS");
                request.Headers.Add("password", "CHMS@1234");
                request.Method = "POST";
                request.Proxy = null;
                request.ContentType = "application/json";
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                //JavaScriptSerializer JS = new JavaScriptSerializer();
                jsonData = JsonConvert.SerializeObject(AzureSingleReq);

                System.IO.StreamWriter sw = new System.IO.StreamWriter(request.GetRequestStream());
                sw.Write(jsonData);
                sw.Flush();
                sw.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (System.IO.Stream responseStream = response.GetResponseStream())
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(responseStream, Encoding.UTF8);
                    strResponse = reader.ReadToEnd();
                    Status Msgstaus = JsonConvert.DeserializeObject<Status>(strResponse);
                    status = Msgstaus.status;
                    string smsTokenId = Msgstaus.smsTokenId;
                    string errorDescription = Msgstaus.errorDescription;
                }
            }
            catch (Exception e)
            {
                //DataUtil.ExecuteNonQuery("SMS_exception", e.Message, "SendSmsForIceServiceRequestEndorsementNo_ServiceError");
                LogAPIError("FuncSmsSend", e.ToString());
            }
            //return status;
        }

        public void LogAPIError(string method, string error)
        {
            DataSet dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@LogDtime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            ht.Add("@AppName", "CBCMSAPI");
            ht.Add("@PageName", "BitlyUtil");
            ht.Add("@FuncName", method);
            ht.Add("@ErrDesc", error);
            ht.Add("@UserId", "API");
            ht.Add("@Severity", "High");
            dstemp = objDAL.selectFromDatabase("Mob_LogError", ht, "USRMGMT");
        }

        public void demo (){
            try
            {
                // URL of the Web API endpoint.
                string apiUrl = "http://kmi.centralindia.cloudapp.azure.com/ckycwebapi/api/CKYC/CKYCSearch";

                // Create an HttpClient instance.
                using (HttpClient httpClient = new HttpClient())
                {
                   string  inputData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                        "<REQ_ROOT>" +
                            "<CKYC_INQ>" +
                                "<ID_TYPE>C</ID_TYPE>" +
                                "<ID_NO>DTIPS7526U</ID_NO>" +
                            "</CKYC_INQ>" +
                        "</REQ_ROOT>";
                    // Serialize the request data to JSON.
                    //string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);

                    // Create a StringContent with the JSON content.
                    StringContent content = new StringContent(inputData, Encoding.UTF8, "application/xml");

                    // Make a POST request with the request body.
                    HttpResponseMessage response = httpClient.PostAsync(apiUrl, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        // Request was successful. You can process the response here.
                        string responseContent = response.Content.ReadAsStringAsync().Result;
                        Console.WriteLine("Response: " + responseContent);
                    }
                    else
                    {
                        Console.WriteLine("Request failed with status code: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        public class SingleSmsReq
        {
            public int departmentId { get; set; }
            public string mobileNumber { get; set; }
            public string textMessage { get; set; }
            public string appProcess { get; set; }
            public string smsEvent { get; set; }
            public string refName { get; set; }
            public string refValue { get; set; }
            public string sourceRequestId { get; set; }
        }

        public class Status
        {
            public string status { get; set; }
            public string smsTokenId { get; set; }
            public string errorDescription { get; set; }
        }

    }
}