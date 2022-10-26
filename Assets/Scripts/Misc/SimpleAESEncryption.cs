using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public static class SimpleAESEncryption
{
    /// summary
    /// A class containing AES-encrypted text, plus the IV value required to decrypt it (with the correct password)
    /// /summary
    public struct AESEncryptedText
    {
        public string IV;
        public string EncryptedText;
    }

    /// summary
    /// Encrypts a given text string with a password
    /// /summary
    /// param name="plainText"The text to encrypt/param
    /// param name="password"The password which will be required to decrypt it/param
    /// returnsAn AESEncryptedText object containing the encrypted string and the IV value required to decrypt it./returns
    public static AESEncryptedText Encrypt(string plainText, string password)
    {
        using (var aes = Aes.Create())
        {
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Key = ConvertToKeyBytes(aes, password);

            var textBytes = Encoding.UTF8.GetBytes(plainText);

            var aesEncryptor = aes.CreateEncryptor();
            var encryptedBytes = aesEncryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);

            return new AESEncryptedText
            {
                IV = Convert.ToBase64String(aes.IV),
                EncryptedText = Convert.ToBase64String(encryptedBytes)
            };
        }
    }

    public static string Encrypt2(string plainText, string keyString)
    {
        byte[] cipherData;
        Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(keyString);
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        ICryptoTransform cipher = aes.CreateEncryptor(aes.Key, aes.IV);

        using (MemoryStream ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms, cipher, CryptoStreamMode.Write))
            {
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
            }

            cipherData = ms.ToArray();
        }

        byte[] combinedData = new byte[aes.IV.Length + cipherData.Length];
        Array.Copy(aes.IV, 0, combinedData, 0, aes.IV.Length);
        Array.Copy(cipherData, 0, combinedData, aes.IV.Length, cipherData.Length);
        string resultString = "";
        string part1 = "";
        string part2 = "";
        for (int i = 0; i < combinedData.Length; i++)
        {
            resultString += combinedData[i].ToString("x2");
        }

        for (int i = 0; i < aes.IV.Length; i++)
        {
            part1 += aes.IV[i].ToString("x2");
        }

        for (int i = 0; i < cipherData.Length; i++)
        {
            part2 += cipherData[i].ToString("x2");
        }

        return part1 + " :: " + part2 + " :: " + resultString; //Convert.ToBase64String(combinedData);
    }

    /// summary
    /// Decrypts an AESEncryptedText with a password
    /// /summary
    /// param name="encryptedText"The AESEncryptedText object to decrypt/param
    /// param name="password"The password to use when decrypting/param
    /// returnsThe original plainText string./returns
    public static string Decrypt(AESEncryptedText encryptedText, string password)
    {
        return Decrypt(encryptedText.EncryptedText, encryptedText.IV, password);
    }

    /// summary
    /// Decrypts an encrypted string with an IV value password
    /// /summary
    /// param name="encryptedText"The encrypted string to be decrypted/param
    /// param name="iv"The IV value which was generated when the text was encrypted/param
    /// param name="password"The password to use when decrypting/param
    /// returnsThe original plainText string./returns
    public static string Decrypt(string encryptedText, string iv, string password)
    {
        using (Aes aes = Aes.Create())
        {
            var ivBytes = Convert.FromBase64String(iv);
            var encryptedTextBytes = Convert.FromBase64String(encryptedText);

            var decryptor = aes.CreateDecryptor(ConvertToKeyBytes(aes, password), ivBytes);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedTextBytes, 0, encryptedTextBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }

    // Ensure the AES key byte-array is the right size - AES will reject it otherwise
    private static byte[] ConvertToKeyBytes(SymmetricAlgorithm algorithm, string password)
    {
        algorithm.GenerateKey();

        var keyBytes = Encoding.UTF8.GetBytes(password);
        var validKeySize = algorithm.Key.Length;

        if (keyBytes.Length != validKeySize)
        {
            var newKeyBytes = new byte[validKeySize];
            Array.Copy(keyBytes, newKeyBytes, Math.Min(keyBytes.Length, newKeyBytes.Length));
            keyBytes = newKeyBytes;
        }

        return keyBytes;
    }

    public static string Decrypt2(string combinedString, string keyString)
    {
        string plainText;
        byte[] combinedData = Convert.FromBase64String(combinedString);
        Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(keyString);
        byte[] iv = new byte[aes.BlockSize / 8];
        byte[] cipherText = new byte[combinedData.Length - iv.Length];
        Array.Copy(combinedData, iv, iv.Length);
        Array.Copy(combinedData, iv.Length, cipherText, 0, cipherText.Length);
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        ICryptoTransform decipher = aes.CreateDecryptor(aes.Key, aes.IV);

        using (MemoryStream ms = new MemoryStream(cipherText))
        {
            using (CryptoStream cs = new CryptoStream(ms, decipher, CryptoStreamMode.Read))
            {
                using (StreamReader sr = new StreamReader(cs))
                {
                    plainText = sr.ReadToEnd();
                }
            }

            return plainText;
        }
    }
}