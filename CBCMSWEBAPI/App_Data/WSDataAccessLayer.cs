using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System;
using System.Configuration;

namespace CBCMSWEBAPI
{
    /// <summary>
    /// A taliored sub fork of Original DataAccess layer from ISys Suite.
    /// This Class handles all the inside database clockwork necessary for this service.
    /// </summary>
    ///
    public class WSDataAccessLayer
    {
        public WSDataAccessLayer()
        {
            //Nothing Declared
        }

        private SqlConnection objCon;
        SqlDataAdapter objAdp;
        private SqlCommand objCom;
        private string constrCHMS = System.Configuration.ConfigurationManager.ConnectionStrings["IsysCHMS"].ToString();
        private string constrARTL = System.Configuration.ConfigurationManager.ConnectionStrings["IsysARTL"].ToString();

        public class Collections
        {
            string Name;
            int size;
            object type;
            public Collections()
            {
                //Nothing Declared
            }
            public Collections(string Name, int size, object type)
            {
                this.Name = Name;
                this.size = size;
                this.type = type;
            }
            public string GetName()
            {
                return Name;
            }
            public int GetSize()
            {
                return size;
            }
            public new object GetType()
            {
                return type;
            }
        }

        /// <summary>
        ///  Gets all Unapproved candidates for the specific SAPCode.
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <param name="param3"></param>
        /// <param name="param4"></param>
        /// <param name="param5"></param>
        /// <param name="connstr"></param>
        /// <returns></returns>
        public DataSet selectFromDatabase(string strSql, string param1, string param2, string param3, string param4, string connstr)
        {
            Hashtable htable = new Hashtable();
            htable.Clear();
            htable.Add("@CarrierCode", "2");
            htable.Add("@CndNo", String.Empty);
            htable.Add("@Appno", param2);
            htable.Add("@GivenName", param3);
            htable.Add("@Surname", String.Empty);
            htable.Add("@PAN", param4);
            htable.Add("@CreateFrmDtim", String.Empty);
            htable.Add("@CreateToDtim", String.Empty);
            htable.Add("@LangCode", String.Empty);
            htable.Add("@UserType", String.Empty);
            htable.Add("@UserAuthCode", String.Empty);
            htable.Add("@Page", "CndApr");
            htable.Add("@MemberCode", param1);
            htable.Add("@MemberType", "BH");

            DataSet dsResult = new DataSet();
            SqlCommand cmdObj = new SqlCommand();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[connstr.ToString().Trim()].ToString());
            cmdObj.CommandText = strSql;
            cmdObj.CommandType = CommandType.StoredProcedure;
            cmdObj.Connection = conn;
            try
            {
                objAdp = new SqlDataAdapter(cmdObj);
                foreach (DictionaryEntry dist in htable)
                {
                    objAdp.SelectCommand.Parameters.AddWithValue((string)dist.Key, dist.Value);
                }
                objAdp.Fill(dsResult);
            }
            catch
            {
                throw;
            }
            return dsResult;
        }

