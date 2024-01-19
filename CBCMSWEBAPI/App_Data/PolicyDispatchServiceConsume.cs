

using System.Collections;
using System.Data;
using System.Xml;
using System;
using System.Web;
using System.IO;
using CBCMSWEBAPI;
//using PolicyDispatchService;




/// <summary>
/// Summary description for PolicyDispatchServiceConsume
/// </summary>
/// nam
//namespace PolicyDispatchService
//{
    public class PolicyDispatchServiceConsume
    {

    #region Declare Veriables
    //private DataAccessClass dataAccessRecruit = new DataAccessClass();
    private WSDataAccessLayer dataAccessRecruit = new WSDataAccessLayer();
        //ErrLog objErr = new ErrLog();
        XmlDocument xmlInPut = new XmlDocument();
        String reqXML = string.Empty;
        Hashtable htParam = new Hashtable();
        DataSet dsResult = new DataSet();
        string SeqNo = string.Empty;
        string strRecID = string.Empty;
        DataSet dsDispatch = new DataSet();
    #endregion
    //PolicyDispatchService ps = new PolicyDispatchService();
    //PolicyDispatchService.GetPolicyDispatchStatusByAgentorBasCodeCompletedEventArgs GetPolicyDispatch =new GetPolicyDispatchStatusByAgentorBasCodeCompletedEventArgs()
    //    PolicyDispatchService.UserCredential UserCredential = new PolicyDispatchService.UserCredential();
    //// PolicyDispatchService Dispatch = new PolicyDispatchService();
    // PolicyDispatchService.PolicyDispatchServiceSoapClient Dispatch = new PolicyDispatchService.PolicyDispatchServiceSoapClient();
    PolicyDispatchService policy = new PolicyDispatchService();
    UserCredential user = new UserCredential();
    
        #region Method
        public DataSet GetAgentDispatchDetails(string AgencyCode)
        {
            try
            {
                htParam.Clear();
                htParam.Add("@AuthID", 2);//Added by usha  
                //dsResult = dataAccessRecruit.GetDataSetForPrcRecruits("Prc_GetUserCredential", htParam);//change sameer
                dsResult = dataAccessRecruit.selectFromDatabase("Prc_GetUserCredential", htParam, "IsysARTL");
                user.UserID = dsResult.Tables[0].Rows[0]["UserId"].ToString();
                user.UserPassword = dsResult.Tables[0].Rows[0]["Password"].ToString();
                SeqNo = InsertWSSyncLog(AgencyCode, "GetAgentDispatchDetaipls");
                dsResult.Clear();
               //AgencyCode = "19A21713";
            // dsDispatch = Dispatch.GetPolicyDispatchStatus(AgencyCode);
            policy.UserCredentialValue=  user;
             dsDispatch = policy.GetPolicyDispatchStatus(AgencyCode);
            //
            // dsDispatch = policy.GetPolicyDispatchStatus(AgencyCode);
                reqXML = ConvertDataSetToXml(dsDispatch);
                htParam.Clear();
                dsResult.Clear();
                htParam.Add("@strSeqno", SeqNo);
                htParam.Add("@Response", reqXML);
                htParam.Add("@Flag", 1);
                //dsResult = dataAccessRecruit.GetDataSetForPrcRecruits("Prc_InsAadharServiceSyncLog", htParam);//change sameer
                dsResult = dataAccessRecruit.selectFromDatabase("Prc_InsAadharServiceSyncLog", htParam, "IsysARTL");
        }
            catch (Exception ex)
            {
                string currentFile = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                System.IO.FileInfo oInfo = new System.IO.FileInfo(currentFile);
                string sRet = oInfo.Name;
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                String LogClassName = method.ReflectedType.Name;
                //objErr.LogErr("ISYS-RGIC", sRet, method.Name.ToString(), ex.Message.ToString(), HttpContext.Current.Session["UserID"].ToString().Trim());
            }
            return dsDispatch;
        }

        #endregion


        #region Insert into InsertWSSyncLog
        public String InsertWSSyncLog(String UID, String Method)
        {
            try
            {
                htParam.Clear();
                htParam.Add("@ReqParameter1", UID.ToString());
                htParam.Add("@CreatedBy", HttpContext.Current.Session["UserId"].ToString().Trim());
                htParam.Add("@Method", Method);
                //dsResult = dataAccessRecruit.GetDataSetForPrcRecruits("Prc_InsAadharServiceSyncLog", htParam);//change sameer
                dsResult = dataAccessRecruit.selectFromDatabase("Prc_InsAadharServiceSyncLog", htParam, "IsysARTL");
                strRecID = dsResult.Tables[0].Rows[0]["ReqId"].ToString().Trim();
            }

            catch (Exception ex)
            {
                string currentFile = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                System.IO.FileInfo oInfo = new System.IO.FileInfo(currentFile);
                string sRet = oInfo.Name;
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                String LogClassName = method.ReflectedType.Name;
                //objErr.LogErr("ISYS-RGIC", sRet, method.Name.ToString(), ex.Message.ToString(), HttpContext.Current.Session["UserID"].ToString().Trim());
            }

            return strRecID;
        }
        #endregion InsertWSSyncLog

        public static string ConvertDataSetToXml(DataSet ds)
        {
            StringWriter sw = new StringWriter();
            ds.WriteXml(sw, XmlWriteMode.IgnoreSchema);
            string XML = sw.ToString();
            return XML;
        }
    }
//}