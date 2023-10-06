using Insc.Common.Cryptography;
using System.Configuration;
using System.DirectoryServices;
using System.Threading;

/// <summary>
/// Summary description for ConsumeLdap
/// </summary>
public class ConsumeLdap
{
    public static bool Authenticate(string UserID, string Password)
    {
        bool success = false;
        try
        {
            Password = DecryptMsg(Password);
            DirectoryEntry deADS = new DirectoryEntry("LDAP://" + ConfigurationManager.AppSettings["ADSDomain"].ToString(), UserID, Password);
            DirectorySearcher dsADS = new DirectorySearcher(deADS);
            dsADS.CacheResults = true;
            dsADS.Filter = "(&(objectCategory=User) (samAccountName=" + UserID + "))";
            SearchResult searchResult;

            try
            {
                searchResult = dsADS.FindOne();
            }

            catch (System.Runtime.InteropServices.COMException exc)
            {
                // throw exc;
                if ((exc.ErrorCode & 0xFFFF) == 0xC472)
                {   // Excel is busy
                    Thread.Sleep(500); // Wait, and...
                    success = false;  // ...try again
                }
                else
                {   // Re-throw!
                    throw exc;
                }
                deADS.Close();
                deADS.Dispose();
                success = false;
            }
            finally
            {
                deADS.Close();
                deADS.Dispose();
            }
            success = true;

        }
        catch (System.Runtime.InteropServices.COMException exc)
        {
            // throw exc;

            success = false;
        }
        return success;
    }

    public static string DecryptMsg(string sInputFileName)
    {
        
        cSymmetric.Provider symPro = cSymmetric.Provider.Rijndael;
        cSymmetric sym = new cSymmetric(symPro, true);
        cData key = new cData("mykey");
        cData enCryptedData = new cData();
        enCryptedData.Base64 = sInputFileName;
        cData enDeCryptedData = new cData();
        enDeCryptedData = sym.Decrypt(enCryptedData, key);
        return enDeCryptedData.cToString();
    }
}