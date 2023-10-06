using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

/// <summary>
/// Summary description for API_Bridge
/// </summary>
public class API_Bridge
{
    public API_Bridge()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    string GetUrl = Convert.ToString(ConfigurationManager.AppSettings["APIPath"]);
    public string CallGetMethodBoolean(string Method, string ParameterList)
    {
        // Restful service URL
        string url = "";
        if (ParameterList != "")
        {
            //url = GetUrl + Method + "/" + ParameterList;
            url = GetUrl + ParameterList;

        }

        string strResult = string.Empty;
        // declare httpwebrequet wrt url defined above
        HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
        // set method as post
        webrequest.Method = "GET";
        // set content type
        webrequest.ContentType = "application/x-www-form-urlencoded";
        // declare & read response from service
        HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
        // set utf8 encoding
        Encoding enc = System.Text.Encoding.UTF8;
        // read response stream from response object
        StreamReader loResponseStream = new StreamReader(webresponse.GetResponseStream(), enc);
        // read string from stream data
        strResult = loResponseStream.ReadToEnd();
        //strResult = strResult.Remove(strResult.Length - 1);
        //  strResult = strResult.Remove(0, 1);
        // close the stream object
        loResponseStream.Close();
        // close the response object
        webresponse.Close();

        // string test = strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", "");

        return strResult;




    }

    public static bool AuthenticateInActiveDirectory(string UserID, string Password)
    {
        try
        {
            if (ConfigurationManager.AppSettings["LDAPstatus"].ToString().Trim().ToLower() == "true")
            {

                string GetUrl = Convert.ToString(ConfigurationManager.AppSettings["APIPath"]);
                API_Bridge api = new API_Bridge();

                string Encrypt_UserID = encode(UserID);
                string Encrypt_Pass = encode(Password);
                string UID = GetUniqueKey(10);
                string Upass = GetUniqueKey(10);
                Encrypt_UserID = UID + Encrypt_UserID + GetUniqueKey(5);
                Encrypt_Pass = Upass + Encrypt_Pass + GetUniqueKey(5);
                string parameter = "ID=" + Encrypt_UserID + "/" + "?value=" + Encrypt_Pass;

                string url = GetUrl + parameter;
                string strResult = string.Empty;
                // declare httpwebrequet wrt url defined above
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
                // set method as post
                webrequest.Method = "GET";
                // set content type
                webrequest.ContentType = "application/x-www-form-urlencoded";
                // declare & read response from service
                HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
                Encoding enc = System.Text.Encoding.UTF8;
                StreamReader loResponseStream = new StreamReader(webresponse.GetResponseStream(), enc);
                strResult = loResponseStream.ReadToEnd();

                loResponseStream.Close();
                webresponse.Close();


                bool result = Convert.ToBoolean(strResult);
                return result;
            }
            else
            {
                return true;
            }
        }
        catch (Exception Ex)
        {
            string str;
            str = Ex.ToString();
            return false;
        }

    }

    public static string GetUniqueKey(int size)
    {
        char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        byte[] data = new byte[size];
        RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
        {
            crypto.GetBytes(data);
        }
        StringBuilder result = new StringBuilder(size);
        foreach (byte b in data)
        {
            result.Append(chars[b % (chars.Length)]);
        }
        return result.ToString();
    }

    public static string encode(string text)
    {
        byte[] mybyte = System.Text.Encoding.UTF8.GetBytes(text);
        string returntext = System.Convert.ToBase64String(mybyte);
        return returntext;
    }

    //public DataSet CallGetMethodDS(string Method, string ParameterList)
    //{
    //    // Restful service URL
    //    string url = "";
    //    if (ParameterList != "")
    //    {
    //        url = GetUrl + Method + "/" + ParameterList;
    //    }
    //    else
    //    {
    //        url = GetUrl + Method;
    //    }
    //    string strResult = string.Empty;
    //    HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
    //    webrequest.Method = "GET";
    //    webrequest.ContentType = "application/x-www-form-urlencoded";
    //    HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
    //    Encoding enc = System.Text.Encoding.UTF8;
    //    StreamReader loResponseStream = new StreamReader(webresponse.GetResponseStream(), enc);
    //    strResult = loResponseStream.ReadToEnd();
    //    strResult = strResult.Remove(strResult.Length - 1);
    //    strResult = strResult.Remove(0, 1);
    //    loResponseStream.Close();
    //    webresponse.Close();
    //    string test = strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", "");
    //    DataSet ds = JsonConvert.DeserializeObject<DataSet>(strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", ""));
    //    return ds;
    //    // // DataTable dt = (DataTable)JsonConvert.DeserializeObject(test, (typeof(DataTable)));
    //    ////  DataSet ds = (DataSet)JsonConvert.DeserializeObject(test, (typeof(DataSet)));
    //    //  DataTable dt = (DataTable)JsonConvert.DeserializeObject(test, (typeof(DataTable)));
    //    //return dt;



