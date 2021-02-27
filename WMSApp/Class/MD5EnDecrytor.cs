using System;
using System.Security.Cryptography;
using System.Text;
namespace WMSApp.Class
{
    public class MD5EnDecrytor //: IDisposable
    {
        public string lastErrMsg { get; set; } ="";
        public void Dispose() { }// => GC.Collect();

        public string Encrypt(string toEncrypt, bool useHashing, string SecurityKey)
        {
            string result = string.Empty;
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
                string key = SecurityKey;
                //If hashing use get hashcode regards to your key
                if (useHashing)
                {
                    var hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    hashmd5.Clear();
                }
                else
                    keyArray = UTF8Encoding.UTF8.GetBytes(key);

                var tdes = new TripleDESCryptoServiceProvider();
                //set the secret key for the tripleDES algorithm
                tdes.Key = keyArray;
                //mode of operation. there are other 4 modes. We choose ECB(Electronic code Book)
                tdes.Mode = CipherMode.ECB;
                //padding mode(if any extra byte added)
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateEncryptor();
                //transform the specified region of bytes array to resultArray
                byte[] resultArray = cTransform.TransformFinalBlock
                        (toEncryptArray, 0, toEncryptArray.Length);
                //Release resources held by TripleDes Encryptor
                tdes.Clear();
                //Return the encrypted data into unreadable string format
                result = Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception excep)
            {
                lastErrMsg = excep.ToString();
            }
            return result;
        }

        public string Decrypt(string cipherString, bool useHashing, string SecurityKey)
        {
            string result = string.Empty;
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = Convert.FromBase64String(cipherString);
                string key = SecurityKey;

                if (useHashing)
                {
                    //if hashing was used get the hash code with regards to your key
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    hashmd5.Clear();
                }
                else
                {
                    //if hashing was not implemented get the byte code of the key
                    keyArray = UTF8Encoding.UTF8.GetBytes(key);
                }

                var tdes = new TripleDESCryptoServiceProvider();
                //set the secret key for the tripleDES algorithm
                tdes.Key = keyArray;
                //mode of operation. there are other 4 modes.
                tdes.Mode = CipherMode.ECB;
                //padding mode(if any extra byte added)
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock
                        (toEncryptArray, 0, toEncryptArray.Length);
                //Release resources held by TripleDes Encryptor
                tdes.Clear();
                //return the Clear decrypted TEXT
                result = UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception excep)
            {
                lastErrMsg = excep.ToString();
            }
            return result;
        }
    }
}
