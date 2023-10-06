using System;
using System.Data;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
using Insc.Common.Cryptography;
using System.Net;
using System.Text;
using System.Drawing.Imaging;
using System.Xml;
using Microsoft.Reporting.WebForms;
using Warning = Microsoft.Reporting.WebForms.Warning;
using System.Text.RegularExpressions;

/// <summary>
/// This webservice handles requests form the Candidate Approval Android App.
/// Gets 5 parameters; User's Registered Mobile number being the mandatory and is fetched automatically.
/// Sends String based Json object of the Data obtained.
/// {[{Table: {"Candidate No":"3000100", "Name":"Larry Page", "Flag":"0"}; }] is the format.
/// </summary>

//[WebService(Namespace = "https://mservices.reliancegeneral.co.in/ISysWSCndApprvl/")]
//[WebService(Namespace = "https://dailydiaryuat.reliancegeneral.co.in/ISysWSCndApprvl/")]
[WebService(Namespace = "http://kmidev.centralus.cloudapp.azure.com/ISysWSCndApprvl/")]
//[WebService(Namespace = "https://dailydiary.reliancegeneral.co.in/ISysWSCndApprvl/")]
//[WebService(Namespace = "http://122.169.117.148/ISysWSCndApprvl/")]
//[WebService(Namespace = "http://192.168.1.101/ISysWSCndApprvl/")]
[WebServiceBinding(ConformsTo = WsiProfiles.None)]

public class Service : System.Web.Services.WebService
{
    Hashtable htParam = new Hashtable();
    WSDataAccessLayer objDAL = new WSDataAccessLayer();
    EncryptDecrypt encryptDecrypt = new EncryptDecrypt();
    DataSet dstemp = new DataSet();
    String RefNo = String.Empty;
    String XMLRequest = String.Empty;
    String Reqno = String.Empty;
    String SeqNo = String.Empty;
    String strJsonResponce = String.Empty;
    XmlDocument xmlDocument;
   

    //AadhaarServiceInterface.AadharSoapClient AadharSoapClient = new AadhaarServiceInterface.AadharSoapClient();
    //AadhaarServiceInterface.UserCredential UserCredential = new AadhaarServiceInterface.UserCredential();

    PolicyDispatchService.UserCredential UserCredentialDispatch = new PolicyDispatchService.UserCredential();
    // PolicyDispatchService Dispatch = new PolicyDispatchService();
    PolicyDispatchService.PolicyDispatchServiceSoapClient Dispatch = new PolicyDispatchService.PolicyDispatchServiceSoapClient();


    public Service() {
        
    }

    private void InitializeComponent()
    {
    }

    [WebMethod]
    public String TestMethod(String str)
    {
        //From C#
        //String encStr = encryptDecrypt.EncryptAndEncode(str);
        //String decStr = encryptDecrypt.DecodeAndDecrypt(encStr);

        // From Android
        String decStr = encryptDecrypt.DecodeAndDecrypt(str);
        String encStr = encryptDecrypt.EncryptAndEncode(decStr);


        //String decStr = encryptDecrypt.Decrypt_String(str);

        //String encStr = encryptDecrypt.Encrypt_String(decStr);


        //String encStr = encryptDecrypt.Encrypt_String(str);
        //String decStr = encryptDecrypt.Decrypt_String(encStr);

        return encStr;
    }

    /*
    [WebMethod]
    public string OTP(String UID, String UserID)
    {
        try
        {
            htParam.Clear();
            dstemp.Clear();
            UID = "515246426787";
            UserID = "70011354";
            dstemp = objDAL.selectFromDatabase("Prc_GetAadharSoapUserCredential", htParam, "ISysARTL");
            UserCredential.UserID = dstemp.Tables[0].Rows[0]["UserId"].ToString();
            UserCredential.UserPassword = dstemp.Tables[0].Rows[0]["Password"].ToString();

            RefNo = InsertSynlog(UID, "", "GetOTPID", UserID);

            //XMLRequest = AadharSoapClient.GetOTPID(UserCredential, UID); //AadharOTP(UID);
            XMLRequest = "Success";

            htParam.Clear();
            dstemp.Clear();
            htParam.Add("@strSeqno", RefNo);
            htParam.Add("@Response", XMLRequest);
            htParam.Add("@Flag", 1);
            dstemp = objDAL.selectFromDatabase("Prc_InsAadharServiceSyncLog", htParam, "ISysARTL");

            dstemp = ConvertXMLToDataSet(XMLRequest);

            String strresponse = ds2json(dstemp).ToString();

            return strresponse;

        }

        catch (Exception ex)
        {
            return ex.ToString();
        }
    }

    [WebMethod]
    public String GetCustomerDetails(String OTP, String UID, String UserID)
    {
        try
        {
            dstemp.Clear();
            OTP = "914125";
            UID = "515246426787";
            UserID = "70011354";
            dstemp = objDAL.selectFromDatabase("Prc_GetAadharSoapUserCredential", htParam, "USRMGMT");
            UserCredential.UserID = dstemp.Tables[0].Rows[0]["UserId"].ToString();
            UserCredential.UserPassword = dstemp.Tables[0].Rows[0]["Password"].ToString();


            RefNo = InsertSynlog(UID, OTP, "GetCustomerDetails", UserID);
            //XMLRequest = AadharSoapClient.GetCustomerDetails(UserCredential, UID, OTP);
            XMLRequest = "<AADHAAR_NO>515246426787</AADHAAR_NO><ADDRESSLINE_1>Chaudhary Bhavan,Near Sai Baba Mandir</ADDRESSLINE_1><ADDRESSLINE_2>Devisha Road</ADDRESSLINE_2><ADDRESSLINE_3></ADDRESSLINE_3><FULL_NAME>Anand Surendra Chaudhary</FULL_NAME><SALUTE>Mr</SALUTE><FIRST_NAME>Anand</FIRST_NAME><MIDDLE_NAME>Surendra</MIDDLE_NAME><LAST_NAME>Chaudhary</LAST_NAME><DOB>31-01-1975</DOB><PHONE>9867910796</PHONE><GENDER>M</GENDER><EMAIL>anand3175@gmail.com</EMAIL><STATE>Maharashtra</STATE><DISTRICT>Thane</DISTRICT><CITY>Palghar West</CITY><PINCODE>401404</PINCODE><MOM_CITYCODE>253147</MOM_CITYCODE><MOM_CITYNAME>PALGHAR</MOM_CITYNAME><MOM_DISTRICTCODE>336</MOM_DISTRICTCODE><MOM_DISTRICTNAME>THANE</MOM_DISTRICTNAME><MOM_STATECODE>21</MOM_STATECODE><MOM_STATENAME>MAHARASHTRA</MOM_STATENAME><AADHAAR_NO_TXN_ID>UKC:113077</AADHAAR_NO_TXN_ID><STATUS>Success</STATUS><CODE></CODE><ERROR_DESCR></ERROR_DESCR>";
            htParam.Clear();
            dstemp.Clear();
            htParam.Add("@strSeqno", RefNo.ToString());
            htParam.Add("@Response", XMLRequest.Trim());
            htParam.Add("@Flag", 1);
            dstemp = objDAL.selectFromDatabase("Prc_InsAadharServiceSyncLog", htParam, "ISysARTL");

            xmlDocument = createRequestXml(XMLRequest);


            dstemp = ConvertXMLToDataSet(xmlDocument.InnerXml.ToString());

            String strresponse = ds2json(dstemp).ToString();

            return strresponse;

        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        // return XMLRequest;
        //  return XMLRequest;

    }
    */
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

