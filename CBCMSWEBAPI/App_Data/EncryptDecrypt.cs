using System;
using System.Security.Cryptography;
using System.Text;

namespace CBCMSWEBAPI
{
    public class EncryptDecrypt
    {
        String SecurityKey = "KmI?rgIC@agenroll+2023!$";
        String api_key = "AIzaSyCHXrNBIDvtgIa5CN48b3INHI_N6eNtFJI";
        String sharedkey = "rgicl@123";
        String Padding = "AES/CBC/PKCS5Padding";
        //String SecurityIV = "Rgicl#1234567890";
        //String SecuritySALT = "Rgicl#1234567890";

        public EncryptDecrypt()
        {

        }

        public String GetKeys()
        {
            String server = "";
            String akey = "";
            String skey = "";
            String pad = "";
            String DBName = "";
            
            server = Convert.ToBase64String(Encoding.UTF8.GetBytes(SecurityKey));
            akey = Convert.ToBase64String(Encoding.UTF8.GetBytes(api_key));
            skey = Convert.ToBase64String(Encoding.UTF8.GetBytes(sharedkey));
            pad = Convert.ToBase64String(Encoding.UTF8.GetBytes(Padding));
            DBName = Convert.ToBase64String(Encoding.UTF8.GetBytes("CBCMS.db"));

            return "{\"Table\": [ {\"ResponseCode\": \"0\",\"server\":\"" + server + "\",\"pad\":\"" + pad + "\",\"api_key\":\"" + akey + "\",\"shared_key\":\"" + skey + "\",\"DBName\":\"" + DBName + "\"}]}";
        }


        public string EncryptAndEncode(string raw)
        {
            try
            {
                using (var csp = new AesCryptoServiceProvider())
                {
                    ICryptoTransform e = GetCryptoTransform(csp, true);
                    byte[] inputBuffer = Encoding.UTF8.GetBytes(raw);
                    byte[] output = e.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
                    string encrypted = Convert.ToBase64String(output);
                    return encrypted;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string DecodeAndDecrypt(string encrypted)
        {
            try
            {
                using (var csp = new AesCryptoServiceProvider())
                {
                    var d = GetCryptoTransform(csp, false);
                    byte[] output = Convert.FromBase64String(encrypted);
                    byte[] decryptedOutput = d.TransformFinalBlock(output, 0, output.Length);
                    string decypted = Encoding.UTF8.GetString(decryptedOutput);
                    return decypted;
                }
            }
            catch (Exception e)
            {
                return "";
            }
        }

        private ICryptoTransform GetCryptoTransform(AesCryptoServiceProvider csp, bool encrypting)
        {
            byte[] iv = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            csp.KeySize = 256;
            csp.BlockSize = 128;
            csp.Mode = CipherMode.CBC;
            csp.Padding = PaddingMode.PKCS7;

            //var spec = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(SecurityKey), Encoding.UTF8.GetBytes(SecuritySALT), 65536);
            //byte[] key = spec.GetBytes(16);

            byte[] keyBytes;
            using (SHA256 hash = SHA256.Create())
            {
                keyBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(SecurityKey));
            }


            //csp.IV = Encoding.UTF8.GetBytes(SecurityIV);
            csp.IV = iv;
            csp.Key = keyBytes;
            if (encrypting)
            {
                return csp.CreateEncryptor();
            }
            else
            {
                return csp.CreateDecryptor();
            }
        }

    }
}