    //}
    //public DataTable CallGetMethodDT(string Method, string ParameterList)
    //{
    //    // Restful service URL
    //    string url = "";
    //    if (ParameterList != "")
    //    {
    //        url = GetUrl + Method + "/" + ParameterList;
    //    }
    //    else
    //    {
    //        url = GetUrl + Method;
    //    }
    //    string strResult = string.Empty;
    //    // declare httpwebrequet wrt url defined above
    //    HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
    //    // set method as post
    //    webrequest.Method = "GET";
    //    // set content type
    //    webrequest.ContentType = "application/x-www-form-urlencoded";
    //    // declare & read response from service
    //    HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
    //    // set utf8 encoding
    //    Encoding enc = System.Text.Encoding.UTF8;
    //    // read response stream from response object
    //    StreamReader loResponseStream = new StreamReader(webresponse.GetResponseStream(), enc);
    //    // read string from stream data
    //    strResult = loResponseStream.ReadToEnd();
    //    strResult = strResult.Remove(strResult.Length - 1);
    //    strResult = strResult.Remove(0, 1);
    //    // close the stream object
    //    loResponseStream.Close();
    //    // close the response object
    //    webresponse.Close();
    //    // assign the final result to text box
    //    // DataSet ds = JsonConvert.DeserializeObject<DataSet>(stringResult);
    //    string test = strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", "");
    //    //using (MemoryStream stream = new MemoryStream(byteArrayData))
    //    //{
    //    //    DataTable dt;
    //    //    BinaryFormatter bformatter = new BinaryFormatter();
    //    //    dt = (DataTable)bformatter.Deserialize(stream);
    //    //}

    //    // DataSet ds = JsonConvert.DeserializeObject<DataSet>(strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", ""));
    //    // return ds;
    //    // DataTable dt = (DataTable)JsonConvert.DeserializeObject(test, (typeof(DataTable)));
    //    //  DataSet ds = (DataSet)JsonConvert.DeserializeObject(test, (typeof(DataSet)));

    //    DataTable dt = (DataTable)JsonConvert.DeserializeObject(test, (typeof(DataTable)));
    //    return dt;



    //}
    //public List<T> CallGetMethodlist<T>(string Method, string ParameterList)
    //{

    //    // Restful service URL
    //    string url = "";
    //    if (ParameterList != null)
    //    {
    //        url = GetUrl + Method + "/" + ParameterList;
    //    }
    //    else
    //    {
    //        url = GetUrl + Method;
    //    }
    //    string strResult = string.Empty;
    //    // declare httpwebrequet wrt url defined above
    //    HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
    //    // set method as post
    //    webrequest.Method = "GET";
    //    // set content type
    //    webrequest.ContentType = "application/x-www-form-urlencoded";
    //    // declare & read response from service
    //    HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
    //    // set utf8 encoding
    //    Encoding enc = System.Text.Encoding.UTF8;
    //    // read response stream from response object
    //    StreamReader loResponseStream = new StreamReader(webresponse.GetResponseStream(), enc);
    //    // read string from stream data
    //    strResult = loResponseStream.ReadToEnd();
    //    strResult = strResult.Remove(strResult.Length - 1);
    //    strResult = strResult.Remove(0, 1);
    //    // close the stream object
    //    loResponseStream.Close();
    //    // close the response object
    //    webresponse.Close();
    //    // assign the final result to text box
    //    // DataSet ds = JsonConvert.DeserializeObject<DataSet>(stringResult);
    //    string test = strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", "");
    //    //using (MemoryStream stream = new MemoryStream(byteArrayData))
    //    //{
    //    //    DataTable dt;
    //    //    BinaryFormatter bformatter = new BinaryFormatter();
    //    //    dt = (DataTable)bformatter.Deserialize(stream);
    //    //}

    //    // DataSet ds = JsonConvert.DeserializeObject<DataSet>(strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", ""));
    //    // return ds;
    //    // DataTable dt = (DataTable)JsonConvert.DeserializeObject(test, (typeof(DataTable)));
    //    //  DataSet ds = (DataSet)JsonConvert.DeserializeObject(test, (typeof(DataSet)));
    //    //List<T> datalist = new List<T>();
    //    //T Tlist = JsonConvert.DeserializeObject<T>(test);
    //    T[] items = JsonConvert.DeserializeObject<T[]>(test);

    //    List<T> list = items.Cast<T>().ToList();
    //    //JavaScriptSerializer js = new JavaScriptSerializer();
    //    //Array[] dataarr = js.Deserialize<Array[]>(test);