        public DataSet selectFromDatabase(string strSql, string param1, string param2, string param3, string param4, string param5, string connstr)
        {
            Hashtable htable = new Hashtable();
            htable.Clear();
            htable.Add("@CarrierCode", "2");
            htable.Add("@CndNo", String.Empty);
            htable.Add("@Appno", param2);
            htable.Add("@GivenName", param3);
            htable.Add("@Surname", String.Empty);
            htable.Add("@PAN", param4);
            htable.Add("@CreateFrmDtim", String.Empty);
            htable.Add("@CreateToDtim", String.Empty);
            htable.Add("@LangCode", String.Empty);
            htable.Add("@UserType", String.Empty);
            htable.Add("@UserAuthCode", String.Empty);
            htable.Add("@Page", param5);
            htable.Add("@MemberCode", param1);
            htable.Add("@MemberType", "sm");

            DataSet dsResult = new DataSet();
            SqlCommand cmdObj = new SqlCommand();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[connstr.ToString().Trim()].ToString());
            cmdObj.CommandText = strSql;
            cmdObj.CommandType = CommandType.StoredProcedure;
            cmdObj.Connection = conn;
            try
            {
                objAdp = new SqlDataAdapter(cmdObj);
                foreach (DictionaryEntry dist in htable)
                {
                    objAdp.SelectCommand.Parameters.AddWithValue((string)dist.Key, dist.Value);
                }
                objAdp.Fill(dsResult);
            }
            catch
            {
                throw;
            }
            return dsResult;
        }

        public DataSet GetDataForAppSearch(string strSql, string param1, string param2, string param3, string param4, string param5, string connstr)
        {
            Hashtable htable = new Hashtable();
            htable.Clear();
            htable.Add("@BMCode", param1);
            htable.Add("@AppNo", param2);
            htable.Add("@CndName", param3);
            htable.Add("@PanNo", param4);
            htable.Add("@Flag", param5);

            DataSet dsResult = new DataSet();
            SqlCommand cmdObj = new SqlCommand();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[connstr.ToString().Trim()].ToString());
            cmdObj.CommandText = strSql;
            cmdObj.CommandType = CommandType.StoredProcedure;
            cmdObj.Connection = conn;
            try
            {
                objAdp = new SqlDataAdapter(cmdObj);
                foreach (DictionaryEntry dist in htable)
                {
                    objAdp.SelectCommand.Parameters.AddWithValue((string)dist.Key, dist.Value);
                }
                objAdp.Fill(dsResult);
            }
            catch (Exception ex)
            {
                return dsResult;
            }
            return dsResult;
        }

        //

        public DataSet GetRemainderData(string strSql, string param1, string connstr)
        {
            Hashtable htable = new Hashtable();
            htable.Clear();
            htable.Add("@CreateBy", param1);

            DataSet dsResult = new DataSet();
            SqlCommand cmdObj = new SqlCommand();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[connstr.ToString().Trim()].ToString());
            cmdObj.CommandText = strSql;
            cmdObj.CommandType = CommandType.StoredProcedure;
            cmdObj.Connection = conn;
            try
            {
                objAdp = new SqlDataAdapter(cmdObj);
                foreach (DictionaryEntry dist in htable)
                {
                    objAdp.SelectCommand.Parameters.AddWithValue((string)dist.Key, dist.Value);
                }
                objAdp.Fill(dsResult);
            }
            catch (Exception ex)
            {
                return dsResult;
            }
            return dsResult;
        }

        public DataSet SetRemainderData(string strSql, string TodayDate, string RemainderDate, string SystemTime, string RemainderTime, string Remainder, string connstr)

        {
            Hashtable htable = new Hashtable();
            htable.Clear();

            htable.Add("@TodayDate", TodayDate);
            htable.Add("@RemainderDate", RemainderDate);
            htable.Add("@SystemTime", SystemTime);
            htable.Add("@RemainderTime", RemainderTime);
            htable.Add("@Remainder", Remainder);

            DataSet dsResult = new DataSet();
            SqlCommand cmdObj = new SqlCommand();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[connstr.ToString().Trim()].ToString());
            cmdObj.CommandText = strSql;
            cmdObj.CommandType = CommandType.StoredProcedure;
            cmdObj.Connection = conn;
            try
            {
                objAdp = new SqlDataAdapter(cmdObj);
                foreach (DictionaryEntry dist in htable)
                {
                    objAdp.SelectCommand.Parameters.AddWithValue((string)dist.Key, dist.Value);
                }
                objAdp.Fill(dsResult);
            }
            catch (Exception ex)
            {

                return dsResult;
            }
            return dsResult;
        }




        /// <summary>
        ///     Updates the Approval Flag of the provided Candidate Number.
        /// </summary>
        /// <param name="dsArg"></param>
        /// <param name="connstr"></param>
        /// <param name="sprc_name"></param>
        /// <returns></returns>
        public int updateToDataBase(DataSet dsArg, string param1, string connstr, string sprc_name)
        {
            try
            {
                SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(objAdp);
                objCon = new SqlConnection(ConfigurationManager.ConnectionStrings[connstr.ToString().Trim()].ToString());
                objCom = new SqlCommand(sprc_name, objCon);
                objCom.CommandType = CommandType.StoredProcedure;

                Hashtable htable = new Hashtable();
                htable.Add("@CarrierCode", "2");
                htable.Add("@CndNo", param1);
                htable.Add("@CndStatus", "50");
                htable.Add("@Remarks", "Approved");
                htable.Add("@UpdateBy", "");
                htable.Add("@Stage", "");
                htable.Add("@CndAppRej_Flag", "0");
                htable.Add("@Reason", "Approved");

                foreach (DictionaryEntry dist in htable)
                {
                    objCom.Parameters.AddWithValue((string)dist.Key, dist.Value);
                }
                objCon.Open();
                return objCom.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
            finally
            {
                objCon.Close();
            }
        }

        /// <summary>
        ///     Returns a Password matched with SAPCode.
        /// </summary>
        /// <param name="SAPCode"></param>
        /// <param name="strSql"></param>
        /// <param name="connstr"></param>
        /// <returns></returns>
        public DataSet queryIMEI(String SAPCode, String strSql, String connstr)
        {
            Hashtable htable = new Hashtable();
            htable.Clear();
            htable.Add("@SAPCode", SAPCode);

            DataSet dsResult = new DataSet();
            SqlCommand cmdObj = new SqlCommand();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[connstr.ToString().Trim()].ToString());
            cmdObj.CommandText = strSql;
            cmdObj.CommandType = CommandType.StoredProcedure;
            cmdObj.Connection = conn;
            try
            {
                objAdp = new SqlDataAdapter(cmdObj);
                foreach (DictionaryEntry dist in htable)
                {
                    objAdp.SelectCommand.Parameters.AddWithValue((string)dist.Key, dist.Value);
                }
                objAdp.Fill(dsResult);
            }
            catch
            {
                throw;
            }
            return dsResult;
        }

        /// <summary>
        /// Method to return row in dataset containg details of provided candidate number
        /// </summary>
        /// <param name="CandidateNumber"></param>
        /// <param name="strSql"></param>
        /// <param name="connstr"></param>
        /// <returns></returns>
        public DataSet fetchDetails(String CandidateNumber, String strSql, String connstr)
        {
            Hashtable htable = new Hashtable();
            htable.Clear();
            htable.Add("@CndNo", CandidateNumber);

            DataSet dsResult = new DataSet();
            SqlCommand cmdObj = new SqlCommand();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[connstr.ToString().Trim()].ToString());
            cmdObj.CommandText = strSql;
            cmdObj.CommandType = CommandType.StoredProcedure;
            cmdObj.Connection = conn;
            try
            {
                objAdp = new SqlDataAdapter(cmdObj);
                foreach (DictionaryEntry dist in htable)
                {
                    objAdp.SelectCommand.Parameters.AddWithValue((string)dist.Key, dist.Value);
                }
                objAdp.Fill(dsResult);
            }
            catch
            {
                throw;
            }
            return dsResult;
        }

        public DataSet selectFromDatabase(string strSql, Hashtable htable, string connstr)
        {
            //Hashtable htable = new Hashtable();
            //htable.Clear();
            DataSet dsResult = new DataSet();
            SqlCommand cmdObj = new SqlCommand();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[connstr.ToString().Trim()].ToString());
            cmdObj.CommandText = strSql;
            cmdObj.CommandType = CommandType.StoredProcedure;
            cmdObj.Connection = conn;
            try
            {
                objAdp = new SqlDataAdapter(cmdObj);
                foreach (DictionaryEntry dist in htable)
                {
                    objAdp.SelectCommand.Parameters.AddWithValue((string)dist.Key, dist.Value);
                }
                objAdp.Fill(dsResult);
            }
            catch
            {
                throw;
            }
            return dsResult;
        }


        public DataSet selectFromDatabase_singleValue(string strSql, string param1, string connstr)
        {
            Hashtable htable = new Hashtable();
            htable.Clear();
            htable.Add("@SAPCode", param1);

            DataSet dsResult = new DataSet();
            SqlCommand cmdObj = new SqlCommand();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[connstr.ToString().Trim()].ToString());
            cmdObj.CommandText = strSql;
            cmdObj.CommandType = CommandType.StoredProcedure;
            cmdObj.Connection = conn;
            try
            {
                objAdp = new SqlDataAdapter(cmdObj);
                foreach (DictionaryEntry dist in htable)
                {
                    objAdp.SelectCommand.Parameters.AddWithValue((string)dist.Key, dist.Value);
                }
                objAdp.Fill(dsResult);
            }
            catch
            {
                throw;
            }
            return dsResult;
        }

        public DataSet GetDataSetForPrcDBConn(string strSql, Hashtable htable, string strDBKey)
        {
            DataSet dsResult = new DataSet();
            SqlCommand cmdObj = new SqlCommand();
            cmdObj.CommandTimeout = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["CommandTimeOut"].ToString());
            SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[strDBKey].ToString());
            cmdObj.CommandText = strSql;
            cmdObj.CommandType = CommandType.StoredProcedure;
            cmdObj.Connection = conn;
            try
            {
                objAdp = new SqlDataAdapter(cmdObj);
                objAdp.SelectCommand.CommandTimeout = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["CommandTimeOut"].ToString());
                foreach (DictionaryEntry dist in htable)
                {
                    objAdp.SelectCommand.Parameters.AddWithValue((string)dist.Key, dist.Value);
                }
                objAdp.Fill(dsResult);

            }
            catch
            {
                throw;
            }
            return dsResult;
        }

    }
}