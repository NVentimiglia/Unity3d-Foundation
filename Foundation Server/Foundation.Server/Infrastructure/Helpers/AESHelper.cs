using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Foundation.Server.Infrastructure.Helpers
{
    /// <summary>
    /// A simple wrapper to the AesManaged class and the AES algorithm.
    /// Requires a securely stored key which should be a random string of characters that an attacker could never guess.
    /// Make sure to save the Key if you want to decrypt your data later!
    /// If you're using this with a Web app, put the key in the web.config and encrypt the web.config.
    /// </summary>
    public class AESHelper
    {
        protected static readonly int SaltSize = 32;
        //private const string characterSet = "abcdefghijklmnopqrstuvwxyzABCDDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()_+=-";
        private const string characterSet = "abcdefghijklmnopqrstuvwxyzABCDDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        
        /// <summary>
        /// Encrypts the plainText input using the given Key.
        /// A 128 bit random salt will be generated and prepended to the ciphertext before it is base64 encoded.
        /// </summary>
        /// <param name="plainText">The plain text to encrypt.</param>
        /// <param name="key">The plain text encryption key.</param>
        /// <returns>The salt and the ciphertext, Base64 encoded for convenience.</returns>
        public static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            // Derive a new Salt and IV from the Key
            var keyDerivationFunction = new Rfc2898DeriveBytes(key, SaltSize);

            // note Get Bytes uses 90% of the time TODO
            var saltBytes = keyDerivationFunction.Salt;
            var keyBytes = keyDerivationFunction.GetBytes(32);
            var ivBytes = keyDerivationFunction.GetBytes(16);

            // Create an encryptor to perform the stream transform.
            // Create the streams used for encryption.
            using (var aesManaged = new AesManaged())
            using (var encryptor = aesManaged.CreateEncryptor(keyBytes, ivBytes))
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                using (var streamWriter = new StreamWriter(cryptoStream))
                {
                    // Send the data through the StreamWriter, through the CryptoStream, to the underlying MemoryStream
                    streamWriter.Write(plainText);
                }

                // Return the encrypted bytes from the memory stream, in Base64 form so we can send it right to a database (if we want).
                var cipherTextBytes = memoryStream.ToArray();
                Array.Resize(ref saltBytes, saltBytes.Length + cipherTextBytes.Length);
                Array.Copy(cipherTextBytes, 0, saltBytes, SaltSize, cipherTextBytes.Length);

                return Convert.ToBase64String(saltBytes);
            }
        }

        /// <summary>
        /// Decrypts the ciphertext using the Key.
        /// </summary>
        /// <param name="ciphertext">The ciphertext to decrypt.</param>
        /// <param name="key">The plain text encryption key.</param>
        /// <returns>The decrypted text.</returns>
        public static string Decrypt(string ciphertext, string key)
        {
            if (string.IsNullOrEmpty(ciphertext))
                throw new ArgumentNullException("ciphertext");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            // Extract the salt from our ciphertext
            var allTheBytes = Convert.FromBase64String(ciphertext);
            var saltBytes = allTheBytes.Take(SaltSize).ToArray();
            var ciphertextBytes = allTheBytes.Skip(SaltSize).Take(allTheBytes.Length - SaltSize).ToArray();

            var keyDerivationFunction = new Rfc2898DeriveBytes(key, saltBytes);

            // note Get Bytes uses 90% of the time TODO
            // Derive the previous IV from the Key and Salt
            var keyBytes = keyDerivationFunction.GetBytes(32);
            var ivBytes = keyDerivationFunction.GetBytes(16);

            // Create a decrytor to perform the stream transform.
            // Create the streams used for decryption.
            // The default Cipher Mode is CBC and the Padding is PKCS7 which are both good
            using (var aesManaged = new AesManaged())
            using (var decryptor = aesManaged.CreateDecryptor(keyBytes, ivBytes))
            using (var memoryStream = new MemoryStream(ciphertextBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            using (var streamReader = new StreamReader(cryptoStream))
            {
                // Return the decrypted bytes from the decrypting stream.
                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// random string builder
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetPassword(int length = 44)
        {
            var Random = new Random();

            var builder = new StringBuilder();
            while (builder.Length < length)
            {
                builder.Append(characterSet[Random.Next(characterSet.Length)]);
            }
            return builder.ToString();
        }


    }
}