    //    //datalist = dataarr.OfType<Array>().ToList();
    //    //DataTable dt = (DataTable)JsonConvert.DeserializeObject(test, (typeof(DataTable)));

    //    ////return dt;
    //    //object data=CommonUtility.ConvertDataTable<T>(dt);
    //    //  datalist =
    //    //List<T> datalistsdsa = new List<T>();
    //    return list;

    //}
    //public List<T> CallPostMethodlist<T>(string Method, string Json)
    //{

    //    // Restful service URL
    //    string url = GetUrl + Method;

    //    // declare ascii encoding
    //    ASCIIEncoding encoding = new ASCIIEncoding();
    //    string strResult = string.Empty;
    //    // sample xml sent to Service & this data is sent in POST
    //    string postData = Json.ToString();
    //    // convert xmlstring to byte using ascii encoding
    //    byte[] data = encoding.GetBytes(postData);
    //    // declare httpwebrequet wrt url defined above
    //    HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
    //    // set method as post
    //    webrequest.Method = "POST";
    //    // set content type
    //    webrequest.ContentType = "application/json";
    //    // set content length
    //    webrequest.ContentLength = data.Length;
    //    // get stream data out of webrequest object
    //    Stream newStream = webrequest.GetRequestStream();
    //    newStream.Write(data, 0, data.Length);
    //    newStream.Close();
    //    // declare & read response from service
    //    HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
    //    // set utf8 encoding
    //    Encoding enc = System.Text.Encoding.UTF8;
    //    // read response stream from response object
    //    StreamReader loResponseStream = new StreamReader(webresponse.GetResponseStream(), enc);
    //    // read string from stream data
    //    strResult = loResponseStream.ReadToEnd();
    //    strResult = strResult.Remove(strResult.Length - 1);
    //    strResult = strResult.Remove(0, 1);
    //    // close the stream object
    //    loResponseStream.Close();
    //    // close the response object
    //    webresponse.Close();
    //    string JsonString = strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", "");
    //    T[] items = JsonConvert.DeserializeObject<T[]>(JsonString);
    //    List<T> list = items.Cast<T>().ToList();
    //    return list;
    //}
    //public DataTable CallGetMethodJson(string Method, string json)
    //{
    //    // Restful service URL
    //    string url = "";
    //    if (json != "")
    //    {
    //        url = GetUrl + Method + "/" + json;
    //    }
    //    else
    //    {
    //        url = GetUrl + Method;
    //    }
    //    string strResult = string.Empty;
    //    // declare httpwebrequet wrt url defined above
    //    HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
    //    // set method as post
    //    webrequest.Method = "GET";
    //    // set content type
    //    webrequest.ContentType = "application/x-www-form-urlencoded";
    //    // declare & read response from service
    //    HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
    //    // set utf8 encoding
    //    Encoding enc = System.Text.Encoding.UTF8;
    //    // read response stream from response object
    //    StreamReader loResponseStream = new StreamReader(webresponse.GetResponseStream(), enc);
    //    // read string from stream data
    //    strResult = loResponseStream.ReadToEnd();
    //    strResult = strResult.Remove(strResult.Length - 1);
    //    strResult = strResult.Remove(0, 1);
    //    // close the stream object
    //    loResponseStream.Close();
    //    // close the response object
    //    webresponse.Close();
    //    // assign the final result to text box
    //    // DataSet ds = JsonConvert.DeserializeObject<DataSet>(stringResult);
    //    string test = strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", "");
    //    //using (MemoryStream stream = new MemoryStream(byteArrayData))
    //    //{
    //    //    DataTable dt;
    //    //    BinaryFormatter bformatter = new BinaryFormatter();
    //    //    dt = (DataTable)bformatter.Deserialize(stream);
    //    //}

    //    //DataSet ds = JsonConvert.DeserializeObject<DataSet>(strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", ""));
    //    //return ds;
    //    // // DataTable dt = (DataTable)JsonConvert.DeserializeObject(test, (typeof(DataTable)));
    //    ////  DataSet ds = (DataSet)JsonConvert.DeserializeObject(test, (typeof(DataSet)));
    //    //  DataTable dt = (DataTable)JsonConvert.DeserializeObject(test, (typeof(DataTable)));
    //    //return dt;
    //    DataTable dt = (DataTable)JsonConvert.DeserializeObject(test, (typeof(DataTable)));
    //    return dt;


    //}
    //public DataSet CallPostMethod(string Method, string Json)
    //{
    //    try
    //    {
    //        // Restful service URL
    //        string url = GetUrl + Method;

