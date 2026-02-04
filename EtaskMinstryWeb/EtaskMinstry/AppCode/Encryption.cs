using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;


public class Encryption
{
    // Security Helper to encrypt and decrypt text
    #region Encryption

    /// <summary>
    /// To Encrpt Text 
    /// </summary>
    /// <param name="toEncrypt"></param>
    /// <returns></returns>
    public static string Encrypt(string toEncrypt)
    {
        byte[] keyArray;
        byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

        System.Configuration.AppSettingsReader settingsReader =
                                            new AppSettingsReader();
        // Get the key from config file

        string key = "KEY";//(string)settingsReader.GetValue("SecurityKey", typeof(String));
        //System.Windows.Forms.MessageBox.Show(key);
        //If hashing use get hashcode regards to your key
        MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
        keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
        //Always release the resources and flush data
        // of the Cryptographic service provide. Best Practice

        hashmd5.Clear();


        TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
        //set the secret key for the tripleDES algorithm
        tdes.Key = keyArray;
        //mode of operation. there are other 4 modes.
        //We choose ECB(Electronic code Book)
        tdes.Mode = CipherMode.ECB;
        //padding mode(if any extra byte added)

        tdes.Padding = PaddingMode.PKCS7;

        ICryptoTransform cTransform = tdes.CreateEncryptor();
        //transform the specified region of bytes array to resultArray
        byte[] resultArray =
          cTransform.TransformFinalBlock(toEncryptArray, 0,
          toEncryptArray.Length);
        //Release resources held by TripleDes Encryptor
        tdes.Clear();
        //Return the encrypted data into unreadable string format
        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }

    /// <summary>
    /// To Decrpt Text
    /// </summary>
    /// <param name="cipherString"></param>
    /// <returns></returns>
    public static string Decrypt(string cipherString)
    {
        byte[] keyArray;
        //get the byte code of the string

        byte[] toEncryptArray = Convert.FromBase64String(cipherString);

        System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
        //Get your key from config file to open the lock!
        string key = "KEY"; //(string)settingsReader.GetValue("SecurityKey",  typeof(String));


        //if hashing was used get the hash code with regards to your key
        MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
        keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
        //release any resource held by the MD5CryptoServiceProvider
        hashmd5.Clear();


        TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
        //set the secret key for the tripleDES algorithm
        tdes.Key = keyArray;
        //mode of operation. there are other 4 modes. 
        //We choose ECB(Electronic code Book)

        tdes.Mode = CipherMode.ECB;
        //padding mode(if any extra byte added)
        tdes.Padding = PaddingMode.PKCS7;

        ICryptoTransform cTransform = tdes.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(
                             toEncryptArray, 0, toEncryptArray.Length);
        //Release resources held by TripleDes Encryptor                
        tdes.Clear();
        //return the Clear decrypted TEXT
        return UTF8Encoding.UTF8.GetString(resultArray);
    }

    #endregion

    #region Hashing

    #endregion
}

namespace HashAndSalt
{
    static class Utility
    {
        // utilty function to convert string to byte[]        
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(str);//new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        // utilty function to convert byte[] to string        
        public static string GetString(byte[] bytes)
        {
            //char[] chars = new char[bytes.Length / sizeof(char)];
            //System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return System.Text.Encoding.ASCII.GetString(bytes);//new string(chars);
        }
    }
    public static class SaltGenerator
    {
        private static RNGCryptoServiceProvider m_cryptoServiceProvider = null;
        private const int SALT_SIZE = 24;

        static SaltGenerator()
        {
            m_cryptoServiceProvider = new RNGCryptoServiceProvider();
        }

        public static string GetSaltString()
        {
            // Lets create a byte array to store the salt bytes
            byte[] saltBytes = new byte[SALT_SIZE];

            // lets generate the salt in the byte array
            m_cryptoServiceProvider.GetNonZeroBytes(saltBytes);

            // Let us get some string representation for this salt
            string saltString = Utility.GetString(saltBytes);

            // Now we have our salt string ready lets return it to the caller
            return saltString;
        }
    }
    public class PasswordManager
    {
        HashComputer m_hashComputer = new HashComputer();

        public string GeneratePasswordHash(string plainTextPassword, out string salt)
        {
            salt = SaltGenerator.GetSaltString();

            string finalString = plainTextPassword + salt;

            return m_hashComputer.GetPasswordHashAndSalt(finalString);
        }

        public bool IsPasswordMatch(string password, string salt, string hash)
        {
            string finalString = password + salt;
            return hash == m_hashComputer.GetPasswordHashAndSalt(finalString);
        }
    }
    public class HashComputer
    {
        public string GetPasswordHashAndSalt(string message)
        {
            // Let us use SHA256 algorithm to 
            // generate the hash from this salted password
            SHA256 sha = new SHA256CryptoServiceProvider();
            byte[] dataBytes = Utility.GetBytes(message);
            byte[] resultBytes = sha.ComputeHash(dataBytes);

            // return the hash string to the caller
            return Utility.GetString(resultBytes);
        }
    }
    public class TestUsage
    {
        // Let us use the Password manager class to generate the password ans salt
        static PasswordManager pwdManager = new PasswordManager();
        public static string SimulateUserCreation()
        {
            Console.WriteLine("Let us first test the password hash creation i.e. User creation");
            Console.WriteLine("Please enter user id");
            string userid = Console.ReadLine();

            Console.WriteLine("Please enter password");
            string password = Console.ReadLine();

            string salt = null;

            string passwordHash = pwdManager.GeneratePasswordHash(password, out salt);

            // Let us save the values in the database 
            //User user = new User
            //{
            //    UserId = userid,
            //    PasswordHash = passwordHash,
            //    Salt = salt
            //};    //create use object

            // Lets Add the User to the database
            //userRepo.AddUser(user); //add user to data base.

            return salt;
        }
        public static void SimulateLogin(string salt)
        {
            Console.WriteLine("Now let is simulate the password comparison");

            Console.WriteLine("Please enter user id");
            string userid = Console.ReadLine();

            Console.WriteLine("Please enter password");
            string password = Console.ReadLine();

            // Let us retrieve the values from the database
            //User user2 = userRepo.GetUser(userid); //get user object fro DB

            //bool result = pwdManager.IsPasswordMatch(password, user2.Salt, user2.PasswordHash); //check user authentication

            //if (result == true)
            //{
            //    Console.WriteLine("Password Matched");
            //}
            //else
            //{
            //    Console.WriteLine("Password not Matched");
            //}
        }
    }
}
