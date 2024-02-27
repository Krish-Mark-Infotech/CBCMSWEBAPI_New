using CBCMSWEBAPI.Models;
using ExtExmDtls;
using Insc.Common.Cryptography;
using Microsoft.Ajax.Utilities;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.PeerToPeer;
using System.ServiceProcess;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Services;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using WindowsServiceDwnlzZip;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace CBCMSWEBAPI.Controllers
{
    public class CBCMSController : ApiController
    {
        Hashtable htParam = new Hashtable();

        WSDataAccessLayer objDAL = new WSDataAccessLayer();
        EncryptDecrypt encryptDecrypt = new EncryptDecrypt();
        private PolicyDispatchServiceConsume dispatch = new PolicyDispatchServiceConsume();
        DataSet dstemp = new DataSet();
        String SeqNo = String.Empty;
        String JsonResponce = String.Empty;
        private SendAutoCreateRdlc sendRdlcViaMail = new SendAutoCreateRdlc();
        //api/CBCMS/GetName?name="30036813"
        [Route("api/CBCMS/GetName")]
        [HttpGet]
        public string GetName(string name)
        {
            BitlyUtil bitly = new BitlyUtil();
            //bitly.CreateBitlyLink("10037973","9767917308","chandrashekhar.tayade@krishmark.com");
            //bitly.ShareLink("10037973");
            ConvertRDLCToPDF(name, "System");
            return "";
        }

        [Route("api/CBCMS/ValidateCaptchaMob")]
        [HttpPost]
        public string ValidateCaptchaMob([FromBody] InputModel model)
        {
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["UserId"]).Trim();
                //string param = "SapCode : " + Convert.ToString(jObject["UserId"]).Trim() + ", Ans :" + Convert.ToString(jObject["Answer"]).Trim();

                SeqNo = ServerTrnReqLog(json, "ValidateCaptchaMob", strSapCode, "", "Candidate Manager");

                string result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else
                {
                    htParam.Clear();
                    htParam.Add("@UserId", Convert.ToString(jObject["UserId"]).Trim());
                    htParam.Add("@Answer", Convert.ToString(jObject["Answer"]).Trim());
                    htParam.Add("@DeviceInfo", Convert.ToString(jObject["DeviceInfo"]).Trim());
                    dstemp = objDAL.selectFromDatabase("prc_validateCaptcha_request", htParam, "USRMGMT");

                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        if (dstemp.Tables[0].Rows[0]["Result1"].ToString() == "Valid")
                        {
                            JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"0\",\"Message\":\"Success\" }]}";
                        }
                        else
                        {
                            JsonResponce = GetCaptcha(strSapCode);
                        }
                    }
                    else
                    {
                        JsonResponce = GetCaptcha(strSapCode);
                    }
                }
            }
            catch (Exception e)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + e.ToString() + "\" }]}";
            }
            updateResponse(JsonResponce, SeqNo);
            return encryptDecrypt.EncryptAndEncode(JsonResponce);
        }

        [Route("api/CBCMS/GetCaptchaMob")]
        [HttpPost]
        public string GetCaptchaMob([FromBody] InputModel model)
        {
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["UserId"]).Trim();
                //string param = "SapCode : " + strSapCode;
                SeqNo = ServerTrnReqLog(json, "GetCaptchaMob", strSapCode, "", "Candidate Manager");
                string result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else
                {
                    JsonResponce = GetCaptcha(strSapCode);
                }
            }
            catch (Exception e)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + e.ToString() + "\" }]}";
            }
            updateResponse(JsonResponce, SeqNo);
            return encryptDecrypt.EncryptAndEncode(JsonResponce);
        }

        [Route("api/CBCMS/GetAPIVal")]
        [HttpGet]
        public String GetAPIVal(String json)
        {

            //String response = "", strAppName = "";
            //byte[] byteAppName = Convert.FromBase64String(AppName);
            //strAppName = Encoding.UTF8.GetString(byteAppName, 0, byteAppName.Length); ;
            try
            {
                //string json = model.jsonInput;
                //string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                //string strAppName = Convert.ToString(jObject["AppName"]).Trim();
                //string param = "AppName : "+ strAppName;
                SeqNo = ServerTrnReqLog(json, "GetAPIValues", "", "", "Candidate Manager");
                byte[] byteAppName = Convert.FromBase64String(Convert.ToString(jObject["AppName"]).Trim());
                string strAppName = Encoding.UTF8.GetString(byteAppName, 0, byteAppName.Length);

                string result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else
                {
                    dstemp = null;
                    Hashtable ht = new Hashtable();
                    if (strAppName == "Agenroll+")
                    {
                        JsonResponce = encryptDecrypt.GetKeys();
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Invalid request\"}]}";
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                //JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Either inavalid request or something went wrong\"}]}";
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonResponce));

        }

        [Route("api/CBCMS/GetAPIValues")]
        [HttpPost]
        public String GetAPIValues([FromBody] InputModel model)
        {

            //String response = "", strAppName = "";
            //byte[] byteAppName = Convert.FromBase64String(AppName);
            //strAppName = Encoding.UTF8.GetString(byteAppName, 0, byteAppName.Length); ;
            try
            {
                string json = model.jsonInput;
                //string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                //string strAppName = Convert.ToString(jObject["AppName"]).Trim();
                //string param = "AppName : "+ strAppName;
                SeqNo = ServerTrnReqLog(json, "GetAPIValues", "", "", "Candidate Manager");
                byte[] byteAppName = Convert.FromBase64String(Convert.ToString(jObject["AppName"]).Trim());
                string strAppName = Encoding.UTF8.GetString(byteAppName, 0, byteAppName.Length);

                string result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else
                {
                    dstemp = null;
                    Hashtable ht = new Hashtable();
                    if (strAppName == "Agenroll+")
                    {
                        JsonResponce = encryptDecrypt.GetKeys();
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Invalid request\"}]}";
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                //JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Either inavalid request or something went wrong\"}]}";
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }
            updateResponse(JsonResponce, SeqNo);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonResponce));

        }

        [Route("api/CBCMS/AuthenticateUser_WithAppVersion")]
        [HttpPost]
        public string AuthenticateUser_WithAppVersion([FromBody] InputModel model)
        {
            //String SAPCode, String Password, String IMEIstring, String AppVersion, String strTokenId
            //AuthenticateUser_WithAppVersion{SAPCode=vhP1/k33fFgVuQa3T/bTkA==; Password=ptYFTjnomq3y63YcV601Xw==; IMEIstring=353400090000527|5f8ccb33e655eb76; AppVersion=oeL5RvZzgJoxSGv5G0owDQ==; }
            //AuthenticateUser_WithAppVersion{SAPCode=AbS2uQblrxQKRpVAEBoTDw==; Password=lLqPrcd9bwxxgywc0azKoA==; IMEIstring=353400090000527|5f8ccb33e655eb76; AppVersion=oeL5RvZzgJoxSGv5G0owDQ==;}
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);

                string strDecSapCode = Convert.ToString(jObject["UserId"]).Trim();
                string strDecPwd = Convert.ToString(jObject["Password"]).Trim();
                string strAppVersion = Convert.ToString(jObject["AppVersion"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string IMEIstring = Convert.ToString(jObject["IMEIstring"]).Trim();

                string param = "SAPCode : " + strDecSapCode + ",IMEIstring : " + IMEIstring + ",AppVersion : " + strAppVersion;

                SeqNo = ServerTrnReqLog(param, "AuthenticateUser_WithAppVersion", strDecSapCode, "", "Candidate Manager");

                string result = "Valid"; //ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else
                {
                    string status = CheckADAuth(strDecSapCode, strDecPwd);

                    string strEncPassword = UserSetup.UserSetupBAL.GetEncryptedPwd(strDecPwd);

                    //return strEncPassword;
                    if (status == "X")
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Access Denied\"}]}";
                    }
                    else if (status == "Failed")
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"AD Authentication Failed\"}]}";
                    }
                    else if (status == "A" || status == "D")
                    {
                        if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                            && ValidateToken(strDecSapCode, strTokenId) == "Valid"))
                        {
                            dstemp = null;
                            Hashtable ht = new Hashtable();
                            ht.Add("@UserID", strDecSapCode);//SAPCode.ToString().Trim());
                            ht.Add("@Password", strEncPassword.ToString().Trim());
                            //ht.Add("@Password", Password.ToString().Trim());
                            ht.Add("@IMEINo", IMEIstring.ToString().Trim());
                            ht.Add("@AppVersion", strAppVersion.ToString().Trim());
                            ht.Add("@ADFlag", status);
                            dstemp = objDAL.selectFromDatabase("prc_ChkUserMobLogin", ht, "USRMGMT");
                            if (dstemp != null)
                            {
                                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                                {
                                    JsonResponce = ds2json(dstemp);
                                    //return ds2json(dstemp);
                                    //return "Success";
                                }
                                else
                                {
                                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Authentication failed.\"}]}";
                                    //return "No Memcode found for your SAPCode.";
                                }
                            }
                            else
                            {
                                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Authentication failed.\"}]}";
                                //return "ds is null";
                            }
                        }
                        else
                        {
                            JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                            updateResponse(JsonResponce, SeqNo);

                            return HttpStatusCode.NonAuthoritativeInformation.ToString();
                            //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();
                        }
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"AD Authentication Failed\"}]}";
                    }
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
                //return ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetAuthenticationUserDetails")]
        [HttpPost]
        public String GetAuthenticationUserDetails([FromBody] InputModel model)
        {
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);

                string strDecSapCode = Convert.ToString(jObject["UserId"]).Trim();
                string strDecPwd = Convert.ToString(jObject["Password"]).Trim();
                string strAppVersion = Convert.ToString(jObject["AppVersion"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string IMEIstring = Convert.ToString(jObject["IMEIstring"]).Trim();

                //string param = "SAPCode : " + strDecSapCode + ",IMEIstring : " + IMEIstring + ",AppVersion : " + strAppVersion;

                SeqNo = ServerTrnReqLog(json, "GetAuthenticationUserDetails", strDecSapCode, "", "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) &&
                    ValidateToken(strDecSapCode, strTokenId) == "Valid"))
                {
                    dstemp = null;
                    Hashtable ht = new Hashtable();
                    ht.Add("@UserID", strDecSapCode);
                    ht.Add("@IMEINo", IMEIstring.ToString().Trim());
                    ht.Add("@AppVersion", strAppVersion.ToString().Trim());
                    dstemp = objDAL.selectFromDatabase("prc_AuthGetUserDetails", ht, "USRMGMT");
                    if (dstemp != null)
                    {
                        if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                        {
                            JsonResponce = ds2json(dstemp);
                            //return ds2json(dstemp);
                            //return "Success";
                        }
                        else
                        {
                            JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Authentication failed.\"}]}";
                            //return "No Memcode found for your SAPCode.";
                        }
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Authentication failed.\"}]}";
                        //return "ds is null";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
                //return ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetARTLDashBoardDtls")]
        [HttpPost]
        public string GetARTLDashBoardDtls([FromBody] InputModel model)
        {
            //String param = "Flag : " + Flag + ",SapCode : " + SAPCode;
            //string SAPCode, string Flag, String strTokenId
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetARTLDashBoardDtls", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@SAPCode", Convert.ToString(jObject["SapCode"]).Trim());
                htParam.Add("@Flag", Convert.ToString(jObject["Flag"]).Trim());
                htParam.Add("@MemTypeFlag", Convert.ToString(jObject["MemTypeFlag"]).Trim());
                dstemp = objDAL.selectFromDatabase("prc_GetARTLDashBoardDtls_mobile", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No record\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetSectionMapping")]
        [HttpPost]
        public string GetSectionMapping([FromBody] InputModel model)
        {
            try
            {
                dstemp.Clear();
                htParam.Clear();
                //string PAN, string ApplicationNo, String SapCode, String strTokenId
                //string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());

                //String param = "PAN : " + strPAN + ",ApplicationNo : " + ApplicationNo;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);

                SeqNo = ServerTrnReqLog(json, "GetSectionMapping", "", "", "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                //if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                //    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                //{
                //    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                //    updateResponse(JsonResponce, SeqNo);

                //    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                //}

                htParam.Clear();
                //htParam.Add("@Capt", strCaptcha);
                dstemp = objDAL.selectFromDatabase("PrcGetSectionMapping", htParam, "IsysARTL");


                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Captcha failed\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }


        [Route("api/CBCMS/GetCfrSecMapping")]
        [HttpPost]
        public string GetCfrSecMapping([FromBody] InputModel model)
        {
            //String param = "Flag : " + Flag + ",SapCode : " + SAPCode;
            //string SAPCode, string Flag, String strTokenId
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetCfrSecMapping", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                //else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                //    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                //{
                //    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                //    updateResponse(JsonResponce, SeqNo);

                //    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                //}
                dstemp.Clear();
                htParam.Clear();
                dstemp = objDAL.selectFromDatabase("PrcGetCfrSecMapping", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No record\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/AllCounts_Mobile")]
        [HttpPost]
        public string AllCounts_Mobile([FromBody] InputModel model)
        {
            try
            {
                //string SAPCode, String AgentType, String strTokenId
                //string strSAPCode = encryptDecrypt.DecodeAndDecrypt(SAPCode);
                //String param = "SapCode : " + strSAPCode;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "AllCounts_Mobile", strSapCode, "", "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@SAPCode", strSapCode);
                htParam.Add("@AgentType", Convert.ToString(jObject["AgentType"]).Trim());
                dstemp = objDAL.selectFromDatabase("prc_AllCounts_mobile", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Memcode found for your SAPCode.\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;

        }

        [Route("api/CBCMS/GetDataForSearch")]
        [HttpPost]
        public string GetDataForSearch([FromBody] InputModel model)
        {
            try
            {
                //string SapCode, string AppNo, string CndName, string PanNo, string Flag, string EntryDate, String strTokenId
                //string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //string strPanNo = encryptDecrypt.DecodeAndDecrypt(PanNo);
                //String param = "AppNo : " + strAppNo + ",SapCode : " + strSapCode + ",CndName : " + CndName + ",PanNo : " + strPanNo + ",Flag : " + Flag + ",EntryDate : " + EntryDate;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetDataForSearch", strSapCode, Convert.ToString(jObject["AppNo"]).Trim(), "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                dstemp.Clear();
                Hashtable htable = new Hashtable();
                htable.Clear();
                if (Convert.ToString(jObject["Flag"]).Trim() == "ARS")
                {
                    //for agent survey
                    htable.Add("@CndNo", string.Empty);
                    htable.Add("@Appno", Convert.ToString(jObject["AppNo"]).Trim());
                    htable.Add("@GivenName", Convert.ToString(jObject["CndName"]).Trim());
                    htable.Add("@Surname", string.Empty);
                    htable.Add("@PAN", Convert.ToString(jObject["PanNo"]).Trim());
                    htable.Add("@AgentBrokeCode", string.Empty);
                    htable.Add("@CreateFrmDtim", Convert.ToString(jObject["EntryDate"]).Trim());
                    htable.Add("@CreateToDtim", string.Empty);
                    dstemp = objDAL.selectFromDatabase("prc_GetAgentCodeDetails", htable, "ISysCHMS");
                }
                else
                {
                    htable.Add("@BMCode", strSapCode);
                    htable.Add("@AppNo", Convert.ToString(jObject["AppNo"]).Trim());
                    htable.Add("@CndName", Convert.ToString(jObject["CndName"]).Trim());
                    htable.Add("@PanNo", Convert.ToString(jObject["PanNo"]).Trim());
                    htable.Add("@Flag", Convert.ToString(jObject["Flag"]).Trim());
                    htable.Add("@EntryDate", Convert.ToString(jObject["EntryDate"]).Trim());
                    htable.Add("@AgentType", Convert.ToString(jObject["AgentType"]).Trim());
                    dstemp = objDAL.selectFromDatabase("Prc_GetDataForAppSearch", htable, "ISysARTL");
                }
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                    JsonResponce = "No Candidates found for your Search.";

            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/FetchPrefExmCndList")]
        [HttpPost]
        public string FetchPrefExmCndList([FromBody] InputModel model)
        {
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "FetchPrefExmCndList", strSapCode, Convert.ToString(jObject["AppNo"]).Trim(), "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                dstemp = objDAL.selectFromDatabase_singleValue("prc_GetMemberCode_App", strSapCode, "ISysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    string memCode = dstemp.Tables[0].Rows[0]["MemCode"].ToString();
                    dstemp.Clear();

                    //dstemp = objDAL.selectFromDatabase("Prc_EnqCandListForPreffExmDtls", memCode, AppNo, String.Empty, String.Empty, "cndEnq", "ISysARTL");
                    dstemp = objDAL.fetchDetails(strSapCode, "prc_GetPrefExmDateForApp", "ISysARTL");
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Candidates found for your Search.\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Memcode found for your SAPCode.\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetAgentSurveyDetails")]
        [HttpPost]
        public string GetAgentSurveyDetails([FromBody] InputModel model)
        {
            try
            {
                //string SapCode, string AppNo, string AgentBrokerCode, String strTokenId
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strAgentBrokerCode = Convert.ToString(jObject["AgentBrokerCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                DataSet dsAgentSurveyQuestion = new DataSet();

                //string param = "SapCode : " + strSapCode + " , AppNo : " + strAppNo + " , AgentBrokerCode : " + strAgentBrokerCode; ;
                SeqNo = ServerTrnReqLog(json, "GetAgentSurveyDetails", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                dstemp.Clear();
                Hashtable ht = new Hashtable();
                ht.Add("@Agntbrokercode", strAgentBrokerCode.ToString().Trim());
                dstemp = objDAL.selectFromDatabase("prc_getAgentSurveyQuestion", ht, "ISysCHMS");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        string jsonResponse = "";
                        for (int i = 0; i < dstemp.Tables[0].Rows.Count; i++)
                        {
                            ht.Remove("@QuesId");
                            ht.Remove("@Surveycode");
                            ht.Add("@QuesId", dstemp.Tables[0].Rows[i]["QuesId"].ToString().Trim());
                            ht.Add("@Surveycode", dstemp.Tables[0].Rows[i]["SurvyCode"].ToString().Trim());
                            dsAgentSurveyQuestion = objDAL.selectFromDatabase("prc_getAnswer", ht, "ISysCHMS");
                            string jsonAgentSurveyAnswer = ds2json(dsAgentSurveyQuestion);
                            string jsonAgentSurveyQuestion = ds2json(dstemp);
                            string response = "{\"Question\":" + jsonAgentSurveyQuestion + ",\"Answer\":" + jsonAgentSurveyAnswer + "}";
                            if (i > 0)
                            {
                                response = "," + response;
                            }

                            jsonResponse = jsonResponse + response;
                        }
                        JsonResponce = "[" + jsonResponse + "]";
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No record found\"}]}";
                    }
                }
                else
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No record found\"}]}";
            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/SubmitAgentSurveyDetail")]
        [HttpPost]
        public string SubmitAgentSurveyDetail([FromBody] InputModel model)
        {
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strSMCode = Convert.ToString(jObject["SMCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                DataSet dsAgentSurveyQuestion = new DataSet();

                SeqNo = ServerTrnReqLog(json, "SubmitAgentSurveyDetail", strSapCode, strSMCode, "Candidate Manager");

                string result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();
                }


                dstemp.Clear();
                Hashtable ht = new Hashtable();
                ht.Add("@QuestionID", Convert.ToString(jObject["QuestionID"]).Trim());
                ht.Add("@SMResoremark", Convert.ToString(jObject["SMResolutionRemark"]).Trim());
                ht.Add("@SMCode", strSMCode.ToString().Trim());
                ht.Add("@SurveyTypeCode", Convert.ToString(jObject["SurveyCode"]).Trim());
                ht.Add("@AgntBrokerCode", Convert.ToString(jObject["AgentBrokerCode"]).Trim());
                ht.Add("@CreatedBy", Convert.ToString(jObject["SapCode"]).Trim());
                ht.Add("@CallingDate", Convert.ToString(jObject["CallingDate"]).Trim());

                dstemp = objDAL.selectFromDatabase("InsAgntSurvyFedbck", ht, "ISysCHMS");
                if (dstemp != null)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No record found\"}]}"; ;
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetImageForDocType")]
        [HttpPost]
        public string GetImageForDocType([FromBody] InputModel model)
        {
            try
            {
                //[FromBody]InputModel model
                //string json = "{\"AppNo\":\"Android-b2db7c37-32e8-4f9e-b534-c763c1753b89\",\"UniqueRefID\":\"\",\"docType\":\"Photo\",\"SapCode\":\"70011721\",\"strTokenId\":\"84C9A497-4AE8-429B-856C-D8BE8BDF755B\",\"CallerId\":\"CBCMSCM\",\"CallerPass\":\"CBCMSCM\"}";
                //String Appno, String DocType, String SapCode, String strTokenId
                //string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);
                //String param = "AppNo : " + strAppno + ",DocType : " + DocType;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetImageForDocType", strSapCode, Convert.ToString(jObject["AppNo"]).Trim(), "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@Appno", Convert.ToString(jObject["AppNo"]).Trim());
                htParam.Add("@DocType", Convert.ToString(jObject["docType"]).Trim());
                dstemp = objDAL.selectFromDatabase("Prc_GetDocForMob", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);

                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Record not found\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();

            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;

        }

        [Route("api/CBCMS/InsertCandDocsFrmMob")]
        [HttpPost]
        public string InsertCandDocsFrmMob([FromBody] InputModel model)
        {
            try
            {
                //String Appno, byte[] Image, String DocType, String UploadedBy,
                //                        String lblimgshrt, String lblCndNoValue, String FileType, String strTokenId
                //string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);
                //string strUploadedBy = encryptDecrypt.DecodeAndDecrypt(UploadedBy);
                //string strlblCndNoValue = encryptDecrypt.DecodeAndDecrypt(lblCndNoValue);

                //String param = "AppNo : " + strAppno + ",DocType : " + DocType + ",UploadedBy : " + strUploadedBy + ",lblimgshrt : " + lblimgshrt + ",lblCndNoValue : " + strlblCndNoValue + ",FileType : " + FileType + ", strTokenId" + strTokenId;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strAppno = Convert.ToString(jObject["AppNo"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "InsertCandDocsFrmMob", strSapCode, strAppno, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp = null;
                byte[] Image = Convert.FromBase64String(jObject["Image"].ToString().Trim());
                Hashtable ht = new Hashtable();
                ht.Add("@Appno", strAppno.ToString().Trim());
                ht.Add("@Image", Image);
                ht.Add("@DocType", jObject["DocType"].ToString().Trim());
                ht.Add("@UploadedBy", jObject["UploadedBy"].ToString().Trim());
                ht.Add("@FileType", jObject["FileType"].ToString().Trim());
                ht.Add("@UnqRefId", jObject["UniqueRefID"].ToString().Trim());
                ht.Add("@CandType", Convert.ToString(jObject["CandType"]).Trim());
                ht.Add("@DocCode", Convert.ToString(jObject["DocCode"]).Trim());

                dstemp = objDAL.selectFromDatabase("Prc_InsCandDocsFrmMob", ht, "IsysARTL");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        //return ds2json(dstemp);
                        //CreateFolderOfDoc(lblimgshrt, strlblCndNoValue, Image);
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = dstemp.Tables[0].Rows.Count.ToString();
                    }
                }
                else
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Record not inserted\"}]}"; ;
            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";

            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;

        }

        [Route("api/CBCMS/GetAllSupportDocs")]
        [HttpPost]
        public string GetAllSupportDocs([FromBody] InputModel model)
        {
            //String param = "AppNo : " + AppNo; String AppNo, String SapCode, String strTokenId
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "GetAllSupportDocs", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();
                }
                dstemp = null;
                Hashtable ht = new Hashtable();
                ht.Add("@Appno", strAppNo);
                dstemp = objDAL.selectFromDatabase("Prc_GetAllSupportDocs", ht, "IsysARTL");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = dstemp.Tables[0].Rows.Count.ToString();
                    }
                }
                else
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Record\"}]}";
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/ProspectRegistration_mob")]
        [HttpPost]
        public string ProspectRegistration_mob([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                // SmMemberCode = "70011354"; CarrierCode = "2"; Pan = "LLJPP3838J"; EntryDate = "09/01/2020"; Occupation = "SLP"; Prefix = "MR"; GivenName = "NDJD"; Surname = "DHHD"; FatherName = "DHJD"; CndCat = "U"; DOB = "01/01/1989"; Gender = "M"; Nationality = "IND"; MaritalStat = "S"; CnctType = ""; HomeTel = " "; WorkTel = ""; Mobilecode = "91"; MobileTel = "9835956494"; Email = "dbdh@hdjd.sjdj"; MobileCode2 = ""; Mobile2 = ""; Email2 = ""; AgentType = ""; RecruitAgtCode = "70011354"; Bizsrc = "AG"; ChnCls = "AGLM"; RecruitUnitCode = ""; cmsunitcode = ""; MobUniquerecId = "ANDROID-20200109185002-6240ff09-6fb9-4ee0-a65a-785"; CreateBy = "70011354";
                //string jReqString = "SmMemberCode= " + SmMemberCode + ", CarrierCode= " + CarrierCode + ", Pan= " + Pan + ", EntryDate=" + Pan + ", Occupation= " + Occupation +
                //    " Prefix= " + Prefix + ", GivenName= " + GivenName + ", Surname= " + Surname + ", FatherName= " + FatherName + ", CndCat= " + CndCat + ", DOB= " + DOB + ", Gender= " + Gender +
                //    " Nationality= " + Nationality + ", MaritalStat= " + MaritalStat + ", CnctType= " + CnctType + ", HomeTel= " + HomeTel + " , WorkTel= " + WorkTel + ", Mobilecode= " + Mobilecode +
                //    " MobileTel= " + MobileTel + ", Email=" + Email + ", MobileCode2= " + MobileCode2 + ", Mobile2= " + Mobile2 + ", Email2= " + Email2 + ", AgentType= " + AgentType +
                //    " RecruitAgtCode= " + RecruitAgtCode + ", Bizsrc= " + Bizsrc + ", ChnCls= " + ChnCls + ", RecruitUnitCode= " + RecruitUnitCode + ", cmsunitcode= " + cmsunitcode +
                //    " MobUniquerecId= " + MobUniquerecId + ", CreateBy=" + CreateBy + ", CandType=" + CandType + ", AnniversaryDate=" + AnniversaryDate + ", AddRptMgr=" + AddRptMgr + ", ModuleID=" + ModuleID;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                //string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "ProspectRegistration_mob", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                objDaset.Clear();
                htable.Clear();

                htable.Add("@SapCode", strSapCode);
                htable.Add("@SmMemberCode", Convert.ToString(jObject["SmMemberCode"]).Trim());
                htable.Add("@CarrierCode", Convert.ToString(jObject["CarrierCode"]).Trim());
                htable.Add("@Pan", Convert.ToString(jObject["Pan"]).Trim());
                htable.Add("@EntryDate1", Convert.ToString(jObject["EntryDate"]).Trim());
                htable.Add("@Occupation", Convert.ToString(jObject["Occupation"]).Trim());
                htable.Add("@Prefix", Convert.ToString(jObject["Prefix"]).Trim());
                htable.Add("@GivenName", Convert.ToString(jObject["GivenName"]).Trim());
                htable.Add("@Surname", Convert.ToString(jObject["Surname"]).Trim());
                htable.Add("@FatherName", Convert.ToString(jObject["FatherName"]).Trim());
                htable.Add("@CndCat", Convert.ToString(jObject["CndCat"]).Trim());
                htable.Add("@BirthRegDate1", Convert.ToString(jObject["DOB"]).Trim());
                htable.Add("@Gender", Convert.ToString(jObject["Gender"]).Trim());
                htable.Add("@Nationality", Convert.ToString(jObject["Nationality"]).Trim());
                htable.Add("@MaritalStat", Convert.ToString(jObject["MaritalStat"]).Trim());
                htable.Add("@CnctType", Convert.ToString(jObject["CnctType"]).Trim());
                htable.Add("@HomeTel", Convert.ToString(jObject["HomeTel"]).Trim());
                htable.Add("@WorkTel", Convert.ToString(jObject["WorkTel"]).Trim());
                htable.Add("@Mobilecode", Convert.ToString(jObject["Mobilecode"]).Trim());
                htable.Add("@MobileTel", Convert.ToString(jObject["MobileTel"]).Trim());
                htable.Add("@Email", Convert.ToString(jObject["Email"]).Trim());
                htable.Add("@MobileCode2", Convert.ToString(jObject["MobileCode2"]).Trim());
                htable.Add("@Mobile2", Convert.ToString(jObject["Mobile2"]).Trim());
                htable.Add("@Email2", Convert.ToString(jObject["Email2"]).Trim());
                htable.Add("@AgentType", Convert.ToString(jObject["AgentType"]).Trim());
                htable.Add("@RecruitAgtCode", Convert.ToString(jObject["RecruitAgtCode"]).Trim());
                htable.Add("@Bizsrc", Convert.ToString(jObject["Bizsrc"]).Trim());
                htable.Add("@ChnCls", Convert.ToString(jObject["ChnCls"]).Trim());
                htable.Add("@cmsunitcode", Convert.ToString(jObject["cmsunitcode"]).Trim());
                htable.Add("@MobUniquerecId", Convert.ToString(jObject["MobUniquerecId"]).Trim());
                htable.Add("@CreateBy", Convert.ToString(jObject["CreateBy"]).Trim());
                htable.Add("@RecruitUnitCode", Convert.ToString(jObject["RecruitUnitCode"]).Trim());
                htable.Add("@CandType", Convert.ToString(jObject["CandType"]).Trim());
                htable.Add("@AnniversaryDate", Convert.ToString(jObject["AnniversaryDate"]).Trim());
                htable.Add("@AddRptMgr", Convert.ToString(jObject["AddRptMgr"]).Trim());
                htable.Add("@ModuleID", Convert.ToString(jObject["ModuleID"]).Trim());
                htable.Add("@CasteCat", Convert.ToString(jObject["CasteCat"]).Trim());

                objDaset = objDAL.selectFromDatabase("prc_InsProspectRegMob", htable, "IsysARTL");
                if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                {
                    JSonString = ds2json(objDaset);
                    //if (objDaset.Tables[0].Rows[0]["status"].ToString() == "SUCCESS")
                    //{
                    //    ObjTask.UpdateCndTask(objDaset.Tables[0].Rows[0]["AppNo"].ToString(), Convert.ToString(jObject["CreateBy"]).Trim());//To create task in CRM  in 18.12.2017
                    //}
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No records to display\"}]}";
                }

            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }

        [Route("api/CBCMS/GetShareLinkForApplicant")]
        [HttpPost]
        public string GetShareLinkForApplicantAsync([FromBody] InputModel model)

        {

            //[FromBody] InputModel model

            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            try
            {
                string jsonInput = model.jsonInput;
                //InputModel model = new InputModel();
                //model.jsonInput = "{\"jsonInput\":\"MhaN0eH+IjQi1ttuN7xiCuhTupugm+1U5dfquEUuhdz\\/iSR3\\/njIxaQdXj\\/f6hfZHrp4Zfm4DFPR\\nLhJ0TByVLCU6SGo7YesQTB\\/JoucIqNZ28vgb3bmJzwly+BIklv93F9QWK8a++zK5iqTkRDXYBCA\\/\\ngPX91Y\\/lAyxErYo2HA1SkZ2TxvKavV9K4AbJ4iXf\\n\"}";
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                //string json = "{\"SapCode\": \"70011354\",\"strTokenId\":\"123\",\"AppNo\":\"10028854\"}";
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                string MobileNo = Convert.ToString(jObject["MobileNo"]).Trim();
                string EmailID = Convert.ToString(jObject["EmailID"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetShareLinkForApplicant", "", "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                BitlyUtil bitly = new BitlyUtil();
                JSonString = bitly.CreateBitlyLink(strAppNo, MobileNo, EmailID);
            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }


        [Route("api/CBCMS/ShareLinkToApplicant")]
        [HttpPost]
        public string ShareLinkToApplicant([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                string WebLink = Convert.ToString(jObject["WebLink"]).Trim();
                string AppLink = Convert.ToString(jObject["AppLink"]).Trim();
                string EmailID = Convert.ToString(jObject["EmailID"]).Trim();
                string MobileNo = Convert.ToString(jObject["MobileNo"]).Trim();
                string Name = Convert.ToString(jObject["Name"]).Trim();
                SeqNo = ServerTrnReqLog(json, "ShareLinkToApplicant", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                BitlyUtil util = new BitlyUtil();
                JSonString = util.ShareLink(strAppNo);

            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }


        [Route("api/CBCMS/ConvertPStoIS")]
        [HttpPost]
        public string ConvertPStoIS([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                string strFlag = Convert.ToString(jObject["SFlag"]).Trim();
                SeqNo = ServerTrnReqLog(json, "ConvertPStoIS", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }

                objDaset.Clear();
                htable.Clear();

                htable.Add("@AppNo", strAppNo);
                htable.Add("@SapCode", strSapCode);
                htable.Add("@BizSrc", Convert.ToString(jObject["BizSrc"]).Trim());
                htable.Add("@ChnCls", Convert.ToString(jObject["ChnCls"]).Trim());
                htable.Add("@UNitcode", Convert.ToString(jObject["UNitcode"]).Trim());
                htable.Add("@CMSUNitcode", Convert.ToString(jObject["CMSUNitcode"]).Trim());
                htable.Add("@SFlag", strFlag);

                objDaset = objDAL.selectFromDatabase("Prc_ConvertPStoIS", htable, "IsysARTL");

                if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                {
                    JSonString = ds2json(objDaset);
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No records to display\"}]}";
                }

            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }

        [Route("api/CBCMS/GenerateOTPRequest")]
        [HttpPost]
        public string GenerateOTPRequest([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                string strOtpFlag = Convert.ToString(jObject["GenOTPFlag"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GenerateOTPRequest", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                //else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                //    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                //{
                //    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                //    updateResponse(JsonResponce, SeqNo);

                //    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                //}
                objDaset.Clear();
                htable.Clear();
                if (strOtpFlag.Equals("GenLoginOTP"))
                {
                    htable.Add("@AppNo", strAppNo);
                    htable.Add("@MobileNo", Convert.ToString(jObject["MobileNo"]).Trim());
                    htable.Add("@Flag", Convert.ToString(jObject["Flag"]).Trim());

                    objDaset = objDAL.selectFromDatabase("Prc_GenLoginOTP_mob", htable, "IsysARTL");
                }
                else
                {
                    htable.Add("@AppNo", strAppNo);
                    htable.Add("@MobileNo", Convert.ToString(jObject["MobileNo"]).Trim());
                    htable.Add("@EmailID", Convert.ToString(jObject["Email"]).Trim());
                    htable.Add("@WebLink", Convert.ToString(jObject["WebLink"]).Trim());
                    htable.Add("@AppLink", Convert.ToString(jObject["AppLink"]).Trim());
                    htable.Add("@Flag", Convert.ToString(jObject["Flag"]).Trim());

                    objDaset = objDAL.selectFromDatabase("Prc_GenerateOTP", htable, "IsysARTL");
                }


                if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                {
                    JSonString = ds2json(objDaset);
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No records to display\"}]}";
                }

            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }

        [Route("api/CBCMS/SubmitOTPRequest")]
        [HttpPost]
        public string SubmitOTPRequest([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                string strOtpFlag = Convert.ToString(jObject["SubOTPFlag"]).Trim();
                SeqNo = ServerTrnReqLog(json, "SubmitOTPRequest", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                //else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                //    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                //{
                //    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                //    updateResponse(JsonResponce, SeqNo);

                //    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                //}
                objDaset.Clear();
                htable.Clear();

                if (strOtpFlag.Equals("SubmitLoginOTP"))
                {
                    htable.Add("@AppNo", strAppNo);
                    htable.Add("@textOTP", Convert.ToString(jObject["textOTP"]).Trim());

                    objDaset = objDAL.selectFromDatabase("Prc_SubLoginOTP_mob", htable, "IsysARTL");
                }
                else
                {
                    htable.Add("@AppNo", strAppNo);
                    htable.Add("@textOTP", Convert.ToString(jObject["textOTP"]).Trim());

                    objDaset = objDAL.selectFromDatabase("Prc_CheckCnfmOTP", htable, "IsysARTL");
                }

                if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                {
                    JSonString = ds2json(objDaset);
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No records to display\"}]}";
                }

            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }

        [Route("api/CBCMS/SendPaymentLink")]
        [HttpPost]
        public string SendPaymentLink([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "SendPaymentLink", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                objDaset.Clear();
                htable.Clear();

                htable.Add("@AppNo", strAppNo);

                objDaset = objDAL.selectFromDatabase("Prc_OnboradingPaymentForARTL", htable, "ISysARTL");
                if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                {
                    JSonString = ds2json(objDaset);
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No records to display\"}]}";
                }

            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }

        [Route("api/CBCMS/GetFeesWavier")]
        [HttpPost]
        public string GetFeesWavier([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                //string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetFeesWavier", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                objDaset.Clear();
                htable.Clear();

                htable.Add("@BIZSRC", Convert.ToString(jObject["BizSrc"]).Trim());
                htable.Add("@CHNCLS", Convert.ToString(jObject["ChnCls"]).Trim());
                htable.Add("@WAVCNT", Convert.ToString(jObject["WavCount"]).Trim());

                objDaset = objDAL.selectFromDatabase("Prc_Get_WavierRes", htable, "ISysARTL");
                if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                {
                    JSonString = ds2json(objDaset);
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No records to display\"}]}";
                }

            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }

        [Route("api/CBCMS/GetProspectDetails")]
        [HttpPost]
        public string GetProspectDetails([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                //string AppNo, string CarrierCode, String SapCode, String strTokenId
                //String param = "AppNo= " + AppNo + ", CarrierCode= " + CarrierCode;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetProspectDetails", "", "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                objDaset.Clear();
                htable.Clear();

                htable.Add("@SapCode", strSapCode);
                htable.Add("@CarrierCode", Convert.ToString(jObject["CarrierCode"]).Trim());
                htable.Add("@Appno", strAppNo);


                objDaset = objDAL.selectFromDatabase("prc_getProspectDetails_mob", htable, "IsysARTL");
                if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                {
                    JSonString = ds2json(objDaset);
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No records to display\"}]}";
                }
            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }

        [Route("api/CBCMS/GetCandidateDetails")]
        [HttpPost]
        public string GetCandidateDetails([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                //string AppNo, string CarrierCode, String SapCode, String strTokenId
                //String param = "AppNo= " + AppNo + ", CarrierCode= " + CarrierCode;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetCandidateDetails", "", "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                objDaset.Clear();
                htable.Clear();

                htable.Add("@SapCode", strSapCode);
                htable.Add("@CarrierCode", Convert.ToString(jObject["CarrierCode"]).Trim());
                htable.Add("@Appno", strAppNo);

                objDaset = objDAL.selectFromDatabase("Prc_GetCandidateDetails_Mob", htable, "IsysARTL");
                if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                {
                    JSonString = ds2json(objDaset);
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No records to display\"}]}";
                }
            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }


        [Route("api/CBCMS/CnandidateApproved_mobile")]
        [HttpPost]
        public string CnandidateApproved_mobile([FromBody] InputModel model)
        {
            try
            {
                //String Appno, String Flag, String ModuleID, String SapCode, String strTokenId
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(Appno);
                //String param = "AppNo : " + strAppNo + ",Flag : " + Flag + ",ModuleID : " + ModuleID;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppno = Convert.ToString(jObject["Appno"]).Trim();
                SeqNo = ServerTrnReqLog(json, "CnandidateApproved_mobile", strSapCode, strAppno, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@Appno", strAppno.ToString().Trim());
                htParam.Add("@Flag", Convert.ToString(jObject["Flag"]).Trim());
                htParam.Add("@ModuleID", Convert.ToString(jObject["ModuleID"]).Trim());
                htParam.Add("@SapCode", strSapCode);
                htParam.Add("@CarrierCode", Convert.ToString(jObject["CarrierCode"]).Trim());
                //dstemp = objDAL.selectFromDatabase("prc_CnandidateApproved_mobile", htParam, "IsysARTL");
                dstemp = objDAL.selectFromDatabase("prc_UpdateCandidateapproval_mob", htParam, "IsysARTL");
                //JsonResponce = "Candidate Approved";
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Candidates found for your Search.\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/setPrefDate_mobile")]
        [HttpPost]
        public string setPrefDate_mobile([FromBody] InputModel model)
        {
            try
            {
                //String Appno, String date, String ModuleID, String SapCode, String strTokenId
                //string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);
                //String param = "AppNo : " + strAppno + ",date : " + date + ", ModuleID : " + ModuleID;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppno = Convert.ToString(jObject["Appno"]).Trim();
                SeqNo = ServerTrnReqLog(json, "setPrefDate_mobile", strSapCode, strAppno, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@Appno", strAppno.ToString().Trim());
                htParam.Add("@date", Convert.ToString(jObject["date"]).Trim());
                htParam.Add("@ModuleID", Convert.ToString(jObject["ModuleID"]).Trim());
                dstemp = objDAL.selectFromDatabase("prc_SetPreferedDate_mobile", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"0\",\"Message\":\"Date is set\"}]}";
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Memcode found for your SAPCode.\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetCFRStatus_mobile")]
        [HttpPost]
        public string GetCFRStatus_mobile([FromBody] InputModel model)
        {
            try
            {
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //string AppNo, String SapCode, String strTokenId
                //String param = "AppNo : " + strAppNo;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppno = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetCFRStatus_mobile", strSapCode, strAppno, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                Hashtable htparam = new Hashtable();
                htparam.Add("@AppNo", strAppno);
                dstemp = objDAL.selectFromDatabase("Prc_GetProcessType_Mob", htparam, "ISysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    String _ProcessType = Convert.ToString(dstemp.Tables[0].Rows[0]["ProcessType"]).Trim();
                    String _CndNo = Convert.ToString(dstemp.Tables[0].Rows[0]["CndNo"]).Trim();
                    dstemp.Clear();
                    Hashtable htTable = new Hashtable();
                    htTable.Add("@CndNo", _CndNo);
                    htTable.Add("@Flag", _ProcessType);
                    //dstemp = objDAL.selectFromDatabase("Prc_GetCFRStatus_Mob", htTable, "ISysARTL");
                    dstemp = objDAL.selectFromDatabase("Prc_GetCFRdesc_Mob", htTable, "ISysARTL");
                    //dstemp = objDAL.GetDataForAppSearch("Prc_GetCFRStatus_Mob", _CndNo, _ProcessType, "ISysARTL");
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Candidates found for your Search.\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Memcode found for your SAPCode.\"}]}";
                }
            }
            catch (Exception ex)
            {
                //return ex.ToString();
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/CFR_Responde_mobile")]
        [HttpPost]
        public string CFR_Responde_mobile([FromBody] InputModel model)
        {
            try
            {
                //string AppNo, string CfrFor, string RemarkId, string RespondedBy, string ModuleID, String SapCode, String strTokenId
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //string strRespondedBy = encryptDecrypt.DecodeAndDecrypt(RespondedBy);
                //String param = "AppNo : " + strAppNo + ",CfrFor : " + CfrFor + "RemarkId : " + RemarkId +
                // "RespondedBy : " + strRespondedBy + " ModuleID : " + ModuleID;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppno = Convert.ToString(jObject["AppNo"]).Trim();

                SeqNo = ServerTrnReqLog(json, "CFR_Responde_mobile", strSapCode, strAppno, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                Hashtable htparam = new Hashtable();
                htparam.Add("@AppNo", Convert.ToString(jObject["AppNo"]).Trim());
                htparam.Add("@CfrFor", Convert.ToString(jObject["CfrFor"]).Trim());
                htparam.Add("@RemarkId", Convert.ToString(jObject["RemarkId"]).Trim());
                htparam.Add("@RespondedBy", Convert.ToString(jObject["RespondedBy"]).Trim());
                htparam.Add("@ModuleID", Convert.ToString(jObject["ModuleID"]).Trim());

                dstemp = objDAL.selectFromDatabase("Prc_Submit_AssignedCFR_bracheduser_Mob", htparam, "ISysARTL");

                //JsonResponce = ds2json(dstemp);
                if (dstemp != null)
                {
                    JsonResponce = ds2json(dstemp);

                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        //return ds2json(dstemp);ResponseCode
                        if (dstemp.Tables[0].Rows[0]["ResponseCode"].ToString().Trim().Equals("0")
                            && dstemp.Tables[0].Rows[0]["AllCfrResponded"].ToString().Trim().Equals("Y"))
                        {
                            ConvertRDLCToPDF(dstemp.Tables[0].Rows[0]["CndNo"].ToString().Trim(), strSapCode);
                        }
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"CFR Respond failed\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
                //return "error";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/UpdateProfileFrmMob")]
        [HttpPost]
        public string UpdateProfileFrmMob([FromBody] InputModel model)
        {
            try
            {
                //String AppNo, String AgntType, String IsWrkngInOthrRelnce, String CompName,
                //String YrsInInsrnce, String YrsInRelnce, String TypOfVchl, String VechManf, String DlrCompName, String DlrOthr, String AvgMnthVol,
                //String Comp1Name, String MnthVolFrComp1, String Comp2Name, String MnthVolFrComp2, String RGIMnthVol, String TotBsnMtr, String TotBsnHlth,
                //String TotBsnCmrclLine, String RGIBsnMtr, String RGIBsnHlth, String RGIBsnCmrclLine, String UpdateBy, String SapCode, String strTokenId
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "UpdateProfileFrmMob", "", strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();
                }
                dstemp = null;
                //Hashtable ht = new Hashtable();
                htParam.Clear();
                htParam.Add("@AppNo", strAppNo);
                htParam.Add("@agntype", Convert.ToString(jObject["AgntType"]).Trim());
                htParam.Add("@IsWrkng", Convert.ToString(jObject["IsWrkngInOthrRelnce"]).Trim());
                htParam.Add("@compName", Convert.ToString(jObject["CompName"]).Trim());
                htParam.Add("@NoOfYrsIns", Convert.ToString(jObject["YrsInInsrnce"]).Trim());
                htParam.Add("@NoOfYrs", Convert.ToString(jObject["YrsInRelnce"]).Trim());
                htParam.Add("@TypOfVchl", Convert.ToString(jObject["TypOfVchl"]).Trim());
                htParam.Add("@VechManf", Convert.ToString(jObject["VechManf"]).Trim());
                htParam.Add("@DlrCompName", Convert.ToString(jObject["DlrCompName"]).Trim());
                htParam.Add("@DlrOthr", Convert.ToString(jObject["DlrOthr"]).Trim());
                htParam.Add("@AvgMnthlyIncm", Convert.ToString(jObject["AvgMnthVol"]).Trim());
                htParam.Add("@Comp1Name", Convert.ToString(jObject["Comp1Name"]).Trim());
                htParam.Add("@MnthVol1", Convert.ToString(jObject["MnthVolFrComp1"]).Trim());
                htParam.Add("@Comp2Name", Convert.ToString(jObject["Comp2Name"]).Trim());
                htParam.Add("@MnthVol2", Convert.ToString(jObject["MnthVolFrComp2"]).Trim());
                htParam.Add("@RGIMnthVol", Convert.ToString(jObject["RGIMnthVol"]).Trim());
                htParam.Add("@TotBsnMtr", Convert.ToString(jObject["TotBsnMtr"]).Trim());
                htParam.Add("@TotBsnHlth", Convert.ToString(jObject["TotBsnHlth"]).Trim());
                htParam.Add("@TotBsnCmrclLine", Convert.ToString(jObject["TotBsnCmrclLine"]).Trim());
                htParam.Add("@RGIBsnMtr", Convert.ToString(jObject["RGIBsnMtr"]).Trim());
                htParam.Add("@RGIBsnHlth", Convert.ToString(jObject["RGIBsnHlth"]).Trim());
                htParam.Add("@RGIBsnCmrclLine", Convert.ToString(jObject["RGIBsnCmrclLine"]).Trim());
                htParam.Add("@UpdtedBy", Convert.ToString(jObject["UpdateBy"]).Trim());
                dstemp = objDAL.selectFromDatabase("Prc_UpdateProfileDetails_Mob", htParam, "IsysARTL");

                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        //return ds2json(dstemp);
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"0\",\"Message\":\"Successfully Updated\"}]}";
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Update failed\"}]}";
                    }
                }
                else
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Update failed\"}]}";
            }
            catch (Exception ex)
            {
                JsonResponce = "In Catch block = " + ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetCfrRaisedCandidate")]
        [HttpPost]
        public string GetCfrRaisedCandidate([FromBody] InputModel model)
        {
            try
            {
                //string SapCode, string AppNo, string CndName, string MemberType, string EntryDate, String strTokenId
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);

                //String param = "AppNo : " + strAppNo + ",SapCode : " + strSapCode + "CndName : " + CndName + "MemberType : " + MemberType + "EntryDate : " + EntryDate;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();

                SeqNo = ServerTrnReqLog(json, "GetCfrRaisedCandidate", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();
                }

                dstemp.Clear();
                Hashtable htCFR = new Hashtable();
                htCFR.Add("@BMCode", strSapCode);
                htCFR.Add("@AppNo", strAppNo);
                htCFR.Add("@CndName", Convert.ToString(jObject["CndName"]).Trim());
                htCFR.Add("@MemberType", Convert.ToString(jObject["MemberType"]).Trim());
                htCFR.Add("@EntryDate", Convert.ToString(jObject["EntryDate"]).Trim());

                dstemp = objDAL.selectFromDatabase("Prc_GetAdvDetailsForCFR_mobile", htCFR, "ISysARTL");
                if (dstemp.Tables.Count > 0)
                {
                    if (dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No record found\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Candidates found for your Search.\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/Get_SyncedPAN_verification_mobile")]
        [HttpPost]
        public string Get_SyncedPAN_verification_mobile([FromBody] InputModel model)
        {
            try
            {
                dstemp.Clear();
                htParam.Clear();
                //string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());
                //string strRegID = encryptDecrypt.DecodeAndDecrypt(MobUniqueRegID.Trim());
                //string PAN, string MobUniqueRegID, String SapCode, String strTokenId
                //String param = "PAN : " + strPAN + ",MobUniqueRegID : " + strRegID;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "Get_SyncedPAN_verification_mobile", strSapCode, "", "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@PAN", Convert.ToString(jObject["PAN"]).Trim());
                htParam.Add("@MobUniquerecId", Convert.ToString(jObject["MobUniqueRegID"]).Trim());
                dstemp = objDAL.selectFromDatabase("prc_syncedPANValidation", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Verification failed\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/Get_PAN_verification_mobile")]
        [HttpPost]
        public string Get_PAN_verification_mobile([FromBody] InputModel model)
        {
            try
            {
                dstemp.Clear();
                htParam.Clear();
                //string PAN, string ApplicationNo, String SapCode, String strTokenId
                //string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());

                //String param = "PAN : " + strPAN + ",ApplicationNo : " + ApplicationNo;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strAppNo = Convert.ToString(jObject["ApplicationNo"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "Get_PAN_verification_mobile", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                htParam.Clear();
                htParam.Add("@PanNo", Convert.ToString(jObject["PAN"]).Trim());
                //htParam.Add("@ApplicationNo", Convert.ToString(jObject["ApplicationNo"]).Trim());
                htParam.Add("@AgentType", Convert.ToString(jObject["MemType"]).Trim());
                htParam.Add("@SapCode", strSapCode);
                dstemp = objDAL.selectFromDatabase("Prc_GetChkPANExist_Mob", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Verification failed\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/Get_NomineePAN_verification_mobile")]
        [HttpPost]
        public string Get_NomineePAN_verification_mobile([FromBody] InputModel model)
        {
            try
            {
                dstemp.Clear();
                htParam.Clear();
                //string PAN, string ApplicationNo, String SapCode, String strTokenId
                //string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());

                //String param = "PAN : " + strPAN + ",ApplicationNo : " + ApplicationNo;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strAppNo = Convert.ToString(jObject["ApplicationNo"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "Get_NomineePAN_verification_mobile", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                htParam.Clear();
                htParam.Add("@PanNo", Convert.ToString(jObject["PAN"]).Trim());
                //htParam.Add("@ApplicationNo", Convert.ToString(jObject["ApplicationNo"]).Trim());
                htParam.Add("@CndAgtType", Convert.ToString(jObject["CndAgtType"]).Trim());
                dstemp = objDAL.selectFromDatabase("Prc_GetChkNomineePANExist_MobApp", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Verification failed\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetActiveIntegrations")]
        [HttpPost]
        public string GetActiveIntegrations([FromBody] InputModel model)
        {
            try
            {
                dstemp.Clear();
                htParam.Clear();
                //string PAN, string ApplicationNo, String SapCode, String strTokenId
                //string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());

                //String param = "PAN : " + strPAN + ",ApplicationNo : " + ApplicationNo;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "GetActiveIntegrations", strSapCode, "", "Candidate Manager");

                if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                htParam.Clear();
                dstemp = objDAL.selectFromDatabase("Prc_GetActiveIntegration", htParam, "ISysARTL");


                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No record found\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/VerifyPAN")]
        [HttpPost]
        public string VerifyPAN([FromBody] InputModel model)
        {
            try
            {
                dstemp.Clear();
                htParam.Clear();
                //string PAN, string ApplicationNo, String SapCode, String strTokenId
                //string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());

                //String param = "PAN : " + strPAN + ",ApplicationNo : " + ApplicationNo;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strPanNo = Convert.ToString(jObject["PAN"]).Trim();
                string strMobNo = Convert.ToString(jObject["MobNo"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strFlag = Convert.ToString(jObject["Flag"]).Trim();
                string strIntgID = Convert.ToString(jObject["IntgId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "VerifyPAN", strSapCode, strPanNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                htParam.Clear();
                htParam.Add("@Panno", strPanNo);
                htParam.Add("@Sid", strTokenId);
                htParam.Add("@IntgID", strIntgID);
                htParam.Add("@SapCode", strSapCode);

                if (strFlag.Equals("Retry"))
                {
                    dstemp = objDAL.selectFromDatabase("Prc_Get_PanSts", htParam, "INSCRecruit");
                }
                else
                {
                    htParam.Add("@MobileNo", strMobNo);
                    dstemp = objDAL.selectFromDatabase("Prc_Ins_TranstStatus", htParam, "INSCRecruit");
                }

                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Verification failed\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/SubmitCaptcha")]
        [HttpPost]
        public string SubmitCaptcha([FromBody] InputModel model)
        {
            try
            {
                dstemp.Clear();
                htParam.Clear();
                //string PAN, string ApplicationNo, String SapCode, String strTokenId
                //string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());

                //String param = "PAN : " + strPAN + ",ApplicationNo : " + ApplicationNo;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strPanNo = Convert.ToString(jObject["Pan"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strCaptcha = Convert.ToString(jObject["Captcha"]).Trim();
                string strIntgId = Convert.ToString(jObject["IntgId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "SubmitCaptcha", strSapCode, strPanNo, "Candidate Manager");

                if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                htParam.Clear();
                htParam.Add("@Panno", strPanNo);
                htParam.Add("@Sid", strTokenId);
                htParam.Add("@IntgId", strIntgId);
                htParam.Add("@Capt", strCaptcha);
                dstemp = objDAL.selectFromDatabase("Prc_Upd_Captcha", htParam, "INSCRecruit");


                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Verification failed\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/CheckExtExmCentre")]
        [HttpPost]
        public string CheckExtExmCentre([FromBody] InputModel model)
        {
            try
            {
                dstemp.Clear();
                htParam.Clear();
             
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strExmCntr = Convert.ToString(jObject["ExmCntr"]).Trim();
                string strFlag = Convert.ToString(jObject["Flag"]).Trim();

                SeqNo = ServerTrnReqLog(json, "CheckExtExmCentre", strSapCode, strAppNo, "Candidate Manager");

                if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                else
                {
                    DataTable dataTable = new DataTable();
                    EXTAPIClient eXTAPI = new EXTAPIClient();
                    dataTable = eXTAPI.ExtChkReg(strExmCntr, strFlag);
                    JsonResponce = ds2json(dataTable);
                }
            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/InsCndProfiling_mobile")]
        [HttpPost]
        public string InsCndProfiling_mobile([FromBody] InputModel model)
        {
            try
            {
                //        String AgntType, String CndNo, String AppNo, String IsWrkngInOthrRelnce, String CompName,
                //String YrsInInsrnce, String YrsInRelnce, String TypOfVchl, String VechManf, String DlrCompName, String DlrOthr, String AvgMnthVol,
                //String Comp1Name, String MnthVolFrComp1, String Comp2Name, String MnthVolFrComp2, String RGIMnthVol, String TotBsnMtr, String TotBsnHlth,
                //String TotBsnCmrclLine, String RGIBsnMtr, String RGIBsnHlth, String RGIBsnCmrclLine, String CreateBy, String strTokenId
                //        string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //        string strCndNo = encryptDecrypt.DecodeAndDecrypt(CndNo);
                //        string strCreateBy = encryptDecrypt.DecodeAndDecrypt(CreateBy);

                //        String param = "AgntType : " + AgntType + ",CndNo : " + strCndNo + "AppNo : " + strAppNo + ",IsWrkngInOthrRelnce : " + IsWrkngInOthrRelnce +
                //       "CompName : " + CompName + "YrsInInsrnce : " + YrsInInsrnce + "YrsInRelnce : " + YrsInRelnce + "TypOfVchl : " + TypOfVchl + "VechManf : " + VechManf +
                //       "DlrCompName : " + DlrCompName + "DlrOthr : " + DlrOthr + "AvgMnthVol : " + AvgMnthVol + "Comp1Name : " + Comp1Name + "MnthVolFrComp1 : " + MnthVolFrComp1 +
                //       "Comp2Name : " + Comp2Name + "MnthVolFrComp2 : " + MnthVolFrComp2 + "RGIMnthVol : " + RGIMnthVol + "TotBsnMtr : " + TotBsnMtr + "TotBsnHlth : " + TotBsnHlth +
                //       "TotBsnCmrclLine : " + TotBsnCmrclLine + "RGIBsnMtr : " + RGIBsnMtr + "RGIBsnHlth : " + RGIBsnHlth + "RGIBsnCmrclLine : " + RGIBsnCmrclLine + "CreateBy : " + strCreateBy;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["CreateBy"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();

                SeqNo = ServerTrnReqLog(json, "InsCndProfiling_mobile", "", strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@agntype", Convert.ToString(jObject["AgntType"]).Trim());
                htParam.Add("@CndNo", Convert.ToString(jObject["CndNo"]).Trim());
                htParam.Add("@AppNo", Convert.ToString(jObject["AppNo"]).Trim());
                htParam.Add("@IsWrkng", Convert.ToString(jObject["IsWrkngInOthrRelnce"]).Trim());
                htParam.Add("@compName", Convert.ToString(jObject["CompName"]).Trim());
                htParam.Add("@NoOfYrsIns", Convert.ToString(jObject["YrsInInsrnce"]).Trim());
                htParam.Add("@NoOfYrs", Convert.ToString(jObject["YrsInRelnce"]).Trim());
                htParam.Add("@TypOfVchl", Convert.ToString(jObject["TypOfVchl"]).Trim());
                htParam.Add("@VechManf", Convert.ToString(jObject["VechManf"]).Trim());
                htParam.Add("@DlrCompName", Convert.ToString(jObject["DlrCompName"]).Trim());
                htParam.Add("@DlrOthr", Convert.ToString(jObject["DlrOthr"]).Trim());
                htParam.Add("@AvgMnthlyIncm", Convert.ToString(jObject["AvgMnthVol"]).Trim());
                htParam.Add("@Comp1Name", Convert.ToString(jObject["Comp1Name"]).Trim());
                htParam.Add("@MnthVol1", Convert.ToString(jObject["MnthVolFrComp1"]).Trim());
                htParam.Add("@Comp2Name", Convert.ToString(jObject["Comp2Name"]).Trim());
                htParam.Add("@MnthVol2", Convert.ToString(jObject["MnthVolFrComp2"]).Trim());
                htParam.Add("@RGIMnthVol", Convert.ToString(jObject["RGIMnthVol"]).Trim());
                htParam.Add("@TotBsnMtr", Convert.ToString(jObject["TotBsnMtr"]).Trim());
                htParam.Add("@TotBsnHlth", Convert.ToString(jObject["TotBsnHlth"]).Trim());
                htParam.Add("@TotBsnCmrclLine", Convert.ToString(jObject["TotBsnCmrclLine"]).Trim());
                htParam.Add("@RGIBsnMtr", Convert.ToString(jObject["RGIBsnMtr"]).Trim());
                htParam.Add("@RGIBsnHlth", Convert.ToString(jObject["RGIBsnHlth"]).Trim());
                htParam.Add("@RGIBsnCmrclLine", Convert.ToString(jObject["RGIBsnCmrclLine"]).Trim());
                htParam.Add("@CreateBy", Convert.ToString(jObject["CreateBy"]).Trim());

                dstemp = objDAL.selectFromDatabase("prc_InsCndProfiling_Mob", htParam, "IsysARTL");
                //JsonResponce = "SUCCESS";
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Verification failed\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/Get_Profile_detail")]
        [HttpPost]
        public string Get_Profile_detail([FromBody] InputModel model)
        {
            try
            {
                //string AppNo, String SapCode, String strTokenId
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);

                //String param = "AppNo : " + strAppNo;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "Get_Profile_detail", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@AppNo", strAppNo.ToString().Trim());
                dstemp = objDAL.selectFromDatabase("Prc_GetCndProfilingDtls_Mob", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No record\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/Set_CompositeTransfer_detail")]
        [HttpPost]
        public string Set_CompositeTransfer_detail([FromBody] InputModel model)
        {
            try
            {
                //       string CreateBy, string CndNo, string AppNo,
                //string NameofInsurer, string AgencyCodeNumber, string DateOfAppointmentAsAgent,
                //string DateOfCessationOfAgency, string ReasonForCessationOfAgency, string Category, string ICDate, String strTokenId
                //       string strCreateBy = encryptDecrypt.DecodeAndDecrypt(CreateBy);
                //       string strCndNo = encryptDecrypt.DecodeAndDecrypt(CndNo);
                //       string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);

                //       String param = "AppNo : " + strAppNo + ",CreateBy : " + strCreateBy + "CndNo : " + strCndNo + ",NameofInsurer : " + NameofInsurer + "AgencyCodeNumber : " + AgencyCodeNumber +
                //       ",DateOfAppointmentAsAgent : " + DateOfAppointmentAsAgent + "DateOfCessationOfAgency : " + DateOfCessationOfAgency + ",ReasonForCessationOfAgency : " + ReasonForCessationOfAgency +
                //       "Category : " + Category + ",ICDate : " + ICDate;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SAPcode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();

                SeqNo = ServerTrnReqLog(json, "Set_CompositeTransfer_detail", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@CreateBy", strSapCode);
                htParam.Add("@CndNo", Convert.ToString(jObject["CndNo"]).Trim());
                htParam.Add("@ApplicationNo", strAppNo);
                htParam.Add("@NameofInsurer", Convert.ToString(jObject["InsurerName"]).Trim());
                htParam.Add("@AgencyCodeNumber", Convert.ToString(jObject["AgencyCodeNo"]).Trim());
                htParam.Add("@DateOfAppointmentAsAgent", Convert.ToString(jObject["AppointmentDate"]).Trim());
                htParam.Add("@DateOfCessationOfAgency", Convert.ToString(jObject["CessationDate"]).Trim());
                htParam.Add("@ReasonForCessationOfAgency", Convert.ToString(jObject["CessationReason"]).Trim());
                htParam.Add("@Category", Convert.ToString(jObject["Category"]).Trim());
                htParam.Add("@ICDate", Convert.ToString(jObject["ICdate"]).Trim());
                htParam.Add("@Status", Convert.ToString(jObject["Status"]).Trim());
                htParam.Add("@IsActive", Convert.ToString(jObject["IsActive"]).Trim());
                // htParam.Add("@ActiveCnt", Convert.ToString(jObject["ActiveCnt"]).Trim());
                int cnt = Convert.ToInt16(Convert.ToString(jObject["ActiveCnt"]).Trim());

                dstemp = objDAL.selectFromDatabase("prc_compositedetailsIns_Mob", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                    if (dstemp.Tables[0].Rows[0]["ResponseCode"].ToString().Trim() == "0")
                    {
                        string cndno = dstemp.Tables[0].Rows[0]["CndNo"].ToString().Trim();
                        string act = dstemp.Tables[0].Rows[0]["IsActive"].ToString().Trim();
                        int tcnt = Convert.ToInt16(dstemp.Tables[0].Rows[0]["Cnt"].ToString().Trim());
                        if (!cndno.IsNullOrWhiteSpace() && act == "Y" && cnt == tcnt)
                        {
                            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Candidate Composite RDLC" + DateTime.Now.ToString() + Environment.NewLine);

                            ConvertRDLCToPDF(cndno, strSapCode);

                            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Candidate Composite RDLC end" + DateTime.Now.ToString() + Environment.NewLine);

                        }
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Failed to update details\"}]}";
                }

            }
            catch (Exception ex)

            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/Get_Profile_Prsnldetail")]
        [HttpPost]
        public string Get_Profile_Prsnldetail([FromBody] InputModel model)
        {
            try
            {
                //string AppNo, String SapCode, String strTokenId
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //String param = "AppNo : " + strAppNo;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "Get_Profile_Prsnldetail", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@AppNo", strAppNo.ToString().Trim());
                dstemp = objDAL.selectFromDatabase("Prc_GetCndProfiling_PrsnlDtls_Mob", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/Get_ExamCentre")]
        [HttpPost]
        public string Get_ExamCentre([FromBody] InputModel model)
        {
            //string ExmCentre, String SapCode, String strTokenId
            //String param = "ExmCentre : " + ExmCentre;
            //SeqNo = ServerTrnReqLog(param, "Get_ExamCentre", "", "", "Candidate Manager");
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "Get_ExamCentre", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp = null;
                Hashtable ht = new Hashtable();
                ht.Add("@ExmCentre", "");

                dstemp = objDAL.selectFromDatabase("Prc_GetExmCntrDtls", ht, "IsysARTL");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
                    }
                }
                else
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}"; ;
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/RegisterCandidate_mob")]
        [HttpPost]
        public string RegisterCandidate_mob([FromBody] InputModel model)
        {
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();

                SeqNo = ServerTrnReqLog(json, "RegisterCandidate_mob", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@AppNo", Convert.ToString(jObject["AppNo"]).Trim());
                htParam.Add("@CndNo", Convert.ToString(jObject["CndNo"]).Trim());
                htParam.Add("@SmMemberCode", Convert.ToString(jObject["SmMemberCode"]).Trim());
                htParam.Add("@CarrierCode", Convert.ToString(jObject["CarrierCode"]).Trim());
                htParam.Add("@CndCat", Convert.ToString(jObject["CndCat"]).Trim());
                htParam.Add("@EntryDate", Convert.ToString(jObject["EntryDate"]).Trim());
                htParam.Add("@Title", Convert.ToString(jObject["Title"]).Trim());
                htParam.Add("@GivenName", Convert.ToString(jObject["GivenName"]).Trim());
                htParam.Add("@SurName", Convert.ToString(jObject["SurName"]).Trim());
                htParam.Add("@FatherName", Convert.ToString(jObject["FatherName"]).Trim());
                htParam.Add("@Relation", Convert.ToString(jObject["Relation"]).Trim());
                htParam.Add("@Gender", Convert.ToString(jObject["Gender"]).Trim());
                htParam.Add("@DOB", Convert.ToString(jObject["DOB"]).Trim());
                htParam.Add("@MaritalStat", Convert.ToString(jObject["MaritalStat"]).Trim());
                htParam.Add("@Nationality", Convert.ToString(jObject["Nationality"]).Trim());
                htParam.Add("@CnctType", Convert.ToString(jObject["CnctType"]).Trim());
                htParam.Add("@Address1", Convert.ToString(jObject["Address1"]).Trim());
                htParam.Add("@Address2", Convert.ToString(jObject["Address2"]).Trim());
                htParam.Add("@Address3", Convert.ToString(jObject["Address3"]).Trim());
                htParam.Add("@City", Convert.ToString(jObject["City"]).Trim());
                htParam.Add("@Area", Convert.ToString(jObject["Area"]).Trim());
                htParam.Add("@Village", Convert.ToString(jObject["Village"]).Trim());
                htParam.Add("@District", Convert.ToString(jObject["District"]).Trim());
                htParam.Add("@StateCode", Convert.ToString(jObject["StateCode"]).Trim());
                htParam.Add("@Pincode", Convert.ToString(jObject["Pincode"]).Trim());
                htParam.Add("@CntryCode", Convert.ToString(jObject["CntryCode"]).Trim());
                htParam.Add("@MobileTel", Convert.ToString(jObject["MobileTel"]).Trim());
                htParam.Add("@Email", Convert.ToString(jObject["Email"]).Trim());
                htParam.Add("@Mobile2", Convert.ToString(jObject["Mobile2"]).Trim());
                htParam.Add("@Email2", Convert.ToString(jObject["Email2"]).Trim());
                htParam.Add("@DstbnMethod", Convert.ToString(jObject["DstbnMethod"]).Trim());
                htParam.Add("@PAN", Convert.ToString(jObject["PAN"]).Trim());
                htParam.Add("@AgentType", Convert.ToString(jObject["AgentType"]).Trim());
                htParam.Add("@RecruitAgtCode", Convert.ToString(jObject["RecruitAgtCode"]).Trim());
                htParam.Add("@ProofIdDoc", Convert.ToString(jObject["ProofIdDoc"]).Trim());
                htParam.Add("@CreateBy", Convert.ToString(jObject["CreateBy"]).Trim());
                htParam.Add("@CasteCat", Convert.ToString(jObject["CasteCat"]).Trim());
                htParam.Add("@PrimProf", Convert.ToString(jObject["PrimProf"]).Trim());
                htParam.Add("@BasicQual", Convert.ToString(jObject["BasicQual"]).Trim());
                htParam.Add("@BasicBoardName", Convert.ToString(jObject["BasicBoardName"]).Trim());
                htParam.Add("@BasicRNo", Convert.ToString(jObject["BasicRNo"]).Trim());
                htParam.Add("@BasicYrPass", Convert.ToString(jObject["BasicYrPass"]).Trim());
                htParam.Add("@Bizsrc", Convert.ToString(jObject["Bizsrc"]).Trim());
                htParam.Add("@ChnCls", Convert.ToString(jObject["ChnCls"]).Trim());
                htParam.Add("@CmsUnitCode", Convert.ToString(jObject["CmsUnitCode"]).Trim());
                htParam.Add("@UniqueRefID", Convert.ToString(jObject["UniqueRefID"]).Trim());
                htParam.Add("@AadharNo", Convert.ToString(jObject["AadharNo"]).Trim());
                htParam.Add("@AadharOTP", Convert.ToString(jObject["AadharOTP"]).Trim());
                htParam.Add("@TypeofAadharVerify", Convert.ToString(jObject["TypeofAadharVerify"]).Trim());
                htParam.Add("@Cand_Type", Convert.ToString(jObject["Cand_Type"]).Trim());
                htParam.Add("@FeesWavier", Convert.ToString(jObject["FeesWavier"]).Trim());
                htParam.Add("@AnniversaryDate", Convert.ToString(jObject["AnniversaryDate"]).Trim());
                htParam.Add("@AddRptMgr", Convert.ToString(jObject["AddRptMgr"]).Trim());
                htParam.Add("@ModuleID", Convert.ToString(jObject["ModuleID"]).Trim());
                htParam.Add("@RegRemark", Convert.ToString(jObject["RegRemark"]).Trim());
                htParam.Add("@NomineeName", Convert.ToString(jObject["NomineeName"]).Trim());
                htParam.Add("@NomineeRel", Convert.ToString(jObject["NomineeRel"]).Trim());
                htParam.Add("@NomineeAge", Convert.ToString(jObject["NomineeAge"]).Trim());
                htParam.Add("@NomineePAN", Convert.ToString(jObject["NomineePAN"]).Trim());
                htParam.Add("@NomineeEmail", Convert.ToString(jObject["NomineeEmail"]).Trim());
                htParam.Add("@NomineeContact", Convert.ToString(jObject["NomineeContact"]).Trim());
                htParam.Add("@HigherQual", Convert.ToString(jObject["HigherQual"]).Trim());
                htParam.Add("@ExamMode", Convert.ToString(jObject["ExamMode"]).Trim());
                htParam.Add("@ExmBody", Convert.ToString(jObject["ExmBody"]).Trim());
                htParam.Add("@ExmLang", Convert.ToString(jObject["ExmLang"]).Trim());
                htParam.Add("@ExmCentre", Convert.ToString(jObject["ExmCentre"]).Trim());
                htParam.Add("@SapCode", strSapCode);
                htParam.Add("@SetPrefDt", Convert.ToString(jObject["SetPrefDt"]).Trim());
                htParam.Add("@PAFlag", Convert.ToString(jObject["PAFlag"]).Trim());
                htParam.Add("@WavierType", Convert.ToString(jObject["WavierType"]).Trim());
                //htParam.Add("@RegRemark", Convert.ToString(jObject["RegRemark"]).Trim());
                htParam.Add("@RegReferralCode", Convert.ToString(jObject["RegReferralCode"]).Trim());
                htParam.Add("@WavierCount", Convert.ToString(jObject["WavierCount"]).Trim());
                //added for tab based exam centre by chandrashekhar
                htParam.Add("@RegStDt", Convert.ToString(jObject["RegStDt"]).Trim());
                htParam.Add("@RegEdDt", Convert.ToString(jObject["RegEdDt"]).Trim());

                // dstemp = objDAL.selectFromDatabase("prc_CndAdmin_InsCnd1_mobile", htParam, "IsysARTL");

                //dstemp = objDAL.selectFromDatabase("prc_CndAdmin_InsCnd1_Androidmobile", htParam, "IsysARTL");
                dstemp = objDAL.selectFromDatabase("Prc_InsRgstrDtls_Mob", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0)//&& dstemp.Tables[0].Rows.Count > 0 CndStatus
                {
                    JsonResponce = ds2json(dstemp);
                    if (dstemp.Tables[0].Rows[0]["ResponseCode"].ToString().Trim() == "0")
                    {
                        string cndno = dstemp.Tables[0].Rows[0]["CndNo"].ToString().Trim();
                        string cndtype = dstemp.Tables[0].Rows[0]["Cand_Type"].ToString().Trim();
                        if (!cndno.IsNullOrWhiteSpace() && cndtype == "F")
                        {
                            ConvertRDLCToPDF(cndno, strSapCode);
                        }
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Registration failed\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/SubmitCNDDocumentRequest")]
        [HttpPost]
        public string SubmitCNDDocumentRequest([FromBody] InputModel model)
        {
            try
            {
                //String MemCode, String MemType, String CreatedBy, String strTokenId, String EntityType, String GSTNAFlag, String TNCFlag
                //string strMemCode = encryptDecrypt.DecodeAndDecrypt(MemCode);
                ////string strMemType = encryptDecrypt.DecodeAndDecrypt(MemType);
                //string strCreatedBy = encryptDecrypt.DecodeAndDecrypt(CreatedBy);

                //String param = "MemCode : " + strMemCode + ", MemType : " + MemType + ", CreatedBy : " + strCreatedBy
                //    + ", EntityType:" + EntityType + ", GSTNAFlag:" + GSTNAFlag + ", TNCFlag:" + TNCFlag;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();

                SeqNo = ServerTrnReqLog(json, "SubmitCNDDocumentRequest", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@AppNo", strAppNo);
                htParam.Add("@CandType", Convert.ToString(jObject["CandType"]).Trim());
                htParam.Add("@SapCode", strSapCode);
                htParam.Add("@ModuleID", Convert.ToString(jObject["ModuleID"]).Trim());
                dstemp = objDAL.selectFromDatabase("Prc_InsCandDocRequest_Mob", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                    try
                    {
                        if (dstemp.Tables[0].Rows[0]["ResponseCode"].ToString().Trim() == "0")
                        {
                            string cndno = dstemp.Tables[0].Rows[0]["CndNo"].ToString().Trim();
                            if (!cndno.IsNullOrWhiteSpace())
                            {
                                ConvertRDLCToPDF(cndno, strSapCode);
                            }
                        }
                    }
                    catch (Exception es)
                    {
                        es.ToString();
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;

        }

        [Route("api/CBCMS/CandidateModification_mob")]
        [HttpPost]
        public string CandidateModification_mob([FromBody] InputModel model)
        {
            try
            {
                //[FromBody] InputModel model
                //string jsonInput = "J4llsrvEDEK3Pgp3nkv93pFkOg981fKr7XACkyUj9ZP42wNItwVh0s7QCIQ9pj2O42sNZL4rQ90o\nVjwoJu5gn2CViyQ2nB0v77otyXIl9vbP4xqHmEOWjyF46Dn1y/qf+Kbgft8IWbBu9aV9H8irs9qY\nuU6XMDgmKLPpp88l61fU2JtlSThzi7iWnD6UCmJRXRYrGQq4pDcHlAuo1edoGxcUUU1e0gv87iIB\ngh8bYxe81uMYkUcMCs20uqCPo7ZOAaOSoQOEp8zO4t4vLQpe7XWyPnVlG+zS2VI0qvyhcFuDoQqY\n95FfF5LwjqBZQSTyzBVYFi6zT35pecUVz3odEiU3qEMZ6766zpbqUqhUJjSaIJWBFrm+aIrOgFgE\nkN3qrARv7NMNlrZ46Sefqopi9ivphZLzT/t/VamtP0aIZWqvK7e6tSk8Q9xJo8dRKd4vYidwAGUm\nCbayd3Koxi/BXT36ttfXJRD7ovOrex2I+al14EyQ9LcZBe4YnOxSSetxrH7vKaXfR3bWZKjmsN2T\n2nO/n+DYf5Uu45k9uhJArc2M0trR+Pp/dg4U1ltyCe5mscLtbZVfp/GbjeBnwxrMOvJ7Mj9Syx3z\nzeUUdZMZkyokQj+gLKLuBLN1Oa5y3Tjvu3lawdalXbAUkWryPjuDrDEqX30xzhkwLoLBBFJskGZE\nc2D6tBq3TkhQEq/WNHXTHLa9OWw2/XHevIVcX+1ttCp9OX6lldoLEBI1zkbiHm5isl41oERztleF\nTrLqZ9tVIckcJt+qXtDjbtINwdgt0ihVsCy0e6qvF2P4juFtr/YcQEd3wSIo/OiM6zbPzkXmY+qF\nCrf6SoMrHZu0nuZw+OsmKrBPfZkpsrQBoZdkQjYLGA++9nLLp/rfw75MvFhBnnH7WPh1Pz3MUoSc\n9pO+iYpiiTU7VwmvUoVc8mjXJWfmPL82D9/EdJcfGoAa+406sSY0TuPVYfO8/wSuKFtVFsGVFSAs\nBndFMDxfOzb+zQFpPxATI0J5qSzfb2vP0miBCOkr+jzvNuJ5gv4hH2LWU92JbpminMcxhTh4dU5o\npZyfbhjRcAuiPy2Wzoktff2aKgMn2Pp68pGITUsFDDo7Hp1v94WHyrYAztbEtk6ZzpbtEe1xRMBw\nA9OcErXhFUeA8n7fvqO/M4nJJsJBsRMAfj+GzLSr5XrPgRtOWIBF3WAtyDtroQcVQBj0y9knbPJP\nd5XpCegLOCTjKMEPmY4pug+H7CoGFR8hoCCuRyYB6bqsE/q4RpOFSQgo1V8iqlti59KwWnfUA4tb\n/qfG4sWFHoZYZhxpQEfx/XWdIInMIDOhC91392aHCWTq+9+9jK0rlprOsS1ca59+jbPwNg3BXplr\nOg3z8ofEjWl+VW4tY3i36WDXo6EoMfv/JFNLKeyiS7d8X/6nhZNG/EjkKKJiwE8AYB8YFgm2gGMm\ncLVyqhdg2kGApibdCCPmScd4d3T/LB888xGxNYJRB/n0QpJqahFF6/KFVdUTpWnwMK/EsTaxbdkq\n7lA24IiKbbdMStKejBGqOeNxeI1n7nRta1SI8gBLty8gWYmdXfwuH2YE4zVAG5X7uJo3ggsBN97/\nXOcJ7NePSfRoi9vxgqVpMRV4KrZrAgy5wZZueR2CHx0vev+1atZz2MJdJR23oA9+7Z0m+CZ9Hhe/\nnG9rjOqFdp5pvobwbYG4QOV5mmht+418r+Du/qDXtkmqS5PKgf4hsjnMrRwqYJp3m2upuFKPRFfY\nSND07jjzsabIETQqnYs49pQ3QYW53YbWzZJvcH2cVNi0WhmW0kYsqOdyqGcmVCUlA4C5P46Lomru\nflDyAe21BnaBIa0Wh0ek77WLuxtGq05vtDP8iTWPrqKOsmUs/j0/trlBQuP/oaa7dLFQVIjfMIR0\nN77jvE9aHmSS8IpS7IuX49wQb3BMp+dqhg7D2an82tiJ4lbhkKQEc9gGtmTJw9skQwaFqW3kBHwz\nKZ8HSRp3GCbubapHGhSfxJ7tpUay0Q==\n";
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                //string json = "{\"AppNo\":\"10038079\",\"CndNo\":\"\",\"SmMemberCode\":\"20011205\",\"CarrierCode\":\"2\",\"CndCat\":\"\",\"EntryDate\":\"06/10/2023\",\"Title\":\"MR\",\"GivenName\":\"FAHAD \",\"SurName\":\"\",\"Gender\":\"M\",\"FatherName\":\"HAH\",\"Relation\":\"\",\"DOB\":\"01/01/1989\",\"MaritalStat\":\"\",\"Nationality\":\"\",\"MobileTel\":\"8426543456\",\"Email\":\"hregsihsukkz8@mnil.up\",\"Mobile2\":\"\",\"Email2\":\"\",\"DstbnMethod\":\"\",\"PAN\":\"YESPL6749S\",\"AgentType\":\"SM\",\"CndAgtType\":\"IS\",\"RecruitAgtCode\":\"\",\"ProofIdDoc\":\"MBA\",\"CreateBy\":\"RGI32189\",\"CasteCat\":\"SC\",\"PrimProf\":\"SLP\",\"BasicQual\":\"HSC\",\"BasicBoardName\":\"BANI\",\"BasicRNo\":\"BABAB\",\"BasicYrPass\":\"06/10/2023\",\"Bizsrc\":\"SE\",\"ChnCls\":\"ACSO\",\"CmsUnitCode\":\"7071\",\"UniqueRefID\":\"Android-22ddcf2f-c7e3-4a80-a4e8-88402b87e7fb\",\"AadharNo\":\"\",\"AadharOTP\":\"\",\"TypeofAadharVerify\":\"\",\"Cand_Type\":\"F\",\"FeesWavier\":\"Y\",\"AnniversaryDate\":\"\",\"AddRptMgr\":\"\",\"RegRemark\":\"\",\"NomineeName\":\"\",\"NomineeRel\":\"\",\"NomineeAge\":\"\",\"NomineePAN\":\"\",\"NomineeEmail\":\"\",\"NomineeContact\":\"\",\"HigherQual\":\"MBA\",\"ExamMode\":\"ONLINE\",\"ExmBody\":\"Insurance Institute of India\",\"ExmLang\":\"MLM\",\"ExmCentre\":\"NSEIT Limited - Akola\",\"SetPrefDt\":\"7/10/2023\",\"PAFlag\":\"N\",\"WavierType\":\"GEN\",\"CnctType\":\"P1\",\"Address1\":\"JBA\",\"Address2\":\"\",\"Address3\":\"\",\"City\":\"THANE\",\"Area\":\"KALWA \",\"AreaID\":\"45574\",\"Village\":\"\",\"District\":\"THANE\",\"DistrictID\":\"336\",\"StateCode\":\"21\",\"StateID\":\"21\",\"Pincode\":\"400605\",\"StateName\":\"MAHARASHTRA\",\"CntryCode\":\"\",\"ModuleID\":\"8017\",\"SapCode\":\"RGI32189\",\"strTokenId\":\"7519BDF7-3B77-45D1-9A51-2C776500842A\",\"CallerId\":\"CBCMSCM\",\"CallerPass\":\"CBCMSCM\"}";
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();

                SeqNo = ServerTrnReqLog(json, "CandidateModification_mob", "", strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();
                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@AppNo", strAppNo);
                htParam.Add("@CndNo", Convert.ToString(jObject["CndNo"]).Trim());
                htParam.Add("@SmMemberCode", Convert.ToString(jObject["SmMemberCode"]).Trim());
                htParam.Add("@CarrierCode", Convert.ToString(jObject["CarrierCode"]).Trim());
                htParam.Add("@CndCat", Convert.ToString(jObject["CndCat"]).Trim());
                htParam.Add("@EntryDate", Convert.ToString(jObject["EntryDate"]).Trim());
                htParam.Add("@Title", Convert.ToString(jObject["Title"]).Trim());
                htParam.Add("@GivenName", Convert.ToString(jObject["GivenName"]).Trim());
                htParam.Add("@SurName", Convert.ToString(jObject["SurName"]).Trim());
                htParam.Add("@FatherName", Convert.ToString(jObject["FatherName"]).Trim());
                htParam.Add("@Relation", Convert.ToString(jObject["Relation"]).Trim());
                htParam.Add("@Gender", Convert.ToString(jObject["Gender"]).Trim());
                htParam.Add("@DOB", Convert.ToString(jObject["DOB"]).Trim());
                htParam.Add("@MaritalStat", Convert.ToString(jObject["MaritalStat"]).Trim());
                htParam.Add("@Nationality", Convert.ToString(jObject["Nationality"]).Trim());
                htParam.Add("@CnctType", Convert.ToString(jObject["CnctType"]).Trim());
                htParam.Add("@Address1", Convert.ToString(jObject["Address1"]).Trim());
                htParam.Add("@Address2", Convert.ToString(jObject["Address2"]).Trim());
                htParam.Add("@Address3", Convert.ToString(jObject["Address3"]).Trim());
                htParam.Add("@City", Convert.ToString(jObject["City"]).Trim());
                htParam.Add("@Area", Convert.ToString(jObject["Area"]).Trim());
                htParam.Add("@Village", Convert.ToString(jObject["Village"]).Trim());
                htParam.Add("@District", Convert.ToString(jObject["District"]).Trim());
                htParam.Add("@StateCode", Convert.ToString(jObject["StateCode"]).Trim());
                htParam.Add("@Pincode", Convert.ToString(jObject["Pincode"]).Trim());
                htParam.Add("@CntryCode", Convert.ToString(jObject["CntryCode"]).Trim());
                htParam.Add("@MobileTel", Convert.ToString(jObject["MobileTel"]).Trim());
                htParam.Add("@Email", Convert.ToString(jObject["Email"]).Trim());
                htParam.Add("@Mobile2", Convert.ToString(jObject["Mobile2"]).Trim());
                htParam.Add("@Email2", Convert.ToString(jObject["Email2"]).Trim());
                htParam.Add("@DstbnMethod", Convert.ToString(jObject["DstbnMethod"]).Trim());
                htParam.Add("@PAN", Convert.ToString(jObject["PAN"]).Trim());
                htParam.Add("@AgentType", Convert.ToString(jObject["AgentType"]).Trim());
                htParam.Add("@RecruitAgtCode", Convert.ToString(jObject["RecruitAgtCode"]).Trim());
                htParam.Add("@ProofIdDoc", Convert.ToString(jObject["ProofIdDoc"]).Trim());
                htParam.Add("@CreateBy", Convert.ToString(jObject["CreateBy"]).Trim());
                htParam.Add("@CasteCat", Convert.ToString(jObject["CasteCat"]).Trim());
                htParam.Add("@PrimProf", Convert.ToString(jObject["PrimProf"]).Trim());
                htParam.Add("@BasicQual", Convert.ToString(jObject["BasicQual"]).Trim());
                htParam.Add("@BasicBoardName", Convert.ToString(jObject["BasicBoardName"]).Trim());
                htParam.Add("@BasicRNo", Convert.ToString(jObject["BasicRNo"]).Trim());
                htParam.Add("@BasicYrPass", Convert.ToString(jObject["BasicYrPass"]).Trim());
                htParam.Add("@Bizsrc", Convert.ToString(jObject["Bizsrc"]).Trim());
                htParam.Add("@ChnCls", Convert.ToString(jObject["ChnCls"]).Trim());
                htParam.Add("@CmsUnitCode", Convert.ToString(jObject["CmsUnitCode"]).Trim());
                htParam.Add("@UniqueRefID", Convert.ToString(jObject["UniqueRefID"]).Trim());
                htParam.Add("@AadharNo", Convert.ToString(jObject["AadharNo"]).Trim());
                htParam.Add("@AadharOTP", Convert.ToString(jObject["AadharOTP"]).Trim());
                htParam.Add("@TypeofAadharVerify", Convert.ToString(jObject["TypeofAadharVerify"]).Trim());
                htParam.Add("@Cand_Type", Convert.ToString(jObject["Cand_Type"]).Trim());
                htParam.Add("@FeesWavier", Convert.ToString(jObject["FeesWavier"]).Trim());
                htParam.Add("@AnniversaryDate", Convert.ToString(jObject["AnniversaryDate"]).Trim());
                htParam.Add("@AddRptMgr", Convert.ToString(jObject["AddRptMgr"]).Trim());
                htParam.Add("@ModuleID", Convert.ToString(jObject["ModuleID"]).Trim());
                htParam.Add("@RegRemark", Convert.ToString(jObject["RegRemark"]).Trim());
                htParam.Add("@NomineeName", Convert.ToString(jObject["NomineeName"]).Trim());
                htParam.Add("@NomineeRel", Convert.ToString(jObject["NomineeRel"]).Trim());
                htParam.Add("@NomineeAge", Convert.ToString(jObject["NomineeAge"]).Trim());
                htParam.Add("@NomineePAN", Convert.ToString(jObject["NomineePAN"]).Trim());
                htParam.Add("@NomineeEmail", Convert.ToString(jObject["NomineeEmail"]).Trim());
                htParam.Add("@NomineeContact", Convert.ToString(jObject["NomineeContact"]).Trim());
                htParam.Add("@HigherQual", Convert.ToString(jObject["HigherQual"]).Trim());
                htParam.Add("@ExamMode", Convert.ToString(jObject["ExamMode"]).Trim());
                htParam.Add("@ExmBody", Convert.ToString(jObject["ExmBody"]).Trim());
                htParam.Add("@ExmLang", Convert.ToString(jObject["ExmLang"]).Trim());
                htParam.Add("@ExmCentre", Convert.ToString(jObject["ExmCentre"]).Trim());
                htParam.Add("@SapCode", strSapCode);
                htParam.Add("@SetPrefDt", Convert.ToString(jObject["SetPrefDt"]).Trim());
                htParam.Add("@PAFlag", Convert.ToString(jObject["PAFlag"]).Trim());
                htParam.Add("@WavierType", Convert.ToString(jObject["WavierType"]).Trim());
                //htParam.Add("@RegRemark", Convert.ToString(jObject["RegRemark"]).Trim());
                htParam.Add("@RegReferralCode", Convert.ToString(jObject["RegReferralCode"]).Trim());
                htParam.Add("@WavierCount", Convert.ToString(jObject["WavierCount"]).Trim());
                //added for tab based exam centre by chandrashekhar
                htParam.Add("@RegStDt", Convert.ToString(jObject["RegStDt"]).Trim());
                htParam.Add("@RegEdDt", Convert.ToString(jObject["RegEdDt"]).Trim());
                //dstemp = objDAL.selectFromDatabase("prc_CndAdmin_UpdtCndRcrd_mobile", htParam, "IsysARTL");
                dstemp = objDAL.selectFromDatabase("Prc_UpdRegDtls_Mob", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                    try
                    {
                        //JsonResponce = ds2json(dstemp);
                        if (dstemp.Tables[0].Rows[0]["ResponseCode"].ToString().Trim() == "0")
                        {
                            string cndno = dstemp.Tables[0].Rows[0]["CndNo"].ToString().Trim();
                            string PAFlag = dstemp.Tables[0].Rows[0]["PAFlag"].ToString().Trim();
                            string panNo = Convert.ToString(jObject["PAN"]).Trim();
                            string cndtype = dstemp.Tables[0].Rows[0]["Cand_Type"].ToString().Trim();

                            if (!cndno.IsNullOrWhiteSpace())
                            {
                                //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Candidate Modification RDLC" + DateTime.Now.ToString() + Environment.NewLine);
                                if (cndtype == "F" || cndtype == "P")
                                {
                                    //if(PAFlag != "Y")
                                    //{
                                    ConvertRDLCToPDF(cndno, strSapCode);
                                    //}
                                }

                                //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Candidate Modification RDLC end" + DateTime.Now.ToString() + Environment.NewLine);

                                if (PAFlag == "Y")
                                {
                                    htParam.Clear();
                                    dstemp.Clear();
                                    htParam.Add("@PAN", panNo);
                                    dstemp = objDAL.selectFromDatabase("Prc_getCndDtls", htParam, "IsysARTL");
                                    if (dstemp.Tables[0].Rows.Count > 0)
                                    {
                                        string cnd_no = dstemp.Tables[0].Rows[0]["CndNo"].ToString().Trim();
                                        if (!cnd_no.IsNullOrWhiteSpace())
                                        {
                                            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "PA Candidate Modification RDLC" + DateTime.Now.ToString() + Environment.NewLine);

                                            ConvertRDLCToPDF(cnd_no, strSapCode);

                                            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "PA Candidate Modification RDLC end" + DateTime.Now.ToString() + Environment.NewLine);

                                        }
                                    }
                                    //ConvertRDLCToPDF(cndno, strSapCode);

                                }
                            }
                        }
                    }
                    catch (Exception es)
                    {
                        es.ToString();
                    }
                }
                else
                {
                    JsonResponce = "No Memcode found for your SAPCode.";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/SubmitBankDetails")]
        [HttpPost]
        public string SubmitBankDetails([FromBody] InputModel model)
        {
            //    string AppNo, string ExmMode, string ExmLang, string ExmCentre, string ExmBody, string UpdatedBy
            //, String SapCode, String strTokenId
            //    String param = "AppNo : " + AppNo + ",ExmMode : " + ExmMode + "ExmLang : " + ExmLang + "ExmCentre : " + ExmCentre + "ExmBody : " + ExmBody + "UpdatedBy : " + UpdatedBy;            
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["CreatedBy"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "SubmitBankDetails", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp = null;
                Hashtable htParam = new Hashtable();
                htParam.Add("@AppNo", Convert.ToString(jObject["AppNo"]).Trim());
                htParam.Add("@CndNo", Convert.ToString(jObject["CndNo"]).Trim());
                htParam.Add("@AccNo", Convert.ToString(jObject["AccNo"]).Trim());
                htParam.Add("@AccType", Convert.ToString(jObject["AccType"]).Trim());
                htParam.Add("@AccHolderName", Convert.ToString(jObject["AccHolderName"]).Trim());
                htParam.Add("@BankName", Convert.ToString(jObject["BankName"]).Trim());
                htParam.Add("@BankBranch", Convert.ToString(jObject["BankBranch"]).Trim());
                htParam.Add("@IFSCCode", Convert.ToString(jObject["IFSCCode"]).Trim());
                htParam.Add("@MICRCode", Convert.ToString(jObject["MICRCode"]).Trim());
                htParam.Add("@BDFor", Convert.ToString(jObject["BDFor"]).Trim());
                htParam.Add("@CreatedBy", Convert.ToString(jObject["CreatedBy"]).Trim());
                htParam.Add("@CreatedDate", Convert.ToString(jObject["CreatedDate"]).Trim());
                htParam.Add("@IsActive", Convert.ToString(jObject["IsActive"]).Trim());
                htParam.Add("@RegRemark", Convert.ToString(jObject["RegRemark"]).Trim());
                htParam.Add("@UniqueRefID", Convert.ToString(jObject["UniqueRefID"]).Trim());


                dstemp = objDAL.selectFromDatabase("Prc_UpdateBankDetails_Mob", htParam, "IsysARTL");
                JsonResponce = "Updated Successfully";
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/SubmitCandContact")]
        [HttpPost]
        public string SubmitCandContact([FromBody] InputModel model)
        {
            //    string AppNo, string ExmMode, string ExmLang, string ExmCentre, string ExmBody, string UpdatedBy
            //, String SapCode, String strTokenId
            //    String param = "AppNo : " + AppNo + ",ExmMode : " + ExmMode + "ExmLang : " + ExmLang + "ExmCentre : " + ExmCentre + "ExmBody : " + ExmBody + "UpdatedBy : " + UpdatedBy;            
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "SubmitCandContact", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp = null;
                Hashtable htable = new Hashtable();
                htable.Add("@SapCode", strSapCode);
                htable.Add("@AppNo", strAppNo);
                htable.Add("@MobileNo", Convert.ToString(jObject["MobileNo"]).Trim());
                htable.Add("@EmailID", Convert.ToString(jObject["EmailID"]).Trim());
                htable.Add("@IsPrimaryNo", Convert.ToString(jObject["IsPrimaryNo"]).Trim());
                htable.Add("@IsPrimaryEmail", Convert.ToString(jObject["IsPrimaryEmail"]).Trim());
                htable.Add("@UniqRefId", Convert.ToString(jObject["UniqueRefID"]).Trim());

                dstemp = objDAL.selectFromDatabase("PrcInsCndCntDtls", htable, "IsysARTL");
                JsonResponce = "Updated Successfully";
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetCandContact")]
        [HttpPost]
        public string GetCandContact([FromBody] InputModel model)
        {
            //    string AppNo, string ExmMode, string ExmLang, string ExmCentre, string ExmBody, string UpdatedBy
            //, String SapCode, String strTokenId
            //    String param = "AppNo : " + AppNo + ",ExmMode : " + ExmMode + "ExmLang : " + ExmLang + "ExmCentre : " + ExmCentre + "ExmBody : " + ExmBody + "UpdatedBy : " + UpdatedBy;            
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetCandContact", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp = null;
                Hashtable htable = new Hashtable();
                htable.Add("@SapCode", Convert.ToString(jObject["SapCode"]).Trim());
                htable.Add("@AppNo", Convert.ToString(jObject["AppNo"]).Trim());
                htable.Add("@MobileNo", Convert.ToString(jObject["MobileNo"]).Trim());
                htable.Add("@EmailID", Convert.ToString(jObject["EmailID"]).Trim());
                htable.Add("@IsPrimary", Convert.ToString(jObject["IsPrimaryNo"]).Trim());
                htable.Add("@IsEmailPrimary", Convert.ToString(jObject["IsPrimaryEmail"]).Trim());
                htable.Add("@UniqRefId", Convert.ToString(jObject["UniqueRefID"]).Trim());

                dstemp = objDAL.selectFromDatabase("PrcGetCandContact", htable, "IsysARTL");
                JsonResponce = "Updated Successfully";
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/MarkPrimaryContact")]
        [HttpPost]
        public string MarkPrimaryContact([FromBody] InputModel model)
        {
            //    string AppNo, string ExmMode, string ExmLang, string ExmCentre, string ExmBody, string UpdatedBy
            //, String SapCode, String strTokenId
            //    String param = "AppNo : " + AppNo + ",ExmMode : " + ExmMode + "ExmLang : " + ExmLang + "ExmCentre : " + ExmCentre + "ExmBody : " + ExmBody + "UpdatedBy : " + UpdatedBy;            
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "MarkPrimaryContact", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp = null;
                Hashtable htable = new Hashtable();
                htable.Add("@SapCode", Convert.ToString(jObject["SapCode"]).Trim());
                htable.Add("@AppNo", Convert.ToString(jObject["AppNo"]).Trim());
                htable.Add("@MobileNo", Convert.ToString(jObject["MobileNo"]).Trim());
                htable.Add("@EmailID", Convert.ToString(jObject["EmailID"]).Trim());
                htable.Add("@IsPrimary", Convert.ToString(jObject["IsPrimaryNo"]).Trim());
                htable.Add("@IsEmailPrimary", Convert.ToString(jObject["IsPrimaryEmail"]).Trim());

                dstemp = objDAL.selectFromDatabase("Prc_UpdCustCnct", htable, "IsysARTL");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No records updated\"}]}";
                    }
                }
                else
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No records updated\"}]}";
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetLookUpParams")]
        [HttpPost]
        public String GetLookUpParams([FromBody] InputModel model)
        {
            //String SapCode, String strTokenId
            //String param = "SapCode : " + encryptDecrypt.DecodeAndDecrypt(SapCode) + ", strTokenId : " + strTokenId;
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "GetLookUpParams", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp = null;
                Hashtable ht = new Hashtable();
                ht.Add("@SapCode", strSapCode);

                dstemp = objDAL.selectFromDatabase("Prc_GetLookUpParams", ht, "IsysARTL");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "No Records found.";
                    }
                }
                else
                    JsonResponce = "ds is null";
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/Get_SupportDocs")]
        [HttpPost]
        public string Get_SupportDocs([FromBody] InputModel model)
        {
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                //string AppNo, String SapCode, String strTokenId
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //String param = "AppNo : " + strAppNo;
                SeqNo = ServerTrnReqLog(json, "Get_SupportDocs", "", strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp = null;
                Hashtable ht = new Hashtable();
                ht.Add("@Appno", strAppNo.ToString().Trim());
                ht.Add("@CandType", Convert.ToString(jObject["CandType"]).Trim());

                dstemp = objDAL.selectFromDatabase("Prc_GetCand_SupportDocs", ht, "IsysARTL");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
                    }
                }
                else
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/VerifyContactDetails")]
        [HttpPost]
        public string VerifyContactDetails([FromBody] InputModel model)
        {
            try
            {
                //string Email, string Mobile, String SapCode, String strTokenId
                //string strEmail = encryptDecrypt.DecodeAndDecrypt(Email);
                //string strMobile = encryptDecrypt.DecodeAndDecrypt(Mobile);
                //String param = "Email : " + strEmail + ",Mobile : " + strMobile;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "VerifyContactDetails", strSapCode, "", "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@Email", Convert.ToString(jObject["Email"]).Trim());
                htParam.Add("@Mobile", Convert.ToString(jObject["Mobile"]).Trim());
                dstemp = objDAL.selectFromDatabase("Prc_GetQualCheckverifyEmail_mobile", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/Get_RecruitersChannel_Mob")]
        [HttpPost]
        public String Get_RecruitersChannel_Mob([FromBody] InputModel model)
        {
            //String Chncls, String Bizsrc, String MemType, String SapCode, String strTokenId
            //String param = "Chncls : " + Chncls + ",Bizsrc : " + Bizsrc;
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "Get_RecruitersChannel_Mob", "", "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp = null;
                Hashtable ht = new Hashtable();
                ht.Add("@Chncls", Convert.ToString(jObject["Chncls"]).Trim());
                ht.Add("@Bizsrc", Convert.ToString(jObject["Bizsrc"]).Trim());
                ht.Add("@MemType", Convert.ToString(jObject["MemType"]).Trim());
                dstemp = objDAL.selectFromDatabase("Prc_getrecruitslschannel_Mob", ht, "IsysARTL");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
                    }
                }
                else
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetAdditionalReportingManager")]
        [HttpPost]
        public String GetAdditionalReportingManager([FromBody] InputModel model)
        {
            //String mCode, String mType, String SapCode, String strTokenId
            //String param = "MemCode : " + mCode + ",Memtype : " + mType;

            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetAdditionalReportingManager", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp = null;
                Hashtable ht = new Hashtable();
                ht.Add("@MemberCode", Convert.ToString(jObject["mCode"]).Trim());
                ht.Add("@MemberType", Convert.ToString(jObject["mType"]).Trim());

                dstemp = objDAL.selectFromDatabase("Prc_GetAddRptMgrCodeDtls", ht, "IsysARTL");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
                    }
                }
                else
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetTeamView")]
        [HttpPost]
        public string GetTeamView([FromBody] InputModel model)
        {
            try
            {
                //String Empcode, String SapCode, String strTokenId
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "GetTeamView", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                htParam.Clear();
                dstemp.Clear();
                htParam.Add("@Empcode", Convert.ToString(jObject["Empcode"]).Trim());
                //htParam.Add("@Chncls", Chncls.ToString().Trim());
                //htParam.Add("@Bizsrc", Bizsrc.ToString().Trim());
                dstemp = objDAL.selectFromDatabase("prc_GetTeamViewData", htParam, "ISysCHMS");

                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                    //return encryptDecrypt.EncryptAndEncode(ds2json(dstemp));
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
                }
            }
            catch (Exception e)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + e.ToString() + "\"}]}";
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;

        }

        [Route("api/CBCMS/GetDataForSearchPOSP")]
        [HttpPost]
        public string GetDataForSearchPOSP([FromBody] InputModel model)
        {
            try
            {
                //string SapCode, string AppNo, string CndName, string PanNo, string Flag, string EntryDate, String strTokenId
                //string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //string strPanNo = encryptDecrypt.DecodeAndDecrypt(PanNo);
                //String param = "AppNo : " + strAppNo + ",SapCode : " + strSapCode + ",CndName : " + CndName + ",PanNo : " + strPanNo + ",Flag : " + Flag + ",EntryDate : " + EntryDate;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetDataForSearchPOSP", strSapCode, strAppNo, "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();
                }

                dstemp.Clear();
                Hashtable htable = new Hashtable();
                htable.Clear();
                if (Convert.ToString(jObject["Flag"]).Trim() == "ARS")
                {
                    //for agent survey	
                    htable.Add("@CndNo", string.Empty);
                    htable.Add("@Appno", strAppNo);
                    htable.Add("@GivenName", Convert.ToString(jObject["CndName"]).Trim());
                    htable.Add("@Surname", string.Empty);
                    htable.Add("@PAN", Convert.ToString(jObject["PanNo"]).Trim());
                    htable.Add("@AgentBrokeCode", string.Empty);
                    htable.Add("@CreateFrmDtim", Convert.ToString(jObject["EntryDate"]).Trim());
                    htable.Add("@CreateToDtim", string.Empty);
                    dstemp = objDAL.selectFromDatabase("prc_GetAgentCodeDetails", htable, "ISysCHMS");
                }
                else
                {
                    htable.Add("@BMCode", strSapCode);
                    htable.Add("@AppNo", strAppNo);
                    htable.Add("@CndName", Convert.ToString(jObject["CndName"]).Trim());
                    htable.Add("@PanNo", Convert.ToString(jObject["PanNo"]).Trim());
                    htable.Add("@Flag", Convert.ToString(jObject["Flag"]).Trim());
                    htable.Add("@EntryDate", Convert.ToString(jObject["EntryDate"]).Trim());
                    dstemp = objDAL.selectFromDatabase("Prc_GetSearchDataForPOSP_Mob", htable, "ISysARTL");
                }
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";

            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetCndRegistrationBasicDetails")]
        [HttpPost]
        public string GetCndRegistrationBasicDetails([FromBody] InputModel model)
        {
            try
            {
                //String AppNo, String SapCode, String strTokenId
                //string strAppno = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //String param = "AppNo : " + strAppno + "";
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetCndRegistrationBasicDetails", strSapCode, strAppNo, "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@Appno", strSapCode);
                //htParam.Add("@Flag", Flag.ToString().Trim());	
                dstemp = objDAL.selectFromDatabase("prc_getCndRegistrationDetails_mob", htParam, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/InsProfilingDetailsFrmMob")]
        [HttpPost]
        public string InsProfilingDetailsFrmMob([FromBody] InputModel model)
        {
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();

                SeqNo = ServerTrnReqLog(json, "InsProfilingDetailsFrmMob", strSapCode, strAppNo, "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();
                }
                dstemp = null;
                //Hashtable ht = new Hashtable();	
                htParam.Clear();
                htParam.Add("AppNo", Convert.ToString(jObject["AppNo"]).Trim());
                htParam.Add("CndNo", Convert.ToString(jObject["CndNo"]).Trim());
                //htParam.Add("SMTitle", Convert.ToString(jObject["SMTitle"]).Trim());
                htParam.Add("SMName", Convert.ToString(jObject["SMName"]).Trim());
                htParam.Add("SapCode", Convert.ToString(jObject["SapCode"]).Trim());
                htParam.Add("SMBranchCode", Convert.ToString(jObject["SMBranchCode"]).Trim());
                htParam.Add("AgentType", Convert.ToString(jObject["AgentType"]).Trim());
                htParam.Add("Title", Convert.ToString(jObject["Title"]).Trim());
                htParam.Add("Name", Convert.ToString(jObject["Name"]).Trim());
                htParam.Add("MobileNo", Convert.ToString(jObject["MobileNo"]).Trim());
                htParam.Add("DOB", Convert.ToString(jObject["DOB"]).Trim());
                htParam.Add("PAN", Convert.ToString(jObject["PAN"]).Trim());
                htParam.Add("Qualification", Convert.ToString(jObject["Qualification"]).Trim());
                htParam.Add("IsCnctPrsnSame", Convert.ToString(jObject["IsCnctPrsnSame"]).Trim());
                htParam.Add("CnctPrsnTitle", Convert.ToString(jObject["CnctPrsnTitle"]).Trim());
                htParam.Add("CnctPrsnName", Convert.ToString(jObject["CnctPrsnName"]).Trim());
                htParam.Add("CnctPrsnRelation", Convert.ToString(jObject["CnctPrsnRelation"]).Trim());
                htParam.Add("CnctPrsnMobileNo", Convert.ToString(jObject["CnctPrsnMobileNo"]).Trim());
                htParam.Add("CnctPrsnAddress", Convert.ToString(jObject["CnctPrsnAddress"]).Trim());
                htParam.Add("CnctPrsnEmail", Convert.ToString(jObject["CnctPrsnEmail"]).Trim());
                htParam.Add("Profile", Convert.ToString(jObject["Profile"]).Trim());
                htParam.Add("IsWorkOtrGICompany", Convert.ToString(jObject["IsWorkOtrGICompany"]).Trim());
                htParam.Add("NameOtrGICompany", Convert.ToString(jObject["NameOtrGICompany"]).Trim());
                htParam.Add("IsWorkOtrGICompnyClub", Convert.ToString(jObject["IsWorkOtrGICompnyClub"]).Trim());
                htParam.Add("NameOtrGICompnyClub", Convert.ToString(jObject["NameOtrGICompnyClub"]).Trim());
                htParam.Add("MotorContrbuton", Convert.ToString(jObject["MotorPotential"]).Trim());
                htParam.Add("MotorPotential", Convert.ToString(jObject["MotorPotential"]).Trim());
                htParam.Add("HealthContrbuton", Convert.ToString(jObject["HealthContrbuton"]).Trim());
                htParam.Add("HealthPotential", Convert.ToString(jObject["HealthPotential"]).Trim());
                htParam.Add("CLContrbuton", Convert.ToString(jObject["CLContrbuton"]).Trim());
                htParam.Add("CLPotential", Convert.ToString(jObject["CLPotential"]).Trim());
                htParam.Add("TotalContrbuton", Convert.ToString(jObject["TotalContrbuton"]).Trim());
                htParam.Add("TotalPotential", Convert.ToString(jObject["TotalPotential"]).Trim());
                htParam.Add("CarrierCode", Convert.ToString(jObject["CarrierCode"]).Trim());
                htParam.Add("CreateBy", Convert.ToString(jObject["CreateBy"]).Trim());
                htParam.Add("Remark", Convert.ToString(jObject["Remark"]).Trim());
                htParam.Add("MntrDOB", Convert.ToString(jObject["MntrDOB"]).Trim());
                htParam.Add("MntrMaritalStat", Convert.ToString(jObject["MntrMaritalStat"]).Trim());
                htParam.Add("MntrAnvrsrtDt", Convert.ToString(jObject["MntrAnvrsrtDt"]).Trim());
                htParam.Add("ModuleID", Convert.ToString(jObject["ModuleID"]).Trim());
                dstemp = objDAL.selectFromDatabase("prc_InsProfilingDtls_mob", htParam, "IsysARTL");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        //return ds2json(dstemp);	
                        JsonResponce = ds2json(dstemp); ;
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Profiling failed\"}]}";
                    }
                }
                else
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Profiling failed\"}]}";
            }
            catch (Exception ex)
            {
                JsonResponce = "In Catch block = " + ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetPOSPCfrRaisedCandidate")]
        [HttpPost]
        public string GetPOSPCfrRaisedCandidate([FromBody] InputModel model)
        {
            try
            {
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);

                //String param = "AppNo : " + strAppNo + ",SapCode : " + strSapCode + "CndName : " + CndName + "MemberType : " + MemberType + "EntryDate : " + EntryDate;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetPOSPCfrRaisedCandidate", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();
                }

                dstemp.Clear();
                Hashtable htCFR = new Hashtable();
                htCFR.Add("@BMCode", strSapCode);
                htCFR.Add("@AppNo", strAppNo);
                htCFR.Add("@CndName", Convert.ToString(jObject["CndName"]).Trim());
                htCFR.Add("@MemberType", Convert.ToString(jObject["MemberType"]).Trim());
                htCFR.Add("@EntryDate", Convert.ToString(jObject["EntryDate"]).Trim());

                dstemp = objDAL.selectFromDatabase("Prc_GetAdvDetailsForPOSPCFR_mobile", htCFR, "ISysARTL");
                if (dstemp.Tables.Count > 0)
                {
                    if (dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Records Found\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetDispatchDetails")]
        [HttpPost]
        public string GetDispatchDetails([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            DataSet dsDispatch = new DataSet();

            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                //string AppNo, string CarrierCode;string AppNo, string CarrierCode, String SapCode, String strTokenId
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //String param = "AppNo : " + strAppNo + ",CarrierCode : " + CarrierCode;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                string Agent_Broker_Code = Convert.ToString(jObject["Agent_Broker_Code"]).Trim();

                SeqNo = ServerTrnReqLog(json, "GetDispatchDetails", strSapCode, strAppNo, "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                if (strAppNo != null && strAppNo.Trim() != "")
                {
                    objDaset.Clear();
                    htable.Clear();
                    htable.Add("@AgentCode", Convert.ToString(Agent_Broker_Code));
                    objDaset = objDAL.selectFromDatabase("Prc_GetDispatchDtlsTX", htable, "ISysARTL");
                    if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                    {
                        if (objDaset.Tables[0].Rows[0]["DeliveryStatus"].ToString().Trim() == "Delivered")
                        {
                            JSonString = ds2json(objDaset);
                        }
                        else
                        {
                            dsDispatch = dispatch.GetAgentDispatchDetails(Convert.ToString(Agent_Broker_Code));
                            //objDaset = objDAL.selectFromDatabase("Prc_GetDispatchDtlsTX", htable, "ISysARTL");
                            JSonString = ds2json(dsDispatch);
                        }
                    }
                    else
                    {
                        //JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Data Found.\"}]}";
                        dsDispatch = dispatch.GetAgentDispatchDetails(Convert.ToString(Agent_Broker_Code));
                        //objDaset = objDAL.selectFromDatabase("Prc_GetDispatchDtlsTX", htable, "ISysARTL");
                        JSonString = ds2json(dsDispatch);
                    }
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid api call\"}]}";
                }

            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }

        [Route("api/CBCMS/GetCandidateExam_detail")]
        [HttpPost]
        public string GetCandidateExam_detail([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                //string AppNo, string CarrierCode;string AppNo, string CarrierCode, String SapCode, String strTokenId
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //String param = "AppNo : " + strAppNo + ",CarrierCode : " + CarrierCode;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();

                SeqNo = ServerTrnReqLog(json, "GetCandidateExam_detail", strSapCode, strAppNo, "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                if (strAppNo != null && strAppNo.Trim() != "")
                {
                    objDaset.Clear();
                    htable.Clear();
                    htable.Add("@AppNo", Convert.ToString(strAppNo).Trim());
                    objDaset = objDAL.selectFromDatabase("Prc_GetReExamDetails_mob", htable, "ISysARTL");
                    if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                    {
                        JSonString = ds2json(objDaset);

                    }
                    else
                    {
                        JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Data Found.\"}]}";
                    }
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid api call\"}]}";
                }

            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }

        [Route("api/CBCMS/UpdateCandidateReExam_details")]
        [HttpPost]
        public string UpdateCandidateReExam_details([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                //string AppNo, string CarrierCode;
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //string strCndNo = encryptDecrypt.DecodeAndDecrypt(CndNo);
                //String param = "AppNo : " + strAppNo + ",CndNo : " + CndNo + ",PreffExmDt1 : " + PreffExmDt1 + ", ExmDtApprvr: " + ExmDtApprvr +
                //    ", ExamMode: " + ExamMode + ", ExmBody: " + ExmBody + ", ExmLang: " + ExmLang + ", ExmCentre: " + ExmCentre + ", CreatedBy:" + CreatedBy +
                //    ", SysExmDt:" + SysExmDt + ", PreffExmDt2: " + PreffExmDt2 + ", FeesWavier: " + FeesWavier + ", ModuleID: " + ModuleID;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["CreatedBy"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "UpdateCandidateReExam_details", strSapCode, strAppNo, "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                if (strAppNo != null && strAppNo != "")
                {
                    objDaset.Clear();
                    htable.Clear();

                    htable.Add("@CndNo", Convert.ToString(jObject["CndNo"]).Trim());
                    htable.Add("@AppNo", Convert.ToString(jObject["AppNo"]).Trim());
                    htable.Add("@PreffExmDt1", Convert.ToString(jObject["PreffExmDt1"]).Trim());
                    htable.Add("@ExmDtApprvr", Convert.ToString(jObject["ExmDtApprvr"]).Trim());
                    htable.Add("@ExamMode", Convert.ToString(jObject["ExamMode"]).Trim());
                    htable.Add("@ExmBody", Convert.ToString(jObject["ExmBody"]).Trim());
                    htable.Add("@ExmLang", Convert.ToString(jObject["ExmLang"]).Trim());
                    htable.Add("@ExmCentre", Convert.ToString(jObject["ExmCentre"]).Trim());
                    htable.Add("@CreatedBy", Convert.ToString(jObject["CreatedBy"]).Trim());
                    htable.Add("@SysExmDt", Convert.ToString(jObject["SysExmDt"]).Trim());
                    htable.Add("@PreffExmDt2", Convert.ToString(jObject["PreffExmDt2"]).Trim());
                    if (Convert.ToString(jObject["FeesWavier"]).Trim() != null
                        && Convert.ToString(jObject["FeesWavier"]).Trim() == "Y")
                    {
                        htable.Add("@WaiverFlag", "1");
                        htable.Add("@FeesWaiverFlag", Convert.ToString(jObject["FeesWavier"]).Trim());
                    }
                    else
                    {
                        htable.Add("@WaiverFlag", "");
                        htable.Add("@FeesWaiverFlag", "NA");
                    }
                    //htable.Add("@WavierCount", Convert.ToString(jObject["WavierCount"]).Trim());
                    htable.Add("@ModuleID", Convert.ToString(jObject["ModuleID"]).Trim());

                    objDaset = objDAL.selectFromDatabase("Prc_UpdValidTCCDtls_mob", htable, "IsysARTL");
                    if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                    {
                        JSonString = ds2json(objDaset);
                        if (Convert.ToString(strAppNo).Trim() != "")
                        {
                            // ObjTask.UpdateCndTask(Convert.ToString(jObject["AppNo"]).Trim(), Convert.ToString(jObject["CreatedBy"]).Trim());//To create task in CRM  in 18.12.2017
                        }
                    }
                    else
                    {
                        JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Data Found.\"}]}";
                    }
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid api call\"}]}";
                }

            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }

        [Route("api/CBCMS/GetBankDetails")]
        [HttpPost]
        public string GetBankDetails([FromBody] InputModel model)
        {
            try
            {
                dstemp.Clear();
                htParam.Clear();
                //string PAN, string ApplicationNo, String SapCode, String strTokenId
                //string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());

                //String param = "PAN : " + strPAN + ",ApplicationNo : " + ApplicationNo;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strIFSCCode = Convert.ToString(jObject["IFSCCode"]).Trim();

                SeqNo = ServerTrnReqLog(json, "GetBankDetails", "", "", "Candidate Manager");

                if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                htParam.Clear();
                htParam.Add("@IFSCCode", strIFSCCode);
                dstemp = objDAL.selectFromDatabase("PrcGetBankDetails", htParam, "ISysARTL");


                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No record found\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetPinDetails")]
        [HttpPost]
        public string GetPinDetails([FromBody] InputModel model)
        {
            try
            {
                //[FromBody] InputModel model
                //string jsonInput = "hyiMIBgTA9B+0mdSbVigH35B/X+1aUL4pEmwfgttGXgKW56+M4QX+vldUPyVlYR/IRRwenr4LzxGncqLV4Prm37muZOsDBpETAfNhrfzvOZPJriPjfNH9fvPdHSU4AHwi5svl46mPIXna0WO0PNsd4togntbgQSVen+CEHgA6LM4crWTEYHuhLB2VFUTWaE2CT";
                dstemp.Clear();
                htParam.Clear();
                //string PAN, string ApplicationNo, String SapCode, String strTokenId
                //string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());

                //String param = "PAN : " + strPAN + ",ApplicationNo : " + ApplicationNo;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strPinCode = Convert.ToString(jObject["PinCode"]).Trim();

                SeqNo = ServerTrnReqLog(json, "GetPinDetails", "", "", "Candidate Manager");

                if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                htParam.Clear();
                htParam.Add("@PinCode", strPinCode);
                dstemp = objDAL.selectFromDatabase("Prc_GetStateDetails_mobile", htParam, "USRMGMT");


                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No record found\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/ModifyCFRDetails")]
        [HttpPost]
        public string ModifyCFRDetails([FromBody] InputModel model)
        {
            try
            {
                //[FromBody] InputModel model
                //string jsonInput = "hyiMIBgTA9B+0mdSbVigH35B/X+1aUL4pEmwfgttGXgKW56+M4QX+vldUPyVlYR/IRRwenr4LzxGncqLV4Prm37muZOsDBpETAfNhrfzvOZPJriPjfNH9fvPdHSU4AHwi5svl46mPIXna0WO0PNsd4togntbgQSVen+CEHgA6LM4crWTEYHuhLB2VFUTWaE2CT";
                dstemp.Clear();
                htParam.Clear();
                //string PAN, string ApplicationNo, String SapCode, String strTokenId
                //string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());

                //String param = "PAN : " + strPAN + ",ApplicationNo : " + ApplicationNo;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strPinCode = Convert.ToString(jObject["PinCode"]).Trim();

                SeqNo = ServerTrnReqLog(json, "ModifyCFRDetails", "", "", "Candidate Manager");

                if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                htParam.Clear();
                htParam.Add("@AppNo", Convert.ToString(jObject["AppNo"]).Trim());
                htParam.Add("@CndNo", Convert.ToString(jObject["CndNo"]).Trim());
                htParam.Add("@UniqueRefID", Convert.ToString(jObject["UniqueRefID"]).Trim());
                htParam.Add("@SapCode", Convert.ToString(jObject["SapCode"]).Trim());
                htParam.Add("@CreateBy", Convert.ToString(jObject["CreateBy"]).Trim());
                htParam.Add("@Title", Convert.ToString(jObject["Title"]).Trim());
                htParam.Add("@GivenName", Convert.ToString(jObject["GivenName"]).Trim());
                htParam.Add("@SurName", Convert.ToString(jObject["SurName"]).Trim());
                htParam.Add("@Gender", Convert.ToString(jObject["Gender"]).Trim());
                htParam.Add("@PAN", Convert.ToString(jObject["PAN"]).Trim());
                htParam.Add("@FatherName", Convert.ToString(jObject["FatherName"]).Trim());
                htParam.Add("@DOB", Convert.ToString(jObject["DOB"]).Trim());
                htParam.Add("@BasicQual", Convert.ToString(jObject["BasicQual"]).Trim());
                htParam.Add("@BasicBoardName", Convert.ToString(jObject["BasicBoardName"]).Trim());
                htParam.Add("@BasicRNo", Convert.ToString(jObject["BasicRNo"]).Trim());
                htParam.Add("@BasicYrPass", Convert.ToString(jObject["BasicYrPass"]).Trim());
                htParam.Add("@HigherQual", Convert.ToString(jObject["HigherQual"]).Trim());
                htParam.Add("@ExamMode", Convert.ToString(jObject["ExamMode"]).Trim());
                htParam.Add("@ExmBody", Convert.ToString(jObject["ExmBody"]).Trim());
                htParam.Add("@ExmLang", Convert.ToString(jObject["ExmLang"]).Trim());
                htParam.Add("@ExmCentre", Convert.ToString(jObject["ExmCentre"]).Trim());
                htParam.Add("@SetPrefDt", Convert.ToString(jObject["SetPrefDt"]).Trim());
                htParam.Add("@NomineeName", Convert.ToString(jObject["NomineeName"]).Trim());
                htParam.Add("@NomineeRel", Convert.ToString(jObject["NomineeRel"]).Trim());
                htParam.Add("@NomineeAge", Convert.ToString(jObject["NomineeAge"]).Trim());
                htParam.Add("@NomineePAN", Convert.ToString(jObject["NomineePAN"]).Trim());
                htParam.Add("@NomineeEmail", Convert.ToString(jObject["NomineeEmail"]).Trim());
                htParam.Add("@NomineeContact", Convert.ToString(jObject["NomineeContact"]).Trim());
                htParam.Add("@CnctType", Convert.ToString(jObject["CnctType"]).Trim());
                htParam.Add("@Address1", Convert.ToString(jObject["Address1"]).Trim());
                htParam.Add("@Address2", Convert.ToString(jObject["Address2"]).Trim());
                htParam.Add("@Address3", Convert.ToString(jObject["Address3"]).Trim());
                htParam.Add("@City", Convert.ToString(jObject["City"]).Trim());
                htParam.Add("@Area", Convert.ToString(jObject["Area"]).Trim());
                htParam.Add("@AreaID", Convert.ToString(jObject["AreaID"]).Trim());
                htParam.Add("@Village", Convert.ToString(jObject["Village"]).Trim());
                htParam.Add("@District", Convert.ToString(jObject["District"]).Trim());
                htParam.Add("@DistrictID", Convert.ToString(jObject["DistrictID"]).Trim());
                htParam.Add("@StateCode", Convert.ToString(jObject["StateCode"]).Trim());
                htParam.Add("@StateID", Convert.ToString(jObject["StateID"]).Trim());
                htParam.Add("@Pincode", Convert.ToString(jObject["Pincode"]).Trim());
                htParam.Add("@StateName", Convert.ToString(jObject["StateName"]).Trim());
                htParam.Add("@CntryCode", Convert.ToString(jObject["CntryCode"]).Trim());
                htParam.Add("@Country", Convert.ToString(jObject["Country"]).Trim());
                htParam.Add("@IsActive", Convert.ToString(jObject["IsActive"]).Trim());
                htParam.Add("@FeesWavier", Convert.ToString(jObject["FeesWavier"]).Trim());
                htParam.Add("@WavierType", Convert.ToString(jObject["WavierType"]).Trim());
                htParam.Add("@ADModify", Convert.ToString(jObject["ADModify"]).Trim());
                htParam.Add("@NDModify", Convert.ToString(jObject["NDModify"]).Trim());
                htParam.Add("@EDModify", Convert.ToString(jObject["EDModify"]).Trim());
                htParam.Add("@PDModify", Convert.ToString(jObject["PDModify"]).Trim());
                htParam.Add("@WDModify", Convert.ToString(jObject["WDModify"]).Trim());

                dstemp = objDAL.selectFromDatabase("PrcModifyCFRDetails_Mob", htParam, "ISysARTL");


                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"CFR respond failed\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }


        [Route("api/CBCMS/GetSupportCounts")]
        [HttpPost]
        public string GetSupportCounts([FromBody] InputModel model)
        {
            #region Declaration
            DataSet objDaset = new DataSet();
            XmlDocument xmlDoc = new XmlDocument();
            string SeqNo = string.Empty;
            Hashtable htable = new Hashtable();
            String JSonString = String.Empty;
            #endregion
            try
            {
                //String UserID, String App_ID, String CustomerName, String strTokenId
                //String param = "UserID : " + UserID + " App_ID : " + App_ID + " CustomerName : " + CustomerName + "";
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "GetSupportCounts", strSapCode, "", "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                if (Convert.ToString(jObject["App_ID"]).Trim() != null
                    && Convert.ToString(jObject["App_ID"]).Trim() != "")
                {
                    objDaset.Clear();
                    htable.Clear();
                    htable.Add("@UserID", Convert.ToString(jObject["UserID"]).Trim());
                    htable.Add("@App_ID", Convert.ToString(jObject["App_ID"]).Trim());
                    htable.Add("@CustomerName", Convert.ToString(jObject["CustomerName"]).Trim());
                    //htable.Add("@CarrierCode", Convert.ToString(jObject["CarrierCode"]).Trim());
                    objDaset = objDAL.selectFromDatabase("Prc_getsupportrelatedcount", htable, "IsysARTL");
                    if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                    {
                        JSonString = ds2json(objDaset);
                    }
                    else
                    {
                        JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No record found.\"}]}";
                    }
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid api call\"}]}";
                }

            }
            catch (Exception ex)
            {
                JSonString = "{\"Table\": [ {\"ResponseCode\": \"2\",\"ErrorDescription\":\"" + ex.Message.ToString() + "\"}]}";
            }
            updateResponse(JSonString, SeqNo);
            //JSonString = encryptDecrypt.EncryptAndEncode(JSonString);
            return JSonString;
        }

        //Franchisee
        [Route("api/CBCMS/FranchiseeRegistration")]
        [HttpPost]
        public string FranchiseeRegistration([FromBody] InputModel model)
        {
            XmlDocument xmlInPut = new XmlDocument();

            try
            {
                //String objDecXML = encryptDecrypt.DecodeAndDecrypt(request);
                //xmlInPut.LoadXml(objDecXML);
                //XmlDocument Objxml = new XmlDocument();
                //Objxml.InnerXml = objDecXML;
                //DataSet xmlDS = new DataSet();
                //xmlDS = FuncConvertToDataset(Objxml);
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["CreatedBy"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "FranchiseeRegistration", strSapCode, "", "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                Hashtable htable = new Hashtable();
                htable.Add("@MemCode", Convert.ToString(jObject["MemCode"]).ToString().Trim());
                htable.Add("@SAPCODE", Convert.ToString(jObject["SAPCODE"]).ToString().Trim());
                htable.Add("@Prefix", Convert.ToString(jObject["Prefix"]).ToString().Trim());
                htable.Add("@LegalName", Convert.ToString(jObject["LegalName"]).ToString().Trim());
                htable.Add("@PAN", Convert.ToString(jObject["PAN"]).ToString().Trim());
                htable.Add("@DOB", Convert.ToString(jObject["DOB"]).ToString().Trim());
                htable.Add("@Gender", Convert.ToString(jObject["Gender"]).ToString().Trim());
                htable.Add("@MStatus", Convert.ToString(jObject["MStatus"]).ToString().Trim());
                htable.Add("@MADate", Convert.ToString(jObject["MADate"]).ToString().Trim());
                htable.Add("@Education", Convert.ToString(jObject["Education"]).ToString().Trim());
                htable.Add("@Mobile", Convert.ToString(jObject["Mobile"]).ToString().Trim());
                htable.Add("@Mobile1", Convert.ToString(jObject["Mobile1"]).ToString().Trim());
                htable.Add("@Email", Convert.ToString(jObject["Email"]).ToString().Trim());
                htable.Add("@Email1", Convert.ToString(jObject["Email1"]).ToString().Trim());
                htable.Add("@FAX", Convert.ToString(jObject["FAX"]).ToString().Trim());
                htable.Add("@TwiterId", Convert.ToString(jObject["TwiterId"]).ToString().Trim());
                htable.Add("@FacebookId", Convert.ToString(jObject["FacebookId"]).ToString().Trim());
                htable.Add("@InstagramId", Convert.ToString(jObject["InstagramId"]).ToString().Trim());
                htable.Add("@crAddrType", Convert.ToString(jObject["crAddrType"]).ToString().Trim());
                htable.Add("@crAddressLine1", Convert.ToString(jObject["crAddressLine1"]).ToString().Trim());
                htable.Add("@crAddressLine2", Convert.ToString(jObject["crAddressLine2"]).ToString().Trim());
                htable.Add("@crAddressLine3", Convert.ToString(jObject["crAddressLine3"]).ToString().Trim());
                htable.Add("@crPostCode", Convert.ToString(jObject["crPostCode"]).ToString().Trim());
                htable.Add("@crCity", Convert.ToString(jObject["crCity"]).ToString().Trim());
                htable.Add("@crStateCode", Convert.ToString(jObject["crStateCode"]).ToString().Trim());
                htable.Add("@crState", Convert.ToString(jObject["crState"]).ToString().Trim());
                htable.Add("@crDistrict", Convert.ToString(jObject["crDistrict"]).ToString().Trim());
                htable.Add("@crVillage", Convert.ToString(jObject["crVillage"]).ToString().Trim());
                htable.Add("@crArea", Convert.ToString(jObject["crArea"]).ToString().Trim());
                htable.Add("@crCntryCode", Convert.ToString(jObject["crCntryCode"]).ToString().Trim());
                htable.Add("@crCountry", Convert.ToString(jObject["crCountry"]).ToString().Trim());
                htable.Add("@idAddrType", Convert.ToString(jObject["idAddrType"]).ToString().Trim());
                htable.Add("@idAddressLine1", Convert.ToString(jObject["idAddressLine1"]).ToString().Trim());
                htable.Add("@idAddressLine2", Convert.ToString(jObject["idAddressLine2"]).ToString().Trim());
                htable.Add("@idAddressLine3", Convert.ToString(jObject["idAddressLine3"]).ToString().Trim());
                htable.Add("@idPostCode", Convert.ToString(jObject["idPostCode"]).ToString().Trim());
                htable.Add("@idCity", Convert.ToString(jObject["idCity"]).ToString().Trim());
                htable.Add("@idStateCode", Convert.ToString(jObject["idStateCode"]).ToString().Trim());
                htable.Add("@idState", Convert.ToString(jObject["idState"]).ToString().Trim());
                htable.Add("@idDistrict", Convert.ToString(jObject["idDistrict"]).ToString().Trim());
                htable.Add("@idVillage", Convert.ToString(jObject["idVillage"]).ToString().Trim());
                htable.Add("@idArea", Convert.ToString(jObject["idArea"]).ToString().Trim());
                htable.Add("@idCntryCode", Convert.ToString(jObject["idCntryCode"]).ToString().Trim());
                htable.Add("@idCountry", Convert.ToString(jObject["idCountry"]).ToString().Trim());
                htable.Add("@BaddrType", Convert.ToString(jObject["BaddrType"]).ToString().Trim());
                htable.Add("@BAddressLine1", Convert.ToString(jObject["BAddressLine1"]).ToString().Trim());
                htable.Add("@BAddressLine2", Convert.ToString(jObject["BAddressLine2"]).ToString().Trim());
                htable.Add("@BAddressLine3", Convert.ToString(jObject["BAddressLine3"]).ToString().Trim());
                htable.Add("@BPostCode", Convert.ToString(jObject["BPostCode"]).ToString().Trim());
                htable.Add("@BCity", Convert.ToString(jObject["BCity"]).ToString().Trim());
                htable.Add("@BStateCode", Convert.ToString(jObject["BStateCode"]).ToString().Trim());
                htable.Add("@BState", Convert.ToString(jObject["BState"]).ToString().Trim());
                htable.Add("@BDistrict", Convert.ToString(jObject["BDistrict"]).ToString().Trim());
                htable.Add("@BVillage", Convert.ToString(jObject["BVillage"]).ToString().Trim());
                htable.Add("@BArea", Convert.ToString(jObject["BArea"]).ToString().Trim());
                htable.Add("@BCntryCode", Convert.ToString(jObject["BCntryCode"]).ToString().Trim());
                htable.Add("@BCountry", Convert.ToString(jObject["BCountry"]).ToString().Trim());
                htable.Add("@ExpMonBusnsHealth", Convert.ToString(jObject["ExpMonBusnsHealth"]).ToString().Trim());
                htable.Add("@ExpTeamStrgt", Convert.ToString(jObject["ExpTeamStrgt"]).ToString().Trim());
                htable.Add("@ExpAgntInSixMnth", Convert.ToString(jObject["ExpAgntInSixMnth"]).ToString().Trim());
                htable.Add("@ExpAgntInTwelveMnth", Convert.ToString(jObject["ExpAgntInTwelveMnth"]).ToString().Trim());
                htable.Add("@ExpPOSPInSixMnth", Convert.ToString(jObject["ExpPOSPInSixMnth"]).ToString().Trim());
                htable.Add("@ExpPOSPInTwelveMnth", Convert.ToString(jObject["ExpPOSPInTwelveMnth"]).ToString().Trim());
                htable.Add("@DtlsOfOthrbusiness", Convert.ToString(jObject["DtlsOfOthrbusiness"]).ToString().Trim());
                htable.Add("@MrktingInFinservicebusiness", Convert.ToString(jObject["MrktingInFinservicebusiness"]).ToString().Trim());
                htable.Add("@DtlsOfLawSuit", Convert.ToString(jObject["DtlsOfLawSuit"]).ToString().Trim());
                htable.Add("@DtlsOfOthrAssignmt", Convert.ToString(jObject["DtlsOfOthrAssignmt"]).ToString().Trim());
                htable.Add("@Remark", Convert.ToString(jObject["Remark"]).ToString().Trim());
                htable.Add("@BckgrndExp", Convert.ToString(jObject["BckgrndExp"]).ToString().Trim());
                htable.Add("@BExpMYear", Convert.ToString(jObject["BExpMYear"]).ToString().Trim());
                htable.Add("@Organization", Convert.ToString(jObject["Organization"]).ToString().Trim());
                htable.Add("@TenureFrm", Convert.ToString(jObject["TenureFrm"]).ToString().Trim());
                htable.Add("@TenureTo", Convert.ToString(jObject["TenureTo"]).ToString().Trim());
                htable.Add("@OtherSector", Convert.ToString(jObject["OtherSector"]).ToString().Trim());
                htable.Add("@ChnlType", Convert.ToString(jObject["ChnlType"]).ToString().Trim());
                htable.Add("@BizSrc", Convert.ToString(jObject["BizSrc"]).ToString().Trim());
                htable.Add("@ChnCls", Convert.ToString(jObject["ChnCls"]).ToString().Trim());
                htable.Add("@MemType", Convert.ToString(jObject["MemType"]).ToString().Trim());
                htable.Add("@ConstCode", Convert.ToString(jObject["ConstCode"]).ToString().Trim());
                htable.Add("@ConstType", Convert.ToString(jObject["ConstType"]).ToString().Trim());
                htable.Add("@UntCode", Convert.ToString(jObject["UntCode"]).ToString().Trim());
                htable.Add("@BankAcntName", Convert.ToString(jObject["BankAcntName"]).ToString().Trim());
                htable.Add("@BankAcntType", Convert.ToString(jObject["BankAcntType"]).ToString().Trim());
                htable.Add("@BankAccntNo", Convert.ToString(jObject["BankAccntNo"]).ToString().Trim());
                htable.Add("@BankName", Convert.ToString(jObject["BankName"]).ToString().Trim());
                htable.Add("@BankBranch", Convert.ToString(jObject["BankBranch"]).ToString().Trim());
                htable.Add("@BankBrnCode", Convert.ToString(jObject["BankBrnCode"]).ToString().Trim());
                htable.Add("@IFSCCode", Convert.ToString(jObject["IFSCCode"]).ToString().Trim());
                htable.Add("@MICRCode", Convert.ToString(jObject["MICRCode"]).ToString().Trim());
                htable.Add("@BankBranchAddr", Convert.ToString(jObject["BankBranchAddr"]).ToString().Trim());
                htable.Add("@NomineeName", Convert.ToString(jObject["NomineeName"]).ToString().Trim());
                htable.Add("@NomineeRel", Convert.ToString(jObject["NomineeRel"]).ToString().Trim());
                htable.Add("@NomineeDOB", Convert.ToString(jObject["NomineeDOB"]).ToString().Trim());
                htable.Add("@NomineeMobileTel", Convert.ToString(jObject["NomineeMobileTel"]).ToString().Trim());
                htable.Add("@NomineeEmailID", Convert.ToString(jObject["NomineeEmailID"]).ToString().Trim());
                //htable.Add("@TempMemCode", Convert.ToString(jObject["TempMemCode"]).ToString().Trim());
                htable.Add("@CreatedBy", Convert.ToString(jObject["CreatedBy"]).ToString().Trim());
                htable.Add("@CreatedDate", Convert.ToString(jObject["CreatedDate"]).ToString().Trim());
                //htable.Add("@UpdatedBy", Convert.ToString(jObject["UpdatedBy"]).ToString().Trim());
                //htable.Add("@UpdatedDate", Convert.ToString(jObject["UpdatedDate"]).ToString().Trim());
                htable.Add("@UniqueRefID", Convert.ToString(jObject["UniqueRefID"]).ToString().Trim());
                // added by chandrashekhar for ad-hoc req 
                htable.Add("@GSTNAFlag", Convert.ToString(jObject["GSTNAFlag"]).ToString().Trim());
                htable.Add("@TNCFlag", Convert.ToString(jObject["TNCFlag"]).ToString().Trim());
                //htable.Add("@SyncStatus", Convert.ToString(jObject["SyncStatus"]).ToString().Trim());

                dstemp = objDAL.selectFromDatabase("Prc_InsMemberDetails_Mob", htable, "ISysCHMS");
                if (dstemp.Tables.Count > 0)
                {
                    if (dstemp.Tables[0].Rows.Count > 0)
                    {
                        try
                        {
                            if (dstemp.Tables[0].Rows[0]["ResponseCode"].ToString().Trim() == "0"
                                && dstemp.Tables[0].Rows[0]["WStatus"].ToString().Trim() == "20")
                            {
                                ConvertRDLCToPDF(dstemp.Tables[0].Rows[0]["MemCode"].ToString().Trim(),
                                    dstemp.Tables[0].Rows[0]["EmpCode"].ToString().Trim(),
                                    Convert.ToString(jObject["CreatedBy"]).ToString().Trim());
                            }
                        }
                        catch (Exception es)
                        {
                            es.ToString();
                        }

                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Registration Failed\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/FranchiseeModification")]
        [HttpPost]
        public string FranchiseeModification([FromBody] InputModel model)
        {
            XmlDocument xmlInPut = new XmlDocument();

            try
            {
                //String objDecXML = encryptDecrypt.DecodeAndDecrypt(request);
                //xmlInPut.LoadXml(objDecXML);
                //XmlDocument Objxml = new XmlDocument();
                //Objxml.InnerXml = objDecXML;
                //DataSet xmlDS = new DataSet();
                //xmlDS = FuncConvertToDataset(Objxml);
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["CreatedBy"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "FranchiseeModification", strSapCode, "", "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                Hashtable htable = new Hashtable();
                htable.Add("@MemCode", Convert.ToString(jObject["MemCode"]).ToString().Trim());
                htable.Add("@Prefix", Convert.ToString(jObject["Prefix"]).ToString().Trim());
                htable.Add("@LegalName", Convert.ToString(jObject["LegalName"]).ToString().Trim());
                htable.Add("@PAN", Convert.ToString(jObject["PAN"]).ToString().Trim());
                htable.Add("@DOB", Convert.ToString(jObject["DOB"]).ToString().Trim());
                htable.Add("@Gender", Convert.ToString(jObject["Gender"]).ToString().Trim());
                htable.Add("@MStatus", Convert.ToString(jObject["MStatus"]).ToString().Trim());
                htable.Add("@MADate", Convert.ToString(jObject["MADate"]).ToString().Trim());
                htable.Add("@Education", Convert.ToString(jObject["Education"]).ToString().Trim());
                htable.Add("@Mobile", Convert.ToString(jObject["Mobile"]).ToString().Trim());
                htable.Add("@Mobile1", Convert.ToString(jObject["Mobile1"]).ToString().Trim());
                htable.Add("@Email", Convert.ToString(jObject["Email"]).ToString().Trim());
                htable.Add("@Email1", Convert.ToString(jObject["Email1"]).ToString().Trim());
                htable.Add("@FAX", Convert.ToString(jObject["FAX"]).ToString().Trim());
                htable.Add("@TwiterId", Convert.ToString(jObject["TwiterId"]).ToString().Trim());
                htable.Add("@FacebookId", Convert.ToString(jObject["FacebookId"]).ToString().Trim());
                htable.Add("@InstagramId", Convert.ToString(jObject["InstagramId"]).ToString().Trim());
                htable.Add("@crAddrType", Convert.ToString(jObject["crAddrType"]).ToString().Trim());
                htable.Add("@crAddressLine1", Convert.ToString(jObject["crAddressLine1"]).ToString().Trim());
                htable.Add("@crAddressLine2", Convert.ToString(jObject["crAddressLine2"]).ToString().Trim());
                htable.Add("@crAddressLine3", Convert.ToString(jObject["crAddressLine3"]).ToString().Trim());
                htable.Add("@crPostCode", Convert.ToString(jObject["crPostCode"]).ToString().Trim());
                htable.Add("@crCity", Convert.ToString(jObject["crCity"]).ToString().Trim());
                htable.Add("@crStateCode", Convert.ToString(jObject["crStateCode"]).ToString().Trim());
                htable.Add("@crState", Convert.ToString(jObject["crState"]).ToString().Trim());
                htable.Add("@crDistrict", Convert.ToString(jObject["crDistrict"]).ToString().Trim());
                htable.Add("@crVillage", Convert.ToString(jObject["crVillage"]).ToString().Trim());
                htable.Add("@crArea", Convert.ToString(jObject["crArea"]).ToString().Trim());
                htable.Add("@crCntryCode", Convert.ToString(jObject["crCntryCode"]).ToString().Trim());
                htable.Add("@crCountry", Convert.ToString(jObject["crCountry"]).ToString().Trim());
                htable.Add("@idAddrType", Convert.ToString(jObject["idAddrType"]).ToString().Trim());
                htable.Add("@idAddressLine1", Convert.ToString(jObject["idAddressLine1"]).ToString().Trim());
                htable.Add("@idAddressLine2", Convert.ToString(jObject["idAddressLine2"]).ToString().Trim());
                htable.Add("@idAddressLine3", Convert.ToString(jObject["idAddressLine3"]).ToString().Trim());
                htable.Add("@idPostCode", Convert.ToString(jObject["idPostCode"]).ToString().Trim());
                htable.Add("@idCity", Convert.ToString(jObject["idCity"]).ToString().Trim());
                htable.Add("@idStateCode", Convert.ToString(jObject["idStateCode"]).ToString().Trim());
                htable.Add("@idState", Convert.ToString(jObject["idState"]).ToString().Trim());
                htable.Add("@idDistrict", Convert.ToString(jObject["idDistrict"]).ToString().Trim());
                htable.Add("@idVillage", Convert.ToString(jObject["idVillage"]).ToString().Trim());
                htable.Add("@idArea", Convert.ToString(jObject["idArea"]).ToString().Trim());
                htable.Add("@idCntryCode", Convert.ToString(jObject["idCntryCode"]).ToString().Trim());
                htable.Add("@idCountry", Convert.ToString(jObject["idCountry"]).ToString().Trim());
                htable.Add("@BaddrType", Convert.ToString(jObject["BaddrType"]).ToString().Trim());
                htable.Add("@BAddressLine1", Convert.ToString(jObject["BAddressLine1"]).ToString().Trim());
                htable.Add("@BAddressLine2", Convert.ToString(jObject["BAddressLine2"]).ToString().Trim());
                htable.Add("@BAddressLine3", Convert.ToString(jObject["BAddressLine3"]).ToString().Trim());
                htable.Add("@BPostCode", Convert.ToString(jObject["BPostCode"]).ToString().Trim());
                htable.Add("@BCity", Convert.ToString(jObject["BCity"]).ToString().Trim());
                htable.Add("@BStateCode", Convert.ToString(jObject["BStateCode"]).ToString().Trim());
                htable.Add("@BState", Convert.ToString(jObject["BState"]).ToString().Trim());
                htable.Add("@BDistrict", Convert.ToString(jObject["BDistrict"]).ToString().Trim());
                htable.Add("@BVillage", Convert.ToString(jObject["BVillage"]).ToString().Trim());
                htable.Add("@BArea", Convert.ToString(jObject["BArea"]).ToString().Trim());
                htable.Add("@BCntryCode", Convert.ToString(jObject["BCntryCode"]).ToString().Trim());
                htable.Add("@BCountry", Convert.ToString(jObject["BCountry"]).ToString().Trim());
                htable.Add("@ExpMonBusnsHealth", Convert.ToString(jObject["ExpMonBusnsHealth"]).ToString().Trim());
                htable.Add("@ExpTeamStrgt", Convert.ToString(jObject["ExpTeamStrgt"]).ToString().Trim());
                htable.Add("@ExpAgntInSixMnth", Convert.ToString(jObject["ExpAgntInSixMnth"]).ToString().Trim());
                htable.Add("@ExpAgntInTwelveMnth", Convert.ToString(jObject["ExpAgntInTwelveMnth"]).ToString().Trim());
                htable.Add("@ExpPOSPInSixMnth", Convert.ToString(jObject["ExpPOSPInSixMnth"]).ToString().Trim());
                htable.Add("@ExpPOSPInTwelveMnth", Convert.ToString(jObject["ExpPOSPInTwelveMnth"]).ToString().Trim());
                htable.Add("@DtlsOfOthrbusiness", Convert.ToString(jObject["DtlsOfOthrbusiness"]).ToString().Trim());
                htable.Add("@MrktingInFinservicebusiness", Convert.ToString(jObject["MrktingInFinservicebusiness"]).ToString().Trim());
                htable.Add("@DtlsOfLawSuit", Convert.ToString(jObject["DtlsOfLawSuit"]).ToString().Trim());
                htable.Add("@DtlsOfOthrAssignmt", Convert.ToString(jObject["DtlsOfOthrAssignmt"]).ToString().Trim());
                htable.Add("@Remark", Convert.ToString(jObject["Remark"]).ToString().Trim());
                htable.Add("@BckgrndExp", Convert.ToString(jObject["BckgrndExp"]).ToString().Trim());
                htable.Add("@BExpMYear", Convert.ToString(jObject["BExpMYear"]).ToString().Trim());
                htable.Add("@Organization", Convert.ToString(jObject["Organization"]).ToString().Trim());
                htable.Add("@TenureFrm", Convert.ToString(jObject["TenureFrm"]).ToString().Trim());
                htable.Add("@TenureTo", Convert.ToString(jObject["TenureTo"]).ToString().Trim());
                htable.Add("@OtherSector", Convert.ToString(jObject["OtherSector"]).ToString().Trim());
                htable.Add("@ChnlType", Convert.ToString(jObject["ChnlType"]).ToString().Trim());
                htable.Add("@BizSrc", Convert.ToString(jObject["BizSrc"]).ToString().Trim());
                htable.Add("@ChnCls", Convert.ToString(jObject["ChnCls"]).ToString().Trim());
                htable.Add("@MemType", Convert.ToString(jObject["MemType"]).ToString().Trim());
                htable.Add("@ConstCode", Convert.ToString(jObject["ConstCode"]).ToString().Trim());
                htable.Add("@ConstType", Convert.ToString(jObject["ConstType"]).ToString().Trim());
                htable.Add("@UntCode", Convert.ToString(jObject["UntCode"]).ToString().Trim());
                htable.Add("@BankAcntName", Convert.ToString(jObject["BankAcntName"]).ToString().Trim());
                htable.Add("@BankAcntType", Convert.ToString(jObject["BankAcntType"]).ToString().Trim());
                htable.Add("@BankAccntNo", Convert.ToString(jObject["BankAccntNo"]).ToString().Trim());
                htable.Add("@BankName", Convert.ToString(jObject["BankName"]).ToString().Trim());
                htable.Add("@BankBranch", Convert.ToString(jObject["BankBranch"]).ToString().Trim());
                htable.Add("@BankBrnCode", Convert.ToString(jObject["BankBrnCode"]).ToString().Trim());
                htable.Add("@IFSCCode", Convert.ToString(jObject["IFSCCode"]).ToString().Trim());
                htable.Add("@MICRCode", Convert.ToString(jObject["MICRCode"]).ToString().Trim());
                htable.Add("@BankBranchAddr", Convert.ToString(jObject["BankBranchAddr"]).ToString().Trim());
                htable.Add("@NomineeName", Convert.ToString(jObject["NomineeName"]).ToString().Trim());
                htable.Add("@NomineeRel", Convert.ToString(jObject["NomineeRel"]).ToString().Trim());
                htable.Add("@NomineeDOB", Convert.ToString(jObject["NomineeDOB"]).ToString().Trim());
                htable.Add("@NomineeMobileTel", Convert.ToString(jObject["NomineeMobileTel"]).ToString().Trim());
                htable.Add("@NomineeEmailID", Convert.ToString(jObject["NomineeEmailID"]).ToString().Trim());
                //htable.Add("@TempMemCode", Convert.ToString(jObject["TempMemCode"]).ToString().Trim());
                htable.Add("@CreatedBy", Convert.ToString(jObject["CreatedBy"]).ToString().Trim());
                htable.Add("@CreatedDate", Convert.ToString(jObject["CreatedDate"]).ToString().Trim());
                htable.Add("@UpdatedBy", Convert.ToString(jObject["UpdatedBy"]).ToString().Trim());
                htable.Add("@UpdatedDate", Convert.ToString(jObject["UpdatedDate"]).ToString().Trim());
                htable.Add("@UniqueRefID", Convert.ToString(jObject["UniqueRefID"]).ToString().Trim());
                //htable.Add("@SyncStatus", Convert.ToString(jObject["SyncStatus"]).ToString().Trim());

                // added by chandrashekhar for ad-hoc req 
                htable.Add("@GSTNAFlag", Convert.ToString(jObject["GSTNAFlag"]).ToString().Trim());
                htable.Add("@TNCFlag", Convert.ToString(jObject["TNCFlag"]).ToString().Trim());

                dstemp = objDAL.selectFromDatabase("Prc_UpdateMemberDetails_Mob", htable, "ISysCHMS");
                if (dstemp.Tables.Count > 0)
                {
                    if (dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                        try
                        {
                            if (dstemp.Tables[0].Rows[0]["ResponseCode"].ToString().Trim() == "0"
                                && dstemp.Tables[0].Rows[0]["WStatus"].ToString().Trim() == "20")
                            {
                                ConvertRDLCToPDF(dstemp.Tables[0].Rows[0]["MemCode"].ToString().Trim(),
                                    dstemp.Tables[0].Rows[0]["EmpCode"].ToString().Trim(),
                                    Convert.ToString(jObject["CreatedBy"]).ToString().Trim());
                            }
                        }
                        catch (Exception es)
                        {
                            es.ToString();
                        }
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Modification Failed\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/FranchiseeMgrDetails")]
        [HttpPost]
        public string FranchiseeMgrDetails([FromBody] InputModel model)
        {
            XmlDocument xmlInPut = new XmlDocument();

            try
            {
                //String objDecXML = encryptDecrypt.DecodeAndDecrypt(request);
                ////String objDecXML = "<?xml version=\"1.0\" encoding=\"UTF - 8\"?><MemRelation> <SrNo>null</SrNo> <CarrierCode>2</CarrierCode> <Bizsrc>XX</Bizsrc> <chncls>XXSO</chncls> <MemType>PM</MemType> <MemCode>40086623</MemCode> <RelBizsrc>XX</RelBizsrc> <RelChnCls>XXSO</RelChnCls> <RelMemType>PP</RelMemType> <RelMemCode>40086619</RelMemCode> <RelOrder>1</RelOrder> <Reltype>CF</Reltype> <Status>0</Status> <CeaseDate/> <CreatedBy>77F10144</CreatedBy> <CreatedDate>03/04/2022</CreatedDate> <UpdatedBy>77F10144</UpdatedBy> <UpdatedDate>03/04/2022</UpdatedDate> <UniqueRefID>Android-3f6792ea-0719-4518-8077-59de2fb8c8eb</UniqueRefID> <SyncStatus>Pending</SyncStatus> <MemName>RAJU</MemName> <RelMemName>MAHESH</RelMemName> <EmpCode/> <RelEmpCode>77F10144</RelEmpCode> <strTokenId>5CECAA78-5C7E-474F-9ADF-45DCC7A20081</strTokenId> </MemRelation> ";
                //xmlInPut.LoadXml(objDecXML);
                //XmlDocument Objxml = new XmlDocument();
                //Objxml.InnerXml = objDecXML;
                //DataSet xmlDS = new DataSet();
                //xmlDS = FuncConvertToDataset(Objxml);
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["CreatedBy"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "FranchiseeMgrDetails", strSapCode, "", "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                Hashtable htable = new Hashtable();
                htable.Add("@CarrierCode", Convert.ToString(jObject["CarrierCode"]).ToString().Trim());
                htable.Add("@Bizsrc", Convert.ToString(jObject["Bizsrc"]).ToString().Trim());
                htable.Add("@chncls", Convert.ToString(jObject["chncls"]).ToString().Trim());
                htable.Add("@MemType", Convert.ToString(jObject["MemType"]).ToString().Trim());
                htable.Add("@MemCode", Convert.ToString(jObject["MemCode"]).ToString().Trim());
                htable.Add("@RelBizsrc", Convert.ToString(jObject["RelBizsrc"]).ToString().Trim());
                htable.Add("@RelChnCls", Convert.ToString(jObject["RelChnCls"]).ToString().Trim());
                htable.Add("@RelMemType", Convert.ToString(jObject["RelMemType"]).ToString().Trim());
                htable.Add("@RelMemCode", Convert.ToString(jObject["RelMemCode"]).ToString().Trim());
                htable.Add("@RelOrder", Convert.ToString(jObject["RelOrder"]).ToString().Trim());
                htable.Add("@Reltype", Convert.ToString(jObject["Reltype"]).ToString().Trim());
                htable.Add("@Status", Convert.ToString(jObject["Status"]).ToString().Trim());
                htable.Add("@CreatedBy", Convert.ToString(jObject["CreatedBy"]).ToString().Trim());
                htable.Add("@UpdatedBy", Convert.ToString(jObject["UpdatedBy"]).ToString().Trim());
                htable.Add("@UniqueRefID", Convert.ToString(jObject["UniqueRefID"]).ToString().Trim());
                //htable.Add("@SyncStatus", Convert.ToString(jObject["SyncStatus"]).ToString().Trim());

                dstemp = objDAL.selectFromDatabase("Prc_InsMemRelDetails_Mob", htable, "ISysCHMS");
                if (dstemp.Tables.Count > 0)
                {
                    if (dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Failed\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/FranchiseeExpDetails")]
        [HttpPost]
        public string FranchiseeExpDetails([FromBody] InputModel model)
        {
            //XmlDocument xmlInPut = new XmlDocument();

            try
            {
                //String objDecXML = encryptDecrypt.DecodeAndDecrypt(request);
                //xmlInPut.LoadXml(objDecXML);
                //XmlDocument Objxml = new XmlDocument();
                //Objxml.InnerXml = objDecXML;
                //DataSet xmlDS = new DataSet();
                //xmlDS = FuncConvertToDataset(Objxml);
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["CreatedBy"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "FranchiseeExpDetails", strSapCode, "", "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                Hashtable htable = new Hashtable();
                htable.Add("@SrNo", Convert.ToString(jObject["SrNo"]).ToString().Trim());
                htable.Add("@MemCode", Convert.ToString(jObject["MemCode"]).ToString().Trim());
                htable.Add("@BckgrndExp", Convert.ToString(jObject["BckgrndExp"]).ToString().Trim());
                htable.Add("@ExpTime", Convert.ToString(jObject["ExpTime"]).ToString().Trim());
                htable.Add("@Organization", Convert.ToString(jObject["Organization"]).ToString().Trim());
                htable.Add("@TenureFrm", Convert.ToString(jObject["TenureFrm"]).ToString().Trim());
                htable.Add("@TenureTo", Convert.ToString(jObject["TenureTo"]).ToString().Trim());
                htable.Add("@OtherSector", Convert.ToString(jObject["OtherSector"]).ToString().Trim());
                htable.Add("@IsActive", Convert.ToString(jObject["IsActive"]).ToString().Trim());
                htable.Add("@CreatedBy", Convert.ToString(jObject["CreatedBy"]).ToString().Trim());
                htable.Add("@UpdatedBy", Convert.ToString(jObject["UpdatedBy"]).ToString().Trim());
                htable.Add("@SrvrRecID", Convert.ToString(jObject["SrvrID"]).ToString().Trim());
                htable.Add("@UniqueRefID", Convert.ToString(jObject["UniqueRefID"]).ToString().Trim());

                dstemp = objDAL.selectFromDatabase("Prc_InsMemBckgrndExp", htable, "ISysCHMS");
                if (dstemp.Tables.Count > 0)
                {
                    if (dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Failed\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetFPAddRptMgr")]
        [HttpPost]
        public String GetFPAddRptMgr([FromBody] InputModel model)
        {
            //String strmCode = encryptDecrypt.DecodeAndDecrypt(mCode);
            //String param = "MemCode : " + strmCode + ",Memtype : " + mType + ", BizSrc:" + BizSrc + ", ChnCls:" + ChnCls + ", UnitCode:" + UnitCode;
            //SeqNo = ServerTrnReqLog(param, "GetFPAddRptMgr", "", "", "Candidate Manager");

            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "GetFPAddRptMgr", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp = null;
                Hashtable ht = new Hashtable();
                ht.Add("@MemCode", Convert.ToString(jObject["mCode"]).Trim());
                ht.Add("@MemType", Convert.ToString(jObject["mType"]).Trim());
                ht.Add("@BizSrc", Convert.ToString(jObject["BizSrc"]).Trim());
                ht.Add("@ChnCls", Convert.ToString(jObject["ChnCls"]).Trim());
                ht.Add("@UnitCode", Convert.ToString(jObject["UnitCode"]).Trim());

                dstemp = objDAL.selectFromDatabase("Prc_GetAddRptMgrCodeDtls", ht, "IsysCHMS");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Failed\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/SearchMemberDetails")]
        [HttpPost]
        public string SearchMemberDetails([FromBody] InputModel model)
        {
            try
            {
                //string strFranchiseeCode = encryptDecrypt.DecodeAndDecrypt(FranchiseeCode);
                //string strFranchiseeSAP = encryptDecrypt.DecodeAndDecrypt(FranchiseeSAP);
                //string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN);
                //string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);

                //String param = "FranchiseeCode : " + strFranchiseeCode + ", FranchiseeSAP : " + strFranchiseeSAP + ", FranchiseeName : "
                //    + FranchiseeName + ", RegDateFrom : " + RegDateFrom + ", RegDateTo : " + RegDateTo + ", PAN : " + strPAN
                //    + ", flag" + flag;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "SearchMemberDetails", strSapCode,
                    Convert.ToString(jObject["FranchiseeCode"]).Trim(), "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                Hashtable htCFR = new Hashtable();
                htCFR.Add("@SapCode", strSapCode);
                htCFR.Add("@MemCode", Convert.ToString(jObject["FranchiseeCode"]).Trim());
                htCFR.Add("@EmpCode", Convert.ToString(jObject["FranchiseeSAP"]).Trim());
                htCFR.Add("@MemName", Convert.ToString(jObject["FranchiseeName"]).Trim());
                htCFR.Add("@RegDateFrom", Convert.ToString(jObject["RegDateFrom"]).Trim());
                htCFR.Add("@RegDateTo", Convert.ToString(jObject["RegDateTo"]).Trim());
                htCFR.Add("@PAN", Convert.ToString(jObject["PAN"]).Trim());
                htCFR.Add("@flag", Convert.ToString(jObject["flag"]).Trim());

                dstemp = objDAL.selectFromDatabase("Prc_SearchMemberDetails", htCFR, "ISysCHMS");
                if (dstemp.Tables.Count > 0)
                {
                    if (dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Record Found\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetMemberDetails")]
        [HttpPost]
        public string GetMemberDetails([FromBody] InputModel model)
        {
            try
            {
                //string FranchiseeCode, string SapCode, String strTokenId
                //string strFranchiseeCode = encryptDecrypt.DecodeAndDecrypt(FranchiseeCode);
                ////string strFranchiseeSAP = encryptDecrypt.DecodeAndDecrypt(FranchiseeSAP);
                //// string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN);
                //string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);

                //String param = "FranchiseeCode : " + strFranchiseeCode + ", SAP CODE : " + strSapCode;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "GetMemberDetails", strSapCode, Convert.ToString(jObject["FranchiseeCode"]).Trim(), "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                Hashtable htCFR = new Hashtable();
                htCFR.Add("@CarrierCode", '2');
                htCFR.Add("@MemCode", Convert.ToString(jObject["FranchiseeCode"]).Trim());
                htCFR.Add("@SAPCode", strSapCode);


                dstemp = objDAL.selectFromDatabase("Prc_GetMemberDetails_Mob", htCFR, "ISysCHMS");
                if (dstemp.Tables.Count > 0)
                {
                    if (dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Record Found\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetCfrRaisedFranchisee")]
        [HttpPost]
        public string GetCfrRaisedFranchisee([FromBody] InputModel model)
        {
            try
            {
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);

                //String param = "AppNo : " + strAppNo + ",SapCode : " + strSapCode + "CndName : " + CndName + "MemberType : " + MemberType + "EntryDate : " + EntryDate;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetCfrRaisedFranchisee", strSapCode, strAppNo, "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                string _memCode = "";
                dstemp.Clear();
                dstemp = objDAL.selectFromDatabase_singleValue("prc_GetMemberCode_App", strSapCode, "IsysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    _memCode = Convert.ToString(dstemp.Tables[0].Rows[0]["MemCode"]);

                }
                dstemp.Clear();
                Hashtable htCFR = new Hashtable();
                htCFR.Add("@MemCode", strAppNo);
                htCFR.Add("@EmpCode", strSapCode);
                htCFR.Add("@MemName", Convert.ToString(jObject["CndName"]).Trim());
                htCFR.Add("@MemberCode", _memCode);
                htCFR.Add("@MemberType", Convert.ToString(jObject["MemberType"]).Trim());
                htCFR.Add("@Flag", string.Empty);

                dstemp = objDAL.selectFromDatabase("Prc_GetMemAdvDetailsForCFR", htCFR, "ISysCHMS");
                if (dstemp.Tables.Count > 0)
                {
                    if (dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Record Found\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetCFRStatus_Franchisee")]
        [HttpPost]
        public string GetCFRStatus_Franchisee([FromBody] InputModel model)
        {
            try
            {
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //String param = "AppNo : " + strAppNo;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetCFRStatus_Franchisee", strSapCode, strAppNo, "Candidate Manager");

                String _ProcessType = "NR";
                String _MemCode = strAppNo.Trim();

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                Hashtable htTable = new Hashtable();
                htTable.Add("@Memcode", _MemCode);
                htTable.Add("@Flag", _ProcessType);
                //dstemp = objDAL.selectFromDatabase("Prc_GetCFRStatus_Mob", htTable, "ISysCHMS");
                dstemp = objDAL.selectFromDatabase("Prc_GetCFRdesc_Mob", htTable, "ISysCHMS");
                //dstemp = objDAL.GetDataForAppSearch("Prc_GetCFRStatus_Mob", _CndNo, _ProcessType, "ISysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/CFR_Responde_Franchisee")]
        [HttpPost]
        public string CFR_Responde_Franchisee([FromBody] InputModel model)
        {
            try
            {
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //string strRespondedBy = encryptDecrypt.DecodeAndDecrypt(RespondedBy);

                //String param = "AppNo : " + strAppNo + ",CfrFor : " + CfrFor + "RemarkId : " + RemarkId +
                // "RespondedBy : " + strRespondedBy + " ModuleID : " + ModuleID;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "CFR_Responde_Franchisee", strSapCode, strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                Hashtable htparam = new Hashtable();
                htparam.Add("@AppNo", strAppNo);
                htparam.Add("@CfrFor", Convert.ToString(jObject["CfrFor"]).Trim());
                htparam.Add("@RemarkId", Convert.ToString(jObject["RemarkId"]).Trim());
                htparam.Add("@RespondedBy", Convert.ToString(jObject["RespondedBy"]).Trim());
                htparam.Add("@ModuleID", Convert.ToString(jObject["ModuleID"]).Trim());

                dstemp = objDAL.selectFromDatabase("Prc_Submit_AssignedCFR_Mob", htparam, "ISysCHMS");

                //JsonResponce = "Responded Successfully";
                JsonResponce = ds2json(dstemp);
            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
                //return "error";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/Get_SupportDocs_Franchisee")]
        [HttpPost]
        public string Get_SupportDocs_Franchisee([FromBody] InputModel model)
        {
            try
            {
                //string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
                //String param = "AppNo : " + strAppNo + " , cand_type:" + CandType + ", EntityType:" + EntityType;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "Get_SupportDocs_Franchisee", strSapCode, strAppNo, "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp = null;
                Hashtable ht = new Hashtable();
                ht.Add("@Appno", strAppNo.ToString().Trim());
                ht.Add("@Cand_type", Convert.ToString(jObject["CandType"]).Trim().Trim());
                ht.Add("@EntityType", Convert.ToString(jObject["EntityType"]).Trim().Trim());
                dstemp = objDAL.selectFromDatabase("Prc_GetCand_SupportDocs", ht, "ISysCHMS");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Record Found\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/GetImageForMemDocType")]
        [HttpPost]
        public string GetImageForMemDocType([FromBody] InputModel model)
        {
            try
            {
                //string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);
                //String param = "AppNo : " + strAppno + ",DocType : " + DocType;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "GetImageForMemDocType", strSapCode, strAppNo, "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@Appno", Convert.ToString(jObject["AppNo"]).Trim());
                htParam.Add("@DocType", Convert.ToString(jObject["DocType"]).Trim());
                dstemp = objDAL.selectFromDatabase("Prc_GetDocForMob", htParam, "IsysCHMS");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/SubmitFRDocumentRequest")]
        [HttpPost]
        public string SubmitFRDocumentRequest([FromBody] InputModel model)
        {
            try
            {
                //String MemCode, String MemType, String CreatedBy, String strTokenId, String EntityType, String GSTNAFlag, String TNCFlag
                //string strMemCode = encryptDecrypt.DecodeAndDecrypt(MemCode);
                ////string strMemType = encryptDecrypt.DecodeAndDecrypt(MemType);
                //string strCreatedBy = encryptDecrypt.DecodeAndDecrypt(CreatedBy);

                //String param = "MemCode : " + strMemCode + ", MemType : " + MemType + ", CreatedBy : " + strCreatedBy
                //    + ", EntityType:" + EntityType + ", GSTNAFlag:" + GSTNAFlag + ", TNCFlag:" + TNCFlag;

                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strMemCode = Convert.ToString(jObject["MemCode"]).Trim();

                SeqNo = ServerTrnReqLog(json, "SubmitFRDocumentRequest", "", strMemCode, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@MemCode", strMemCode.ToString().Trim());
                htParam.Add("@MemType", Convert.ToString(jObject["MemType"]).Trim());
                htParam.Add("@CreatedBy", Convert.ToString(jObject["CreatedBy"]).Trim());
                htParam.Add("@EntityType", Convert.ToString(jObject["EntityType"]).Trim());
                htParam.Add("@GSTNAFlag", Convert.ToString(jObject["GSTNAFlag"]).Trim());
                htParam.Add("@TNCFlag", Convert.ToString(jObject["TNCFlag"]).Trim());
                dstemp = objDAL.selectFromDatabase("Prc_InsDocumentRequest_Mob", htParam, "IsysCHMS");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                    try
                    {
                        if (dstemp.Tables[0].Rows[0]["ResponseCode"].ToString().Trim() == "0"
                            && dstemp.Tables[0].Rows[0]["WStatus"].ToString().Trim() == "20")
                        {
                            ConvertRDLCToPDF(dstemp.Tables[0].Rows[0]["MemCode"].ToString().Trim(),
                                dstemp.Tables[0].Rows[0]["EmpCode"].ToString().Trim(),
                                strSapCode);
                        }
                    }
                    catch (Exception es)
                    {
                        es.ToString();
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;

        }

        [Route("api/CBCMS/InsertMemDocsFrmMob")]
        [HttpPost]
        public string InsertMemDocsFrmMob([FromBody] InputModel model)
        {
            try
            {
                //String Appno, byte[] Image, String DocType, String UploadedBy,
                //String lblimgshrt, String lblCndNoValue, String FileType,
                //String DocCode, String MemType, String strTokenId
                //string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);
                //string strUploadedBy = encryptDecrypt.DecodeAndDecrypt(UploadedBy);
                //string strlblCndNoValue = encryptDecrypt.DecodeAndDecrypt(lblCndNoValue);

                //String param = "AppNo : " + strAppno + ",DocType : " + DocType + ",UploadedBy : " + strUploadedBy + ",lblimgshrt : "
                //    + lblimgshrt + ",lblCndNoValue : " + strlblCndNoValue + ",FileType : " + FileType + ", DocCode:" + DocCode + ", MemType: " + MemType;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "InsertMemDocsFrmMob", "", strAppNo, "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                byte[] Image = Convert.FromBase64String(Convert.ToString(jObject["Image"]).Trim());
                dstemp = null;
                Hashtable ht = new Hashtable();
                ht.Add("@Appno", Convert.ToString(jObject["AppNo"]).Trim());
                ht.Add("@Image", Image);
                ht.Add("@DocType", Convert.ToString(jObject["DocType"]).Trim());
                ht.Add("@UploadedBy", Convert.ToString(jObject["UploadedBy"]).Trim());
                ht.Add("@FileType", Convert.ToString(jObject["FileType"]).Trim());
                ht.Add("@DocCode", Convert.ToString(jObject["DocCode"]).Trim());
                ht.Add("@MemType", Convert.ToString(jObject["MemType"]).Trim());
                dstemp = objDAL.selectFromDatabase("Prc_InsMemDocsFrmMob", ht, "IsysCHMS");
                if (dstemp != null)
                {
                    if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                    {
                        //return ds2json(dstemp);
                        //CreateFolderOfDoc(lblimgshrt, strlblCndNoValue, Image);
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = dstemp.Tables[0].Rows.Count.ToString();
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;

        }

        [Route("api/CBCMS/SubmitHNINModification")]
        [HttpPost]
        public string SubmitHNINModification([FromBody] InputModel model)
        {
            XmlDocument xmlInPut = new XmlDocument();

            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();

                SeqNo = ServerTrnReqLog(json, "SubmitHNINModification", strSapCode, "", "Candidate Manager");

                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                Hashtable htable = new Hashtable();
                htable.Add("@MemCode", Convert.ToString(jObject["MemCode"]).ToString().Trim());
                htable.Add("@SAPCODE", Convert.ToString(jObject["SAPCODE"]).ToString().Trim());
                htable.Add("@BankAcntName", Convert.ToString(jObject["BankAcntName"]).ToString().Trim());
                htable.Add("@BankAcntType", Convert.ToString(jObject["BankAcntType"]).ToString().Trim());
                htable.Add("@BankAccntNo", Convert.ToString(jObject["BankAccntNo"]).ToString().Trim());
                htable.Add("@BankName", Convert.ToString(jObject["BankName"]).ToString().Trim());
                htable.Add("@BankBranch", Convert.ToString(jObject["BankBranch"]).ToString().Trim());
                htable.Add("@BankBrnCode", Convert.ToString(jObject["BankBrnCode"]).ToString().Trim());
                htable.Add("@IFSCCode", Convert.ToString(jObject["IFSCCode"]).ToString().Trim());
                htable.Add("@MICRCode", Convert.ToString(jObject["MICRCode"]).ToString().Trim());
                htable.Add("@BankBranchAddr", Convert.ToString(jObject["BankBranchAddr"]).ToString().Trim());
                //htable.Add("@UpdateBy", Convert.ToString(jObject["UpdateBy"]).ToString().Trim());
                //htable.Add("@UpdateDtim", Convert.ToString(jObject["UpdateDtim"]).ToString().Trim());
                //htable.Add("@CreatedBy", Convert.ToString(jObject["CreatedBy"]).ToString().Trim());
                //htable.Add("@CreatedDate", Convert.ToString(jObject["CreatedDate"]).ToString().Trim());


                dstemp = objDAL.selectFromDatabase("Prc_UpdHninDtls_mob", htable, "ISysCHMS");
                if (dstemp.Tables.Count > 0)
                {
                    if (dstemp.Tables[0].Rows.Count > 0)
                    {
                        JsonResponce = ds2json(dstemp);
                    }
                    else
                    {
                        JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Record Found\"}]}";
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
                }

            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/Set_MobRegId")]
        [HttpPost]
        public string Set_MobRegId([FromBody] InputModel model)
        {
            //string regId, String SapCode, String strTokenId)//,string Mobile_No,string userId)
            //String param = "regId : " + regId;           
            try
            {
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strAppNo = Convert.ToString(jObject["AppNo"]).Trim();
                SeqNo = ServerTrnReqLog(json, "Set_MobRegId", strSapCode, "", "Candidate Manager");
                String result = ValidateRequest(json);
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }
                else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }
                dstemp.Clear();
                htParam.Clear();
                htParam.Add("@RegId", Convert.ToString(jObject["regId"]).Trim());
                dstemp = objDAL.selectFromDatabase("Prc_PushNotificationMob", htParam, "IsysARTL");
                JsonResponce = "Registered Successfully.";
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/SendNotification")]
        [HttpPost]
        public string SendNotification([FromBody] InputModel model)
        {
            //string deviceId, string message, String SapCode, String strTokenId
            //String param = "deviceId : " + deviceId + ",message : " + message;
            string jsonInput = model.jsonInput;
            string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
            JObject jObject = JObject.Parse(json);
            string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
            string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
            SeqNo = ServerTrnReqLog(json, "SendNotification", strSapCode, "", "Candidate Manager");

            String result = ValidateRequest(json);
            if (!result.Equals("Valid"))
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(JsonResponce, SeqNo);
                return JsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                && ValidateToken(strSapCode, strTokenId) != "Valid"))
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(JsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

            }
            string GoogleAppID = "AIzaSyA4MDfudLMOaeeNNh45Nc-mmPY9Kuf4jLE";
            var SENDER_ID = "947589077663";
            var value = Convert.ToString(jObject["message"]).Trim();
            WebRequest tRequest;
            tRequest = WebRequest.Create("https://android.googleapis.com/gcm/send");
            tRequest.Method = "post";
            tRequest.ContentType = " application/x-www-form-urlencoded;charset=UTF-8";
            tRequest.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));

            tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

            //string postData = "collapse_key=score_update&time_to_live=108&delay_while_idle=1&data.message=" + message + "&data.time=" + 
            //System.DateTime.Now.ToString() + "&registration_id=" + deviceId + "";

            string postData = "collapse_key=score_update&time_to_live=108&delay_while_idle=1&data.message=Candidate Registered Successfully&data.time=" +
            System.DateTime.Now.ToString() + "&registration_id=" + Convert.ToString(jObject["deviceId"]).Trim() + "";

            Console.WriteLine(postData);
            Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            tRequest.ContentLength = byteArray.Length;

            Stream dataStream = tRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse tResponse = tRequest.GetResponse();

            dataStream = tResponse.GetResponseStream();

            StreamReader tReader = new StreamReader(dataStream);

            String sResponseFromServer = tReader.ReadToEnd();

            tReader.Close();
            dataStream.Close();
            tResponse.Close();
            return sResponseFromServer;

        }

        [Route("api/CBCMS/Mob_ErrorLog")]
        [HttpPost]
        public string Mob_ErrorLog([FromBody] InputModel model)
        {

            try
            {
                //       string LogDtime, string AppName, string PageName, string FuncName, string ErrDesc,
                //string UserId, string Severity, String strTokenId
                //       string strUserId = encryptDecrypt.DecodeAndDecrypt(UserId);

                //       String param = "LogDtime : " + LogDtime + ",AppName : " + AppName + "PageName : " + PageName + "FuncName : " + FuncName + "ErrDesc : " + ErrDesc +
                //           "strUserId : " + strUserId + "Severity : " + Severity;
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                SeqNo = ServerTrnReqLog(json, "Mob_ErrorLog", strSapCode, "", "Candidate Manager");

                //if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strUserId, strTokenId) != "Valid"))
                //{
                //    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                //    updateResponse(JsonResponce, SeqNo);

                //    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                //}
                dstemp = null;
                Hashtable ht = new Hashtable();
                ht.Add("@LogDtime", Convert.ToString(jObject["LogDtime"]).Trim());
                ht.Add("@AppName", Convert.ToString(jObject["AppName"]).Trim());
                ht.Add("@PageName", Convert.ToString(jObject["PageName"]).Trim());
                ht.Add("@FuncName", Convert.ToString(jObject["FuncName"]).Trim());
                ht.Add("@ErrDesc", Convert.ToString(jObject["ErrDesc"]).Trim());
                ht.Add("@UserId", Convert.ToString(jObject["UserId"]).Trim());
                ht.Add("@Severity", Convert.ToString(jObject["Severity"]).Trim());

                dstemp = objDAL.selectFromDatabase("Mob_LogError", ht, "USRMGMT");
                JsonResponce = "Error Mentioned successfully";
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }
            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }

        [Route("api/CBCMS/POSPDeativation")]
        [HttpGet]
        public string POSPDeativation()
        {
            //[FromBody] InputModel model
            try
            {
                dstemp.Clear();
                htParam.Clear();

                //string jsonInput = model.jsonInput;
                ////string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                //JObject jObject = JObject.Parse(jsonInput);
                //string strSapCode = Convert.ToString(jObject["UserId"]).Trim();
                //string strFlag = Convert.ToString(jObject["Flag"]).Trim();

                string strSapCode = "E10230";
                string strFlag = "InitService";

                SeqNo = ServerTrnReqLog("InitService", "POSPDeativation", strSapCode, "", "SupportApp");

                //String result = ValidateRequest("InitService");
                String result = "Valid";
                if (!result.Equals("Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                    updateResponse(JsonResponce, SeqNo);
                    return JsonResponce;
                }


                htParam.Clear();
                if (strFlag.Equals("InitService"))
                {
                    dstemp = null;
                    StartService("POSPDeativation", 100);
                }
                else
                {
                    dstemp = objDAL.selectFromDatabase("Prc_Get_TranstStatus_POSPDeactivation", htParam, "INSCRecruit");
                }

                if (dstemp != null && (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0))
                {
                    JsonResponce = ds2json(dstemp);
                    if (dstemp.Tables[0].Rows[0]["IsCompleted"].ToString() == "Y")
                    {
                        StopService("POSPDeativation", 100);
                    }
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Verification failed\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = ex.ToString();
            }

            updateResponse(JsonResponce, SeqNo);
            JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }


        [Route("api/CBCMS/SubmitPOSDCaptcha")]
        [HttpPost]
        public string SubmitPOSDCaptcha([FromBody] InputModel model)
        {
            try
            {
                dstemp.Clear();
                htParam.Clear();
                //string PAN, string ApplicationNo, String SapCode, String strTokenId
                //string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());

                //String param = "PAN : " + strPAN + ",ApplicationNo : " + ApplicationNo;

                string jsonInput = model.jsonInput;
                //string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(jsonInput);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim();
                string strCaptcha = Convert.ToString(jObject["Captcha"]).Trim();

                SeqNo = ServerTrnReqLog(jsonInput, "SubmitPOSDCaptcha", strSapCode, "", "SupportApp");

                if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                    && ValidateToken(strSapCode, strTokenId) != "Valid"))
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(JsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, JsonResponce).ToString();

                }

                htParam.Clear();
                htParam.Add("@Capt", strCaptcha);
                dstemp = objDAL.selectFromDatabase("Prc_Upd_Captcha_POSD", htParam, "INSCRecruit");


                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    JsonResponce = ds2json(dstemp);
                }
                else
                {
                    JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"Captcha failed\"}]}";
                }
            }
            catch (Exception ex)
            {
                JsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + ex.ToString() + "\"}]}";
            }

            updateResponse(JsonResponce, SeqNo);
            //JsonResponce = encryptDecrypt.EncryptAndEncode(JsonResponce);
            return JsonResponce;
        }



        public void StartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch
            {
                // ...
            }
        }

        public void StopService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch
            {
                // ...
            }
        }

        public void RestartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch
            {
                // ...
            }
        }

        public String GetCaptcha(String SapCode)
        {
            DataSet ds = new DataSet();
            Hashtable htParam = new Hashtable();


            Random ran = new Random();
            int number1 = ran.Next(1, 9);
            int number2 = ran.Next(1, 9);
            var operation = ran.Next(1, 3);
            string operatorString = "";
            int answer = 0;

            if (number1 < number2)
            {
                int temp = number1;
                number1 = number2;
                number2 = temp;
            }

            switch (operation)
            {
                case 1:
                    answer = number1 + number2;
                    operatorString = "+";
                    break;
                case 2:
                    answer = number1 - number2;
                    operatorString = "-";
                    break;
                case 3:
                    answer = number1 * number2;
                    operatorString = "*";
                    break;
            }

            htParam.Clear();
            htParam.Add("@UserId", SapCode);
            htParam.Add("@OP1", number1);
            htParam.Add("@OP2", number2);
            htParam.Add("@Operation", operatorString);
            htParam.Add("@Result", answer);
            ds = objDAL.selectFromDatabase("prc_captcha_auth", htParam, "USRMGMT");
            return "{\"Table\": [ {\"ResponseCode\": \"0\",\"SapCode\":\"" + SapCode + "\",\"OP1\":\"" + number1.ToString() + "\",\"OP2\":\"" + number2.ToString() + "\",\"Operation\":\"" + operatorString + "\",\"Answer\":\"" + answer.ToString() + "\" }]}";
        }

        public string CheckADAuth(string strUserID, string strPassword)
        {
            strPassword = strPassword.Replace("\0", "");
            string adAuth = "";
            try
            {
                cSymmetric cSymmetric = new cSymmetric((cSymmetric.Provider)2, true);
                cData cData1 = new cData("mykey");
                cData cData2 = cSymmetric.Encrypt(new cData(strPassword), cData1);
                //Insc.Common.Data.Provider objUserLogonCount = new Insc.Common.Data.Provider();
                DataSet ds = new DataSet();
                Hashtable ht = new Hashtable();
                ht.Clear();
                ht.Add("@UserId", strUserID);
                ds = objDAL.selectFromDatabase("prc_GetAuthenticationType", ht, "USRMGMT");
                // GET THE USER DETAIL FOR AD OR DB - Adarsh 21-Jan-2016
                if (ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0]["AuthType"].ToString().ToUpper() == "A")
                    {
                        try
                        {
                            if (API_Bridge.AuthenticateInActiveDirectory(strUserID, strPassword))
                            {
                                adAuth = "A";
                            }
                            else
                            {
                                adAuth = "Failed";
                            }
                        }
                        catch (Exception e)
                        {
                            adAuth = "Failed";
                        }
                    }
                    else
                    {
                        adAuth = "D";
                    }
                }
                else
                {
                    adAuth = "X";
                }
            }
            catch (Exception e)
            {
                e.ToString();
                adAuth = "Failed";
            }
            return adAuth;
        }

        public string ValidateToken(String UserId, String TokenId)
        {
            try
            {
                DataSet ds = new DataSet();
                Hashtable ht = new Hashtable();
                ht.Clear();
                ht.Add("@UserId", UserId);
                ht.Add("@TokenId", TokenId);
                ds = objDAL.selectFromDatabase("prc_ValidateAuthToken", ht, "USRMGMT");

                if (ds.Tables[0].Rows.Count > 0)
                {
                    return ds.Tables[0].Rows[0]["Result"].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public string ValidateRequest(String request)
        {
            String result = "Valid";
            try
            {
                //Regex.IsMatch(request, "[^a-zA-Z0-9_.,{}:=@\\-+/ ()?<>\"\n]")
                JObject jObject = JObject.Parse(request);

                foreach (var key in jObject)
                {
                    String val = "";
                    if (key.Value.ToString().Contains("|"))
                        val = key.Value.ToString().Replace("|", "");
                    else
                        val = key.Value.ToString();

                    if (key.Key.Contains("Image"))
                    {
                        continue;
                    }
                    else if (key.Key.Contains("Address1") || key.Key.Contains("StateName")
                        || key.Key.Contains("crState") || key.Key.Contains("idState") || key.Key.Contains("BState"))
                    {
                        continue;
                    }
                    else if (Regex.IsMatch(val, "[^a-zA-Z0-9  ()_.,@\\-+/\"\n]"))
                    {
                        result = "Special character(s) detected in " + val;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                //if (Regex.IsMatch(request, "[^a-zA-Z0-9_.,{}:=@\\-+/ ()?<>\"\n]"))
                //{
                //    result = "Special character(s) detected.";
                //}
                //else
                //{
                //    result= "Valid";
                //}
            }
            catch (Exception e)
            {
                result = "Unable to validate the request!";
            }
            return result;
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

        public DataSet ConvertXMLToDataSet(string xmlData)
        {
            StringReader stream = null;
            XmlTextReader reader = null;
            try
            {
                DataSet xmlDS = new DataSet();
                stream = new StringReader(xmlData);
                // Load the XmlTextReader from the stream
                reader = new XmlTextReader(stream);
                xmlDS.ReadXml(reader);
                return xmlDS;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }

        public static string ds2json(DataSet ds)
        {
            return JsonConvert.SerializeObject(ds, Newtonsoft.Json.Formatting.Indented);
        }
        public static string ds2json(DataTable ds)
        {
            return JsonConvert.SerializeObject(ds, Newtonsoft.Json.Formatting.Indented);
        }


        public void ConvertRDLCToPDF(String MemCode, String EmpCode, String UserID)
        {

            try
            {
                //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Franchisee Entered RDLC" + DateTime.Now.ToString() + Environment.NewLine);

                Hashtable htCRP = new Hashtable();
                MemCode = MemCode.Replace("\"", "").Trim();
                htCRP.Add("@MemCode", MemCode);
                DataSet dsCRP = new DataSet();
                //dsCRP = objDAL.GetDataSetForPrcCLP("prc_GetFPWelcomeLetter", htCRP);

                dsCRP = objDAL.selectFromDatabase("Prc_GetDetailsForDocCreation", htCRP, "IsysCHMS");

                //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "RDLC Count" + " " + Convert.ToString(dsCRP.Tables[0].Rows.Count) + Environment.NewLine);
                foreach (DataRow dr in dsCRP.Tables[0].Rows)
                {
                    try
                    {
                        //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "RDLC_PROC " + dr["RDLC_PROC"].ToString() + Environment.NewLine);
                        string prc = dr["RDLC_PROC"].ToString();
                        if (prc.IsNullOrWhiteSpace())
                        {
                            continue;
                        }
                        else
                        {
                            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "RDLC_PROC " + dr["RDLC_PROC"].ToString() + Environment.NewLine);
                            DataSet ds = new DataSet();
                            ds = objDAL.selectFromDatabase(prc, htCRP, "IsysCHMS");

                            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "RDLC_PROC Result " + Convert.ToString(ds.Tables[0].Rows.Count) + Environment.NewLine);
                            ReportDataSource rdsAct = new ReportDataSource(dr["RDLC_DS"].ToString(), ds.Tables[0]);
                            ReportViewer viewer = new ReportViewer();
                            viewer.LocalReport.Refresh();
                            //viewer.LocalReport.ReportPath = Server.MapPath(dr["MRDLC_PATH"].ToString()); //This is your rdlc name.
                            string strRDLC = dr["MRDLC_PATH"].ToString();
                            if (String.IsNullOrEmpty(strRDLC))
                            {
                                //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "MRDLC_PATH empty " + Environment.NewLine);
                                continue;
                            }
                            viewer.LocalReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath(strRDLC); //This is your rdlc name.
                            //System.Web.Hosting.HostingEnvironment.MapPath                                                                           // viewer.LocalReport.SetParameters(param);
                            viewer.LocalReport.DataSources.Add(rdsAct); // Add  datasource here         
                            Warning[] warnings;
                            string[] streamIds;
                            string mimeType = string.Empty; ;
                            string encoding = string.Empty; ;
                            string extension = string.Empty; ;

                            //Microsoft.Reporting.WebForms.Warning[] warnings;
                            //string[] warnings;
                            byte[] bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
                            //using (FileStream fs = new FileStream(strFileRePath + "FPWelcomeLetter.pdf", FileMode.Create))
                            //{
                            //    fs.Write(bytes, 0, bytes.Length);
                            //}

                            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Byte length " + Convert.ToString(bytes.Length) + Environment.NewLine);

                            Hashtable htApp = new Hashtable();
                            htApp.Add("@MemCode", MemCode.ToString().Trim());
                            htApp.Add("@UserFilename", MemCode.ToString().Trim() + "_" + dr["DocCode"].ToString() + ".PDF");
                            htApp.Add("@ServerFilename", MemCode.ToString().Trim() + "_" + dr["DocCode"].ToString() + ".PDF");
                            htApp.Add("@PDFByte", bytes);
                            htApp.Add("@DocCode", dr["DocCode"].ToString());
                            htApp.Add("@DocType", dr["ImgDesc01"].ToString());
                            htApp.Add("@CreateBy", UserID.ToString().Trim());

                            //objDAL.GetDataSetForPrcCLP("Prc_InsFPConRDLCToPDF", htApp);
                            ds = objDAL.selectFromDatabase("Prc_InsFPDocFromRDLCToPDF", htApp, "IsysCHMS");
                            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Complete" + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogAPIError("mConvertRDLCToPDF", ex.ToString());
                        //ex.StackTrace.ToString();
                        //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Exception " + ex.ToString() + Environment.NewLine);
                    }
                }
            }
            catch (Exception e)
            {
                LogAPIError("mConvertRDLCToPDF", e.ToString());
                //e.ToString();
                //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Exception " + e.ToString() + Environment.NewLine);
            }
            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Method Complete " + Environment.NewLine);
        }
        //[Route("api/CBCMS/ConvertRDLCToPDF")]
        //[HttpGet]
        public void ConvertRDLCToPDF(String CndNo, String UserID)
        {
            try
            {
                //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Candidate Entered RDLC" + DateTime.Now.ToString() + Environment.NewLine);
                Hashtable htCRP = new Hashtable();
                CndNo = CndNo.Replace("\"", "").Trim();
                htCRP.Add("@CndNo", CndNo);
                DataSet dsCRP = new DataSet();
                //dsCRP = objDAL.GetDataSetForPrcCLP("prc_GetFPWelcomeLetter", htCRP);
                //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Cnd No " + CndNo + Environment.NewLine);
                dsCRP = objDAL.selectFromDatabase("Prc_GetDetailsForPDFCreation", htCRP, "IsysARTL");

                //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "RDLC Count" + " " + Convert.ToString(dsCRP.Tables[0].Rows.Count) + Environment.NewLine);
                Boolean flag = false;
                foreach (DataRow dr in dsCRP.Tables[0].Rows)
                {
                    try
                    {
                        //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "RDLC_PROC Check " + Environment.NewLine);
                        string prc = dr["RDLC_PROC"].ToString();
                        if (prc.IsNullOrWhiteSpace())
                        {
                            continue;
                        }
                        else
                        {
                            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "RDLC_PROC " + dr["RDLC_PROC"].ToString() + Environment.NewLine);
                            DataSet ds = new DataSet();
                            ds = objDAL.selectFromDatabase(prc, htCRP, "IsysARTL");
                            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "RDLC_PROC Result " + Convert.ToString(ds.Tables[0].Rows.Count) + Environment.NewLine);
                            ReportDataSource rdsAct = new ReportDataSource(dr["RDLC_DS"].ToString(), ds.Tables[0]);
                            ReportViewer viewer = new ReportViewer();
                            viewer.LocalReport.Refresh();
                            //viewer.LocalReport.ReportPath = Server.MapPath(dr["MRDLC_PATH"].ToString()); //This is your rdlc name.
                            string strRDLC = dr["MRDLC_PATH"].ToString();
                            if (String.IsNullOrEmpty(strRDLC))
                            {
                                //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "MRDLC_PATH empty " + Environment.NewLine);
                                continue;
                            }
                            viewer.LocalReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath(strRDLC); //This is your rdlc name.
                            //System.Web.Hosting.HostingEnvironment.MapPath                                                                             // viewer.LocalReport.SetParameters(param);
                            viewer.LocalReport.DataSources.Add(rdsAct); // Add  datasource here         
                            Warning[] warnings;
                            string[] streamIds;
                            string mimeType = string.Empty;
                            string encoding = string.Empty;
                            string extension = string.Empty;

                            //Microsoft.Reporting.WebForms.Warning[] warnings;
                            //string[] warnings;
                            byte[] bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
                            //using (FileStream fs = new FileStream(strFileRePath + "FPWelcomeLetter.pdf", FileMode.Create))
                            //{
                            //    fs.Write(bytes, 0, bytes.Length);
                            //}
                            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Byte length " + Convert.ToString(bytes.Length) + Environment.NewLine);

                            Hashtable htApp = new Hashtable();
                            htApp.Add("@CndNo", CndNo.ToString().Trim());
                            htApp.Add("@UserFilename", CndNo.ToString().Trim() + "_" + dr["DocCode"].ToString() + ".PDF");
                            htApp.Add("@ServerFilename", CndNo.ToString().Trim() + "_" + dr["DocCode"].ToString() + ".PDF");
                            htApp.Add("@PDFByte", bytes);
                            htApp.Add("@DocCode", dr["DocCode"].ToString());
                            htApp.Add("@DocType", dr["ImgDesc01"].ToString());
                            htApp.Add("@CreateBy", UserID.ToString().Trim());

                            //objDAL.GetDataSetForPrcCLP("Prc_InsFPConRDLCToPDF", htApp);
                            ds = objDAL.selectFromDatabase("Prc_InsCndDocFromRDLCToPDF", htApp, "IsysARTL");
                            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Complete" + Environment.NewLine);
                            //if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            //{

                            //}
                            flag = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogAPIError("cConvertRDLCToPDF", ex.ToString());
                        flag = false;
                    }
                }
                if (flag)
                {
                    sendRdlcViaMail.SendAutoGenerateDocs(CndNo.ToString().Trim());
                }
            }
            catch (Exception e)
            {
                LogAPIError("cConvertRDLCToPDF", e.ToString());
            }
            //File.AppendAllText(@"C:\Users\ChandrashekharT\Desktop\RDLCLog.txt", "Method Complete " + Environment.NewLine);
        }

        //[Route("api/CBCMS/LogAPIError")]
        //[HttpGet]
        public void LogAPIError(string method, string error)
        {
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@LogDtime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            ht.Add("@AppName", "CBCMSAPI");
            ht.Add("@PageName", "API");
            ht.Add("@FuncName", method);
            ht.Add("@ErrDesc", error);
            ht.Add("@UserId", "API");
            ht.Add("@Severity", "High");
            dstemp = objDAL.selectFromDatabase("Mob_LogError", ht, "USRMGMT");
        }
    }
}