    //        // declare ascii encoding
    //        ASCIIEncoding encoding = new ASCIIEncoding();
    //        string strResult = string.Empty;
    //        // sample xml sent to Service & this data is sent in POST
    //        string postData = Json.ToString();
    //        // convert xmlstring to byte using ascii encoding
    //        byte[] data = encoding.GetBytes(postData);
    //        // declare httpwebrequet wrt url defined above
    //        HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
    //        // set method as post
    //        webrequest.Method = "POST";
    //        // set content type
    //        webrequest.ContentType = "application/json";
    //        // set content length
    //        webrequest.ContentLength = data.Length;
    //        // get stream data out of webrequest object
    //        Stream newStream = webrequest.GetRequestStream();
    //        newStream.Write(data, 0, data.Length);
    //        newStream.Close();
    //        // declare & read response from service
    //        HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
    //        // set utf8 encoding
    //        Encoding enc = System.Text.Encoding.UTF8;
    //        // read response stream from response object
    //        StreamReader loResponseStream = new StreamReader(webresponse.GetResponseStream(), enc);
    //        // read string from stream data
    //        strResult = loResponseStream.ReadToEnd();
    //        strResult = strResult.Remove(strResult.Length - 1);
    //        strResult = strResult.Remove(0, 1);
    //        // close the stream object
    //        loResponseStream.Close();
    //        // close the response object
    //        webresponse.Close();
    //        // below steps remove unwanted data from response string
    //        // lblmsg.Text = strResult.Replace("</string>", "");
    //        string test = strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", "");
    //        DataSet ds = JsonConvert.DeserializeObject<DataSet>(strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", ""));
    //        return ds;
    //    }
    //    catch (Exception e)
    //    {

    //        throw e;
    //    }
    //}


    //public string CallPostMethodString(string Method, string Json)
    //{
    //    try
    //    {
    //        // Restful service URL
    //        string url = GetUrl + Method;

    //        // declare ascii encoding
    //        ASCIIEncoding encoding = new ASCIIEncoding();
    //        string strResult = string.Empty;
    //        // sample xml sent to Service & this data is sent in POST
    //        string postData = Json.ToString();
    //        // convert xmlstring to byte using ascii encoding
    //        byte[] data = encoding.GetBytes(postData);
    //        // declare httpwebrequet wrt url defined above
    //        HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
    //        // set method as post
    //        webrequest.Method = "POST";
    //        // set content type
    //        webrequest.ContentType = "application/json";
    //        // set content length
    //        webrequest.ContentLength = data.Length;
    //        // get stream data out of webrequest object
    //        Stream newStream = webrequest.GetRequestStream();
    //        newStream.Write(data, 0, data.Length);
    //        newStream.Close();
    //        // declare & read response from service
    //        HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
    //        // set utf8 encoding
    //        Encoding enc = System.Text.Encoding.UTF8;
    //        // read response stream from response object
    //        StreamReader loResponseStream = new StreamReader(webresponse.GetResponseStream(), enc);
    //        // read string from stream data
    //        strResult = loResponseStream.ReadToEnd();
    //        strResult = strResult.Remove(strResult.Length - 1);
    //        strResult = strResult.Remove(0, 1);
    //        // close the stream object
    //        loResponseStream.Close();
    //        // close the response object
    //        webresponse.Close();
    //        // below steps remove unwanted data from response string
    //        // lblmsg.Text = strResult.Replace("</string>", "");
    //        string value = strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", "");
    //        //DataSet ds = JsonConvert.DeserializeObject<DataSet>(strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", ""));
    //        //return ds;
    //        //string value = JsonConvert.DeserializeObject<String>(strResult.Replace("\\r", "").Replace("\\n", "").Replace("\\", ""));
    //        //return value;
    //        return value;
    //    }
    //    catch (Exception e)
    //    {
    //        throw e;
    //    }
    //}
    //public class Response
    //{
    //    public string Code { get; set; }
    //    public string Message { get; set; }
    //    public string byteStrem { get; set; }
    //}
    //public class RootObject
    //{
    //    public List<Response> Response { get; set; }
    //}
    //public static DataTable ToDataTable<T>(List<T> items)
    //{
    //    DataTable dataTable = new DataTable(typeof(T).Name);

    //    //Get all the properties
    //    PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
    //    foreach (PropertyInfo prop in Props)
    //    {
    //        //Setting column names as Property names
    //        dataTable.Columns.Add(prop.Name);
    //    }
    //    foreach (T item in items)
    //    {
    //        var values = new object[Props.Length];
    //        for (int i = 0; i < Props.Length; i++)
    //        {
    //            //inserting property values to datatable rows
    //            values[i] = Props[i].GetValue(item, null);
    //        }
    //        dataTable.Rows.Add(values);
    //    }
    //    //put a breakpoint here and check datatable
    //    return dataTable;
    //}


}