    private XmlDocument createRequestXml(string XMLRequest)
    {
        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            strBuilder.AppendLine("<AadhaarData>");
            strBuilder.AppendLine(XMLRequest);
            strBuilder.AppendLine("</AadhaarData>");
            xmlDocument.LoadXml(strBuilder.ToString());
            return xmlDocument;
        }
        catch (Exception ex)
        {
            ex.ToString();
            return xmlDocument;
        }
    }

    public String InsertSynlog(String UID, String OTP, String Method, String UserID)
    {
        try
        {
            htParam.Clear();
            htParam.Add("@ReqParameter1", UID.ToString());
            htParam.Add("@ReqParameter2", OTP.ToString());
            htParam.Add("@CreatedBy", UserID.ToString().Trim());
            htParam.Add("@Method", Method);
            dstemp = objDAL.selectFromDatabase("Prc_InsAadharServiceSyncLog", htParam, "ISysARTL");
            Reqno = dstemp.Tables[0].Rows[0]["ReqId"].ToString().Trim();
        }

        catch (Exception ex)
        {
            return ex.ToString();
        }

        return Reqno;
    }


    [WebMethod(EnableSession = true)]
    public string SendDataToClient(string phNo, string SapCode, string AppNo, string CndName, string PanNo)
    {
        try
        {
            //dstemp.Clear();
            //dstemp = objDAL.selectFromDatabase_singleValue("prc_GetMemberCode_App", SapCode, "ISysARTL");
            //if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            //{
            //    String _memCode = Convert.ToString(dstemp.Tables[0].Rows[0]["MemCode"]);

            //}
            //return "No Memcode found for your SAPCode.";
            dstemp.Clear();
            dstemp = objDAL.selectFromDatabase("prc_CandAdmin_EnqCandListForApproval", SapCode, AppNo, CndName, PanNo, "ISysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                return encryptDecrypt.EncryptAndEncode(ds2json(dstemp));
            }
            else
                return "No Candidates found for your Search.";
        }
        catch (Exception)
        {
            return "Error!";
        }
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

    [WebMethod(EnableSession = true)]
    public String ValidateCaptchaMob(String SapCode, String Answer)
    {
		String response="";
		try{
			DataSet dstemp = new DataSet();
			Hashtable htParam = new Hashtable();
			String strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);
			String param = "SapCode : " + strSapCode +", Ans :" + Answer;

			SeqNo = ServerTrnReqLog(param, "ValidateCaptchaMob", strSapCode, "", "Candidate Manager");

			htParam.Clear();
			htParam.Add("@UserId", strSapCode);
			htParam.Add("@Answer", Answer);
			dstemp = objDAL.selectFromDatabase("prc_validateCaptcha_request", htParam, "USRMGMT");

			if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
			{
				if (dstemp.Tables[0].Rows[0]["Result1"].ToString() == "Valid")
				{
					response= "{\"Table\": [ {\"ResponseCode\": \"0\",\"Message\":\"Success\" }]}";
				}
				else {
					response =  GetCaptcha(strSapCode);
				}
			}
			else
			{
				response = GetCaptcha(strSapCode);
			}
		}catch(Exception e){
			response= "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\""+e.ToString()+"\" }]}";
		}
		updateResponse(response, SeqNo);
		return encryptDecrypt.EncryptAndEncode(response);
    }

    [WebMethod(EnableSession = true)]
    public String GetCaptchaMob(String SapCode)
    {
		String response="";
		try{
			String strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);
			String param = "SapCode : " + strSapCode;
			SeqNo = ServerTrnReqLog(param, "GetCaptchaMob", strSapCode, "", "Candidate Manager");
			response = GetCaptcha(strSapCode);
		}catch(Exception e){
			response= "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\""+e.ToString()+"\" }]}";
		}
		updateResponse(response, SeqNo);
		return encryptDecrypt.EncryptAndEncode(response);
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

    [WebMethod]
    public string ValidateRequest(String request)
    {
        try
        {
            if(Regex.IsMatch(request, "[^a-zA-Z0-9_.,:=@\\-+/ ()?<>\"\n]"))
            {
                return "Special character(s) detected."; 
            }
            else
            {
                return "Valid";
            }

            //string str = "<>&*!#©™®";
            //var regx = new Regex("^[a-zA-Z0-9_.,:]*$");
            //if (regx.IsMatch(request))
            //{
            //    return "Special character(s) detected.";
            //}
            //else
            //{
            //    return "Valid";
            //}
        }
        catch (Exception e)
        {
            return "Unable to validate the request!";
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
                    if (!ConsumeLdap.Authenticate(strUserID, cData2.cToBase64()))
                    {
                        //countLogon = int.Parse(strUserID);
                        //countLogon = countLogon + 1;
                        adAuth = "Failed";
                    }
                    else
                    {
                        adAuth = "A";
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

    [WebMethod(EnableSession = true)]
    public string updateDataFromClient(string phNo, string AppNo, string SapCode)
    {
        try
        {
            dstemp.Clear();
            int id = objDAL.updateToDataBase(dstemp, AppNo, "ISysARTL", "prc_UpdateCandidateapproval");

            dstemp.Clear();
            //dstemp = objDAL.selectFromDatabase_singleValue("prc_GetMemberCode_App", SapCode, "ISysARTL");
            //if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            //{
            //    String _memCode = Convert.ToString(dstemp.Tables[0].Rows[0]["MemCode"]);
            //    dstemp.Clear();

            //}
            //return "No Memcode found for your SAPCode.";

            dstemp = objDAL.selectFromDatabase("prc_CandAdmin_EnqCandListForApproval", SapCode, String.Empty, String.Empty, String.Empty, "ISysARTL");
            return encryptDecrypt.EncryptAndEncode(ds2json(dstemp));
        }
        catch (Exception)
        {
            return "Error!";
        }
    }

    

    [WebMethod(EnableSession = true)]
    public String GetSOAPValues(String AppName, String AppVersion)
    {
        String response = "",strAppName="";
        byte[] byteAppName = Convert.FromBase64String(AppName);
        strAppName = Encoding.UTF8.GetString(byteAppName, 0, byteAppName.Length); ;
        try
        {
            dstemp = null;
            Hashtable ht = new Hashtable();
            if (strAppName == "Agenroll+") {
                response = encryptDecrypt.GetKeys();
            }
            else
            {
                response = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Invalid request\"}]}";
            }
        }
        catch (Exception ex)
        {
            ex.ToString();
            response = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Either inavalid request or something went wrong\"}]}";
        }

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(response));

    }


    [WebMethod(EnableSession = true)]
    public String AuthenticateUser(String SAPCode, String Password, String IMEIstring, String AppVersion)
    {
        //for agent manager 

        try
        {
            //string strDecSapCode = encryptDecrypt.DecodeAndDecrypt(SAPCode);
            //string strDecPwd = encryptDecrypt.DecodeAndDecrypt(Password);
            //string strAppVersion = encryptDecrypt.DecodeAndDecrypt(AppVersion);



            //return status;

            string strEncPassword = UserSetup.UserSetupBAL.GetEncryptedPwd(Password);
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@UserID", SAPCode);//SAPCode.ToString().Trim());
            ht.Add("@Password", strEncPassword.ToString().Trim());
            //ht.Add("@Password", Password.ToString().Trim());
            ht.Add("@IMEINo", IMEIstring.ToString().Trim());
            ht.Add("@AppVersion", AppVersion);

            dstemp = objDAL.selectFromDatabase("prc_ChkUserMobLogin", ht, "USRMGMT");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    return encryptDecrypt.EncryptAndEncode(ds2json(dstemp));
                    //return "Success";
                }
                else
                {
                    return "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Memcode found for your SAPCode.\"}]}";
                }
            }
            else
            {
                return "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }
        }
        catch (Exception ex)
        {
            ex.ToString();
            return "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Either inavalid request or something went wrong\"}]}";
        }

        //end of agent manager

        //try
        // {
        //string strDecSapCode = encryptDecrypt.DecodeAndDecrypt(SAPCode);
        //string strDecPwd = encryptDecrypt.DecodeAndDecrypt(Password);
        // string strEncPassword = UserSetup.UserSetupBAL.GetEncryptedPwd(strDecPwd);
        // dstemp = null;
        // Hashtable ht = new Hashtable();
        //ht.Add("@UserID", strDecSapCode);//SAPCode.ToString().Trim());
        // ht.Add("@Password", strEncPassword.ToString().Trim());
        //ht.Add("@Password", Password.ToString().Trim());
        // ht.Add("@IMEINo", IMEIstring.ToString().Trim());
        //dstemp = objDAL.selectFromDatabase("prc_ChkUserMobLogin", ht, "USRMGMT");
        //if (dstemp != null)
        //{
        //   if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
        //  {
        //     return ds2json(dstemp);
        //    //return "Success";
        // }
        // else
        //{
        //   return "No Memcode found for your SAPCode.";
        //}
        //}
        // else
        //            {
        //               return "ds is null";
        //          }
        //     }
        //    catch (Exception ex)
        //   {
        //      return ex.ToString();
        // }

        //return "Outdated version";


        //
    }


    [WebMethod(EnableSession = true)]
    public String AuthenticateUser_WithAppVersion(String SAPCode, String Password, String IMEIstring, String AppVersion, String strTokenId)
    {
        //AuthenticateUser_WithAppVersion{SAPCode=vhP1/k33fFgVuQa3T/bTkA==; Password=ptYFTjnomq3y63YcV601Xw==; IMEIstring=353400090000527|5f8ccb33e655eb76; AppVersion=oeL5RvZzgJoxSGv5G0owDQ==; }
        //AuthenticateUser_WithAppVersion{SAPCode=AbS2uQblrxQKRpVAEBoTDw==; Password=lLqPrcd9bwxxgywc0azKoA==; IMEIstring=353400090000527|5f8ccb33e655eb76; AppVersion=oeL5RvZzgJoxSGv5G0owDQ==;}
        try
        {
            string strDecSapCode = encryptDecrypt.DecodeAndDecrypt(SAPCode);
            string strDecPwd = encryptDecrypt.DecodeAndDecrypt(Password);
            string strAppVersion = encryptDecrypt.DecodeAndDecrypt(AppVersion);
            strTokenId = "";

            String param = "SAPCode : " + strDecSapCode + ",IMEIstring : " +
                IMEIstring + ",AppVersion : " + strAppVersion;

            SeqNo = ServerTrnReqLog(param, "AuthenticateUser_WithAppVersion", strDecSapCode, "", "Candidate Manager");

            string status = CheckADAuth(strDecSapCode, strDecPwd);

            string strEncPassword = UserSetup.UserSetupBAL.GetEncryptedPwd(strDecPwd);

            //return strEncPassword;
            if (status == "X")
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Access Denied\"}]}";
            }
            else if (status == "Failed")
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"AD Authentication Failed\"}]}";
            }
            else if (status == "A" || status == "D")
            {
                if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strDecSapCode, strTokenId) == "Valid"))
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
                            strJsonResponce = ds2json(dstemp);
                            //return ds2json(dstemp);
                            //return "Success";
                        }
                        else
                        {
                            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Authentication failed.\"}]}";
                            //return "No Memcode found for your SAPCode.";
                        }
                    }
                    else
                    {
                        strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Authentication failed.\"}]}";
                        //return "ds is null";
                    }
                }
                else
                {
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                    updateResponse(strJsonResponce, SeqNo);

                    return HttpStatusCode.NonAuthoritativeInformation.ToString();
                    //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"AD Authentication Failed\"}]}";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
            //return ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    [WebMethod(EnableSession = true)]
    public String GetAuthenticationUserDetails(String SAPCode, String IMEIstring, String AppVersion, String strTokenId)
    {
        try
        {
            string strDecSapCode = encryptDecrypt.DecodeAndDecrypt(SAPCode);
            string strAppVersion = encryptDecrypt.DecodeAndDecrypt(AppVersion);

            String param = "SAPCode : " + strDecSapCode + ",IMEIstring : " +
                IMEIstring + ",AppVersion : " + strAppVersion;

            SeqNo = ServerTrnReqLog(param, "GetAuthenticationUserDetails", strDecSapCode, "", "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\""+result+"\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strDecSapCode, strTokenId) == "Valid"))
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
                        strJsonResponce = ds2json(dstemp);
                        //return ds2json(dstemp);
                        //return "Success";
                    }
                    else
                    {
                        strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Memcode found for your SAPCode.\"}]}";
                        //return "No Memcode found for your SAPCode.";
                    }
                }
                else
                {
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Memcode found for your SAPCode.\"}]}";
                    //return "ds is null";
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
            //return ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    [WebMethod(EnableSession = true)]
    public String fetchCndDetails(string AppNo, String SapCode, String strTokenId)
    {
        String param = "AppNo : " + AppNo + ",SapCode : " + SapCode;

        SeqNo = ServerTrnReqLog(param, "fetchCndDetails", SapCode, AppNo, "Candidate Manager");

        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(SapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }

            //dstemp.Clear();
            //dstemp = objDAL.selectFromDatabase_singleValue("prc_GetMemberCode_App", SapCode, "ISysARTL");
            //if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            //{
            //    String _memCode = Convert.ToString(dstemp.Tables[0].Rows[0]["MemCode"]);

            //}
            //else
            //{
            //    strJsonResponce = "No Memcode found for your SAPCode.";
            //}
            dstemp.Clear();
            dstemp = objDAL.selectFromDatabase("prc_CandAdmin_EnqCandListForApproval", SapCode, AppNo, String.Empty, String.Empty, "ISysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
                strJsonResponce = "No Details found for your Search.";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string GetAgentSurveyDetails(string SapCode, string AppNo, string AgentBrokerCode, String strTokenId)
    {

        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);
            string strAgentBrokerCode = encryptDecrypt.DecodeAndDecrypt(AgentBrokerCode);
            DataSet dsAgentSurveyQuestion = new DataSet();

            String param = "SapCode : " + strSapCode + " , AppNo : " + strAppNo + " , AgentBrokerCode : " + strAgentBrokerCode; ;
            SeqNo = ServerTrnReqLog(param, "GetAgentSurveyDetails", strSapCode, strAppNo, "Candidate Manager");

            //dstemp.Clear();
            //dstemp = objDAL.selectFromDatabase_singleValue("prc_GetMemberCode_App", strSapCode, "ISysARTL");
            //if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            //{
            //    string _memCode = Convert.ToString(dstemp.Tables[0].Rows[0]["MemCode"]);

            //}
            //else
            //{
            //    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Memcode found for your SAPCode.\"}]}";
            //}

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

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
                    strJsonResponce = "[" + jsonResponse + "]";
                }
                else
                {
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No record found\"}]}";
                }
            }
            else
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No record found\"}]}";
        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string SubmitAgentSurveyDetail(string SapCode, string AgentBrokerCode, string SMCode, string QuestionID, string SMResolutionRemark, string SurveyCode, string CreatedBy, String CallingDate, String strTokenId)
    {
        try
        {
            string strSMCode = encryptDecrypt.DecodeAndDecrypt(SMCode);
            string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);
            string strAgentBrokerCode = encryptDecrypt.DecodeAndDecrypt(AgentBrokerCode);
            DataSet dsAgentSurveyQuestion = new DataSet();

            String param = "SapCode : " + strSapCode + " , SMCode : " + strSMCode + " , AgentBrokerCode : "
                + strAgentBrokerCode + ", CallingDate : " + CallingDate;
            SeqNo = ServerTrnReqLog(param, "SubmitAgentSurveyDetail", strSapCode, strSMCode, "Candidate Manager");


            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();
            }

            //dstemp.Clear();
            //dstemp = objDAL.selectFromDatabase_singleValue("prc_GetMemberCode_App", strSapCode, "ISysARTL");
            //if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            //{
            //    string _memCode = Convert.ToString(dstemp.Tables[0].Rows[0]["MemCode"]);

            //}
            //else
            //{
            //    strJsonResponce = "No Memcode found for your SAPCode.";
            //}

            dstemp.Clear();
            Hashtable ht = new Hashtable();
            ht.Add("@QuestionID", QuestionID.ToString().Trim());
            ht.Add("@SMResoremark", SMResolutionRemark.ToString().Trim());
            ht.Add("@SMCode", strSMCode.ToString().Trim());
            ht.Add("@SurveyTypeCode", SurveyCode.ToString().Trim());
            ht.Add("@AgntBrokerCode", strAgentBrokerCode.ToString().Trim());
            ht.Add("@CreatedBy", strSapCode.ToString().Trim());
            ht.Add("@CallingDate", CallingDate.ToString().Trim());

            dstemp = objDAL.selectFromDatabase("InsAgntSurvyFedbck", ht, "ISysCHMS");
            if (dstemp != null)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    [WebMethod(EnableSession = true)]
    public string FetchPrefExmCndList(string phNo, string SapCode, string AppNo, string CndName, string PanNo, String strTokenId)
    {
        String param = "phNo : " + phNo + ",SapCode : " + SapCode + "AppNo : " + AppNo + "CndName : " + CndName + "PanNo : " + PanNo;
        SeqNo = ServerTrnReqLog(param, "FetchPrefExmCndList", SapCode, AppNo, "Candidate Manager");


        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(SapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            dstemp = objDAL.selectFromDatabase_singleValue("prc_GetMemberCode_App", SapCode, "ISysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                string memCode = dstemp.Tables[0].Rows[0]["MemCode"].ToString();
                dstemp.Clear();

                //dstemp = objDAL.selectFromDatabase("Prc_EnqCandListForPreffExmDtls", memCode, AppNo, String.Empty, String.Empty, "cndEnq", "ISysARTL");
                dstemp = objDAL.fetchDetails(SapCode, "prc_GetPrefExmDateForApp", "ISysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "No Candidates found for your Search.";
                }
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string GetDataForSearch(string SapCode, string AppNo, string CndName, string PanNo, string Flag, string EntryDate, String strTokenId)
    {


        try
        {
            string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strPanNo = encryptDecrypt.DecodeAndDecrypt(PanNo);

            

            String param = "AppNo : " + strAppNo + ",SapCode : " + strSapCode + ",CndName : " + CndName + ",PanNo : " + strPanNo + ",Flag : " + Flag + ",EntryDate : " + EntryDate;
            SeqNo = ServerTrnReqLog(param, "GetDataForSearch", strSapCode, strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }

            //dstemp.Clear();
            //dstemp = objDAL.selectFromDatabase_singleValue("prc_GetMemberCode_App", strSapCode, "ISysARTL");
            //if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            //{
            //    string _memCode = Convert.ToString(dstemp.Tables[0].Rows[0]["MemCode"]);

            //}
            //else
            //{
            //    strJsonResponce = "No Memcode found for your SAPCode.";

            //}

            dstemp.Clear();
            Hashtable htable = new Hashtable();
            htable.Clear();
            if (Flag == "ARS")
            {
                //for agent survey
                htable.Add("@CndNo", string.Empty);
                htable.Add("@Appno", strAppNo);
                htable.Add("@GivenName", CndName);
                htable.Add("@Surname", string.Empty);
                htable.Add("@PAN", strPanNo);
                htable.Add("@AgentBrokeCode", string.Empty);
                htable.Add("@CreateFrmDtim", EntryDate);
                htable.Add("@CreateToDtim", string.Empty);
                dstemp = objDAL.selectFromDatabase("prc_GetAgentCodeDetails", htable, "ISysCHMS");
            }
            else
            {
                htable.Add("@BMCode", strSapCode);
                htable.Add("@AppNo", strAppNo);
                htable.Add("@CndName", CndName);
                htable.Add("@PanNo", strPanNo);
                htable.Add("@Flag", Flag);
                htable.Add("@EntryDate", EntryDate);
                dstemp = objDAL.selectFromDatabase("Prc_GetDataForAppSearch", htable, "ISysARTL");
            }
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
                strJsonResponce = "No Candidates found for your Search.";

        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }



    [WebMethod(EnableSession = true)]
    public string GetReminderData(string SapCode, String strTokenId)
    {
        String param = "SapCode : " + SapCode;
        SeqNo = ServerTrnReqLog(param, "GetReminderData", SapCode, "", "Candidate Manager");

        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(SapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            dstemp = objDAL.GetRemainderData("Prc_GetReminderDate", SapCode, "ISysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
                strJsonResponce = "No Candidates found for your Search.";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string SetReminderData(string TodayDate, string RemainderDate, string SystemTime, string RemainderTime, string Remainder, string SapCode, String strTokenId)
    {
        String param = "TodayDate : " + TodayDate + ",RemainderDate : " + RemainderDate + ",SystemTime : " + SystemTime + "RemainderTime : " + RemainderTime + "Remainder : " + Remainder;
        SeqNo = ServerTrnReqLog(param, "SetReminderData", "", "", "Candidate Manager");

        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            FileStream fs = new FileStream("C:/file22.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(TodayDate);
            sw.Close();
            fs.Close();


            dstemp.Clear();
            dstemp = objDAL.SetRemainderData("Prc_SetReminderDate", TodayDate, RemainderDate, SystemTime, RemainderTime, Remainder, "ISysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
                strJsonResponce = "sandeep";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    /// <summary>
    ///     Uses NEWTON API to Serialize the DataSet to JSON String.
    /// </summary>
    /// <param name="ds"></param>
    /// <returns name="Serialized String"></returns>
    public static string ds2json(DataSet ds)
    {
        return JsonConvert.SerializeObject(ds, Newtonsoft.Json.Formatting.Indented);
    }


    //public string DataTableToJSONWithJSONNet(DataTable table)
    //{
    //    string JSONString = string.Empty;
    //    JSONString = JSONConvert.SerializeObject(table);
    //    return JSONString;
    //}��

    //add by rupesh on 18/9/2014 start


    [WebMethod(EnableSession = true)]
    public string GetLoginDtls(string Uid, string Pwd, String strTokenId)
    {
        String param = "Uid : " + Uid + ",Pwd : " + Pwd;
        SeqNo = ServerTrnReqLog(param, "GetLoginDtls", "", "", "Candidate Manager");

        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(Uid, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@UserId", Uid);
            htParam.Add("@Pwd", Pwd);
            dstemp = objDAL.selectFromDatabase("prc_GetLoginDtls", htParam, "ISysMob");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                //return ds2json(dstemp);
                strJsonResponce = "1";
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception)
        {
            return "Error!";
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    //add by rupesh on 9/10/2014 start
    [WebMethod(EnableSession = true)]
    public void InsertImageByet(byte[] imgByte)
    {
        String param = "imgByte : " + imgByte;
        SeqNo = ServerTrnReqLog(param, "InsertImageByet", "", "", "Candidate Manager");
        try
        {

            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@imgByte", imgByte);

            dstemp = objDAL.selectFromDatabase("prc_imageByte", htParam, "ISysMob");

        }
        catch (Exception ex)
        {
            //return "Error!";
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
    }

    [WebMethod(EnableSession = true)]
    public string GetImageByteCode(String Id, String strTokenId)
    {
        try
        {
            if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(Id, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@Id", Id.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("prc_ConverToImage", htParam, "ISysMob");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                return encryptDecrypt.EncryptAndEncode(ds2json(dstemp));

            }
            else
            {
                return "No Memcode found for your SAPCode.";
            }

        }
        catch (Exception)
        {
            return "Error!";

        }

    }

    //@UrnNo,@Appno,@Givenname,@PAN
    [WebMethod(EnableSession = true)]
    public string GetCandDocs(string UrnNo, string Appno, string Givenname, string PAN, string MemCode, string EntryDate, String SapCode, String strTokenId)
    {
        String param = "UrnNo : " + UrnNo + ",Appno : " + Appno + "Givenname : " + Givenname + "PAN : " + PAN + "MemCode : " + MemCode + "EntryDate : " + EntryDate;
        SeqNo = ServerTrnReqLog(param, "GetCandDocs", "", Appno, "Candidate Manager");

        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@UrnNo", UrnNo.ToString().Trim());
            ht.Add("@Appno", Appno.ToString().Trim());
            ht.Add("@Givenname", Givenname.ToString().Trim());
            ht.Add("@PAN", PAN.ToString().Trim());
            ht.Add("@EmpCode", MemCode.ToString().Trim());
            ht.Add("@EntryDate", EntryDate.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetCandDocs", ht, "IsysARTL");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);

                }
                else
                {
                    strJsonResponce = "No Memcode found for your SAPCode.";
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();

        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;

    }


    [WebMethod(EnableSession = true)]
    public string GetImageForDocType(String Appno, String DocType, String SapCode, String strTokenId)
    {

        try
        {
            string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);

            String param = "AppNo : " + strAppno + ",DocType : " + DocType;
            SeqNo = ServerTrnReqLog(param, "GetImageForDocType", "", strAppno, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }

            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@Appno", strAppno.ToString().Trim());
            htParam.Add("@DocType", DocType.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetDocForMob", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);

            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();

        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;

    }






    [WebMethod(EnableSession = true)]
    public string InsertCandDocsFrmMob(String Appno, byte[] Image, String DocType, String UploadedBy,
                                            String lblimgshrt, String lblCndNoValue, String FileType, String strTokenId)
    {



        try
        {
            string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);
            string strUploadedBy = encryptDecrypt.DecodeAndDecrypt(UploadedBy);
            string strlblCndNoValue = encryptDecrypt.DecodeAndDecrypt(lblCndNoValue);

            String param = "AppNo : " + strAppno + ",DocType : " + DocType + ",UploadedBy : " + strUploadedBy + ",lblimgshrt : " + lblimgshrt + ",lblCndNoValue : " + strlblCndNoValue + ",FileType : " + FileType + ", strTokenId" + strTokenId;
            SeqNo = ServerTrnReqLog(param, "InsertCandDocsFrmMob", "", strAppno, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strUploadedBy, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@Appno", strAppno.ToString().Trim());
            ht.Add("@Image", Image);
            ht.Add("@DocType", DocType.ToString().Trim());
            ht.Add("@UploadedBy", strUploadedBy.ToString().Trim());
            ht.Add("@FileType", FileType.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_InsCandDocsFrmMob", ht, "IsysARTL");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    //return ds2json(dstemp);
                    //CreateFolderOfDoc(lblimgshrt, strlblCndNoValue, Image);
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = dstemp.Tables[0].Rows.Count.ToString();
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();

        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;

    }




    [WebMethod(EnableSession = true)]
    public string GetAllSupportDocs(String AppNo, String SapCode, String strTokenId)
    {
        String param = "AppNo : " + AppNo;
        SeqNo = ServerTrnReqLog(param, "GetAllSupportDocs", "", AppNo, "Candidate Manager");


        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }

            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@Appno", AppNo.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetAllSupportDocs", ht, "IsysARTL");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = dstemp.Tables[0].Rows.Count.ToString();
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }



    //=add by rupesh on 17/10/2014


    [WebMethod(EnableSession = true)]
    public string GetCandidateMvmt_mobile(String Appno, String SapCode, String strTokenId)
    {

        try
        {
            string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);

            String param = "AppNo : " + strAppno;
            SeqNo = ServerTrnReqLog(param, "GetCandidateMvmt_mobile", "", strAppno, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }

            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@Appno", strAppno.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetCandidateMvmt_mobile", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);

            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();

        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    [WebMethod(EnableSession = true)]
    public string GetCandidateBasicDetail_mobile(String Appno, String Flag, String SapCode, String strTokenId)
    {
        try
        {
            string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);
            String param = "AppNo : " + strAppno + ",Flag : " + Flag;
            SeqNo = ServerTrnReqLog(param, "GetCandidateBasicDetail_mobile", "", strAppno, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) 
                && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@Appno", strAppno.ToString().Trim());
            htParam.Add("@Flag", Flag.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetCandidateBasicDetail_mobile", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                if (Flag.ToString().Trim() == "DD")
                {
                    //GetDetailsFromService(strAppno.ToString().Trim());
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Your request for dispatch details submitted, please check after some time\"}]}";
                }
                else
                {
                    strJsonResponce = "No record found";
                }
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    public void GetDetailsFromService(String AppNo)
    {
        DataSet dstemp = new DataSet();
        Hashtable htParam = new Hashtable();
        String SeqNo = string.Empty;
        try
        {

            string reqXML = "", AgentCode = "";
            dstemp.Clear();
            htParam.Clear();

            SeqNo = ServerTrnReqLog(AppNo, "GetDetailsFromService", "", AppNo, "Candidate Manager");


            htParam.Add("@AppNo", AppNo);//Added by usha  
            dstemp = objDAL.selectFromDatabase("Prc_GetAgentCodeForAppNo", htParam, "IsysARTL");

            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                AgentCode = dstemp.Tables[0].Rows[0]["Agent_Broker_Code"].ToString();
            }
            else
            {
                AgentCode = "";
            }

            dstemp.Clear();
            htParam.Clear();

            htParam.Add("@AuthID", 2);//Added by usha  
            dstemp = objDAL.selectFromDatabase("Prc_GetUserCredential", htParam, "IsysARTL");
            UserCredentialDispatch.UserID = dstemp.Tables[0].Rows[0]["UserId"].ToString();
            UserCredentialDispatch.UserPassword = dstemp.Tables[0].Rows[0]["Password"].ToString();
            SeqNo = InsertSynlog(AgentCode, "", "GetAgentDispatchDetaipls", AppNo);
            dstemp.Clear();

            // dsDispatch = Dispatch.GetPolicyDispatchStatus(AgencyCode);
            dstemp = Dispatch.GetPolicyDispatchStatus(UserCredentialDispatch, AgentCode);
            reqXML = ConvertDataSetToXml(dstemp);
            htParam.Clear();
            dstemp.Clear();
            htParam.Add("@strSeqno", SeqNo);
            htParam.Add("@Response", reqXML);
            htParam.Add("@Flag", 1);
            dstemp = objDAL.selectFromDatabase("Prc_InsAadharServiceSyncLog", htParam, "IsysARTL");

            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                updateResponse(ds2json(dstemp), SeqNo);
            }

        }
        catch (Exception ex)
        {
            ex.ToString();
        }
    }

    public static string ConvertDataSetToXml(DataSet ds)
    {
        StringWriter sw = new StringWriter();
        ds.WriteXml(sw, XmlWriteMode.IgnoreSchema);
        string XML = sw.ToString();
        return XML;
    }


    [WebMethod(EnableSession = true)]
    public string CnandidateApproved_mobile(String Appno, String Flag, String ModuleID, String SapCode, String strTokenId)
    {


        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(Appno);

            String param = "AppNo : " + strAppNo + ",Flag : " + Flag + ",ModuleID : " + ModuleID;
            SeqNo = ServerTrnReqLog(param, "CnandidateApproved_mobile", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@Appno", strAppNo.ToString().Trim());
            htParam.Add("@Flag", Flag.ToString().Trim());
            htParam.Add("@ModuleID", ModuleID.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("prc_CnandidateApproved_mobile", htParam, "IsysARTL");
            strJsonResponce = "Candidate Apporved";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string setPrefDate_mobile(String Appno, String date, String ModuleID, String SapCode, String strTokenId)
    {


        try
        {
            string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);

            String param = "AppNo : " + strAppno + ",date : " + date + ", ModuleID : " + ModuleID;
            SeqNo = ServerTrnReqLog(param, "setPrefDate_mobile", "", strAppno, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@Appno", strAppno.ToString().Trim());
            htParam.Add("@date", date.ToString().Trim());
            htParam.Add("@ModuleID", ModuleID.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("prc_SetPreferedDate_mobile", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = "Date is set";
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    [WebMethod(EnableSession = true)]
    public string GetCFRStatus_mobile(string AppNo, String SapCode, String strTokenId)
    {

        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);

            String param = "AppNo : " + strAppNo;
            SeqNo = ServerTrnReqLog(param, "GetCFRStatus_mobile", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            Hashtable htparam = new Hashtable();
            htparam.Add("@AppNo", strAppNo);
            dstemp = objDAL.selectFromDatabase("Prc_GetProcessType_Mob", htparam, "ISysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                String _ProcessType = Convert.ToString(dstemp.Tables[0].Rows[0]["ProcessType"]).Trim();
                String _CndNo = Convert.ToString(dstemp.Tables[0].Rows[0]["CndNo"]).Trim();
                dstemp.Clear();
                Hashtable htTable = new Hashtable();
                htTable.Add("@CndNo", _CndNo);
                htTable.Add("@Flag", _ProcessType);
                dstemp = objDAL.selectFromDatabase("Prc_GetCFRStatus_Mob", htTable, "ISysARTL");
                //dstemp = objDAL.GetDataForAppSearch("Prc_GetCFRStatus_Mob", _CndNo, _ProcessType, "ISysARTL");
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "No Candidates found for your Search.";
                }
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            //return ex.ToString();
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    [WebMethod(EnableSession = true)]
    public string CFR_Responde_mobile(string AppNo, string CfrFor, string RemarkId, string RespondedBy, string ModuleID, String SapCode, String strTokenId)
    {

        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strRespondedBy = encryptDecrypt.DecodeAndDecrypt(RespondedBy);

            String param = "AppNo : " + strAppNo + ",CfrFor : " + CfrFor + "RemarkId : " + RemarkId +
             "RespondedBy : " + strRespondedBy + " ModuleID : " + ModuleID;
            SeqNo = ServerTrnReqLog(param, "CFR_Responde_mobile", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            Hashtable htparam = new Hashtable();
            htparam.Add("@AppNo", strAppNo);
            htparam.Add("@CfrFor", CfrFor);
            htparam.Add("@RemarkId", RemarkId);
            htparam.Add("@RespondedBy", strRespondedBy);
            htparam.Add("@ModuleID", ModuleID);

            dstemp = objDAL.selectFromDatabase("Prc_Submit_AssignedCFR_bracheduser_Mob", htparam, "ISysARTL");

            strJsonResponce = "Responded Successfully";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
            //return "error";
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    [WebMethod(EnableSession = true)]
    public string GetCFRdesc_Mobile(string Appno, string desc, String SapCode, String strTokenId)
    {

        try
        {
            string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);

            String param = "AppNo : " + strAppno + ",desc : " + desc;
            SeqNo = ServerTrnReqLog(param, "GetCFRdesc_Mobile", "", strAppno, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@Appno", strAppno.ToString().Trim());
            htParam.Add("@desc", desc.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetCFRdesc_Mob", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    [WebMethod(EnableSession = true)]
    public string AllCounts_Mobile(string SAPCode, String AgentType, String strTokenId)
    {

        try
        {
            string strSAPCode = encryptDecrypt.DecodeAndDecrypt(SAPCode);

            String param = "SapCode : " + strSAPCode;
            SeqNo = ServerTrnReqLog(param, "AllCounts_Mobile", strSAPCode, "", "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSAPCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@SAPCode", strSAPCode.ToString().Trim());
            htParam.Add("@AgentType", AgentType.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("prc_AllCounts_mobile", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;

    }





    [WebMethod(EnableSession = true)]
    public string UpdateCandDocsFrmMob(String Appno, byte[] Image, String DocType, String DocStatusCode,
            String lblimgshrt, String lblCndNoValue, String SapCode, String strTokenId)
    {

        try
        {
            string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);
            string strlblCndNoValue = encryptDecrypt.DecodeAndDecrypt(lblCndNoValue);

            String param = "AppNo : " + strAppno + ",Image : " + Image + "DocType : " + DocType + "DocStatusCode : " + DocStatusCode + "lblimgshrt : " + lblimgshrt + "lblCndNoValue : " + strlblCndNoValue;
            

            SeqNo = ServerTrnReqLog(param, "UpdateCandDocsFrmMob", "", strAppno, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }

            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@Appno", strAppno.ToString().Trim());
            ht.Add("@Image", Image);
            ht.Add("@DocType", DocType.ToString().Trim());
            ht.Add("@DocStatus", DocStatusCode.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_UpdCandDocsFrmMob", ht, "IsysARTL");

            //ReCreateFolderOfDoc(lblimgshrt, lblCndNoValue, DocStatusCode, Image);

            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    //return ds2json(dstemp);
                    //CreateFolderOfDoc(lblimgshrt, strlblCndNoValue, Image);
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = dstemp.Tables[0].Rows.Count.ToString();
                }
            }
            else
                strJsonResponce = "ds is null";

            //return "Image Uploaded and Updated Successfully";

        }
        catch (Exception ex)
        {
            strJsonResponce = "In Catch block = " + ex.ToString();

        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;

    }


    /*
                string jsonInput = model.jsonInput;
                string json = encryptDecrypt.DecodeAndDecrypt(jsonInput);
                JObject jObject = JObject.Parse(json);
                string strSapCode = Convert.ToString(jObject["SapCode"]).Trim();
                string strTokenId = Convert.ToString(jObject["strTokenId"]).Trim(); 
     */



    [WebMethod(EnableSession = true)]
    public string UpdateCandDocsFrmMob1(String Appno, byte[] Image, String DocType, String DocStatusCode,
            String lblimgshrt, String lblCndNoValue, String SapCode, String strTokenId)
    {

        String param = "AppNo : " + Appno + ",Image : " + Image + ",DocType : " + DocType + ",DocStatusCode : " + DocStatusCode + ",lblimgshrt : " + lblimgshrt + ",lblCndNoValue : " + lblCndNoValue;
        SeqNo = ServerTrnReqLog(param, "UpdateCandDocsFrmMob1", "", Appno, "Candidate Manager");

        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@Appno", Appno.ToString().Trim());
            ht.Add("@Image", Image);
            ht.Add("@DocType", DocType.ToString().Trim());
            ht.Add("@DocStatus", DocStatusCode.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_UpdCandDocsFrmMob", ht, "IsysARTL");

            //ReCreateFolderOfDoc(lblimgshrt, lblCndNoValue, DocStatusCode, Image);
            strJsonResponce = "Image Uploaded and Updated Successfully";

        }
        catch (Exception ex)
        {
            strJsonResponce = "In Catch block = " + ex.ToString();

        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;

    }



    [WebMethod(EnableSession = true)]
    public string UpdateProfileFrmMob(String AppNo, String AgntType, String IsWrkngInOthrRelnce, String CompName,
         String YrsInInsrnce, String YrsInRelnce, String TypOfVchl, String VechManf, String DlrCompName, String DlrOthr, String AvgMnthVol,
         String Comp1Name, String MnthVolFrComp1, String Comp2Name, String MnthVolFrComp2, String RGIMnthVol, String TotBsnMtr, String TotBsnHlth,
         String TotBsnCmrclLine, String RGIBsnMtr, String RGIBsnHlth, String RGIBsnCmrclLine, String UpdateBy, String SapCode, String strTokenId)
    {



        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strUpdateBy = encryptDecrypt.DecodeAndDecrypt(UpdateBy);

            String param = "AppNo : " + strAppNo + ",AgntType : " + AgntType + "IsWrkngInOthrRelnce : " + IsWrkngInOthrRelnce + "CompName : " + CompName + "YrsInInsrnce : "
            + YrsInInsrnce + "YrsInRelnce : " + YrsInRelnce + "TypOfVchl : " + TypOfVchl + "VechManf : " + VechManf + "DlrCompName : " + DlrCompName + "DlrOthr : " + DlrOthr +
            "AvgMnthVol : " + AvgMnthVol + "Comp1Name : " + Comp1Name + "MnthVolFrComp1 : " + MnthVolFrComp1 + "Comp2Name : " + Comp2Name + "MnthVolFrComp2 : " + MnthVolFrComp2 +
            "RGIMnthVol : " + RGIMnthVol + "TotBsnMtr : " + TotBsnMtr + "TotBsnHlth : " + TotBsnHlth + "TotBsnCmrclLine : " + TotBsnCmrclLine + "RGIBsnMtr : " + RGIBsnMtr + "RGIBsnHlth : " + RGIBsnHlth +
            "RGIBsnCmrclLine : " + RGIBsnCmrclLine + "UpdateBy : " + strUpdateBy;
            SeqNo = ServerTrnReqLog(param, "UpdateProfileFrmMob", "", strAppNo, "Candidate Manager");

            if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            //Hashtable ht = new Hashtable();
            htParam.Clear();
            htParam.Add("@AppNo", strAppNo.ToString().Trim());
            htParam.Add("@agntype", AgntType.ToString().Trim());
            htParam.Add("@IsWrkng", IsWrkngInOthrRelnce.ToString().Trim());
            htParam.Add("@compName", CompName.ToString().Trim());
            htParam.Add("@NoOfYrsIns", YrsInInsrnce.ToString().Trim());
            htParam.Add("@NoOfYrs", YrsInRelnce.ToString().Trim());
            htParam.Add("@TypOfVchl", TypOfVchl.ToString().Trim());
            htParam.Add("@VechManf", VechManf.ToString().Trim());
            htParam.Add("@DlrCompName", DlrCompName.ToString().Trim());
            htParam.Add("@DlrOthr", DlrOthr.ToString().Trim());
            htParam.Add("@AvgMnthlyIncm", AvgMnthVol.ToString().Trim());
            htParam.Add("@Comp1Name", Comp1Name.ToString().Trim());
            htParam.Add("@MnthVol1", MnthVolFrComp1.ToString().Trim());
            htParam.Add("@Comp2Name", Comp2Name.ToString().Trim());
            htParam.Add("@MnthVol2", MnthVolFrComp2.ToString().Trim());
            htParam.Add("@RGIMnthVol", RGIMnthVol.ToString().Trim());
            htParam.Add("@TotBsnMtr", TotBsnMtr.ToString().Trim());
            htParam.Add("@TotBsnHlth", TotBsnHlth.ToString().Trim());
            htParam.Add("@TotBsnCmrclLine", TotBsnCmrclLine.ToString().Trim());
            htParam.Add("@RGIBsnMtr", RGIBsnMtr.ToString().Trim());
            htParam.Add("@RGIBsnHlth", RGIBsnHlth.ToString().Trim());
            htParam.Add("@RGIBsnCmrclLine", RGIBsnCmrclLine.ToString().Trim());
            htParam.Add("@UpdtedBy", strUpdateBy.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_UpdateProfileDetails_Mob", htParam, "IsysARTL");

            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    //return ds2json(dstemp);

                    strJsonResponce = "Successfully Updated";
                }
                else
                {
                    strJsonResponce = "No Memcode found for your SAPCode.";
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = "In Catch block = " + ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;

    }


    [WebMethod(EnableSession = true)]
    public String MobileMenu(String SAPCode, String strTokenId)
    {
        String param = "SapCode : " + SAPCode;
        SeqNo = ServerTrnReqLog(param, "MobileMenu", SAPCode, "", "Candidate Manager");
        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(SAPCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@UserID", SAPCode.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("prc_mobMenuList", ht, "USRMGMT_mob");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                    //return "Success";
                }
                else
                {
                    strJsonResponce = "No Memcode found for your SAPCode.";
                }
            }
            else
            {
                strJsonResponce = "ds is null";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string GetARTLDashBoardDtls_mobile(string SAPCode, string Flag, String strTokenId)
    {
        String param = "Flag : " + Flag + ",SapCode : " + SAPCode;
        SeqNo = ServerTrnReqLog(param, "GetARTLDashBoardDtls_mobile", SAPCode, "", "Candidate Manager");
        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(SAPCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@SAPCode", SAPCode.ToString().Trim());
            htParam.Add("@Flag", Flag.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("prc_GetARTLDashBoardDtls_mobile", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }



    [WebMethod(EnableSession = true)]
    public string GetCfrRaisedCandidate(string SapCode, string AppNo, string CndName, string MemberType, string EntryDate, String strTokenId)
    {
        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);

            String param = "AppNo : " + strAppNo + ",SapCode : " + strSapCode + "CndName : " + CndName + "MemberType : " + MemberType + "EntryDate : " + EntryDate;
            SeqNo = ServerTrnReqLog(param, "GetCfrRaisedCandidate", strSapCode, strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();
            }

            dstemp.Clear();
            Hashtable htCFR = new Hashtable();
            htCFR.Add("@BMCode", strSapCode);
            htCFR.Add("@AppNo", strAppNo);
            htCFR.Add("@CndName", CndName);
            htCFR.Add("@MemberType", MemberType);
            htCFR.Add("@EntryDate", EntryDate);

            dstemp = objDAL.selectFromDatabase("Prc_GetAdvDetailsForCFR_mobile", htCFR, "ISysARTL");
            if (dstemp.Tables.Count > 0)
            {
                if (dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "No Candidates found for your Search.";
                }
            }
            else
            {
                strJsonResponce = "No Candidates found for your Search.";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    [WebMethod(EnableSession = true)]
    public string Get_RecruiterDetail(string MemberCode, string MemberType, String SapCode, String strTokenId)
    {
        String param = "MemberCode : " + MemberCode + ",MemberType : " + MemberType;
        SeqNo = ServerTrnReqLog(param, "Get_RecruiterDetail", "", "", "Candidate Manager");

        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            dstemp = objDAL.selectFromDatabase_singleValue("prc_GetMemberCode_App", MemberCode, "ISysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                String _memCode = Convert.ToString(dstemp.Tables[0].Rows[0]["MemCode"]);
                dstemp.Clear();
                Hashtable htCFR = new Hashtable();
                htCFR.Add("@MemberCode", _memCode);
                htCFR.Add("@MemberType", MemberType);
                dstemp = objDAL.selectFromDatabase("Prc_GetRecruiterDtls_forMob", htCFR, "ISysARTL");
                //String _fail= Convert.ToString(dstemp.Tables[0].Rows[0]["FAIL"]);
                if (dstemp.Tables.Count > 0)
                {
                    if (dstemp.Tables[0].Rows.Count > 0 /*&& (!(_fail == "F"))*/)
                    {
                        strJsonResponce = ds2json(dstemp);
                    }

                    else
                    {
                        strJsonResponce = "No Candidates found for your Search.";
                    }

                    //if (dstemp.Tables[0].Rows[0]["FAIL"] == "F")
                    //{
                    //    return "FAIL"
                    //}
                }
                else
                {
                    strJsonResponce = "No Candidates found for your Search.";
                }
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string Get_SyncedPAN_verification_mobile(string PAN, string MobUniqueRegID, String SapCode, String strTokenId)
    {

        try
        {
            
            dstemp.Clear();
            htParam.Clear();
            string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());
            string strRegID = encryptDecrypt.DecodeAndDecrypt(MobUniqueRegID.Trim());

            String param = "PAN : " + strPAN + ",MobUniqueRegID : " + strRegID;
            SeqNo = ServerTrnReqLog(param, "Get_SyncedPAN_verification_mobile", "", "", "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }

            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@PAN", strPAN);
            htParam.Add("@MobUniquerecId", strRegID.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("prc_syncedPANValidation", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string Get_PAN_verification_mobile(string PAN, string ApplicationNo, String SapCode, String strTokenId)
    {

        try
        {
            
            dstemp.Clear();
            htParam.Clear();
            string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN.Trim());

            String param = "PAN : " + strPAN + ",ApplicationNo : " + ApplicationNo;
            SeqNo = ServerTrnReqLog(param, "Get_PAN_verification_mobile", "", "", "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }

            htParam.Clear();
            htParam.Add("@PAN", strPAN);
            htParam.Add("@ApplicationNo", ApplicationNo.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetChkPANExist_MobApp", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    //Getting Information Of state by Rupesh Chakole on 04/06/2015
    [WebMethod(EnableSession = true)]
    public string Get_StateDetails(string StateCode, string District, string City, string Area, string Pincode, String SapCode, String strTokenId)
    {
        String param = "StateCode : " + StateCode + ",District : " + District + "City : " + City + "Area : " + Area + "Pincode : " + Pincode;
        SeqNo = ServerTrnReqLog(param, "Get_StateDetails", "", "", "Candidate Manager");
        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@StateCode", StateCode.ToString().Trim());
            htParam.Add("@District", District.ToString().Trim());
            htParam.Add("@City", City.ToString().Trim());
            htParam.Add("@Area", Area.ToString().Trim());
            htParam.Add("@Pincode", Pincode.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetStateDetails_mobile", htParam, "USRMGMT");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }



    [WebMethod(EnableSession = true)]
    public string Get_SponsershipRequest_detail(string AppNo, string ReqDate, string BranchCode, string AdvName, string Type, String SapCode, String strTokenId)
    {


        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);

            String param = "AppNo : " + strAppNo + ", ReqDate : " + ReqDate + "BranchCode : " + BranchCode + ", AdvName : " + AdvName + ", Type : " + Type
                + ", strTokenId:" + strTokenId + ", strSapCode:" + strSapCode;
            SeqNo = ServerTrnReqLog(param, "Get_SponsershipRequest_detail", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty)
                && ValidateToken(strSapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@AppNo", strAppNo.ToString().Trim());
            htParam.Add("@ReqDate", ReqDate.ToString().Trim());
            htParam.Add("@BranchCode", BranchCode.ToString().Trim());
            htParam.Add("@AdvName", AdvName.ToString().Trim());
            htParam.Add("@Type", Type.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetAdvForHrsTrn_Mob", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    [WebMethod(EnableSession = true)]
    public string Get_VhclManfList()
    {

        //SeqNo = ServerTrnReqLog(param, "fetchCndDetails", "", "", "Candidate Manager");

        try
        {
            dstemp.Clear();

            dstemp = objDAL.selectFromDatabase("Prc_GetddlVchlManf", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string InsCndProfiling_mobile(String AgntType, String CndNo, String AppNo, String IsWrkngInOthrRelnce, String CompName,
        String YrsInInsrnce, String YrsInRelnce, String TypOfVchl, String VechManf, String DlrCompName, String DlrOthr, String AvgMnthVol,
        String Comp1Name, String MnthVolFrComp1, String Comp2Name, String MnthVolFrComp2, String RGIMnthVol, String TotBsnMtr, String TotBsnHlth,
        String TotBsnCmrclLine, String RGIBsnMtr, String RGIBsnHlth, String RGIBsnCmrclLine, String CreateBy, String strTokenId)
    {

        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strCndNo = encryptDecrypt.DecodeAndDecrypt(CndNo);
            string strCreateBy = encryptDecrypt.DecodeAndDecrypt(CreateBy);

            String param = "AgntType : " + AgntType + ",CndNo : " + strCndNo + "AppNo : " + strAppNo + ",IsWrkngInOthrRelnce : " + IsWrkngInOthrRelnce +
           "CompName : " + CompName + "YrsInInsrnce : " + YrsInInsrnce + "YrsInRelnce : " + YrsInRelnce + "TypOfVchl : " + TypOfVchl + "VechManf : " + VechManf +
           "DlrCompName : " + DlrCompName + "DlrOthr : " + DlrOthr + "AvgMnthVol : " + AvgMnthVol + "Comp1Name : " + Comp1Name + "MnthVolFrComp1 : " + MnthVolFrComp1 +
           "Comp2Name : " + Comp2Name + "MnthVolFrComp2 : " + MnthVolFrComp2 + "RGIMnthVol : " + RGIMnthVol + "TotBsnMtr : " + TotBsnMtr + "TotBsnHlth : " + TotBsnHlth +
           "TotBsnCmrclLine : " + TotBsnCmrclLine + "RGIBsnMtr : " + RGIBsnMtr + "RGIBsnHlth : " + RGIBsnHlth + "RGIBsnCmrclLine : " + RGIBsnCmrclLine + "CreateBy : " + strCreateBy;

            SeqNo = ServerTrnReqLog(param, "InsCndProfiling_mobile", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strCreateBy, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@agntype", AgntType.ToString().Trim());
            htParam.Add("@CndNo", strCndNo.ToString().Trim());
            htParam.Add("@AppNo", strAppNo.ToString().Trim());
            htParam.Add("@IsWrkng", IsWrkngInOthrRelnce.ToString().Trim());
            htParam.Add("@compName", CompName.ToString().Trim());
            htParam.Add("@NoOfYrsIns", YrsInInsrnce.ToString().Trim());
            htParam.Add("@NoOfYrs", YrsInRelnce.ToString().Trim());
            htParam.Add("@TypOfVchl", TypOfVchl.ToString().Trim());
            htParam.Add("@VechManf", VechManf.ToString().Trim());
            htParam.Add("@DlrCompName", DlrCompName.ToString().Trim());
            htParam.Add("@DlrOthr", DlrOthr.ToString().Trim());
            htParam.Add("@AvgMnthlyIncm", AvgMnthVol.ToString().Trim());
            htParam.Add("@Comp1Name", Comp1Name.ToString().Trim());
            htParam.Add("@MnthVol1", MnthVolFrComp1.ToString().Trim());
            htParam.Add("@Comp2Name", Comp2Name.ToString().Trim());
            htParam.Add("@MnthVol2", MnthVolFrComp2.ToString().Trim());
            htParam.Add("@RGIMnthVol", RGIMnthVol.ToString().Trim());
            htParam.Add("@TotBsnMtr", TotBsnMtr.ToString().Trim());
            htParam.Add("@TotBsnHlth", TotBsnHlth.ToString().Trim());
            htParam.Add("@TotBsnCmrclLine", TotBsnCmrclLine.ToString().Trim());
            htParam.Add("@RGIBsnMtr", RGIBsnMtr.ToString().Trim());
            htParam.Add("@RGIBsnHlth", RGIBsnHlth.ToString().Trim());
            htParam.Add("@RGIBsnCmrclLine", RGIBsnCmrclLine.ToString().Trim());
            htParam.Add("@CreateBy", strCreateBy.ToString().Trim());

            dstemp = objDAL.selectFromDatabase("prc_InsCndProfiling_Mob", htParam, "IsysARTL");
            strJsonResponce = "SUCCESS";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    [WebMethod(EnableSession = true)]
    public string Get_Profile_detail(string AppNo, String SapCode, String strTokenId)
    {

        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);

            String param = "AppNo : " + strAppNo;
            SeqNo = ServerTrnReqLog(param, "Get_Profile_detail", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@AppNo", strAppNo.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetCndProfilingDtls_Mob", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    [WebMethod(EnableSession = true)]
    public string Set_CompositeTransfer_detail(string CreateBy, string CndNo, string AppNo,
         string NameofInsurer, string AgencyCodeNumber, string DateOfAppointmentAsAgent,
         string DateOfCessationOfAgency, string ReasonForCessationOfAgency, string Category, string ICDate, String strTokenId)
    {


        try
        {
            string strCreateBy = encryptDecrypt.DecodeAndDecrypt(CreateBy);
            string strCndNo = encryptDecrypt.DecodeAndDecrypt(CndNo);
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);

            String param = "AppNo : " + strAppNo + ",CreateBy : " + strCreateBy + "CndNo : " + strCndNo + ",NameofInsurer : " + NameofInsurer + "AgencyCodeNumber : " + AgencyCodeNumber +
            ",DateOfAppointmentAsAgent : " + DateOfAppointmentAsAgent + "DateOfCessationOfAgency : " + DateOfCessationOfAgency + ",ReasonForCessationOfAgency : " + ReasonForCessationOfAgency +
            "Category : " + Category + ",ICDate : " + ICDate;

            SeqNo = ServerTrnReqLog(param, "Set_CompositeTransfer_detail", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strCreateBy, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@CreateBy", strCreateBy.ToString().Trim());
            htParam.Add("@CndNo", strCndNo.ToString().Trim());
            htParam.Add("@ApplicationNo", strAppNo.ToString().Trim());
            htParam.Add("@NameofInsurer", NameofInsurer.ToString().Trim());
            htParam.Add("@AgencyCodeNumber", AgencyCodeNumber.ToString().Trim());
            htParam.Add("@DateOfAppointmentAsAgent", DateOfAppointmentAsAgent.ToString().Trim());
            htParam.Add("@DateOfCessationOfAgency", DateOfCessationOfAgency.ToString().Trim());
            htParam.Add("@ReasonForCessationOfAgency", ReasonForCessationOfAgency.ToString().Trim());
            htParam.Add("@Category", Category.ToString().Trim());
            htParam.Add("@ICDate", ICDate.ToString().Trim());

            dstemp = objDAL.selectFromDatabase("prc_compositedetailsIns_Mob", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    [WebMethod(EnableSession = true)]
    public string Get_CompositeTransfer_detail(string AppNo, String SapCode, String strTokenId)
    {

        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);

            String param = "AppNo : " + strAppNo;
            SeqNo = ServerTrnReqLog(param, "Get_CompositeTransfer_detail", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();

            htParam.Add("@AppNo", strAppNo.ToString().Trim());


            dstemp = objDAL.selectFromDatabase("Prc_GetCompDtls_mob", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Records Found";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    [WebMethod(EnableSession = true)]
    public string Get_Profile_Prsnldetail(string AppNo, String SapCode, String strTokenId)
    {

        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);

            String param = "AppNo : " + strAppNo;
            SeqNo = ServerTrnReqLog(param, "Get_Profile_Prsnldetail", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@AppNo", strAppNo.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetCndProfiling_PrsnlDtls_Mob", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    [WebMethod(EnableSession = true)]
    public string Get_CandidateModification_detail(string AppNo, string CarrierCode, String SapCode, String strTokenId)
    {
        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);

            String param = "AppNo : " + strAppNo + ",CarrierCode : " + CarrierCode;
            SeqNo = ServerTrnReqLog(param, "Get_CandidateModification_detail", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@AppNo", strAppNo.ToString().Trim());
            htParam.Add("@CarrierCode", CarrierCode.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("prc_CndAdmin_getCnd1_Mob", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }



    [WebMethod(EnableSession = true)]
    public string Set_SponsershipRequest_ExamDetail(string AppNo, string CreatedBy, string PAN, string Email,
                                string MobileNo, string ExamMode, string ExmBody, string ExmLang,
                                string ExmCentre, string PanFlag, string Changing_StatusFlag, string ModuleID, String strTokenId)
    {


        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strCreatedBy = encryptDecrypt.DecodeAndDecrypt(CreatedBy);
            string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN);
            string strEmail = encryptDecrypt.DecodeAndDecrypt(Email);
            string strMobileNo = encryptDecrypt.DecodeAndDecrypt(MobileNo);

            String param = "AppNo : " + strAppNo + ",CreatedBy : " + strCreatedBy + "PAN : " + strPAN + ",Email : " + strEmail + "MobileNo : " + strMobileNo + ",ExamMode : " + ExamMode +
           "ExmBody : " + ExmBody + "ExmLang : " + ExmLang + "ExmCentre : " + ExmCentre + "PanFlag : " + PanFlag + "Changing_StatusFlag : " + Changing_StatusFlag + ", ModuleID : " + ModuleID;
            SeqNo = ServerTrnReqLog(param, "Set_SponsershipRequest_ExamDetail", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strCreatedBy, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@AppNo", strAppNo.ToString().Trim());
            htParam.Add("@CreatedBy", strCreatedBy.ToString().Trim());
            htParam.Add("@PAN", strPAN.ToString().Trim());
            htParam.Add("@Email", strEmail.ToString().Trim());
            htParam.Add("@MobileNo", strMobileNo.ToString().Trim());
            htParam.Add("@ExamMode", ExamMode.ToString().Trim());
            htParam.Add("@ExmBody", ExmBody.ToString().Trim());
            htParam.Add("@ExmLang", ExmLang.ToString().Trim());
            htParam.Add("@ExmCentre", ExmCentre.ToString().Trim());
            htParam.Add("@PanFlag", PanFlag.ToString().Trim());
            htParam.Add("@Changing_StatusFlag", Changing_StatusFlag.ToString().Trim());
            htParam.Add("@ModuleID", ModuleID.ToString().Trim());

            dstemp = objDAL.selectFromDatabase("Prc_InsertTrnReqDtl_Mob", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    [WebMethod(EnableSession = true)]
    public string Get_ExamCentre(string ExmCentre, String SapCode, String strTokenId)
    {
        String param = "ExmCentre : " + ExmCentre;
        SeqNo = ServerTrnReqLog(param, "Get_ExamCentre", "", "", "Candidate Manager");
        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@ExmCentre", "");

            dstemp = objDAL.selectFromDatabase("Prc_GetExmCntrDtls", ht, "IsysARTL");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "No Memcode found for your SAPCode.";
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    [WebMethod(EnableSession = true)]
    public string Get_CndExmDetail(string AppNo, String SapCode, String strTokenId)
    {

        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);

            String param = "AppNo : " + strAppNo;
            SeqNo = ServerTrnReqLog(param, "Get_CndExmDetail", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@AppNo", strAppNo.ToString().Trim());

            dstemp = objDAL.selectFromDatabase("Prc_GetCndExmDtls", ht, "IsysARTL");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "No Data Found.";
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    [WebMethod(EnableSession = true)]
    public string Update_CndExmDetail(string AppNo, string ExmMode, string ExmLang, string ExmCentre, string ExmBody, string UpdatedBy
        , String SapCode, String strTokenId)
    {
        String param = "AppNo : " + AppNo + ",ExmMode : " + ExmMode + "ExmLang : " + ExmLang + "ExmCentre : " + ExmCentre + "ExmBody : " + ExmBody + "UpdatedBy : " + UpdatedBy;
        SeqNo = ServerTrnReqLog(param, "Update_CndExmDetail", "", AppNo, "Candidate Manager");
        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@AppNo", AppNo.ToString().Trim());
            ht.Add("@ExamMode", ExmMode.ToString().Trim());
            ht.Add("@ExmLang", ExmLang.ToString().Trim());
            ht.Add("@ExmCentre", ExmCentre.ToString().Trim());
            ht.Add("@ExmBody", ExmBody.ToString().Trim());
            ht.Add("@UpdatedBy", UpdatedBy.ToString().Trim());


            dstemp = objDAL.selectFromDatabase("Prc_UpdateCndExmDtls", ht, "IsysARTL");
            strJsonResponce = "Updated Successfully";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    [WebMethod(EnableSession = true)]
    public string Mob_ErrorLog(string LogDtime, string AppName, string PageName, string FuncName, string ErrDesc,
         string UserId, string Severity, String strTokenId)
    {

        try
        {
            string strUserId = encryptDecrypt.DecodeAndDecrypt(UserId);

            String param = "LogDtime : " + LogDtime + ",AppName : " + AppName + "PageName : " + PageName + "FuncName : " + FuncName + "ErrDesc : " + ErrDesc +
                "strUserId : " + strUserId + "Severity : " + Severity;
            SeqNo = ServerTrnReqLog(param, "Mob_ErrorLog", "", "", "Candidate Manager");

            //if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strUserId, strTokenId) != "Valid"))
            //{
            //    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
            //    updateResponse(strJsonResponce, SeqNo);

            //    return HttpStatusCode.NonAuthoritativeInformation.ToString();
            //    //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            //}
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@LogDtime", LogDtime.ToString().Trim());
            ht.Add("@AppName", AppName.ToString().Trim());
            ht.Add("@PageName", PageName.ToString().Trim());
            ht.Add("@FuncName", FuncName.ToString().Trim());
            ht.Add("@ErrDesc", ErrDesc.ToString().Trim());
            ht.Add("@UserId", strUserId.ToString().Trim());
            ht.Add("@Severity", Severity.ToString().Trim());

            dstemp = objDAL.selectFromDatabase("Mob_LogError", ht, "USRMGMT");
            strJsonResponce = "Error Mentioned successfully";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    // Candidate Registration WebMethod by Rupesh Chakole on 11/05/2015
    // Modified By Rupesh on 10/02/2016

    // Modified By Rupesh on 16/03/2016 encrypted AppNo n all

    // Modified By Bharat on 07/03/2020 fees waiver

    [WebMethod(EnableSession = true)]
    public string RegisterCandidate_mob(
        string SmMemberCode,
        string CarrierCode,
        string EntryDate,
        string GivenName,
        string Surname,
        string FatherName,
        string Relation,
        string Gender,
        string BirthRegDate,
        string MaritalStat,
        string Nationality,
        string CnctType,
        string Adr1,
        string Adr2,
        string Adr3,
        string City,
        string StateCode,
        string PostCode,
        string CntryCode,
        string Area,
        string District,
        string PermAdrInd,
        string PermAdr1,
        string PermAdr2,
        string PermAdr3,
        string PermStateCode,
        string PermCity,
        string PermArea,
        string PermDistrict,
        string PermPostCode,
        string HomeTel,
        string WorkTel,
        string Mobilecode,
        string MobileTel,
        string Email,
        string Pan,
        string AgentType,
        string RecruitAgtCode,
        string HasPhoto,
        string HasMarkSheet,
        string HasCertificate,
        string CreateBy,
        string TrfrFlag,
        string Village,
        string Village_Per,
        string TccCompLcn,
        string CasteCat,
        string PrimProf,
        string BasicQual,
        string BasicBoardName,
        string BasicRNo,
        string BasicYrPass,
        string MobileCode2,
        string Mobile2,
        string Email2,
        string HasPanchayat,
        string CndCat,
        string Title,
        string DstbnMethod,
        string ProofIdDoc,  //Highest Qualification
        string PhotoSignFlag,
        string Bizsrc,
        string ChnCls,
        string cmsunitcode,
        string MobUniquerecId,
        string AccHolderName, // bank details
        string AccNumber,
        string BankName,
        string BranchName,
        string AccType,
        string IFSCCode,
        string MICRCode,
        string AadharNo, //aadhaar details
        string AadharOTP,
        string TypeofAadharVerify,
        string CandType, //addeb by bharat for CandType
        string FeesWavier,
        string AnniversaryDate,
        string AddRptMgr,
        string ModuleID,
        string RegRemark,
        string NomineeName,
        string NomineeRel,
        string NomineeAge,
        string NomineePAN,
        string NomineeEmail,
        string NomineeContact,
        String strTokenId
    )
    {




        try
        {
            string strSmMemberCode = encryptDecrypt.DecodeAndDecrypt(SmMemberCode);
            string strPan = encryptDecrypt.DecodeAndDecrypt(Pan);
            string strMobile = encryptDecrypt.DecodeAndDecrypt(MobileTel);
            string strEmail = encryptDecrypt.DecodeAndDecrypt(Email);
            string strHomeTel = encryptDecrypt.DecodeAndDecrypt(HomeTel);
            string strRecruitAgtCode = encryptDecrypt.DecodeAndDecrypt(RecruitAgtCode);
            string strCreateBy = encryptDecrypt.DecodeAndDecrypt(CreateBy);
            string strMobUniquerecId = encryptDecrypt.DecodeAndDecrypt(MobUniquerecId);

            String param = "SmMemberCode : " + strSmMemberCode + "CarrierCode : " + CarrierCode + "EntryDate : " + EntryDate + "GivenName : " + GivenName + "Surname : " + Surname +
            "FatherName : " + FatherName + "Relation : " + Relation + "Gender : " + Gender + "BirthRegDate : " + BirthRegDate + "MaritalStat : " + MaritalStat +
            "Nationality : " + Nationality + "CnctType : " + CnctType + "Adr1 : " + Adr1 + "Adr2 : " + Adr2 + "Adr3 : " + Adr3 + "City : " + City + "StateCode : " + StateCode +
            "PostCode : " + PostCode + "CntryCode : " + CntryCode + "Area : " + Area + "District : " + District + "PermAdrInd : " + PermAdrInd + "PermAdr1 : " + PermAdr1 + "PermAdr2 : " + PermAdr2 +
            "PermAdr3 : " + PermAdr3 + "PermStateCode : " + PermStateCode + "PermCity : " + PermCity + "PermArea : " + PermArea + "PermDistrict : " + PermDistrict + "PermPostCode : " + PermPostCode +
            "HomeTel : " + strHomeTel + "WorkTel : " + WorkTel + "Mobilecode : " + Mobilecode + "MobileTel : " + strMobile + "Email : " + strEmail + "Pan : " + strPan + "AgentType : " + AgentType +
            "RecruitAgtCode : " + strRecruitAgtCode + "HasPhoto : " + HasPhoto + "HasMarkSheet : " + HasMarkSheet + "HasCertificate : " + HasCertificate + "CreateBy : " + strCreateBy +
            "TrfrFlag : " + TrfrFlag + "Village : " + Village + "Village_Per : " + Village_Per + "TccCompLcn : " + TccCompLcn + "CasteCat : " + CasteCat +
            "PrimProf : " + PrimProf + "BasicQual : " + BasicQual + "BasicBoardName : " + BasicBoardName + "BasicRNo : " + BasicRNo + "BasicYrPass : " + BasicYrPass +
            "MobileCode2 : " + MobileCode2 + "Mobile2 : " + Mobile2 + "Email2 : " + Email2 + "HasPanchayat : " + HasPanchayat + "CndCat : " + CndCat + "Title : " + Title + "DstbnMethod : " + DstbnMethod +
            "ProofIdDoc : " + ProofIdDoc + "PhotoSignFlag : " + PhotoSignFlag + "Bizsrc : " + Bizsrc + "ChnCls : " + ChnCls + "cmsunitcode : " + cmsunitcode + "MobUniquerecId : " + strMobUniquerecId +
            "AccHolderName : " + AccHolderName + "AccNumber : " + AccNumber + "BankName : " + BankName + "BranchName : " + BranchName + "AccType : " + AccType + "IFSCCode : " + IFSCCode +
            "MICRCode : " + MICRCode + "AadharNo : " + AadharNo + "AadharOTP : " + AadharOTP + "TypeofAadharVerify : " + TypeofAadharVerify + " CandType : " + CandType
            + ", FeesWavier: " + FeesWavier + ", AnniversaryDate: " + AnniversaryDate + ", AddRptMgr=" + AddRptMgr + ", ModuleID" + ModuleID
            + " RegRemark : " + RegRemark + " NomineeName : " + NomineeName + " NomineeRel : " + NomineeRel + " NomineeAge : " + NomineeAge + " NomineePAN : " + NomineePAN + " NomineeEmail : " + NomineeEmail + " NomineeContact : " + NomineeContact;

            SeqNo = ServerTrnReqLog(param, "RegisterCandidate_mob", "", "", "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strCreateBy, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@SmMemberCode", strSmMemberCode.ToString().Trim());
            htParam.Add("@CarrierCode", CarrierCode.ToString().Trim());
            htParam.Add("@CndCat", CndCat.ToString().Trim());
            htParam.Add("@EntryDate1", EntryDate.ToString().Trim());
            htParam.Add("@Title", Title.ToString().Trim());
            htParam.Add("@GivenName", GivenName.ToString().Trim());
            htParam.Add("@Surname", Surname.ToString().Trim());
            htParam.Add("@FatherName", FatherName.ToString().Trim());
            htParam.Add("@Relation", Relation.ToString().Trim());
            htParam.Add("@Gender", Gender.ToString().Trim());
            htParam.Add("@BirthRegDate1", BirthRegDate.ToString().Trim());
            htParam.Add("@MaritalStat", MaritalStat.ToString().Trim());
            htParam.Add("@Nationality", Nationality.ToString().Trim());
            htParam.Add("@CnctType", CnctType.ToString().Trim());
            htParam.Add("@Adr1", Adr1.ToString().Trim());
            htParam.Add("@Adr2", Adr2.ToString().Trim());
            htParam.Add("@Adr3", Adr3.ToString().Trim());
            htParam.Add("@City", City.ToString().Trim());
            htParam.Add("@StateCode", StateCode.ToString().Trim());
            htParam.Add("@PostCode", PostCode.ToString().Trim());
            htParam.Add("@CntryCode", CntryCode.ToString().Trim());
            htParam.Add("@Area", Area.ToString().Trim());
            htParam.Add("@District", District.ToString().Trim());
            htParam.Add("@PermAdrInd", PermAdrInd.ToString().Trim());
            htParam.Add("@PermAdr1", PermAdr1.ToString().Trim());
            htParam.Add("@PermAdr2", PermAdr2.ToString().Trim());
            htParam.Add("@PermAdr3", PermAdr3.ToString().Trim());
            htParam.Add("@PermStateCode", PermStateCode.ToString().Trim());
            htParam.Add("@PermCity", PermCity.ToString().Trim());
            htParam.Add("@PermArea", PermArea.ToString().Trim());
            htParam.Add("@PermDistrict", PermDistrict.ToString().Trim());
            htParam.Add("@PermPostCode", PermPostCode.ToString().Trim());
            htParam.Add("@HomeTel", strHomeTel.ToString().Trim());
            htParam.Add("@WorkTel", WorkTel.ToString().Trim());
            htParam.Add("@Mobilecode", Mobilecode.ToString().Trim());
            htParam.Add("@MobileTel", strMobile.ToString().Trim());
            htParam.Add("@Email", strEmail.ToString().Trim());
            htParam.Add("@DstbnMethod", DstbnMethod.ToString().Trim());
            htParam.Add("@Pan", strPan.ToString().Trim());
            htParam.Add("@AgentType", AgentType.ToString().Trim());
            htParam.Add("@RecruitAgtCode", strRecruitAgtCode.ToString().Trim());
            htParam.Add("@ProofIDDoc", ProofIdDoc.ToString().Trim());
            htParam.Add("@HasPhoto", HasPhoto.ToString().Trim());
            htParam.Add("@HasMarkSheet", HasMarkSheet.ToString().Trim());
            htParam.Add("@HasCertificate", HasCertificate.ToString().Trim());
            htParam.Add("@CreateBy", strCreateBy.ToString().Trim());
            htParam.Add("@TrfrFlag", TrfrFlag.ToString().Trim());
            htParam.Add("@Village", Village.ToString().Trim());
            htParam.Add("@Village_Per", Village_Per.ToString().Trim());
            htParam.Add("@TccCompLcn", TccCompLcn.ToString().Trim());
            htParam.Add("@CasteCat", CasteCat.ToString().Trim());
            htParam.Add("@PrimProf", PrimProf.ToString().Trim());
            htParam.Add("@BasicQual", BasicQual.ToString().Trim());
            htParam.Add("@BasicBoardName", BasicBoardName.ToString().Trim());
            htParam.Add("@BasicRNo", BasicRNo.ToString().Trim());
            htParam.Add("@BasicYrPass1", BasicYrPass.ToString().Trim());
            htParam.Add("@MobileCode2", MobileCode2.ToString().Trim());
            htParam.Add("@Mobile2", Mobile2.ToString().Trim());
            htParam.Add("@Email2", Email2.ToString().Trim());
            htParam.Add("@HasPanchayat", HasPanchayat.ToString().Trim());
            htParam.Add("@PhotoSignFlag", PhotoSignFlag.ToString().Trim());
            htParam.Add("@Bizsrc", Bizsrc.ToString().Trim());
            htParam.Add("@ChnCls", ChnCls.ToString().Trim());
            htParam.Add("@cmsunitcode", cmsunitcode.ToString().Trim());
            htParam.Add("@MobUniquerecId", strMobUniquerecId.ToString().Trim());
            htParam.Add("@AccholdrName", AccHolderName.ToString().Trim());
            htParam.Add("@AccntNo", AccNumber.ToString().Trim());
            htParam.Add("@BnkName", BankName.ToString().Trim());
            htParam.Add("@BrnchName", BranchName.ToString().Trim());
            htParam.Add("@AccType", AccType.ToString().Trim());
            htParam.Add("@IFSC", IFSCCode.ToString().Trim());
            htParam.Add("@MCR", MICRCode.ToString().Trim());
            // added by chandrashekhar for aadhaar details on 05/08/2017
            htParam.Add("@AadharNo", AadharNo.ToString().Trim());
            htParam.Add("@AadharOTP", AadharOTP.ToString().Trim());
            htParam.Add("@TypeofAadharVerify", TypeofAadharVerify.ToString().Trim());

            //htParam.Add("AadharNo", "");
            //htParam.Add("AadharOTP", "");
            //htParam.Add("TypeofAadharVerify", "");

            // end of aadhaar details 
            htParam.Add("@Cand_Type", CandType.ToString().Trim()); //added by bharat for POSP
            if (FeesWavier != null)
            {
                htParam.Add("@FeesWavier", FeesWavier.ToString().Trim()); //added by bharat 
            }
            else
            {
                htParam.Add("@FeesWavier", ""); //added by bharat 
            }

            if (AnniversaryDate != null)
            {
                htParam.Add("@AnniversaryDate", AnniversaryDate.Trim());//added by bharat
            }
            else
            {
                htParam.Add("@AnniversaryDate", "");//added by bharat
            }

            if (AddRptMgr != null)
            {
                htParam.Add("@AddRptMgr", AddRptMgr.Trim());//added by Chandrashekhar
            }
            else
            {
                htParam.Add("@AddRptMgr", "");//added by Chandrashekhar
            }

            if (ModuleID != null)
            {
                htParam.Add("@ModuleID", ModuleID.Trim());//added by bharat
            }
            else
            {
                htParam.Add("@ModuleID", "");//added by bharat
            }

            htParam.Add("@RegRemark", RegRemark.ToString().Trim());
            htParam.Add("@NomineeName", NomineeName.ToString().Trim());
            htParam.Add("@NomineeRel", NomineeRel.ToString().Trim());
            htParam.Add("@NomineeAge", NomineeAge.ToString().Trim());
            htParam.Add("@NomineePAN", NomineePAN.ToString().Trim());
            htParam.Add("@NomineeEmail", NomineeEmail.ToString().Trim());
            htParam.Add("@NomineeContact", NomineeContact.ToString().Trim());


            // dstemp = objDAL.selectFromDatabase("prc_CndAdmin_InsCnd1_mobile", htParam, "IsysARTL");

            dstemp = objDAL.selectFromDatabase("prc_CndAdmin_InsCnd1_Androidmobile", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0)//&& dstemp.Tables[0].Rows.Count > 0
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    // Modified By Bharat on 07/03/2020 fees waiver
    [WebMethod(EnableSession = true)]
    public string CandidateModification_mob(
         string AppNo,
         string SmMemberCode,
         string CarrierCode,
         string EntryDate,
         string GivenName,
         string Surname,
         string FatherName,
         string Relation,
         string Gender,
         string BirthRegDate,
         string MaritalStat,
         string Nationality,
         string CnctType,
         string Adr1,
         string Adr2,
         string Adr3,
         string City,
         string StateCode,
         string PostCode,
         string CntryCode,
         string Area,
         string District,
         string PermAdrInd,
         string PermAdr1,
         string PermAdr2,
         string PermAdr3,
         string PermStateCode,
         string PermCity,
         string PermArea,
         string PermDistrict,
         string PermPostCode,
         string HomeTel,
         string WorkTel,
         string Mobilecode,
         string MobileTel,
         string Email,
         string Pan,
         string AgentType,
         string RecruitAgtCode,
         string HasPhoto,
         string HasMarkSheet,
         string HasCertificate,
         string CreateBy,
        //string TrfrFlag,
         string Village,
         string Village_Per,
        //string TccCompLcn,
         string CasteCat,
         string PrimProf,
         string BasicQual,
         string BasicBoardName,
         string BasicRNo,
         string BasicYrPass,
         string MobileCode2,
         string Mobile2,
         string Email2,
         string HasPanchayat,
         string CndCat,
         string Title,
         string DstbnMethod,
         string ProofIdDoc,  //Highest Qualification
         string PhotoSignFlag,
         string Bizsrc,
         string ChnCls,
         string cmsunitcode,
         string AccHolderName, // bank details
         string AccNumber,
         string BankName,
         string BranchName,
         string AccType,
         string IFSCCode,
         string MICRCode,
         string AadharNo, //aadhaar details
         string AadharOTP,
         string TypeofAadharVerify,
         string Cand_Type,
         string FeesWavier,
        string AnniversaryDate,
        string AddRptMgr,
        string ModuleID,
        string RegRemark,
        string NomineeName,
        string NomineeRel,
        string NomineeAge,
        string NomineePAN,
        string NomineeEmail,
        string NomineeContact,
        String strTokenId
     )
    {


        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strSmMemberCode = encryptDecrypt.DecodeAndDecrypt(SmMemberCode);
            string strPan = encryptDecrypt.DecodeAndDecrypt(Pan);
            string strMobileTel = encryptDecrypt.DecodeAndDecrypt(MobileTel);
            string strEmail = encryptDecrypt.DecodeAndDecrypt(Email);
            string strHomeTel = encryptDecrypt.DecodeAndDecrypt(HomeTel);
            string strRecruiteAgtCode = encryptDecrypt.DecodeAndDecrypt(RecruitAgtCode);
            string strCreateBy = encryptDecrypt.DecodeAndDecrypt(CreateBy);

            String param = "AppNo : " + strAppNo + "SmMemberCode : " + strSmMemberCode + "CarrierCode : " + CarrierCode + "EntryDate : " + EntryDate + "GivenName : " + GivenName +
           "Surname : " + Surname + "FatherName : " + FatherName + "Relation : " + Relation + "Gender : " + Gender +
           "BirthRegDate : " + BirthRegDate + "MaritalStat : " + MaritalStat + "Nationality : " + Nationality + "CnctType : " + CnctType + "Adr1 : " + Adr1 + "Adr2 : " + Adr2 + "Adr3 : " + Adr3 +
           "City : " + City + "StateCode : " + StateCode + "Area : " + Area + "District : " + District + "PermAdrInd : " + PermAdrInd + "PermAdr1 : " + PermAdr1 + "PermAdr2 : " + PermAdr2 +
           "PermAdr3 : " + PermAdr3 + "PermStateCode : " + PermStateCode + "PermCity : " + PermCity + "PermArea : " + PermArea + "PermDistrict : " + PermDistrict + "PermPostCode : " + PermPostCode +
           "HomeTel : " + strHomeTel + "WorkTel : " + WorkTel + "Mobilecode : " + Mobilecode + "MobileTel : " + strMobileTel + "Email : " + strEmail + "Pan : " + strPan + "AgentType : " + AgentType +
           "RecruitAgtCode : " + strRecruiteAgtCode + "HasPhoto : " + HasPhoto + "HasMarkSheet : " + HasMarkSheet + "HasCertificate : " + HasCertificate + "CreateBy : " + strCreateBy +
          "Village : " + Village + "Village_Per : " + Village_Per + "CasteCat : " + CasteCat +
           "PrimProf : " + PrimProf + "BasicQual : " + BasicQual + "BasicBoardName : " + BasicBoardName + "BasicRNo : " + BasicRNo + "BasicYrPass : " + BasicYrPass +
         "MobileCode2 : " + MobileCode2 + "Mobile2 : " + Mobile2 + "Email2 : " + Email2 + "HasPanchayat : " + HasPanchayat + "CndCat : " + CndCat + "Title : " + Title + "DstbnMethod : " + DstbnMethod +
         "ProofIdDoc : " + ProofIdDoc + "PhotoSignFlag : " + PhotoSignFlag + "Bizsrc : " + Bizsrc + "ChnCls : " + ChnCls + "cmsunitcode : " + cmsunitcode + "MobUniquerecId : " + "MobUniquerecId" +
       "AccHolderName : " + AccHolderName + "AccNumber : " + AccNumber + "BankName : " + BankName + "BranchName : " + BranchName + "AccType : " + AccType + "IFSCCode : " + IFSCCode +
         "MICRCode : " + MICRCode + "AadharNo : " + AadharNo + "AadharOTP : " + AadharOTP + "TypeofAadharVerify : " + TypeofAadharVerify + ", Cand_Type : "
          + Cand_Type + ", FeesWavier: " + FeesWavier + ", AnniversaryDate: " + AnniversaryDate + ", AddRptMgr=" + AddRptMgr + ", ModuleID=" + ModuleID
          + " RegRemark : " + RegRemark + " NomineeName : " + NomineeName + " NomineeRel : " + NomineeRel + " NomineeAge : " + NomineeAge + " NomineePAN : "
          + NomineePAN + " NomineeEmail : " + NomineeEmail + " NomineeContact : " + NomineeContact;


            SeqNo = ServerTrnReqLog(param, "CandidateModification_mob", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strCreateBy, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();

            htParam.Add("@AppNo", strAppNo.ToString().Trim());
            htParam.Add("@SmMemberCode", strSmMemberCode.ToString().Trim());
            htParam.Add("@CarrierCode", CarrierCode.ToString().Trim());
            htParam.Add("@CndCat", CndCat.ToString().Trim());
            htParam.Add("@EntryDate1", EntryDate.ToString().Trim());
            htParam.Add("@Title", Title.ToString().Trim());
            htParam.Add("@GivenName", GivenName.ToString().Trim());
            htParam.Add("@Surname", Surname.ToString().Trim());
            htParam.Add("@FatherName", FatherName.ToString().Trim());
            htParam.Add("@Relation", Relation.ToString().Trim());
            htParam.Add("@Gender", Gender.ToString().Trim());
            htParam.Add("@BirthRegDate1", BirthRegDate.ToString().Trim());
            htParam.Add("@MaritalStat", MaritalStat.ToString().Trim());
            htParam.Add("@Nationality", Nationality.ToString().Trim());
            htParam.Add("@CnctType", CnctType.ToString().Trim());
            htParam.Add("@Adr1", Adr1.ToString().Trim());
            htParam.Add("@Adr2", Adr2.ToString().Trim());
            htParam.Add("@Adr3", Adr3.ToString().Trim());
            htParam.Add("@City", City.ToString().Trim());
            htParam.Add("@StateCode", StateCode.ToString().Trim());
            htParam.Add("@PostCode", PostCode.ToString().Trim());
            htParam.Add("@CntryCode", CntryCode.ToString().Trim());
            htParam.Add("@Area", Area.ToString().Trim());
            htParam.Add("@District", District.ToString().Trim());
            htParam.Add("@PermAdrInd", PermAdrInd.ToString().Trim());
            htParam.Add("@PermAdr1", PermAdr1.ToString().Trim());
            htParam.Add("@PermAdr2", PermAdr2.ToString().Trim());
            htParam.Add("@PermAdr3", PermAdr3.ToString().Trim());
            htParam.Add("@PermStateCode", PermStateCode.ToString().Trim());
            htParam.Add("@PermCity", PermCity.ToString().Trim());
            htParam.Add("@PermArea", PermArea.ToString().Trim());
            htParam.Add("@PermDistrict", PermDistrict.ToString().Trim());
            htParam.Add("@PermPostCode", PermPostCode.ToString().Trim());
            htParam.Add("@HomeTel", strHomeTel.ToString().Trim());
            htParam.Add("@WorkTel", WorkTel.ToString().Trim());
            htParam.Add("@Mobilecode", Mobilecode.ToString().Trim());
            htParam.Add("@MobileTel", strMobileTel.ToString().Trim());
            htParam.Add("@Email", strEmail.ToString().Trim());
            htParam.Add("@DstbnMethod", DstbnMethod.ToString().Trim());
            htParam.Add("@Pan", strPan.ToString().Trim());
            htParam.Add("@AgentType", AgentType.ToString().Trim());
            htParam.Add("@RecruitAgtCode", strRecruiteAgtCode.ToString().Trim());
            htParam.Add("@ProofIDDoc", ProofIdDoc.ToString().Trim());
            htParam.Add("@HasPhoto", HasPhoto.ToString().Trim());
            htParam.Add("@HasMarkSheet", HasMarkSheet.ToString().Trim());
            htParam.Add("@HasCertificate", HasCertificate.ToString().Trim());
            htParam.Add("@CreateBy", strCreateBy.ToString().Trim());
            //htParam.Add("@TrfrFlag", TrfrFlag.ToString().Trim());
            htParam.Add("@Village", Village.ToString().Trim());
            htParam.Add("@Village_Per", Village_Per.ToString().Trim());
            //htParam.Add("@TccCompLcn", TccCompLcn.ToString().Trim());
            htParam.Add("@CasteCat", CasteCat.ToString().Trim());
            htParam.Add("@PrimProf", PrimProf.ToString().Trim());
            htParam.Add("@BasicQual", BasicQual.ToString().Trim());
            htParam.Add("@BasicBoardName", BasicBoardName.ToString().Trim());
            htParam.Add("@BasicRNo", BasicRNo.ToString().Trim());
            htParam.Add("@BasicYrPass1", BasicYrPass.ToString().Trim());
            htParam.Add("@MobileCode2", MobileCode2.ToString().Trim());
            htParam.Add("@Mobile2", Mobile2.ToString().Trim());
            htParam.Add("@Email2", Email2.ToString().Trim());
            htParam.Add("@HasPanchayat", HasPanchayat.ToString().Trim());
            htParam.Add("@PhotoSignFlag", PhotoSignFlag.ToString().Trim());
            htParam.Add("@Bizsrc", Bizsrc.ToString().Trim());
            htParam.Add("@ChnCls", ChnCls.ToString().Trim());
            htParam.Add("@cmsunitcode", cmsunitcode.ToString().Trim());
            htParam.Add("@AccholdrName", AccHolderName.ToString().Trim());
            htParam.Add("@AccntNo", AccNumber.ToString().Trim());
            htParam.Add("@BnkName", BankName.ToString().Trim());
            htParam.Add("@BrnchName", BranchName.ToString().Trim());
            htParam.Add("@AccType", AccType.ToString().Trim());
            htParam.Add("@IFSC", IFSCCode.ToString().Trim());
            htParam.Add("@MCR", MICRCode.ToString().Trim());
            // added by chandrashekhar for aadhaar details on 05/08/2017
            htParam.Add("@AadharNo", AadharNo.ToString().Trim());
            htParam.Add("@AadharOTP", AadharOTP.ToString().Trim());
            htParam.Add("@TypeofAadharVerify", TypeofAadharVerify.ToString().Trim());

            //htParam.Add("AadharNo", "");
            //htParam.Add("AadharOTP", "");
            //htParam.Add("TypeofAadharVerify", "");
            // end of aadhaar details 
            htParam.Add("@Cand_Type", Cand_Type.ToString().Trim());
            if (FeesWavier != null)
            {
                htParam.Add("@FeesWavier", FeesWavier.ToString().Trim());
            }
            else
            {
                htParam.Add("@FeesWavier", "");
            }

            htParam.Add("@AnniversaryDate", AnniversaryDate.ToString().Trim()); // by bharat on 25-05-21
            htParam.Add("@AddRptMgr", AddRptMgr.ToString().Trim()); // by chandrashekhar  on 03-06-21
            htParam.Add("@ModuleID", ModuleID.ToString().Trim()); // by bharat  on 05-07-21

            //added by chandrashekhar for CR004,005
            htParam.Add("@RegRemark", RegRemark.ToString().Trim());
            if(NomineeName!=null)
                htParam.Add("@NomineeName", NomineeName.ToString().Trim());
            else
                htParam.Add("@NomineeName", "");
            if (NomineeRel != null)
                htParam.Add("@NomineeRel", NomineeRel.ToString().Trim());
            else
                htParam.Add("@NomineeRel", "");
            if (NomineeAge != null)
                htParam.Add("@NomineeAge", NomineeAge.ToString().Trim());
            else
                htParam.Add("@NomineeAge", "");
            if (NomineePAN != null)
                htParam.Add("@NomineePAN", NomineePAN.ToString().Trim());
            else
                htParam.Add("@NomineePAN", "");
            if (NomineeEmail != null)
                htParam.Add("@NomineeEmail", NomineeEmail.ToString().Trim());
            else
                htParam.Add("@NomineeEmail", "");
            if (NomineeContact != null)
                htParam.Add("@NomineeContact", NomineeContact.ToString().Trim());
            else
                htParam.Add("@NomineeContact", "");

            dstemp = objDAL.selectFromDatabase("prc_CndAdmin_UpdtCndRcrd_mobile", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    // Added by Chandrashekhar on 18-12-2021
    [WebMethod(EnableSession = true)]
    public String GetNomineeRelation(String MemCode, String SapCode, String strTokenId)
    {
        String param = "MemCode : " + MemCode;
        SeqNo = ServerTrnReqLog(param, "GetNomineeRelation", "", "", "Candidate Manager");

        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@MemCode", MemCode.ToString().Trim());

            dstemp = objDAL.selectFromDatabase("Prc_GetNomineeRelation", ht, "IsysARTL");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "No Records found.";
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public String GetLookUpParams(String SapCode, String strTokenId)
    {
        String param = "SapCode : " + encryptDecrypt.DecodeAndDecrypt(SapCode) + ", strTokenId : " + strTokenId;

        SeqNo = ServerTrnReqLog(param, "GetLookUpParams", "", "", "Candidate Manager");

        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@SapCode", SapCode.ToString().Trim());

            dstemp = objDAL.selectFromDatabase("Prc_GetLookUpParams", ht, "IsysARTL");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "No Records found.";
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string Set_MobRegId(string regId, String SapCode, String strTokenId)//,string Mobile_No,string userId)
    {
        String param = "regId : " + regId;
        SeqNo = ServerTrnReqLog(param, "Set_MobRegId", "", "", "Candidate Manager");
        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@RegId", regId.ToString().Trim());
            //htParam.Add("@MobileNo", Mobile_No.ToString().Trim());
            //htParam.Add("@UserId", userId.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_PushNotificationMob", htParam, "IsysARTL");
            //if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            //{
            //    return ds2json(dstemp);
            //}
            //else
            //{
            strJsonResponce = "Registered Successfully.";
            //}
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    [WebMethod(EnableSession = true)]
    public string Get_SupportDocs(string AppNo, String SapCode, String strTokenId)
    {

        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);

            String param = "AppNo : " + strAppNo;
            SeqNo = ServerTrnReqLog(param, "Get_SupportDocs", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@Appno", strAppNo.ToString().Trim());

            dstemp = objDAL.selectFromDatabase("Prc_GetCand_SupportDocs", ht, "IsysARTL");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "No Memcode found for your SAPCode.";
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    [WebMethod(EnableSession = true)]
    public string SendNotification(string deviceId, string message, String SapCode, String strTokenId)
    {
        String param = "deviceId : " + deviceId + ",message : " + message;
        SeqNo = ServerTrnReqLog(param, "SendNotification", "", "", "Candidate Manager");

        String result = ValidateRequest(param);
        if (!result.Equals("Valid"))
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
            updateResponse(strJsonResponce, SeqNo);
            return strJsonResponce;
        }
        else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
            updateResponse(strJsonResponce, SeqNo);

            return HttpStatusCode.NonAuthoritativeInformation.ToString();
            //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

        }
        string GoogleAppID = "AIzaSyA4MDfudLMOaeeNNh45Nc-mmPY9Kuf4jLE";
        var SENDER_ID = "947589077663";
        var value = message;
        WebRequest tRequest;
        tRequest = WebRequest.Create("https://android.googleapis.com/gcm/send");
        tRequest.Method = "post";
        tRequest.ContentType = " application/x-www-form-urlencoded;charset=UTF-8";
        tRequest.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));

        tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

        //string postData = "collapse_key=score_update&time_to_live=108&delay_while_idle=1&data.message=" + message + "&data.time=" + 
        //System.DateTime.Now.ToString() + "&registration_id=" + deviceId + "";

        string postData = "collapse_key=score_update&time_to_live=108&delay_while_idle=1&data.message=Candidate Registered Successfully&data.time=" +
        System.DateTime.Now.ToString() + "&registration_id=" + deviceId + "";

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




    //Offline Candidate Registration through mobile on 13/07/2015
    [WebMethod(EnableSession = true)]
    public string OfflineRegisterCandidate_mob(
        string SmMemberCode,
    string CarrierCode,
    string EntryDate,
    string GivenName,
    string Surname,
    string FatherName,
    string Relation,
    string Gender,
    string BirthRegDate,
    string MaritalStat,
    string Nationality,
    string CnctType,
    string Adr1,
    string City,
    string StateCode,
    string PostCode,
    string CntryCode,
    string Area,
    string District,
    string PermAdrInd,
    string PermAdr1,
    string PermStateCode,
    string PermCity,
    string PermArea,
    string PermDistrict,
    string PermPostCode,
    string HomeTel,
    string WorkTel,
    string Mobilecode,
    string MobileTel,
    string Email,
    string Pan,
    string AgentType,
    string RecruitAgtCode,
    string HasPhoto,
    string HasMarkSheet,
    string HasCertificate,
    string CreateBy,
    string TrfrFlag,
    string Village,
    string TccCompLcn,
    string CasteCat,
    string PrimProf,
    string BasicQual,
    string BasicBoardName,
    string BasicRNo,
    string BasicYrPass,
    string MobileCode2,
    string Mobile2,
    string Email2,
    string HasPanchayat,
    string CndCat,
    string Title,
    string DstbnMethod,
    string ProofIdDoc,  //Highest Qualification
    string PhotoSignFlag,

    //Profiling
    string Profile_AgntType,
    string IsWrkngInOthrRelnce,
    string CompName,
    string YrsInInsrnce,
    string YrsInRelnce,
    string TypOfVchl,
    string VechManf,
    string DlrCompName,
    string DlrOthr,
    string AvgMnthVol,
    string Comp1Name,
    string MnthVolFrComp1,
    string Comp2Name,
    string MnthVolFrComp2,
    string RGIMnthVol,
    string TotBsnMtr,
    string TotBsnHlth,
    string TotBsnCmrclLine,
    string RGIBsnMtr,
    string RGIBsnHlth,
    string RGIBsnCmrclLine,
        String strTokenId
    )
    {

        String param = "SmMemberCode : " + SmMemberCode + "CarrierCode : " + CarrierCode + "EntryDate : " + EntryDate + "GivenName : " + GivenName +
            "Surname : " + Surname + "FatherName : " + FatherName + "Relation : " + Relation + "Gender : " + Gender +
            "BirthRegDate : " + BirthRegDate + "MaritalStat : " + MaritalStat + "Nationality : " + Nationality + "CnctType : " + CnctType + "Adr1 : " + Adr1 +
            "City : " + City + "StateCode : " + StateCode + "Area : " + Area + "District : " + District + "PermAdrInd : " + PermAdrInd + "PermAdr1 : " + PermAdr1 +
            "PermStateCode : " + PermStateCode + "PermCity : " + PermCity + "PermArea : " + PermArea + "PermDistrict : " + PermDistrict + "PermPostCode : " + PermPostCode +
            "HomeTel : " + HomeTel + "WorkTel : " + WorkTel + "Mobilecode : " + Mobilecode + "MobileTel : " + MobileTel + "Email : " + Email + "Pan : " + Pan + "AgentType : " + AgentType +
            "RecruitAgtCode : " + RecruitAgtCode + "HasPhoto : " + HasPhoto + "HasMarkSheet : " + HasMarkSheet + "HasCertificate : " + HasCertificate + "CreateBy : " + CreateBy +
           "Village : " + Village + "CasteCat : " + CasteCat + "TccCompLcn : " + TccCompLcn +
        "PrimProf : " + PrimProf + "BasicQual : " + BasicQual + "BasicBoardName : " + BasicBoardName + "BasicRNo : " + BasicRNo + "BasicYrPass : " + BasicYrPass +
      "MobileCode2 : " + MobileCode2 + "Mobile2 : " + Mobile2 + "Email2 : " + Email2 + "HasPanchayat : " + HasPanchayat + "CndCat : " + CndCat + "Title : " + Title + "DstbnMethod : " + DstbnMethod +
      "ProofIdDoc : " + ProofIdDoc + "PhotoSignFlag : " + PhotoSignFlag + "Profile_AgntType : " + Profile_AgntType + "IsWrkngInOthrRelnce : " + IsWrkngInOthrRelnce +
      "CompName : " + CompName + "YrsInInsrnce : " + YrsInInsrnce + "TypOfVchl : " + TypOfVchl + "VechManf : " + VechManf + "DlrCompName : " + DlrCompName + "DlrOthr : " + DlrOthr +
      "AvgMnthVol : " + AvgMnthVol + "Comp1Name : " + Comp1Name + "MnthVolFrComp1 : " + MnthVolFrComp1 + "Comp2Name : " + Comp2Name + "MnthVolFrComp2 : " + MnthVolFrComp2 + "RGIMnthVol : " + RGIMnthVol +
      "TotBsnMtr : " + TotBsnMtr + "TotBsnHlth : " + TotBsnHlth + "TotBsnCmrclLine : " + TotBsnCmrclLine + "RGIBsnMtr : " + RGIBsnMtr + "RGIBsnHlth : " + RGIBsnHlth + "RGIBsnCmrclLine : " + RGIBsnCmrclLine;

        SeqNo = ServerTrnReqLog(param, "OfflineRegisterCandidate_mob", "", "", "Candidate Manager");
        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(CreateBy, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@SmMemberCode", SmMemberCode.ToString().Trim());
            htParam.Add("@CarrierCode", CarrierCode.ToString().Trim());
            htParam.Add("@CndCat", CndCat.ToString().Trim());
            htParam.Add("@EntryDate1", EntryDate.ToString().Trim());
            htParam.Add("@Title", Title.ToString().Trim());
            htParam.Add("@GivenName", GivenName.ToString().Trim());
            htParam.Add("@Surname", Surname.ToString().Trim());
            htParam.Add("@FatherName", FatherName.ToString().Trim());
            htParam.Add("@Relation", Relation.ToString().Trim());
            htParam.Add("@Gender", Gender.ToString().Trim());
            htParam.Add("@BirthRegDate1", BirthRegDate.ToString().Trim());
            htParam.Add("@MaritalStat", MaritalStat.ToString().Trim());
            htParam.Add("@Nationality", Nationality.ToString().Trim());
            htParam.Add("@CnctType", CnctType.ToString().Trim());
            htParam.Add("@Adr1", Adr1.ToString().Trim());
            htParam.Add("@City", City.ToString().Trim());
            htParam.Add("@StateCode", StateCode.ToString().Trim());
            htParam.Add("@PostCode", PostCode.ToString().Trim());
            htParam.Add("@CntryCode", CntryCode.ToString().Trim());
            htParam.Add("@Area", Area.ToString().Trim());
            htParam.Add("@District", District.ToString().Trim());
            htParam.Add("@PermAdrInd", PermAdrInd.ToString().Trim());
            htParam.Add("@PermAdr1", PermAdr1.ToString().Trim());
            htParam.Add("@PermStateCode", PermStateCode.ToString().Trim());
            htParam.Add("@PermCity", PermCity.ToString().Trim());
            htParam.Add("@PermArea", PermArea.ToString().Trim());
            htParam.Add("@PermDistrict", PermDistrict.ToString().Trim());
            htParam.Add("@PermPostCode", PermPostCode.ToString().Trim());
            htParam.Add("@HomeTel", HomeTel.ToString().Trim());
            htParam.Add("@WorkTel", WorkTel.ToString().Trim());
            htParam.Add("@Mobilecode", Mobilecode.ToString().Trim());
            htParam.Add("@MobileTel", MobileTel.ToString().Trim());
            htParam.Add("@Email", Email.ToString().Trim());
            htParam.Add("@DstbnMethod", DstbnMethod.ToString().Trim());
            htParam.Add("@Pan", Pan.ToString().Trim());
            htParam.Add("@AgentType", AgentType.ToString().Trim());
            htParam.Add("@RecruitAgtCode", RecruitAgtCode.ToString().Trim());
            htParam.Add("@ProofIDDoc", ProofIdDoc.ToString().Trim());
            htParam.Add("@HasPhoto", HasPhoto.ToString().Trim());
            htParam.Add("@HasMarkSheet", HasMarkSheet.ToString().Trim());
            htParam.Add("@HasCertificate", HasCertificate.ToString().Trim());
            htParam.Add("@CreateBy", CreateBy.ToString().Trim());
            htParam.Add("@TrfrFlag", TrfrFlag.ToString().Trim());
            htParam.Add("@Village", Village.ToString().Trim());
            htParam.Add("@TccCompLcn", TccCompLcn.ToString().Trim());
            htParam.Add("@CasteCat", CasteCat.ToString().Trim());
            htParam.Add("@PrimProf", PrimProf.ToString().Trim());
            htParam.Add("@BasicQual", BasicQual.ToString().Trim());
            htParam.Add("@BasicBoardName", BasicBoardName.ToString().Trim());
            htParam.Add("@BasicRNo", BasicRNo.ToString().Trim());
            htParam.Add("@BasicYrPass1", BasicYrPass.ToString().Trim());
            htParam.Add("@MobileCode2", MobileCode2.ToString().Trim());
            htParam.Add("@Mobile2", Mobile2.ToString().Trim());
            htParam.Add("@Email2", Email2.ToString().Trim());
            htParam.Add("@HasPanchayat", HasPanchayat.ToString().Trim());
            htParam.Add("@PhotoSignFlag", PhotoSignFlag.ToString().Trim());

            htParam.Add("@agntype", Profile_AgntType.ToString().Trim());
            htParam.Add("@IsWrkng", IsWrkngInOthrRelnce.ToString().Trim());
            htParam.Add("@compName", CompName.ToString().Trim());
            htParam.Add("@NoOfYrsIns", YrsInInsrnce.ToString().Trim());
            htParam.Add("@NoOfYrs", YrsInRelnce.ToString().Trim());
            htParam.Add("@TypOfVchl", TypOfVchl.ToString().Trim());
            htParam.Add("@VechManf", VechManf.ToString().Trim());
            htParam.Add("@DlrCompName", DlrCompName.ToString().Trim());
            htParam.Add("@DlrOthr", DlrOthr.ToString().Trim());
            htParam.Add("@AvgMnthlyIncm", AvgMnthVol.ToString().Trim());
            htParam.Add("@Comp1Name", Comp1Name.ToString().Trim());
            htParam.Add("@MnthVol1", MnthVolFrComp1.ToString().Trim());
            htParam.Add("@Comp2Name", Comp2Name.ToString().Trim());
            htParam.Add("@MnthVol2", MnthVolFrComp2.ToString().Trim());
            htParam.Add("@RGIMnthVol", RGIMnthVol.ToString().Trim());
            htParam.Add("@TotBsnMtr", TotBsnMtr.ToString().Trim());
            htParam.Add("@TotBsnHlth", TotBsnHlth.ToString().Trim());
            htParam.Add("@TotBsnCmrclLine", TotBsnCmrclLine.ToString().Trim());
            htParam.Add("@RGIBsnMtr", RGIBsnMtr.ToString().Trim());
            htParam.Add("@RGIBsnHlth", RGIBsnHlth.ToString().Trim());
            htParam.Add("@RGIBsnCmrclLine", RGIBsnCmrclLine.ToString().Trim());


            dstemp = objDAL.selectFromDatabase("prc_CndAdmin_OfflineCndRegistration_mobile", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
                //return "SUCCESS";
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }




    [WebMethod(EnableSession = true)]
    public string Get_MobileEmailVerification_mobile(string Email, string Mobile, String SapCode, String strTokenId)
    {


        try
        {
            string strEmail = encryptDecrypt.DecodeAndDecrypt(Email);
            string strMobile = encryptDecrypt.DecodeAndDecrypt(Mobile);

            String param = "Email : " + strEmail + ",Mobile : " + strMobile;
            SeqNo = ServerTrnReqLog(param, "Get_MobileEmailVerification_mobile", "", "", "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@Email", strEmail.ToString().Trim());
            htParam.Add("@Mobile", strMobile.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetQualCheckverifyEmail_mobile", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No Memcode found for your SAPCode.";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }






    //public string encryptDecrypt.DecodeAndDecrypt(string data, string key)
    //{
    //    try
    //    {
    //        RijndaelManaged rijndaelCipher = new RijndaelManaged();
    //        rijndaelCipher.Mode = CipherMode.CBC;
    //        rijndaelCipher.Padding = PaddingMode.PKCS7;

    //        rijndaelCipher.KeySize = 0x80;
    //        rijndaelCipher.BlockSize = 0x80;
    //        byte[] encryptedData = Convert.FromBase64String(data);
    //        byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
    //        byte[] keyBytes = new byte[0x10];
    //        int len = pwdBytes.Length;

    //        if (len > keyBytes.Length)
    //        {
    //            len = keyBytes.Length;
    //        }

    //        Array.Copy(pwdBytes, keyBytes, len);
    //        rijndaelCipher.Key = keyBytes;
    //        rijndaelCipher.IV = keyBytes;
    //        byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock
    //                    (encryptedData, 0, encryptedData.Length);

    //        return Encoding.UTF8.GetString(plainText);
    //    }
    //    catch (Exception e)
    //    {
    //        e.ToString();
    //        return "Invalid Request";
    //    }
    //}


    public String CreateFolderOfDoc(String lblimgshrt, String lblCndNoValue, byte[] img)
    {
        String strPhotoExt = "jpeg", strFileName1, strFileName = string.Empty;
        string strFilePath = string.Empty;
        //string strPath="C:\\RUPESH\\Application\\I-SysSuite\\Application\\ISys\\Recruit\\UploadedFiles\\";

        string strPath = "E:\\RGI_CHMS\\UploadedFiles_UAT\\";
        if (Directory.Exists(strPath) == false)
        {
            strPath = strPath + lblCndNoValue.Trim();
            Directory.CreateDirectory(strPath);
        }
        else
        {
            strFilePath = strPath + lblCndNoValue.Trim();
            if (!Directory.Exists(strFilePath))
                Directory.CreateDirectory(strFilePath);
            else
                strFilePath = strPath + lblCndNoValue.Trim();
        }

        if (strPhotoExt == "JPEG" || strPhotoExt == "jpeg")
        {
            strFileName1 = lblCndNoValue.Trim() + "_" + lblimgshrt + "." + strPhotoExt;
            strFileName = strFilePath + "\\" + strFileName1;
        }
        FileInfo fi = new FileInfo(strFileName);
        using (FileStream fileStream = fi.Open(FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
        {
            fileStream.Write(img, 0, img.Length);
        }
        return "Folder Created Successfully";
    }


    public String ReCreateFolderOfDoc(String lblimgshrt, String lblCndNoValue, String DocStatusCount, byte[] img)
    {
        String strPhotoExt = "jpeg", strFileName1 = string.Empty, strFileName = string.Empty;
        string strFilePath = string.Empty;

        string strPath = "E:\\RGI_CHMS\\UploadedFiles_UAT\\";
        string strFileRePath = string.Empty;

        strFileRePath = strPath + lblCndNoValue.Trim();



        if (strPhotoExt == "JPEG" || strPhotoExt == "jpeg")
        {
            strFileName1 = lblCndNoValue.Trim() + "_" + lblimgshrt + "." + strPhotoExt;
            strFileName = strFileRePath + "\\" + strFileName1;
        }


        FileInfo fi = new FileInfo(strPath);
        {
            if (File.Exists(strFileName))
            {
                string stroldpath = strFileRePath + "\\" + strFileName1;
                string[] strfile = strFileName1.Split('.');
                string ImageNamenew = strfile[0];



                string strnewpath = strFileRePath + "\\" + ImageNamenew + "_R" + DocStatusCount + "." + strPhotoExt;
                System.IO.File.Copy(stroldpath, strnewpath, true);

                strFileName = strnewpath;
                ////System.IO.File.Copy(stroldpath, strnewpath, true);
            }

            using (System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(img)))
            {
                image.Save(strFileName, ImageFormat.Jpeg);  // Or Png
            }
        }
        return "Reuploaded Successfully";
    }



    // Added by Rupesh on 10-02-2016
    [WebMethod(EnableSession = true)]
    public String Get_RecruitersChannel_Mob(String Chncls, String Bizsrc, String MemType, String SapCode, String strTokenId)
    {
        String param = "Chncls : " + Chncls + ",Bizsrc : " + Bizsrc;
        SeqNo = ServerTrnReqLog(param, "Get_RecruitersChannel_Mob", "", "", "Candidate Manager");

        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@Chncls", Chncls.ToString().Trim());
            ht.Add("@Bizsrc", Bizsrc.ToString().Trim());
            ht.Add("@MemType", MemType.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_getrecruitslschannel_Mob", ht, "IsysARTL");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "No Records found.";
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    // Added by Chandrashekhar on 02-06-2021
    [WebMethod(EnableSession = true)]
    public String GetAdditionalReportingManager(String mCode, String mType, String SapCode, String strTokenId)
    {
        String param = "MemCode : " + mCode + ",Memtype : " + mType;
        SeqNo = ServerTrnReqLog(param, "GetAdditionalReportingManager", "", "", "Candidate Manager");

        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@MemberCode", mCode.ToString().Trim());
            ht.Add("@MemberType", mType.ToString().Trim());

            dstemp = objDAL.selectFromDatabase("Prc_GetAddRptMgrCodeDtls", ht, "IsysARTL");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "No Records found.";
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    // added by chandrashekhar on 21-09-2017
    [WebMethod(EnableSession = true)]
    public string GetTeamView(String Empcode, String SapCode, String strTokenId)
    {

        if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
            updateResponse(strJsonResponce, SeqNo);

            return HttpStatusCode.NonAuthoritativeInformation.ToString();
            //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

        }
        htParam.Clear();
        dstemp.Clear();
        htParam.Add("@Empcode", Empcode.ToString().Trim());
        //htParam.Add("@Chncls", Chncls.ToString().Trim());
        //htParam.Add("@Bizsrc", Bizsrc.ToString().Trim());
        dstemp = objDAL.selectFromDatabase("prc_GetTeamViewData", htParam, "ISysCHMS");

        if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
        {
            return encryptDecrypt.EncryptAndEncode(ds2json(dstemp));
        }
        else
        {
            return "No Record found for your Search.";
        }

    }

    // Added for POSP Development by bharat on 07-03-2020

    [WebMethod(EnableSession = true)]
    public string GetDataForSearchPOSP(string SapCode, string AppNo, string CndName, string PanNo, string Flag, string EntryDate, String strTokenId)
    {
        try
        {
            string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strPanNo = encryptDecrypt.DecodeAndDecrypt(PanNo);
            String param = "AppNo : " + strAppNo + ",SapCode : " + strSapCode + ",CndName : " + CndName + ",PanNo : " + strPanNo + ",Flag : " + Flag + ",EntryDate : " + EntryDate;
            SeqNo = ServerTrnReqLog(param, "GetDataForSearchPOSP", strSapCode, strAppNo, "Candidate Manager");
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            //dstemp.Clear();
            //dstemp = objDAL.selectFromDatabase_singleValue("prc_GetMemberCode_App", strSapCode, "ISysARTL");
            //if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            //{
            //    string _memCode = Convert.ToString(dstemp.Tables[0].Rows[0]["MemCode"]);

            //}
            //else
            //{
            //    strJsonResponce = "No Memcode found for your SAPCode.";
            //}

            dstemp.Clear();
            Hashtable htable = new Hashtable();
            htable.Clear();
            if (Flag == "ARS")
            {
                //for agent survey	
                htable.Add("@CndNo", string.Empty);
                htable.Add("@Appno", strAppNo);
                htable.Add("@GivenName", CndName);
                htable.Add("@Surname", string.Empty);
                htable.Add("@PAN", strPanNo);
                htable.Add("@AgentBrokeCode", string.Empty);
                htable.Add("@CreateFrmDtim", EntryDate);
                htable.Add("@CreateToDtim", string.Empty);
                dstemp = objDAL.selectFromDatabase("prc_GetAgentCodeDetails", htable, "ISysCHMS");
            }
            else
            {
                htable.Add("@BMCode", strSapCode);
                htable.Add("@AppNo", strAppNo);
                htable.Add("@CndName", CndName);
                htable.Add("@PanNo", strPanNo);
                htable.Add("@Flag", Flag);
                htable.Add("@EntryDate", EntryDate);
                dstemp = objDAL.selectFromDatabase("Prc_GetSearchDataForPOSP_Mob", htable, "ISysARTL");
            }
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
                strJsonResponce = "No Candidates found for your Search.";

        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }


    [WebMethod(EnableSession = true)]
    public string POSPCandidateApprove_Mobile(String AppNo, String Flag, String SapCode, String strTokenId)
    {
        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            String param = "AppNo : " + strAppNo + ",Flag : " + Flag;
            SeqNo = ServerTrnReqLog(param, "POSPCandidateApprove_Mobile", "", strAppNo, "Candidate Manager");
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@AppNo", strAppNo.ToString().Trim());
            htParam.Add("@Flag", Flag.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("prc_POSPCandidateApprove_mob", htParam, "IsysARTL");
            return "Candidate Apporved";
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string GetCndRegistrationBasicDetails(String AppNo, String SapCode, String strTokenId)
    {
        try
        {
            string strAppno = encryptDecrypt.DecodeAndDecrypt(AppNo);
            String param = "AppNo : " + strAppno + "";
            SeqNo = ServerTrnReqLog(param, "GetCndRegistrationBasicDetails", "", strAppno, "Candidate Manager");
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@Appno", strAppno.ToString().Trim());
            //htParam.Add("@Flag", Flag.ToString().Trim());	
            dstemp = objDAL.selectFromDatabase("prc_getCndRegistrationDetails_mob", htParam, "IsysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "No record found";
            }
        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string SendAgentProfilingFrmMob(string AppNo, string CndNo, string ActualPersonBusns, string Title, string Name, string AadharNo, string CityYr, string Gender, string BirthRegDate, string MaritalStatus, string MarriageDate, string NosOfDepend, string Spousename, string SpouseDOB, string SpouseWrkng, string SpouseWrkngOrgName, string child1Name, string child1DOB, string child2Name, string child2DOB, string Occupation, string SocialGrpName, string FBUsages, string OtherSocialGrpName, string IsAssociatIndustry, string NameAssociatIndustry, string FourWheeler, string MakeFourWheeler, string ModelFourWheeler, string TwoWheeler, string OwnHouse, string CreditCard, string VegNonveg, string PassportNo, string Hobby, string Mothertoung, string AgntType, string TotlWorkExp, string TotlWorkExpGI, string NameGIProduct, string TotlWorkExpLI, string IsDealOthfinServcs, string NameOthfinServcs, string IsDealRCLCmpny, string NameRCLCmpny, string IsInsPrimarySource, string AnualFmlyIncm, string AnualBusnsIncm, string MotorContrbuton, string TravelContrbuton, string HealthContrbuton, string CLContrbuton, string TotalContrbuton, string MobileNo, string AlterMobileNo, string STDMobile2, string HomeTel, string Email, string NosOfCnct, string EduQual, string ProfQual, string InsQual, string CreateBy, string CarrierCode, String strTokenId)
    {
        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strCndNo = encryptDecrypt.DecodeAndDecrypt(CndNo);
            string strCreateBy = encryptDecrypt.DecodeAndDecrypt(CreateBy);
            String param = "AppNo: " + strAppNo + ", :CndNo" + strCndNo + ", ActualPersonBusns: " + ActualPersonBusns + ", Title : " + Title + ", Name:" + Name + ", AadharNo:" + AadharNo + ", CityYr:" + CityYr + ", Gender:" + Gender + ", BirthRegDate:" + BirthRegDate + ",MaritalStatus:" + MaritalStatus + ", MarriageDate:" + MarriageDate + ", NosOfDepend: " + NosOfDepend + " ,Spousename" + Spousename + ", SpouseDOB:" + SpouseDOB + ", SpouseWrkng:" + SpouseWrkng + ", SpouseWrkngOrgName: " + SpouseWrkngOrgName + ", child1Name:" + child1Name + ", child1DOB:" + child1DOB + ", child2Name:" + child2Name + ", child2DOB:" + child2DOB + ", Occupation:" + Occupation + ", SocialGrpName:" + SocialGrpName + ", FBUsages:" + FBUsages + ", OtherSocialGrpName:" + OtherSocialGrpName + ", IsAssociatIndustry:" + IsAssociatIndustry + ", NameAssociatIndustry:" + NameAssociatIndustry + ", FourWheeler" + FourWheeler + ", MakeFourWheeler:" + MakeFourWheeler + ", ModelFourWheeler:" + ModelFourWheeler + ", TwoWheeler:" + TwoWheeler + ", OwnHouse:" + OwnHouse + ", CreditCard:" + CreditCard + ", VegNonveg:" + VegNonveg + ", PassportNo:" + PassportNo + ", Hobby:" + Hobby + ", Mothertoung:" + Mothertoung + ", AgntType:" + AgntType + ", TotlWorkExp:" + TotlWorkExp + ", TotlWorkExpGI:" + TotlWorkExpGI + ", NameGIProduct:" + NameGIProduct + ", TotlWorkExpLI:" + TotlWorkExpLI + ", IsDealOthfinServcs:" + IsDealOthfinServcs + ", NameOthfinServcs:" + NameOthfinServcs + ", IsDealRCLCmpny:" + IsDealRCLCmpny + ", NameRCLCmpny:" + NameRCLCmpny + ", IsInsPrimarySource:" + IsInsPrimarySource + ", AnualFmlyIncm:" + AnualFmlyIncm + ", AnualBusnsIncm:" + AnualBusnsIncm + ", MotorContrbuton:" + MotorContrbuton + ", TravelContrbuton:" + TravelContrbuton + ", HealthContrbuton:" + HealthContrbuton + ", CLContrbuton:" + CLContrbuton + ", TotalContrbuton:" + TotalContrbuton + ", MobileNo:" + MobileNo + ", AlterMobileNo: " + AlterMobileNo + ", STDMobile2:" + STDMobile2 + ", HomeTel:" + HomeTel + ", Email:" + Email + ", NosOfCnct:" + NosOfCnct + ", EduQual:" + EduQual + ", ProfQual:" + ProfQual + ", InsQual:" + InsQual + ", CreateBy:" + CreateBy + "";

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strCreateBy, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            //Hashtable ht = new Hashtable();	
            htParam.Clear();
            htParam.Add("@AppNo", strAppNo);
            htParam.Add("@CndNo", strCndNo);
            htParam.Add("@ActualPrsnBusns", ActualPersonBusns);
            htParam.Add("@Title", Title);
            htParam.Add("@Name", Name);
            htParam.Add("@AadharNo", AadharNo);
            htParam.Add("@CityYr", CityYr);
            htParam.Add("@Gender", Gender);
            htParam.Add("@BirthRegDate", BirthRegDate);
            htParam.Add("@MaritalStatus", MaritalStatus);
            htParam.Add("@MarriageDate", MarriageDate);
            htParam.Add("@NosOfDepend", Convert.ToInt32(NosOfDepend));
            htParam.Add("@Spousename", Spousename);
            htParam.Add("@SpouseDOB", SpouseDOB);
            htParam.Add("@SpouseWrkng", SpouseWrkng);
            htParam.Add("@SpouseWrkngOrgName", SpouseWrkngOrgName);
            htParam.Add("@child1Name", child1Name);
            htParam.Add("@child1DOB", child1DOB);
            htParam.Add("@child2Name", child2Name);
            htParam.Add("@child2DOB", child2DOB);
            htParam.Add("@Occupation", Occupation);
            htParam.Add("@SocialGrpName", SocialGrpName);
            htParam.Add("@FBUsages", FBUsages);
            htParam.Add("@OtherSocialGrpName", OtherSocialGrpName);
            htParam.Add("@AssociatIndustry", IsAssociatIndustry);
            htParam.Add("@NameAssociatIndustry", NameAssociatIndustry);
            htParam.Add("@FourWheeler", FourWheeler);
            htParam.Add("@MakeFourWheeler", MakeFourWheeler);
            htParam.Add("@ModelFourWheeler", ModelFourWheeler);
            htParam.Add("@TwoWheeler", TwoWheeler);
            htParam.Add("@OwnHouse", OwnHouse);
            htParam.Add("@CreditCard", CreditCard);
            htParam.Add("@VegNonveg", VegNonveg);
            htParam.Add("@PassportNo", PassportNo);
            htParam.Add("@Hobby", Hobby);
            htParam.Add("@Mothertoung", Mothertoung);
            htParam.Add("@AgntType", AgntType);
            htParam.Add("@TotlWorkExp", TotlWorkExp);
            htParam.Add("@TotlWorkExpGI", TotlWorkExpGI);
            htParam.Add("@NameGIProduct", NameGIProduct);
            htParam.Add("@TotlWorkExpLI", TotlWorkExpLI);
            htParam.Add("@IsDealOthfinServcs", IsDealOthfinServcs);
            htParam.Add("@NameOthfinServcs", NameOthfinServcs);
            htParam.Add("@IsDealRCLCmpny", IsDealRCLCmpny);
            htParam.Add("@NameRCLCmpny", NameRCLCmpny);
            htParam.Add("@IsInsPrimarySource", IsInsPrimarySource);
            htParam.Add("@AnualFmlyIncm", AnualFmlyIncm);
            htParam.Add("@AnualBusnsIncm", AnualBusnsIncm);
            htParam.Add("@MotorContrbuton", MotorContrbuton);
            htParam.Add("@TravelContrbuton", TravelContrbuton);
            htParam.Add("@HealthContrbuton", HealthContrbuton);
            htParam.Add("@CLContrbuton", CLContrbuton);
            htParam.Add("@TotalContrbuton", TotalContrbuton);
            htParam.Add("@MobileNo", MobileNo);
            htParam.Add("@AlterMobileNo", AlterMobileNo);
            htParam.Add("@STDMobile2", STDMobile2);
            htParam.Add("@HomeTel", HomeTel);
            htParam.Add("@Email", Email);
            htParam.Add("@NosOfCnct", NosOfCnct);
            htParam.Add("@EduQual", EduQual);
            htParam.Add("@ProfQual", ProfQual);
            htParam.Add("@InsQual", InsQual);
            htParam.Add("@CarrierCode", CarrierCode);
            htParam.Add("@CreateBy", CreateBy);
            dstemp = objDAL.selectFromDatabase("prc_InsAgentProfile_mob", htParam, "IsysARTL");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    //return ds2json(dstemp);	
                    strJsonResponce = "Successfully Updated";
                }
                else
                {
                    strJsonResponce = "No Memcode found for your SAPCode.";
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = "In Catch block = " + ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string ProspectRegistration_mob(String SmMemberCode, String CarrierCode, String Pan, String EntryDate,
                                                                        String Occupation, String Prefix, String GivenName, String Surname,
                                                                        String FatherName, String CndCat, String DOB, String Gender,
                                                                        String Nationality, String MaritalStat, String CnctType, String HomeTel,
                                                                        String WorkTel, String Mobilecode, String MobileTel, String Email,
                                                                        String MobileCode2, String Mobile2, String Email2, String AgentType,
                                                                        String RecruitAgtCode, String Bizsrc, String ChnCls, String RecruitUnitCode, String cmsunitcode,
                                                                        String MobUniquerecId, String CreateBy, String CandType, String AnniversaryDate,
                                                                        String AddRptMgr, String ModuleID, String strTokenId)
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
            string jReqString = "SmMemberCode= " + SmMemberCode + ", CarrierCode= " + CarrierCode + ", Pan= " + Pan + ", EntryDate=" + Pan + ", Occupation= " + Occupation +
                " Prefix= " + Prefix + ", GivenName= " + GivenName + ", Surname= " + Surname + ", FatherName= " + FatherName + ", CndCat= " + CndCat + ", DOB= " + DOB + ", Gender= " + Gender +
                " Nationality= " + Nationality + ", MaritalStat= " + MaritalStat + ", CnctType= " + CnctType + ", HomeTel= " + HomeTel + " , WorkTel= " + WorkTel + ", Mobilecode= " + Mobilecode +
                " MobileTel= " + MobileTel + ", Email=" + Email + ", MobileCode2= " + MobileCode2 + ", Mobile2= " + Mobile2 + ", Email2= " + Email2 + ", AgentType= " + AgentType +
                " RecruitAgtCode= " + RecruitAgtCode + ", Bizsrc= " + Bizsrc + ", ChnCls= " + ChnCls + ", RecruitUnitCode= " + RecruitUnitCode + ", cmsunitcode= " + cmsunitcode +
                " MobUniquerecId= " + MobUniquerecId + ", CreateBy=" + CreateBy + ", CandType=" + CandType + ", AnniversaryDate=" + AnniversaryDate + ", AddRptMgr=" + AddRptMgr + ", ModuleID=" + ModuleID;

            SeqNo = ServerTrnReqLog(jReqString, "ProspectRegistration_mob", "", "", "Candidate Manager");
            String result = ValidateRequest(jReqString);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(CreateBy, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            objDaset.Clear();
            htable.Clear();

            htable.Add("@SmMemberCode", SmMemberCode);
            htable.Add("@CarrierCode", CarrierCode);
            htable.Add("@Pan", Pan);
            htable.Add("@EntryDate1", EntryDate);
            htable.Add("@Occupation", Occupation);
            htable.Add("@Prefix", Prefix);
            htable.Add("@GivenName", GivenName);
            htable.Add("@Surname", Surname);
            htable.Add("@FatherName", FatherName);
            htable.Add("@CndCat", CndCat);
            htable.Add("@BirthRegDate1", DOB);
            htable.Add("@Gender", Gender);
            htable.Add("@Nationality", Nationality);
            htable.Add("@MaritalStat", MaritalStat);
            htable.Add("@CnctType", CnctType);
            htable.Add("@HomeTel", HomeTel);
            htable.Add("@WorkTel", WorkTel);
            htable.Add("@Mobilecode", Mobilecode);
            htable.Add("@MobileTel", MobileTel);
            htable.Add("@Email", Email);
            htable.Add("@MobileCode2", MobileCode2);
            htable.Add("@Mobile2", Mobile2);
            htable.Add("@Email2", Email2);
            htable.Add("@AgentType", AgentType);
            htable.Add("@RecruitAgtCode", RecruitAgtCode);
            htable.Add("@Bizsrc", Bizsrc);
            htable.Add("@ChnCls", ChnCls);
            htable.Add("@cmsunitcode", cmsunitcode);
            htable.Add("@MobUniquerecId", MobUniquerecId);
            htable.Add("@CreateBy", CreateBy);
            htable.Add("@RecruitUnitCode", RecruitUnitCode);
            htable.Add("@CandType", CandType);
            htable.Add("@AnniversaryDate", AnniversaryDate);
            htable.Add("@AddRptMgr", AddRptMgr);
            htable.Add("@ModuleID", ModuleID);

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

    [WebMethod(EnableSession = true)]
    public string GetProspectDetails(string AppNo, string CarrierCode, String SapCode, String strTokenId)
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
            String param = "AppNo= " + AppNo + ", CarrierCode= " + CarrierCode;
            SeqNo = ServerTrnReqLog(param, "GetProspectDetails", "", "", "Candidate Manager");
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            objDaset.Clear();
            htable.Clear();

			htable.Add("@SapCode", encryptDecrypt.DecodeAndDecrypt(SapCode));
            htable.Add("@CarrierCode", CarrierCode);
            htable.Add("@Appno", AppNo);
			

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


    [WebMethod(EnableSession = true)]
    public string InsProfilingDetailsFrmMob(string AppNo, string CndNo, string SMTitle, string SMName, string SapCode, string SMBranchCode, string AgentType, string Title, string Name, string MobileNo, string DOB,
        string Qualification, string PAN, string IsCnctPrsnSame, string CnctPrsnTitle, string CnctPrsnName, string CnctPrsnRelation, string CnctPrsnMobileNo, string CnctPrsnAddress, string CnctPrsnEmail,
        string Profile, string IsWorkOtrGICompany, string NameOtrGICompany, string IsWorkOtrGICompnyClub, string NameOtrGICompnyClub, string MotorPotential, string MotorContrbuton, string HealthPotential, string HealthContrbuton,
        string CLPotential, string CLContrbuton, string TotalContrbuton, string TotalPotential, string CreateBy, string CarrierCode, 
        string Remark, string MntrDOB, string MntrMaritalStat, string MntrAnvrsrtDt, string ModuleID, String strTokenId)
    {

        try
        {
            //AppNo = "ANUfMyKtzGo9X4y05S9Wpg=="; CndNo = "b1cEZrT1VzpuU6Tw3mjfQQ=="; SMTitle = "MR"; SMName = "Ramchandra Bhosale"; SapCode = "70011354"; SMBranchCode = "1108"; AgentType = "POSP"; Title = "MR"; Name = "PRASHANT SANJAY VIJAY"; MobileNo = "8451895681"; DOB = "01/01/1989"; Qualification = "MBA"; PAN = "ASRPY6889K"; IsCnctPrsnSame = "N"; CnctPrsnTitle = "MS"; CnctPrsnName = "SD"; CnctPrsnRelation = "FA"; CnctPrsnMobileNo = "9847537589"; CnctPrsnAddress = "asa"; CnctPrsnEmail = "asa"; Profile = "GI Agent - PSU"; IsWorkOtrGICompany = "Y"; NameOtrGICompany = "sef"; IsWorkOtrGICompnyClub = "Y"; NameOtrGICompnyClub = "efw"; MotorPotential = "34"; MotorContrbuton = "6"; HealthPotential = "6"; HealthContrbuton = "1"; CLPotential = "76"; CLContrbuton = "6"; TotalContrbuton = "13"; TotalPotential = "116"; CreateBy = "yqWcMmpcbOEYmJ0mElImRg=="; CarrierCode = "2";

            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strCndNo = encryptDecrypt.DecodeAndDecrypt(CndNo);
            string strCreateBy = encryptDecrypt.DecodeAndDecrypt(CreateBy);

            String param = "AppNo= " + strAppNo + ", CndNo= " + strCndNo + ", SMTitle= " + SMTitle + ", SMName= " + SMName + ", SapCode= " + SapCode + ", SMBranchCode = " + @SMBranchCode + ", AgentType= " + AgentType + ", Title= " + Title + ", Name= " + Name + ", MobileNo= " + MobileNo +
                ", DOB= " + DOB + ", Qualification= " + Qualification + ", PAN= " + PAN + ", IsCnctPrsnSame= " + IsCnctPrsnSame + ", CnctPrsnTitle= " + CnctPrsnTitle + ", CnctPrsnName= " + CnctPrsnName + ", CnctPrsnRelation= " + CnctPrsnRelation +
                ", CnctPrsnMobileNo= " + CnctPrsnMobileNo + ", CnctPrsnAddress= " + CnctPrsnAddress + ", CnctPrsnEmail= " + CnctPrsnEmail + ", Profile= " + Profile + ", IsWorkOtrGICompany= " + IsWorkOtrGICompany + ", NameOtrGICompany= " + NameOtrGICompany + ", IsWorkOtrGICompnyClub= " + IsWorkOtrGICompnyClub +
                ", NameOtrGICompnyClub= " + NameOtrGICompnyClub + ", MotorPotential= " + MotorPotential + ", MotorContribution= " + MotorContrbuton + ", HealthPotential= " + HealthPotential + ", HealthContribution= " + HealthContrbuton + ", CommercialLinesPotential= " + CLPotential +
                ", CommercialLinesContribution= " + CLContrbuton + ", TotalContribution= " + TotalContrbuton + ", TotalBusinessPotential= " + TotalPotential + ", CreateBy= " + CreateBy + ", MntrDOB= " + MntrDOB + ", MntrMaritalStat= " + MntrMaritalStat + ", MntrAnvrsrtDt= " + MntrAnvrsrtDt + ", ModuleID= " + ModuleID;

            SeqNo = ServerTrnReqLog(param, "InsProfilingDetailsFrmMob", "", strAppNo, "Candidate Manager");
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(SapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            //Hashtable ht = new Hashtable();	
            htParam.Clear();
            htParam.Add("AppNo", strAppNo);
            htParam.Add("CndNo", strCndNo);
            //htParam.Add("SMTitle", SMTitle);
            htParam.Add("SMName", SMName);
            htParam.Add("SapCode", SapCode);
            htParam.Add("SMBranchCode", SMBranchCode);
            htParam.Add("AgentType", AgentType);
            htParam.Add("Title", Title);
            htParam.Add("Name", Name);
            htParam.Add("MobileNo", MobileNo);
            htParam.Add("DOB", DOB);
            htParam.Add("PAN", PAN);
            htParam.Add("Qualification", Qualification);
            htParam.Add("IsCnctPrsnSame", IsCnctPrsnSame);
            htParam.Add("CnctPrsnTitle", CnctPrsnTitle);
            htParam.Add("CnctPrsnName", CnctPrsnName);
            htParam.Add("CnctPrsnRelation", CnctPrsnRelation);
            htParam.Add("CnctPrsnMobileNo", CnctPrsnMobileNo);
            htParam.Add("CnctPrsnAddress", CnctPrsnAddress);
            htParam.Add("CnctPrsnEmail", CnctPrsnEmail);
            htParam.Add("Profile", Profile);
            htParam.Add("IsWorkOtrGICompany", IsWorkOtrGICompany);
            htParam.Add("NameOtrGICompany", NameOtrGICompany);
            htParam.Add("IsWorkOtrGICompnyClub", IsWorkOtrGICompnyClub);
            htParam.Add("NameOtrGICompnyClub", NameOtrGICompnyClub);
            htParam.Add("MotorContrbuton", MotorPotential);
            htParam.Add("MotorPotential", MotorPotential);
            htParam.Add("HealthContrbuton", HealthContrbuton);
            htParam.Add("HealthPotential", HealthPotential);
            htParam.Add("CLContrbuton", CLContrbuton);
            htParam.Add("CLPotential", CLPotential);
            htParam.Add("TotalContrbuton", TotalContrbuton);
            htParam.Add("TotalPotential", TotalPotential);
            htParam.Add("CarrierCode", CarrierCode);
            htParam.Add("CreateBy", strCreateBy);
            htParam.Add("Remark", Remark);
            htParam.Add("MntrDOB", MntrDOB);
            htParam.Add("MntrMaritalStat", MntrMaritalStat);
            htParam.Add("MntrAnvrsrtDt", MntrAnvrsrtDt);
            htParam.Add("ModuleID", ModuleID);
            dstemp = objDAL.selectFromDatabase("prc_InsProfilingDtls_mob", htParam, "IsysARTL");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    //return ds2json(dstemp);	
                    strJsonResponce = ds2json(dstemp); ;
                }
                else
                {
                    strJsonResponce = "No Memcode found for your SAPCode.";
                }
            }
            else
                strJsonResponce = "ds is null";
        }
        catch (Exception ex)
        {
            strJsonResponce = "In Catch block = " + ex.ToString();
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }



    [WebMethod(EnableSession = true)]
    public string GetPOSPCfrRaisedCandidate(string SapCode, string AppNo, string CndName, string MemberType, string EntryDate, String strTokenId)
    {


        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);

            String param = "AppNo : " + strAppNo + ",SapCode : " + strSapCode + "CndName : " + CndName + "MemberType : " + MemberType + "EntryDate : " + EntryDate;
            SeqNo = ServerTrnReqLog(param, "GetPOSPCfrRaisedCandidate", strSapCode, strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            //dstemp.Clear();
            //dstemp = objDAL.selectFromDatabase_singleValue("prc_GetMemberCode_App", strSapCode, "ISysARTL");
            //if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            //{
            //    string _memCode = Convert.ToString(dstemp.Tables[0].Rows[0]["MemCode"]);

            //}

            dstemp.Clear();
            Hashtable htCFR = new Hashtable();
            htCFR.Add("@BMCode", strSapCode);
            htCFR.Add("@AppNo", strAppNo);
            htCFR.Add("@CndName", CndName);
            htCFR.Add("@MemberType", MemberType);
            htCFR.Add("@EntryDate", EntryDate);

            dstemp = objDAL.selectFromDatabase("Prc_GetAdvDetailsForPOSPCFR_mobile", htCFR, "ISysARTL");
            if (dstemp.Tables.Count > 0)
            {
                if (dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "No Candidates found for your Search.";
                }
            }
            else
            {
                strJsonResponce = "No Candidates found for your Search.";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = ex.ToString();
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string GetCandidateExam_detail(string AppNo, string CarrierCode, String SapCode, String strTokenId)
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
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            String param = "AppNo : " + strAppNo + ",CarrierCode : " + CarrierCode;

            SeqNo = ServerTrnReqLog(param, "GetCandidateExam_detail", "", strAppNo, "Candidate Manager");
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

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

    [WebMethod(EnableSession = true)]
    public string UpdateCandidateReExam_details(string AppNo, string CndNo, string PreffExmDt1, string ExmDtApprvr, string ExamMode,
        string ExmBody, string ExmLang, string ExmCentre, string CreatedBy, string SysExmDt, string PreffExmDt2, string FeesWavier,
        string ModuleID, String strTokenId)
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
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strCndNo = encryptDecrypt.DecodeAndDecrypt(CndNo);
            String param = "AppNo : " + strAppNo + ",CndNo : " + CndNo + ",PreffExmDt1 : " + PreffExmDt1 + ", ExmDtApprvr: " + ExmDtApprvr +
                ", ExamMode: " + ExamMode + ", ExmBody: " + ExmBody + ", ExmLang: " + ExmLang + ", ExmCentre: " + ExmCentre + ", CreatedBy:" + CreatedBy +
                ", SysExmDt:" + SysExmDt + ", PreffExmDt2: " + PreffExmDt2 + ", FeesWavier: " + FeesWavier + ", ModuleID: " + ModuleID;

            SeqNo = ServerTrnReqLog(param, "UpdateCandidateReExam_details", "", Convert.ToString(strAppNo).Trim(), "Candidate Manager");
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(CreatedBy, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            if (strAppNo != null && strAppNo != "")
            {
                objDaset.Clear();
                htable.Clear();

                htable.Add("@CndNo", Convert.ToString(strCndNo).Trim());
                htable.Add("@AppNo", Convert.ToString(strAppNo).Trim());
                htable.Add("@PreffExmDt1", Convert.ToString(PreffExmDt1).Trim());
                htable.Add("@ExmDtApprvr", Convert.ToString(ExmDtApprvr).Trim());
                htable.Add("@ExamMode", Convert.ToString(ExamMode).Trim());
                htable.Add("@ExmBody", Convert.ToString(ExmBody).Trim());
                htable.Add("@ExmLang", Convert.ToString(ExmLang).Trim());
                htable.Add("@ExmCentre", Convert.ToString(ExmCentre).Trim());
                htable.Add("@CreatedBy", Convert.ToString(CreatedBy).Trim());
                htable.Add("@SysExmDt", Convert.ToString(SysExmDt).Trim());
                htable.Add("@PreffExmDt2", Convert.ToString(PreffExmDt2).Trim());
                if (FeesWavier != null && FeesWavier == "Y")
                {
                    htable.Add("@WaiverFlag", "1");
                    htable.Add("@FeesWaiverFlag", Convert.ToString(FeesWavier).Trim());
                }
                else
                {
                    htable.Add("@WaiverFlag", "");
                    htable.Add("@FeesWaiverFlag", "NA");
                }
                htable.Add("@ModuleID", Convert.ToString(ModuleID).Trim());

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

    [WebMethod(EnableSession = true)]
    public string Get_MasterDetails(string SapCode, String strTokenId)
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
            String param = "SapCode : " + encryptDecrypt.DecodeAndDecrypt(SapCode) +"";

            
            SeqNo = ServerTrnReqLog(param, "Get_MasterDetails", Convert.ToString(SapCode).Trim(), "", "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }

            if (SapCode != null && SapCode != "")
            {
                objDaset.Clear();
                htable.Clear();
                htable.Add("@SapCode", Convert.ToString(SapCode).Trim());
                //htable.Add("@CarrierCode", Convert.ToString(jObject["CarrierCode"]).Trim());
                objDaset = objDAL.selectFromDatabase("Prc_GetCndMasterDtls_Mob", htable, "IsysARTL");
                if (objDaset.Tables.Count > 0 && objDaset.Tables[0].Rows.Count > 0)
                {
                    JSonString = ds2json(objDaset);
                }
                else
                {
                    JSonString = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"No Memcode found for your SAPCode.\"}]}";
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

    [WebMethod(EnableSession = true)]
    public string GetSupportCounts(String UserID, String App_ID, String CustomerName, String strTokenId)
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
            String param = "UserID : " + UserID + " App_ID : " + App_ID + " CustomerName : " + CustomerName + "";
            
            SeqNo = ServerTrnReqLog(param, "GetSupportCounts", Convert.ToString(UserID).Trim(), "", "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(UserID, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }

            if (App_ID != null && App_ID != "")
            {
                objDaset.Clear();
                htable.Clear();
                htable.Add("@UserID", Convert.ToString(UserID).Trim());
                htable.Add("@App_ID", Convert.ToString(App_ID).Trim());
                htable.Add("@CustomerName", Convert.ToString(CustomerName).Trim());
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

    // end 07-03-2020

    public void LogException(string FuncName, string ErrDesc, string strUserId)
    {
        try
        {
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@LogDtime", DateTime.Now.ToString().Trim());
            ht.Add("@AppName", "IsysWS");
            ht.Add("@PageName", "Service");
            ht.Add("@FuncName", FuncName.ToString().Trim());
            ht.Add("@ErrDesc", ErrDesc.ToString().Trim());
            ht.Add("@UserId", "WebService");
            ht.Add("@Severity", "Medium");
            dstemp = objDAL.selectFromDatabase("Mob_LogError", ht, "USRMGMT");
        }
        catch (Exception ex)
        {
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@LogDtime", DateTime.Now.ToString().Trim());
            ht.Add("@AppName", "IsysWS");
            ht.Add("@PageName", "Service");
            ht.Add("@FuncName", "LogException");
            ht.Add("@ErrDesc", ex.ToString().Trim());
            ht.Add("@UserId", "WebService");
            ht.Add("@Severity", "Medium");
            dstemp = objDAL.selectFromDatabase("Mob_LogError", ht, "USRMGMT");
        }
    }

    public DataSet FuncConvertToDataset(XmlNode xNode)
    {
        DataSet ds = new DataSet();
        string tmp = string.Empty;
        if ((xNode != null) && xNode.InnerXml.Length > 0)
        {
            XmlTextReader reader = new XmlTextReader(xNode.OuterXml, XmlNodeType.Element, null);
            ds.ReadXml(reader);
        }
        return ds;
    }


    ////Franchisee UAT

    // Franchisee Onboarding - 24-Jan-2022 - Chandrashekhar Tayade
    //public DataSet FuncConvertToDataset(XmlNode xNode)
    //{
    //    DataSet ds = new DataSet();
    //    string tmp = string.Empty;
    //    if ((xNode != null) && xNode.InnerXml.Length > 0)
    //    {
    //        XmlTextReader reader = new XmlTextReader(xNode.OuterXml, XmlNodeType.Element, null);
    //        ds.ReadXml(reader);
    //    }
    //    return ds;
    //}

    [WebMethod(EnableSession = true)]
    public string FranchiseeRegistration(string request)
    {
        XmlDocument xmlInPut = new XmlDocument();

        try
        {
            String objDecXML = encryptDecrypt.DecodeAndDecrypt(request);
            xmlInPut.LoadXml(objDecXML);
            XmlDocument Objxml = new XmlDocument();
            Objxml.InnerXml = objDecXML;
            DataSet xmlDS = new DataSet();
            xmlDS = FuncConvertToDataset(Objxml);

            SeqNo = ServerTrnReqLog(objDecXML, "FranchiseeRegistration",
                xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim(), "", "Candidate Manager");

            String strTokenId = xmlDS.Tables[0].Rows[0]["strTokenId"].ToString().Trim();
            String result = ValidateRequest(objDecXML);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim(), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            Hashtable htable = new Hashtable();
            htable.Add("@MemCode", xmlDS.Tables[0].Rows[0]["MemCode"].ToString().Trim());
            htable.Add("@SAPCODE", xmlDS.Tables[0].Rows[0]["SAPCODE"].ToString().Trim());
            htable.Add("@Prefix", xmlDS.Tables[0].Rows[0]["Prefix"].ToString().Trim());
            htable.Add("@LegalName", xmlDS.Tables[0].Rows[0]["LegalName"].ToString().Trim());
            htable.Add("@PAN", xmlDS.Tables[0].Rows[0]["PAN"].ToString().Trim());
            htable.Add("@DOB", xmlDS.Tables[0].Rows[0]["DOB"].ToString().Trim());
            htable.Add("@Gender", xmlDS.Tables[0].Rows[0]["Gender"].ToString().Trim());
            htable.Add("@MStatus", xmlDS.Tables[0].Rows[0]["MStatus"].ToString().Trim());
            htable.Add("@MADate", xmlDS.Tables[0].Rows[0]["MADate"].ToString().Trim());
            htable.Add("@Education", xmlDS.Tables[0].Rows[0]["Education"].ToString().Trim());
            htable.Add("@Mobile", xmlDS.Tables[0].Rows[0]["Mobile"].ToString().Trim());
            htable.Add("@Mobile1", xmlDS.Tables[0].Rows[0]["Mobile1"].ToString().Trim());
            htable.Add("@Email", xmlDS.Tables[0].Rows[0]["Email"].ToString().Trim());
            htable.Add("@Email1", xmlDS.Tables[0].Rows[0]["Email1"].ToString().Trim());
            htable.Add("@FAX", xmlDS.Tables[0].Rows[0]["FAX"].ToString().Trim());
            htable.Add("@TwiterId", xmlDS.Tables[0].Rows[0]["TwiterId"].ToString().Trim());
            htable.Add("@FacebookId", xmlDS.Tables[0].Rows[0]["FacebookId"].ToString().Trim());
            htable.Add("@InstagramId", xmlDS.Tables[0].Rows[0]["InstagramId"].ToString().Trim());
            htable.Add("@crAddrType", xmlDS.Tables[0].Rows[0]["crAddrType"].ToString().Trim());
            htable.Add("@crAddressLine1", xmlDS.Tables[0].Rows[0]["crAddressLine1"].ToString().Trim());
            htable.Add("@crAddressLine2", xmlDS.Tables[0].Rows[0]["crAddressLine2"].ToString().Trim());
            htable.Add("@crAddressLine3", xmlDS.Tables[0].Rows[0]["crAddressLine3"].ToString().Trim());
            htable.Add("@crPostCode", xmlDS.Tables[0].Rows[0]["crPostCode"].ToString().Trim());
            htable.Add("@crCity", xmlDS.Tables[0].Rows[0]["crCity"].ToString().Trim());
            htable.Add("@crStateCode", xmlDS.Tables[0].Rows[0]["crStateCode"].ToString().Trim());
            htable.Add("@crState", xmlDS.Tables[0].Rows[0]["crState"].ToString().Trim());
            htable.Add("@crDistrict", xmlDS.Tables[0].Rows[0]["crDistrict"].ToString().Trim());
            htable.Add("@crVillage", xmlDS.Tables[0].Rows[0]["crVillage"].ToString().Trim());
            htable.Add("@crArea", xmlDS.Tables[0].Rows[0]["crArea"].ToString().Trim());
            htable.Add("@crCntryCode", xmlDS.Tables[0].Rows[0]["crCntryCode"].ToString().Trim());
            htable.Add("@crCountry", xmlDS.Tables[0].Rows[0]["crCountry"].ToString().Trim());
            htable.Add("@idAddrType", xmlDS.Tables[0].Rows[0]["idAddrType"].ToString().Trim());
            htable.Add("@idAddressLine1", xmlDS.Tables[0].Rows[0]["idAddressLine1"].ToString().Trim());
            htable.Add("@idAddressLine2", xmlDS.Tables[0].Rows[0]["idAddressLine2"].ToString().Trim());
            htable.Add("@idAddressLine3", xmlDS.Tables[0].Rows[0]["idAddressLine3"].ToString().Trim());
            htable.Add("@idPostCode", xmlDS.Tables[0].Rows[0]["idPostCode"].ToString().Trim());
            htable.Add("@idCity", xmlDS.Tables[0].Rows[0]["idCity"].ToString().Trim());
            htable.Add("@idStateCode", xmlDS.Tables[0].Rows[0]["idStateCode"].ToString().Trim());
            htable.Add("@idState", xmlDS.Tables[0].Rows[0]["idState"].ToString().Trim());
            htable.Add("@idDistrict", xmlDS.Tables[0].Rows[0]["idDistrict"].ToString().Trim());
            htable.Add("@idVillage", xmlDS.Tables[0].Rows[0]["idVillage"].ToString().Trim());
            htable.Add("@idArea", xmlDS.Tables[0].Rows[0]["idArea"].ToString().Trim());
            htable.Add("@idCntryCode", xmlDS.Tables[0].Rows[0]["idCntryCode"].ToString().Trim());
            htable.Add("@idCountry", xmlDS.Tables[0].Rows[0]["idCountry"].ToString().Trim());
            htable.Add("@BaddrType", xmlDS.Tables[0].Rows[0]["BaddrType"].ToString().Trim());
            htable.Add("@BAddressLine1", xmlDS.Tables[0].Rows[0]["BAddressLine1"].ToString().Trim());
            htable.Add("@BAddressLine2", xmlDS.Tables[0].Rows[0]["BAddressLine2"].ToString().Trim());
            htable.Add("@BAddressLine3", xmlDS.Tables[0].Rows[0]["BAddressLine3"].ToString().Trim());
            htable.Add("@BPostCode", xmlDS.Tables[0].Rows[0]["BPostCode"].ToString().Trim());
            htable.Add("@BCity", xmlDS.Tables[0].Rows[0]["BCity"].ToString().Trim());
            htable.Add("@BStateCode", xmlDS.Tables[0].Rows[0]["BStateCode"].ToString().Trim());
            htable.Add("@BState", xmlDS.Tables[0].Rows[0]["BState"].ToString().Trim());
            htable.Add("@BDistrict", xmlDS.Tables[0].Rows[0]["BDistrict"].ToString().Trim());
            htable.Add("@BVillage", xmlDS.Tables[0].Rows[0]["BVillage"].ToString().Trim());
            htable.Add("@BArea", xmlDS.Tables[0].Rows[0]["BArea"].ToString().Trim());
            htable.Add("@BCntryCode", xmlDS.Tables[0].Rows[0]["BCntryCode"].ToString().Trim());
            htable.Add("@BCountry", xmlDS.Tables[0].Rows[0]["BCountry"].ToString().Trim());
            htable.Add("@ExpMonBusnsHealth", xmlDS.Tables[0].Rows[0]["ExpMonBusnsHealth"].ToString().Trim());
            htable.Add("@ExpTeamStrgt", xmlDS.Tables[0].Rows[0]["ExpTeamStrgt"].ToString().Trim());
            htable.Add("@ExpAgntInSixMnth", xmlDS.Tables[0].Rows[0]["ExpAgntInSixMnth"].ToString().Trim());
            htable.Add("@ExpAgntInTwelveMnth", xmlDS.Tables[0].Rows[0]["ExpAgntInTwelveMnth"].ToString().Trim());
            htable.Add("@ExpPOSPInSixMnth", xmlDS.Tables[0].Rows[0]["ExpPOSPInSixMnth"].ToString().Trim());
            htable.Add("@ExpPOSPInTwelveMnth", xmlDS.Tables[0].Rows[0]["ExpPOSPInTwelveMnth"].ToString().Trim());
            htable.Add("@DtlsOfOthrbusiness", xmlDS.Tables[0].Rows[0]["DtlsOfOthrbusiness"].ToString().Trim());
            htable.Add("@MrktingInFinservicebusiness", xmlDS.Tables[0].Rows[0]["MrktingInFinservicebusiness"].ToString().Trim());
            htable.Add("@DtlsOfLawSuit", xmlDS.Tables[0].Rows[0]["DtlsOfLawSuit"].ToString().Trim());
            htable.Add("@DtlsOfOthrAssignmt", xmlDS.Tables[0].Rows[0]["DtlsOfOthrAssignmt"].ToString().Trim());
            htable.Add("@Remark", xmlDS.Tables[0].Rows[0]["Remark"].ToString().Trim());
            htable.Add("@BckgrndExp", xmlDS.Tables[0].Rows[0]["BckgrndExp"].ToString().Trim());
            htable.Add("@BExpMYear", xmlDS.Tables[0].Rows[0]["BExpMYear"].ToString().Trim());
            htable.Add("@Organization", xmlDS.Tables[0].Rows[0]["Organization"].ToString().Trim());
            htable.Add("@TenureFrm", xmlDS.Tables[0].Rows[0]["TenureFrm"].ToString().Trim());
            htable.Add("@TenureTo", xmlDS.Tables[0].Rows[0]["TenureTo"].ToString().Trim());
            htable.Add("@OtherSector", xmlDS.Tables[0].Rows[0]["OtherSector"].ToString().Trim());
            htable.Add("@ChnlType", xmlDS.Tables[0].Rows[0]["ChnlType"].ToString().Trim());
            htable.Add("@BizSrc", xmlDS.Tables[0].Rows[0]["BizSrc"].ToString().Trim());
            htable.Add("@ChnCls", xmlDS.Tables[0].Rows[0]["ChnCls"].ToString().Trim());
            htable.Add("@MemType", xmlDS.Tables[0].Rows[0]["MemType"].ToString().Trim());
            htable.Add("@ConstCode", xmlDS.Tables[0].Rows[0]["ConstCode"].ToString().Trim());
            htable.Add("@ConstType", xmlDS.Tables[0].Rows[0]["ConstType"].ToString().Trim());
            htable.Add("@UntCode", xmlDS.Tables[0].Rows[0]["UntCode"].ToString().Trim());
            htable.Add("@BankAcntName", xmlDS.Tables[0].Rows[0]["BankAcntName"].ToString().Trim());
            htable.Add("@BankAcntType", xmlDS.Tables[0].Rows[0]["BankAcntType"].ToString().Trim());
            htable.Add("@BankAccntNo", xmlDS.Tables[0].Rows[0]["BankAccntNo"].ToString().Trim());
            htable.Add("@BankName", xmlDS.Tables[0].Rows[0]["BankName"].ToString().Trim());
            htable.Add("@BankBranch", xmlDS.Tables[0].Rows[0]["BankBranch"].ToString().Trim());
            htable.Add("@BankBrnCode", xmlDS.Tables[0].Rows[0]["BankBrnCode"].ToString().Trim());
            htable.Add("@IFSCCode", xmlDS.Tables[0].Rows[0]["IFSCCode"].ToString().Trim());
            htable.Add("@MICRCode", xmlDS.Tables[0].Rows[0]["MICRCode"].ToString().Trim());
            htable.Add("@BankBranchAddr", xmlDS.Tables[0].Rows[0]["BankBranchAddr"].ToString().Trim());
            htable.Add("@NomineeName", xmlDS.Tables[0].Rows[0]["NomineeName"].ToString().Trim());
            htable.Add("@NomineeRel", xmlDS.Tables[0].Rows[0]["NomineeRel"].ToString().Trim());
            htable.Add("@NomineeDOB", xmlDS.Tables[0].Rows[0]["NomineeDOB"].ToString().Trim());
            htable.Add("@NomineeMobileTel", xmlDS.Tables[0].Rows[0]["NomineeMobileTel"].ToString().Trim());
            htable.Add("@NomineeEmailID", xmlDS.Tables[0].Rows[0]["NomineeEmailID"].ToString().Trim());
            //htable.Add("@TempMemCode", xmlDS.Tables[0].Rows[0]["TempMemCode"].ToString().Trim());
            htable.Add("@CreatedBy", xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim());
            htable.Add("@CreatedDate", xmlDS.Tables[0].Rows[0]["CreatedDate"].ToString().Trim());
            //htable.Add("@UpdatedBy", xmlDS.Tables[0].Rows[0]["UpdatedBy"].ToString().Trim());
            //htable.Add("@UpdatedDate", xmlDS.Tables[0].Rows[0]["UpdatedDate"].ToString().Trim());
            htable.Add("@UniqueRefID", xmlDS.Tables[0].Rows[0]["UniqueRefID"].ToString().Trim());
            // added by chandrashekhar for ad-hoc req 
            htable.Add("@GSTNAFlag", xmlDS.Tables[0].Rows[0]["GSTNAFlag"].ToString().Trim());
            htable.Add("@TNCFlag", xmlDS.Tables[0].Rows[0]["TNCFlag"].ToString().Trim());
            //htable.Add("@SyncStatus", xmlDS.Tables[0].Rows[0]["SyncStatus"].ToString().Trim());

            dstemp = objDAL.selectFromDatabase("Prc_InsMemberDetails_Mob", htable, "ISysCHMS");
            if (dstemp.Tables.Count > 0)
            {
                if (dstemp.Tables[0].Rows.Count > 0)
                {
                    try
                    {
                        if (dstemp.Tables[0].Rows[0]["ResponseCode"].ToString() == "0"
                            && dstemp.Tables[0].Rows[0]["WStatus"].ToString() == "20")
                        {
                            ConvertRDLCToPDF(dstemp.Tables[0].Rows[0]["MemCode"].ToString(),
                                dstemp.Tables[0].Rows[0]["EmpCode"].ToString(),
                                xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim());
                        }
                    }
                    catch (Exception es)
                    {
                        es.ToString();
                    }

                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Registration Failed\"}]}";
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string FranchiseeModification(string request)
    {
        XmlDocument xmlInPut = new XmlDocument();

        try
        {
            String objDecXML = encryptDecrypt.DecodeAndDecrypt(request);
            xmlInPut.LoadXml(objDecXML);
            XmlDocument Objxml = new XmlDocument();
            Objxml.InnerXml = objDecXML;
            DataSet xmlDS = new DataSet();
            xmlDS = FuncConvertToDataset(Objxml);

            SeqNo = ServerTrnReqLog(objDecXML, "FranchiseeModification",
                xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim(), "", "Candidate Manager");

            String strTokenId = xmlDS.Tables[0].Rows[0]["strTokenId"].ToString().Trim();
            String result = ValidateRequest(objDecXML);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim(), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            Hashtable htable = new Hashtable();
            htable.Add("@MemCode", xmlDS.Tables[0].Rows[0]["MemCode"].ToString().Trim());
            htable.Add("@Prefix", xmlDS.Tables[0].Rows[0]["Prefix"].ToString().Trim());
            htable.Add("@LegalName", xmlDS.Tables[0].Rows[0]["LegalName"].ToString().Trim());
            htable.Add("@PAN", xmlDS.Tables[0].Rows[0]["PAN"].ToString().Trim());
            htable.Add("@DOB", xmlDS.Tables[0].Rows[0]["DOB"].ToString().Trim());
            htable.Add("@Gender", xmlDS.Tables[0].Rows[0]["Gender"].ToString().Trim());
            htable.Add("@MStatus", xmlDS.Tables[0].Rows[0]["MStatus"].ToString().Trim());
            htable.Add("@MADate", xmlDS.Tables[0].Rows[0]["MADate"].ToString().Trim());
            htable.Add("@Education", xmlDS.Tables[0].Rows[0]["Education"].ToString().Trim());
            htable.Add("@Mobile", xmlDS.Tables[0].Rows[0]["Mobile"].ToString().Trim());
            htable.Add("@Mobile1", xmlDS.Tables[0].Rows[0]["Mobile1"].ToString().Trim());
            htable.Add("@Email", xmlDS.Tables[0].Rows[0]["Email"].ToString().Trim());
            htable.Add("@Email1", xmlDS.Tables[0].Rows[0]["Email1"].ToString().Trim());
            htable.Add("@FAX", xmlDS.Tables[0].Rows[0]["FAX"].ToString().Trim());
            htable.Add("@TwiterId", xmlDS.Tables[0].Rows[0]["TwiterId"].ToString().Trim());
            htable.Add("@FacebookId", xmlDS.Tables[0].Rows[0]["FacebookId"].ToString().Trim());
            htable.Add("@InstagramId", xmlDS.Tables[0].Rows[0]["InstagramId"].ToString().Trim());
            htable.Add("@crAddrType", xmlDS.Tables[0].Rows[0]["crAddrType"].ToString().Trim());
            htable.Add("@crAddressLine1", xmlDS.Tables[0].Rows[0]["crAddressLine1"].ToString().Trim());
            htable.Add("@crAddressLine2", xmlDS.Tables[0].Rows[0]["crAddressLine2"].ToString().Trim());
            htable.Add("@crAddressLine3", xmlDS.Tables[0].Rows[0]["crAddressLine3"].ToString().Trim());
            htable.Add("@crPostCode", xmlDS.Tables[0].Rows[0]["crPostCode"].ToString().Trim());
            htable.Add("@crCity", xmlDS.Tables[0].Rows[0]["crCity"].ToString().Trim());
            htable.Add("@crStateCode", xmlDS.Tables[0].Rows[0]["crStateCode"].ToString().Trim());
            htable.Add("@crState", xmlDS.Tables[0].Rows[0]["crState"].ToString().Trim());
            htable.Add("@crDistrict", xmlDS.Tables[0].Rows[0]["crDistrict"].ToString().Trim());
            htable.Add("@crVillage", xmlDS.Tables[0].Rows[0]["crVillage"].ToString().Trim());
            htable.Add("@crArea", xmlDS.Tables[0].Rows[0]["crArea"].ToString().Trim());
            htable.Add("@crCntryCode", xmlDS.Tables[0].Rows[0]["crCntryCode"].ToString().Trim());
            htable.Add("@crCountry", xmlDS.Tables[0].Rows[0]["crCountry"].ToString().Trim());
            htable.Add("@idAddrType", xmlDS.Tables[0].Rows[0]["idAddrType"].ToString().Trim());
            htable.Add("@idAddressLine1", xmlDS.Tables[0].Rows[0]["idAddressLine1"].ToString().Trim());
            htable.Add("@idAddressLine2", xmlDS.Tables[0].Rows[0]["idAddressLine2"].ToString().Trim());
            htable.Add("@idAddressLine3", xmlDS.Tables[0].Rows[0]["idAddressLine3"].ToString().Trim());
            htable.Add("@idPostCode", xmlDS.Tables[0].Rows[0]["idPostCode"].ToString().Trim());
            htable.Add("@idCity", xmlDS.Tables[0].Rows[0]["idCity"].ToString().Trim());
            htable.Add("@idStateCode", xmlDS.Tables[0].Rows[0]["idStateCode"].ToString().Trim());
            htable.Add("@idState", xmlDS.Tables[0].Rows[0]["idState"].ToString().Trim());
            htable.Add("@idDistrict", xmlDS.Tables[0].Rows[0]["idDistrict"].ToString().Trim());
            htable.Add("@idVillage", xmlDS.Tables[0].Rows[0]["idVillage"].ToString().Trim());
            htable.Add("@idArea", xmlDS.Tables[0].Rows[0]["idArea"].ToString().Trim());
            htable.Add("@idCntryCode", xmlDS.Tables[0].Rows[0]["idCntryCode"].ToString().Trim());
            htable.Add("@idCountry", xmlDS.Tables[0].Rows[0]["idCountry"].ToString().Trim());
            htable.Add("@BaddrType", xmlDS.Tables[0].Rows[0]["BaddrType"].ToString().Trim());
            htable.Add("@BAddressLine1", xmlDS.Tables[0].Rows[0]["BAddressLine1"].ToString().Trim());
            htable.Add("@BAddressLine2", xmlDS.Tables[0].Rows[0]["BAddressLine2"].ToString().Trim());
            htable.Add("@BAddressLine3", xmlDS.Tables[0].Rows[0]["BAddressLine3"].ToString().Trim());
            htable.Add("@BPostCode", xmlDS.Tables[0].Rows[0]["BPostCode"].ToString().Trim());
            htable.Add("@BCity", xmlDS.Tables[0].Rows[0]["BCity"].ToString().Trim());
            htable.Add("@BStateCode", xmlDS.Tables[0].Rows[0]["BStateCode"].ToString().Trim());
            htable.Add("@BState", xmlDS.Tables[0].Rows[0]["BState"].ToString().Trim());
            htable.Add("@BDistrict", xmlDS.Tables[0].Rows[0]["BDistrict"].ToString().Trim());
            htable.Add("@BVillage", xmlDS.Tables[0].Rows[0]["BVillage"].ToString().Trim());
            htable.Add("@BArea", xmlDS.Tables[0].Rows[0]["BArea"].ToString().Trim());
            htable.Add("@BCntryCode", xmlDS.Tables[0].Rows[0]["BCntryCode"].ToString().Trim());
            htable.Add("@BCountry", xmlDS.Tables[0].Rows[0]["BCountry"].ToString().Trim());
            htable.Add("@ExpMonBusnsHealth", xmlDS.Tables[0].Rows[0]["ExpMonBusnsHealth"].ToString().Trim());
            htable.Add("@ExpTeamStrgt", xmlDS.Tables[0].Rows[0]["ExpTeamStrgt"].ToString().Trim());
            htable.Add("@ExpAgntInSixMnth", xmlDS.Tables[0].Rows[0]["ExpAgntInSixMnth"].ToString().Trim());
            htable.Add("@ExpAgntInTwelveMnth", xmlDS.Tables[0].Rows[0]["ExpAgntInTwelveMnth"].ToString().Trim());
            htable.Add("@ExpPOSPInSixMnth", xmlDS.Tables[0].Rows[0]["ExpPOSPInSixMnth"].ToString().Trim());
            htable.Add("@ExpPOSPInTwelveMnth", xmlDS.Tables[0].Rows[0]["ExpPOSPInTwelveMnth"].ToString().Trim());
            htable.Add("@DtlsOfOthrbusiness", xmlDS.Tables[0].Rows[0]["DtlsOfOthrbusiness"].ToString().Trim());
            htable.Add("@MrktingInFinservicebusiness", xmlDS.Tables[0].Rows[0]["MrktingInFinservicebusiness"].ToString().Trim());
            htable.Add("@DtlsOfLawSuit", xmlDS.Tables[0].Rows[0]["DtlsOfLawSuit"].ToString().Trim());
            htable.Add("@DtlsOfOthrAssignmt", xmlDS.Tables[0].Rows[0]["DtlsOfOthrAssignmt"].ToString().Trim());
            htable.Add("@Remark", xmlDS.Tables[0].Rows[0]["Remark"].ToString().Trim());
            htable.Add("@BckgrndExp", xmlDS.Tables[0].Rows[0]["BckgrndExp"].ToString().Trim());
            htable.Add("@BExpMYear", xmlDS.Tables[0].Rows[0]["BExpMYear"].ToString().Trim());
            htable.Add("@Organization", xmlDS.Tables[0].Rows[0]["Organization"].ToString().Trim());
            htable.Add("@TenureFrm", xmlDS.Tables[0].Rows[0]["TenureFrm"].ToString().Trim());
            htable.Add("@TenureTo", xmlDS.Tables[0].Rows[0]["TenureTo"].ToString().Trim());
            htable.Add("@OtherSector", xmlDS.Tables[0].Rows[0]["OtherSector"].ToString().Trim());
            htable.Add("@ChnlType", xmlDS.Tables[0].Rows[0]["ChnlType"].ToString().Trim());
            htable.Add("@BizSrc", xmlDS.Tables[0].Rows[0]["BizSrc"].ToString().Trim());
            htable.Add("@ChnCls", xmlDS.Tables[0].Rows[0]["ChnCls"].ToString().Trim());
            htable.Add("@MemType", xmlDS.Tables[0].Rows[0]["MemType"].ToString().Trim());
            htable.Add("@ConstCode", xmlDS.Tables[0].Rows[0]["ConstCode"].ToString().Trim());
            htable.Add("@ConstType", xmlDS.Tables[0].Rows[0]["ConstType"].ToString().Trim());
            htable.Add("@UntCode", xmlDS.Tables[0].Rows[0]["UntCode"].ToString().Trim());
            htable.Add("@BankAcntName", xmlDS.Tables[0].Rows[0]["BankAcntName"].ToString().Trim());
            htable.Add("@BankAcntType", xmlDS.Tables[0].Rows[0]["BankAcntType"].ToString().Trim());
            htable.Add("@BankAccntNo", xmlDS.Tables[0].Rows[0]["BankAccntNo"].ToString().Trim());
            htable.Add("@BankName", xmlDS.Tables[0].Rows[0]["BankName"].ToString().Trim());
            htable.Add("@BankBranch", xmlDS.Tables[0].Rows[0]["BankBranch"].ToString().Trim());
            htable.Add("@BankBrnCode", xmlDS.Tables[0].Rows[0]["BankBrnCode"].ToString().Trim());
            htable.Add("@IFSCCode", xmlDS.Tables[0].Rows[0]["IFSCCode"].ToString().Trim());
            htable.Add("@MICRCode", xmlDS.Tables[0].Rows[0]["MICRCode"].ToString().Trim());
            htable.Add("@BankBranchAddr", xmlDS.Tables[0].Rows[0]["BankBranchAddr"].ToString().Trim());
            htable.Add("@NomineeName", xmlDS.Tables[0].Rows[0]["NomineeName"].ToString().Trim());
            htable.Add("@NomineeRel", xmlDS.Tables[0].Rows[0]["NomineeRel"].ToString().Trim());
            htable.Add("@NomineeDOB", xmlDS.Tables[0].Rows[0]["NomineeDOB"].ToString().Trim());
            htable.Add("@NomineeMobileTel", xmlDS.Tables[0].Rows[0]["NomineeMobileTel"].ToString().Trim());
            htable.Add("@NomineeEmailID", xmlDS.Tables[0].Rows[0]["NomineeEmailID"].ToString().Trim());
            //htable.Add("@TempMemCode", xmlDS.Tables[0].Rows[0]["TempMemCode"].ToString().Trim());
            htable.Add("@CreatedBy", xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim());
            htable.Add("@CreatedDate", xmlDS.Tables[0].Rows[0]["CreatedDate"].ToString().Trim());
            htable.Add("@UpdatedBy", xmlDS.Tables[0].Rows[0]["UpdatedBy"].ToString().Trim());
            htable.Add("@UpdatedDate", xmlDS.Tables[0].Rows[0]["UpdatedDate"].ToString().Trim());
            htable.Add("@UniqueRefID", xmlDS.Tables[0].Rows[0]["UniqueRefID"].ToString().Trim());
            //htable.Add("@SyncStatus", xmlDS.Tables[0].Rows[0]["SyncStatus"].ToString().Trim());

            // added by chandrashekhar for ad-hoc req 
            htable.Add("@GSTNAFlag", xmlDS.Tables[0].Rows[0]["GSTNAFlag"].ToString().Trim());
            htable.Add("@TNCFlag", xmlDS.Tables[0].Rows[0]["TNCFlag"].ToString().Trim());

            dstemp = objDAL.selectFromDatabase("Prc_UpdateMemberDetails_Mob", htable, "ISysCHMS");
            if (dstemp.Tables.Count > 0)
            {
                if (dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                    try
                    {
                        if (dstemp.Tables[0].Rows[0]["ResponseCode"].ToString() == "0"
                            && dstemp.Tables[0].Rows[0]["WStatus"].ToString() == "20")
                        {
                            ConvertRDLCToPDF(dstemp.Tables[0].Rows[0]["MemCode"].ToString(),
                                dstemp.Tables[0].Rows[0]["EmpCode"].ToString(),
                                xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim());
                        }
                    }
                    catch (Exception es)
                    {
                        es.ToString();
                    }
                }
                else
                {
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Modification Failed\"}]}";
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string FranchiseeMgrDetails(string request)
    {
        XmlDocument xmlInPut = new XmlDocument();

        try
        {
            String objDecXML = encryptDecrypt.DecodeAndDecrypt(request);
            //String objDecXML = "<?xml version=\"1.0\" encoding=\"UTF - 8\"?><MemRelation> <SrNo>null</SrNo> <CarrierCode>2</CarrierCode> <Bizsrc>XX</Bizsrc> <chncls>XXSO</chncls> <MemType>PM</MemType> <MemCode>40086623</MemCode> <RelBizsrc>XX</RelBizsrc> <RelChnCls>XXSO</RelChnCls> <RelMemType>PP</RelMemType> <RelMemCode>40086619</RelMemCode> <RelOrder>1</RelOrder> <Reltype>CF</Reltype> <Status>0</Status> <CeaseDate/> <CreatedBy>77F10144</CreatedBy> <CreatedDate>03/04/2022</CreatedDate> <UpdatedBy>77F10144</UpdatedBy> <UpdatedDate>03/04/2022</UpdatedDate> <UniqueRefID>Android-3f6792ea-0719-4518-8077-59de2fb8c8eb</UniqueRefID> <SyncStatus>Pending</SyncStatus> <MemName>RAJU</MemName> <RelMemName>MAHESH</RelMemName> <EmpCode/> <RelEmpCode>77F10144</RelEmpCode> <strTokenId>5CECAA78-5C7E-474F-9ADF-45DCC7A20081</strTokenId> </MemRelation> ";
            xmlInPut.LoadXml(objDecXML);
            XmlDocument Objxml = new XmlDocument();
            Objxml.InnerXml = objDecXML;
            DataSet xmlDS = new DataSet();
            xmlDS = FuncConvertToDataset(Objxml);

            SeqNo = ServerTrnReqLog(objDecXML, "FranchiseeMgrDetails",
                xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim(), "", "Candidate Manager");

            String strTokenId = xmlDS.Tables[0].Rows[0]["strTokenId"].ToString().Trim();
            String result = ValidateRequest(objDecXML);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim(), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            Hashtable htable = new Hashtable();
            htable.Add("@CarrierCode", xmlDS.Tables[0].Rows[0]["CarrierCode"].ToString().Trim());
            htable.Add("@Bizsrc", xmlDS.Tables[0].Rows[0]["Bizsrc"].ToString().Trim());
            htable.Add("@chncls", xmlDS.Tables[0].Rows[0]["chncls"].ToString().Trim());
            htable.Add("@MemType", xmlDS.Tables[0].Rows[0]["MemType"].ToString().Trim());
            htable.Add("@MemCode", xmlDS.Tables[0].Rows[0]["MemCode"].ToString().Trim());
            htable.Add("@RelBizsrc", xmlDS.Tables[0].Rows[0]["RelBizsrc"].ToString().Trim());
            htable.Add("@RelChnCls", xmlDS.Tables[0].Rows[0]["RelChnCls"].ToString().Trim());
            htable.Add("@RelMemType", xmlDS.Tables[0].Rows[0]["RelMemType"].ToString().Trim());
            htable.Add("@RelMemCode", xmlDS.Tables[0].Rows[0]["RelMemCode"].ToString().Trim());
            htable.Add("@RelOrder", xmlDS.Tables[0].Rows[0]["RelOrder"].ToString().Trim());
            htable.Add("@Reltype", xmlDS.Tables[0].Rows[0]["Reltype"].ToString().Trim());
            htable.Add("@Status", xmlDS.Tables[0].Rows[0]["Status"].ToString().Trim());
            htable.Add("@CreatedBy", xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim());
            htable.Add("@UpdatedBy", xmlDS.Tables[0].Rows[0]["UpdatedBy"].ToString().Trim());
            htable.Add("@UniqueRefID", xmlDS.Tables[0].Rows[0]["UniqueRefID"].ToString().Trim());
            //htable.Add("@SyncStatus", xmlDS.Tables[0].Rows[0]["SyncStatus"].ToString().Trim());

            dstemp = objDAL.selectFromDatabase("Prc_InsMemRelDetails_Mob", htable, "ISysCHMS");
            if (dstemp.Tables.Count > 0)
            {
                if (dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Failed\"}]}";
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string FranchiseeExpDetails(string request)
    {
        XmlDocument xmlInPut = new XmlDocument();

        try
        {
            String objDecXML = encryptDecrypt.DecodeAndDecrypt(request);
            xmlInPut.LoadXml(objDecXML);
            XmlDocument Objxml = new XmlDocument();
            Objxml.InnerXml = objDecXML;
            DataSet xmlDS = new DataSet();
            xmlDS = FuncConvertToDataset(Objxml);

            SeqNo = ServerTrnReqLog(objDecXML, "FranchiseeExpDetails",
                xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim(), "", "Candidate Manager");

            String strTokenId = xmlDS.Tables[0].Rows[0]["strTokenId"].ToString().Trim();
            String result = ValidateRequest(objDecXML);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim(), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            Hashtable htable = new Hashtable();
            htable.Add("@SrNo", xmlDS.Tables[0].Rows[0]["SrNo"].ToString().Trim());
            htable.Add("@MemCode", xmlDS.Tables[0].Rows[0]["MemCode"].ToString().Trim());
            htable.Add("@BckgrndExp", xmlDS.Tables[0].Rows[0]["BckgrndExp"].ToString().Trim());
            htable.Add("@ExpTime", xmlDS.Tables[0].Rows[0]["ExpTime"].ToString().Trim());
            htable.Add("@Organization", xmlDS.Tables[0].Rows[0]["Organization"].ToString().Trim());
            htable.Add("@TenureFrm", xmlDS.Tables[0].Rows[0]["TenureFrm"].ToString().Trim());
            htable.Add("@TenureTo", xmlDS.Tables[0].Rows[0]["TenureTo"].ToString().Trim());
            htable.Add("@OtherSector", xmlDS.Tables[0].Rows[0]["OtherSector"].ToString().Trim());
            htable.Add("@IsActive", xmlDS.Tables[0].Rows[0]["IsActive"].ToString().Trim());
            htable.Add("@CreatedBy", xmlDS.Tables[0].Rows[0]["CreatedBy"].ToString().Trim());
            htable.Add("@UpdatedBy", xmlDS.Tables[0].Rows[0]["UpdatedBy"].ToString().Trim());
            htable.Add("@SrvrRecID", xmlDS.Tables[0].Rows[0]["SrvrID"].ToString().Trim());
            htable.Add("@UniqueRefID", xmlDS.Tables[0].Rows[0]["UniqueRefID"].ToString().Trim());

            dstemp = objDAL.selectFromDatabase("Prc_InsMemBckgrndExp", htable, "ISysCHMS");
            if (dstemp.Tables.Count > 0)
            {
                if (dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Failed\"}]}";
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public String GetFPAddRptMgr(String mCode, String mType, String BizSrc, String ChnCls, String UnitCode, String SapCode, String strTokenId)
    {
        String strmCode = encryptDecrypt.DecodeAndDecrypt(mCode);
        String param = "MemCode : " + strmCode + ",Memtype : " + mType + ", BizSrc:" + BizSrc + ", ChnCls:" + ChnCls + ", UnitCode:" + UnitCode;
        SeqNo = ServerTrnReqLog(param, "GetFPAddRptMgr", "", "", "Candidate Manager");

        try
        {
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@MemCode", strmCode.ToString().Trim());
            ht.Add("@MemType", mType.ToString().Trim());
            ht.Add("@BizSrc", BizSrc.ToString().Trim());
            ht.Add("@ChnCls", ChnCls.ToString().Trim());
            ht.Add("@UnitCode", UnitCode.ToString().Trim());

            dstemp = objDAL.selectFromDatabase("Prc_GetAddRptMgrCodeDtls", ht, "IsysCHMS");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"Failed\"}]}";
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string SearchMemberDetails(string FranchiseeCode, string FranchiseeSAP, string FranchiseeName,
        string RegDateFrom, string RegDateTo, string PAN, string SapCode, string flag, String strTokenId)
    {
        try
        {
            string strFranchiseeCode = encryptDecrypt.DecodeAndDecrypt(FranchiseeCode);
            string strFranchiseeSAP = encryptDecrypt.DecodeAndDecrypt(FranchiseeSAP);
            string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN);
            string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);

            String param = "FranchiseeCode : " + strFranchiseeCode + ", FranchiseeSAP : " + strFranchiseeSAP + ", FranchiseeName : "
                + FranchiseeName + ", RegDateFrom : " + RegDateFrom + ", RegDateTo : " + RegDateTo + ", PAN : " + strPAN
                + ", flag" + flag;

            SeqNo = ServerTrnReqLog(param, "SearchMemberDetails", strSapCode, strFranchiseeCode, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            Hashtable htCFR = new Hashtable();
            htCFR.Add("@SapCode", strSapCode);
            htCFR.Add("@MemCode", strFranchiseeCode);
            htCFR.Add("@EmpCode", strFranchiseeSAP);
            htCFR.Add("@MemName", FranchiseeName);
            htCFR.Add("@RegDateFrom", RegDateFrom);
            htCFR.Add("@RegDateTo", RegDateTo);
            htCFR.Add("@PAN", strPAN);
            htCFR.Add("@flag", flag);

            dstemp = objDAL.selectFromDatabase("Prc_SearchMemberDetails", htCFR, "ISysCHMS");
            if (dstemp.Tables.Count > 0)
            {
                if (dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Record Found\"}]}";
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string GetMemberDetails(string FranchiseeCode, string SapCode, String strTokenId)
    {
        try
        {
            string strFranchiseeCode = encryptDecrypt.DecodeAndDecrypt(FranchiseeCode);
            //string strFranchiseeSAP = encryptDecrypt.DecodeAndDecrypt(FranchiseeSAP);
            // string strPAN = encryptDecrypt.DecodeAndDecrypt(PAN);
            string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);

            String param = "FranchiseeCode : " + strFranchiseeCode + ", SAP CODE : " + strSapCode;

            SeqNo = ServerTrnReqLog(param, "GetMemberDetails", strSapCode, strFranchiseeCode, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            Hashtable htCFR = new Hashtable();
            htCFR.Add("@CarrierCode", '2');
            htCFR.Add("@MemCode", strFranchiseeCode);
            htCFR.Add("@SAPCode", strSapCode);


            dstemp = objDAL.selectFromDatabase("Prc_GetMemberDetails_Mob", htCFR, "ISysCHMS");
            if (dstemp.Tables.Count > 0)
            {
                if (dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Record Found\"}]}";
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string GetCfrRaisedFranchisee(string SapCode, string AppNo, string CndName, string MemberType, string EntryDate, String strTokenId)
    {
        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strSapCode = encryptDecrypt.DecodeAndDecrypt(SapCode);

            String param = "AppNo : " + strAppNo + ",SapCode : " + strSapCode + "CndName : " + CndName + "MemberType : " + MemberType + "EntryDate : " + EntryDate;
            SeqNo = ServerTrnReqLog(param, "GetCfrRaisedFranchisee", strSapCode, strAppNo, "Candidate Manager");
            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strSapCode, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

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
            htCFR.Add("@MemName", CndName);
            htCFR.Add("@MemberCode", _memCode);
            htCFR.Add("@MemberType", MemberType);
            htCFR.Add("@Flag", string.Empty);

            dstemp = objDAL.selectFromDatabase("Prc_GetMemAdvDetailsForCFR", htCFR, "ISysCHMS");
            if (dstemp.Tables.Count > 0)
            {
                if (dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Record Found\"}]}";
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string GetCFRStatus_Franchisee(string AppNo, String SapCode, String strTokenId)
    {

        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);

            String param = "AppNo : " + strAppNo;
            SeqNo = ServerTrnReqLog(param, "GetCFRStatus_Franchisee", "", strAppNo, "Candidate Manager");

            String _ProcessType = "NR";
            String _CndNo = strAppNo.Trim();

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            Hashtable htTable = new Hashtable();
            htTable.Add("@CndNo", _CndNo);
            htTable.Add("@Flag", _ProcessType);
            dstemp = objDAL.selectFromDatabase("Prc_GetCFRStatus_Mob", htTable, "ISysCHMS");
            //dstemp = objDAL.GetDataForAppSearch("Prc_GetCFRStatus_Mob", _CndNo, _ProcessType, "ISysARTL");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string CFR_Responde_Franchisee(string AppNo, string CfrFor, string RemarkId, string RespondedBy, 
        string ModuleID, String SapCode, String strTokenId)
    {
        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);
            string strRespondedBy = encryptDecrypt.DecodeAndDecrypt(RespondedBy);

            String param = "AppNo : " + strAppNo + ",CfrFor : " + CfrFor + "RemarkId : " + RemarkId +
             "RespondedBy : " + strRespondedBy + " ModuleID : " + ModuleID;
            SeqNo = ServerTrnReqLog(param, "CFR_Responde_Franchisee", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            Hashtable htparam = new Hashtable();
            htparam.Add("@AppNo", strAppNo);
            htparam.Add("@CfrFor", CfrFor);
            htparam.Add("@RemarkId", RemarkId);
            htparam.Add("@RespondedBy", strRespondedBy);
            htparam.Add("@ModuleID", ModuleID);

            dstemp = objDAL.selectFromDatabase("Prc_Submit_AssignedCFR_Mob", htparam, "ISysCHMS");

            strJsonResponce = "Responded Successfully";
        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
            //return "error";
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string GetCFRdesc_Franchisee(string Appno, string desc, String SapCode, String strTokenId)
    {

        try
        {
            string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);

            String param = "AppNo : " + strAppno + ",desc : " + desc;
            SeqNo = ServerTrnReqLog(param, "GetCFRdesc_Franchisee", "", strAppno, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@Appno", strAppno.ToString().Trim());
            htParam.Add("@desc", desc.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetCFRdesc_Mob", htParam, "ISysCHMS");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }

        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string Get_SupportDocs_Franchisee(string AppNo, string CandType, String SapCode, String strTokenId,String EntityType)
    {

        try
        {
            string strAppNo = encryptDecrypt.DecodeAndDecrypt(AppNo);

            String param = "AppNo : " + strAppNo + " , cand_type:" + CandType + ", EntityType:"+ EntityType;
            SeqNo = ServerTrnReqLog(param, "Get_SupportDocs_Franchisee", "", strAppNo, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@Appno", strAppNo.ToString().Trim());
            ht.Add("@Cand_type", CandType.Trim());
            ht.Add("@EntityType", EntityType.Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetCand_SupportDocs", ht, "ISysCHMS");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"No Record Found\"}]}";
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;
    }

    [WebMethod(EnableSession = true)]
    public string GetImageForMemDocType(String Appno, String DocType, String SapCode, String strTokenId)
    {

        try
        {
            string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);

            String param = "AppNo : " + strAppno + ",DocType : " + DocType;
            SeqNo = ServerTrnReqLog(param, "GetImageForMemDocType", "", strAppno, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(encryptDecrypt.DecodeAndDecrypt(SapCode), strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@Appno", strAppno.ToString().Trim());
            htParam.Add("@DocType", DocType.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_GetDocForMob", htParam, "IsysCHMS");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;

    }

    [WebMethod(EnableSession = true)]
    public string SubmitDocumentRequest(String MemCode, String MemType, String CreatedBy, String strTokenId,
        String EntityType, String GSTNAFlag, String TNCFlag)
    {

        try
        {
            string strMemCode = encryptDecrypt.DecodeAndDecrypt(MemCode);
            //string strMemType = encryptDecrypt.DecodeAndDecrypt(MemType);
            string strCreatedBy = encryptDecrypt.DecodeAndDecrypt(CreatedBy);

            String param = "MemCode : " + strMemCode + ", MemType : " + MemType + ", CreatedBy : " + strCreatedBy 
                + ", EntityType:"+ EntityType + ", GSTNAFlag:" + GSTNAFlag + ", TNCFlag:" + TNCFlag;

            SeqNo = ServerTrnReqLog(param, "SubmitDocumentRequest", "", strMemCode, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strCreatedBy, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp.Clear();
            htParam.Clear();
            htParam.Add("@MemCode", strMemCode.ToString().Trim());
            htParam.Add("@MemType", MemType.ToString().Trim());
            htParam.Add("@CreatedBy", strCreatedBy.ToString().Trim());
            htParam.Add("@EntityType", EntityType.ToString().Trim());
            htParam.Add("@GSTNAFlag", GSTNAFlag.ToString().Trim());
            htParam.Add("@TNCFlag", TNCFlag.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_InsDocumentRequest_Mob", htParam, "IsysCHMS");
            if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
            {
                strJsonResponce = ds2json(dstemp);
                try
                {
                    if (dstemp.Tables[0].Rows[0]["ResponseCode"].ToString() == "0"
                        && dstemp.Tables[0].Rows[0]["WStatus"].ToString() == "20")
                    {
                        ConvertRDLCToPDF(dstemp.Tables[0].Rows[0]["MemCode"].ToString(),
                            dstemp.Tables[0].Rows[0]["EmpCode"].ToString(),
                            strCreatedBy);
                    }
                }
                catch (Exception es)
                {
                    es.ToString();
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;

    }

    [WebMethod(EnableSession = true)]
    public string InsertMemDocsFrmMob(
        String Appno, byte[] Image, String DocType, String UploadedBy,
        String lblimgshrt, String lblCndNoValue, String FileType,
        String DocCode, String MemType, String strTokenId)
    {

        try
        {
            string strAppno = encryptDecrypt.DecodeAndDecrypt(Appno);
            string strUploadedBy = encryptDecrypt.DecodeAndDecrypt(UploadedBy);
            string strlblCndNoValue = encryptDecrypt.DecodeAndDecrypt(lblCndNoValue);

            String param = "AppNo : " + strAppno + ",DocType : " + DocType + ",UploadedBy : " + strUploadedBy + ",lblimgshrt : " 
                + lblimgshrt + ",lblCndNoValue : " + strlblCndNoValue + ",FileType : " + FileType + ", DocCode:" + DocCode + ", MemType: " + MemType;
            SeqNo = ServerTrnReqLog(param, "InsertMemDocsFrmMob", "", strAppno, "Candidate Manager");

            String result = ValidateRequest(param);
            if (!result.Equals("Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"" + result + "\"}]}";
                updateResponse(strJsonResponce, SeqNo);
                return strJsonResponce;
            }
            else if (strTokenId.Equals(string.Empty) || (!strTokenId.Equals(string.Empty) && ValidateToken(strUploadedBy, strTokenId) != "Valid"))
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"ErrorDescription\":\"invalid token details\"}]}";
                updateResponse(strJsonResponce, SeqNo);

                return HttpStatusCode.NonAuthoritativeInformation.ToString();
                //(HttpStatusCode.NonAuthoritativeInformation, strJsonResponce).ToString();

            }
            dstemp = null;
            Hashtable ht = new Hashtable();
            ht.Add("@Appno", strAppno.ToString().Trim());
            ht.Add("@Image", Image);
            ht.Add("@DocType", DocType.ToString().Trim());
            ht.Add("@UploadedBy", strUploadedBy.ToString().Trim());
            ht.Add("@FileType", FileType.ToString().Trim());
            ht.Add("@DocCode", DocCode.ToString().Trim());
            ht.Add("@MemType", MemType.ToString().Trim());
            dstemp = objDAL.selectFromDatabase("Prc_InsMemDocsFrmMob", ht, "IsysCHMS");
            if (dstemp != null)
            {
                if (dstemp.Tables.Count > 0 && dstemp.Tables[0].Rows.Count > 0)
                {
                    //return ds2json(dstemp);
                    //CreateFolderOfDoc(lblimgshrt, strlblCndNoValue, Image);
                    strJsonResponce = ds2json(dstemp);
                }
                else
                {
                    strJsonResponce = dstemp.Tables[0].Rows.Count.ToString();
                }
            }
            else
            {
                strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"ds is null\"}]}";
            }

        }
        catch (Exception ex)
        {
            strJsonResponce = "{\"Table\": [ {\"ResponseCode\": \"1\",\"Message\":\"" + ex.ToString() + "\"}]}";
        }
        updateResponse(strJsonResponce, SeqNo);
        strJsonResponce = encryptDecrypt.EncryptAndEncode(strJsonResponce);
        return strJsonResponce;

    }

    public void ConvertRDLCToPDF(String MemCode, String EmpCode,String UserID)
    {
        try
        {
            Hashtable htCRP = new Hashtable();
            htCRP.Add("@MemCode", MemCode);
            DataSet dsCRP = new DataSet();
            //dsCRP = objDAL.GetDataSetForPrcCLP("prc_GetFPWelcomeLetter", htCRP);

            dsCRP = objDAL.selectFromDatabase("Prc_GetDetailsForDocCreation", htCRP, "IsysCHMS");

            foreach (DataRow dr in dsCRP.Tables[0].Rows)
            {
                try
                {
                    if (dr["RDLC_PROC"].ToString() != String.Empty)
                    {
                        DataSet ds = new DataSet();
                        ds = objDAL.selectFromDatabase(dr["RDLC_PROC"].ToString(), htCRP, "IsysCHMS");

                        ReportDataSource rdsAct = new ReportDataSource(dr["RDLC_DS"].ToString(), ds.Tables[0]);
                        ReportViewer viewer = new ReportViewer();
                        viewer.LocalReport.Refresh();
                        viewer.LocalReport.ReportPath = Server.MapPath(dr["MRDLC_PATH"].ToString()); //This is your rdlc name.
                                                                                                     // viewer.LocalReport.SetParameters(param);
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
                    }
                }catch(Exception ex)
                {
                    ex.StackTrace.ToString();
                }
            }
        }
        catch (Exception e)
        {
            e.ToString();
        }

    }



    //public void ConvertRDLCToPDF(String MemCode, String EmpCode)
    //{
    //    try
    //    {

    //        //string strAppoint = string.Empty;
    //        //strAppoint = "WelcomeLetter";
    //        ////string strID = string.Empty;
    //        ////strID = "ID";
    //        //string strFileRePath = string.Empty;

    //        //String strPath = "E:\\IsysSuiteCMS\\Documents\\";

    //        ////  strcndno = Memcode;
    //        ////Added BY Nikhil
    //        //if (Directory.Exists(strPath) == false)
    //        //{
    //        //    strPath = strPath + Memcode + "\\";
    //        //    Directory.CreateDirectory(strPath);
    //        //}
    //        //else
    //        //{
    //        //    strFileRePath = strPath + Memcode + "\\";
    //        //    //if (!Directory.Exists(Server.MapPath(strFilePath)))
    //        //    if (!Directory.Exists(strFileRePath))
    //        //    {
    //        //        // Directory.CreateDirectory(Server.MapPath(strFilePath));
    //        //        Directory.CreateDirectory(strFileRePath);
    //        //    }
    //        //    else
    //        //    {
    //        //        strFileRePath = strPath + Memcode + "\\";
    //        //    }
    //        //}
    //        ////Ended By Nikhil


    //        //strFileRePath = strPath + cndno + "\\"; 
    //        //"C:/CHMS/I-SysSuite/Application/ISys/Recruit/UploadedFiles/"  +cndno + "/";
    //        //"D:/Hosted Application/I-SysSuite/Application/ISys/Recruit/UploadedFiles/" + cndno;// +"/";

    //        Hashtable htCRP = new Hashtable();
    //        htCRP.Add("@MemCode", MemCode);
    //        DataSet dsCRP = new DataSet();
    //        //dsCRP = objDAL.GetDataSetForPrcCLP("prc_GetFPWelcomeLetter", htCRP);
    //        dsCRP = objDAL.selectFromDatabase("prc_GetFPWelcomeLetter", htCRP, "IsysCHMS");
    //        ReportDataSource rdsAct = new ReportDataSource("FPWelcomeLetter", dsCRP.Tables[0]);
    //        ReportViewer viewer = new ReportViewer();
    //        viewer.LocalReport.Refresh();
    //        viewer.LocalReport.ReportPath = Server.MapPath("~/App_Data/FPWelcomeLetter.rdlc");  //This is your rdlc name.
    //        // viewer.LocalReport.SetParameters(param);
    //        viewer.LocalReport.DataSources.Add(rdsAct); // Add  datasource here         
    //        Warning[] warnings;
    //        string[] streamIds;
    //        string mimeType = string.Empty; ;
    //        string encoding = string.Empty; ;
    //        string extension = string.Empty; ;

    //        //Microsoft.Reporting.WebForms.Warning[] warnings;
    //        //string[] warnings;
    //        byte[] bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
    //        //using (FileStream fs = new FileStream(strFileRePath + "FPWelcomeLetter.pdf", FileMode.Create))
    //        //{
    //        //    fs.Write(bytes, 0, bytes.Length);
    //        //}
    //        Hashtable htApp = new Hashtable();
    //        DataSet dsApp = new DataSet();
    //        htApp.Add("@MemCode", MemCode.ToString().Trim());
    //        htApp.Add("@UserFilename", "FPWelcomeLetter.pdf");
    //        htApp.Add("@ServerFilename", "FPWelcomeLetter.pdf");
    //        htApp.Add("@PDFByte", bytes);
    //        htApp.Add("@CreateBy", UserID.ToString().Trim());

    //        //objDAL.GetDataSetForPrcCLP("Prc_InsFPConRDLCToPDF", htApp);

    //        dsApp = objDAL.selectFromDatabase("Prc_InsFPConRDLCToPDF", htApp, "IsysCHMS");

    //    }
    //    catch (Exception e)
    //    {
    //        e.ToString();
    //    }

    //}

}
