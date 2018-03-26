using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Utilities
{
    public class CryptUtils
    {
        const string CONPASSKEY = "ExpenseTracker";

        public static string GetMD5Encryption(string message)
        {
            byte[] data = new byte[15];

            MD5 md5 = new MD5CryptoServiceProvider();

            data = Encoding.ASCII.GetBytes(message);

            byte[] result = md5.ComputeHash(data);

            return Encoding.ASCII.GetString(result);
        }


        //Use MD5 Hash to ensure the password is non-reversible
        public static string GetPasswordEncrypted(string password)
        {
            return CryptUtils.GetMD5Encryption(password);
        }


        public static String EncryptPassword(string strToEncrypt)
        {
            TripleDESCryptoServiceProvider tripledes = new TripleDESCryptoServiceProvider();
            tripledes.IV = new byte[8];
            PasswordDeriveBytes pwdbytes = new PasswordDeriveBytes(CONPASSKEY, new byte[0]);
            tripledes.Key = pwdbytes.CryptDeriveKey("RC2", "MD5", 128, new byte[8]);
            using (MemoryStream mstrm = new MemoryStream(strToEncrypt.Length * 2))
            {
                using (CryptoStream crypStream = new CryptoStream(mstrm, tripledes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] uncodedbytes = Encoding.UTF8.GetBytes(strToEncrypt);
                    crypStream.Write(uncodedbytes, 0, uncodedbytes.Length);
                    crypStream.FlushFinalBlock();
                    byte[] codedbytes = new byte[mstrm.Length];
                    mstrm.Position = 0;
                    mstrm.Read(codedbytes, 0, (int)mstrm.Length);
                    crypStream.Close();
                    return Convert.ToBase64String(codedbytes);
                }
            }
        }

        public static String DecryptPassword(string strToDecrypt)
        {
            TripleDESCryptoServiceProvider tripledes = new TripleDESCryptoServiceProvider();
            tripledes.IV = new byte[8];
            PasswordDeriveBytes pwdbytes = new PasswordDeriveBytes(CONPASSKEY, new byte[0]);
            tripledes.Key = pwdbytes.CryptDeriveKey("RC2", "MD5", 128, new byte[8]);
            byte[] encryptedBytes = Convert.FromBase64String(strToDecrypt);
            using (MemoryStream mstrm = new MemoryStream(strToDecrypt.Length))
            {
                using (CryptoStream crypStream = new CryptoStream(mstrm, tripledes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    crypStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                    crypStream.FlushFinalBlock();
                    byte[] uncodedbytes = new byte[mstrm.Length];
                    mstrm.Position = 0;
                    mstrm.Read(uncodedbytes, 0, (int)mstrm.Length);
                    crypStream.Close();
                    return Encoding.UTF8.GetString(uncodedbytes);
                }
            }
        }

        public static string GetHash(string movingFact, string secret)
        {
            var input = movingFact;
            var keybytes = Encoding.Default.GetBytes(secret);
            var inputBytes = Encoding.Default.GetBytes(input);

            var crypto = new HMACMD5(keybytes);
            var hash = crypto.ComputeHash(inputBytes);

            return hash.Select(b => b.ToString("x2"))
                       .Aggregate(new StringBuilder(),
                                  (current, next) => current.Append(next),
                                  current => current.ToString());
        }

        public static string GetHMACHash(string message, string secretKey)
        {

            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(message);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
            {
                string hexValue = b.ToString("X").ToLower(); // Lowercase for compatibility on case-sensitive systems
                sb.Append((hexValue.Length == 1 ? "0" : "") + hexValue);
            }
            return sb.ToString();
        }
    }
}
