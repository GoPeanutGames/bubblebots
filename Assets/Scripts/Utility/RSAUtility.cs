using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BubbleBots.Server;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using UnityEditor;
using UnityEngine;

public class RSAUtility
{
    public static string[] testKey =
    {
        "-----BEGIN RSA PUBLIC KEY-----",
        "MIIBCgKCAQEA15Pmyp5Z4u/kKUvRdeiajT2IQIusKAWqn131ZWkzQQbQzHs8Sls4",
        "7UkiLglQAGUDnkhlKuMelKnGAVwyT/rA7ikcz4d1YrkWaB2940kzcN4q4sZnvNjL",
        "q4Pno/9wDGCURj4jMRChP5aswCoyRLdhdZykyOFDXzpMTD8i7ltxO4LuxBqSrqvL",
        "cCOCs4Q59H8UfRE3cmBxxzFjSVXr9Weor5lxMJ4yjDCPNQ0HEOnh8y3wUbv6aJPe",
        "Wk8N65jBy9XVT6sTCUPKq0E8OW1utjPweMjt1fFKTt24yIXT5eMBslfd4OGamYAP",
        "ubdkkuF348HsabuRZs3FdC93cJErpbFETwIDAQAB",
        "-----END RSA PUBLIC KEY-----",
    };

    private static List<string> SplitDataIntoList(string data)
    {
        List<string> split = new();
        while (data.Length > 200)
        {
            split.Add(data.Substring(0, 200));
            data = data.Remove(0, 200);
        }

        split.Add(data);
        return split;
    }

    public static string Decrypt(List<string> encryptedData, string publicKey = "")
    {
        publicKey = string.IsNullOrEmpty(publicKey) ? EnvironmentManager.Instance.GetCurrentPublicKey() : publicKey;

        List<string> decryptedData = new();
        foreach (string part in encryptedData)
        {
            byte[] bytesToDecrypt = Convert.FromBase64String(part);
            Pkcs1Encoding engine = new Pkcs1Encoding(new RsaEngine());
            using (StringReader txtReader = new StringReader(publicKey))
            {
                AsymmetricKeyParameter keyParam = (AsymmetricKeyParameter)new PemReader(txtReader).ReadObject();
                engine.Init(false, keyParam);
            }

            string decrypted = Encoding.UTF8.GetString(engine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));
            decryptedData.Add(decrypted);
        }

        return string.Join("", decryptedData);
    }

    public static List<string> Encrypt(string data, string publicKey = "")
    {
        publicKey = string.IsNullOrEmpty(publicKey) ? EnvironmentManager.Instance.GetCurrentPublicKey() : publicKey;

        List<string> encryptedData = new();
        List<string> spltData = SplitDataIntoList(data);
        foreach (string part in spltData)
        {
            byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(part);
            Pkcs1Encoding engine = new Pkcs1Encoding(new RsaEngine());
            using (StringReader txtReader = new StringReader(publicKey))
            {
                AsymmetricKeyParameter keyParam = (AsymmetricKeyParameter)new PemReader(txtReader).ReadObject();
                engine.Init(true, keyParam);
            }

            string encrypted = Convert.ToBase64String(engine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
            encryptedData.Add(encrypted);
        }

        return encryptedData;
    }

#if UNITY_EDITOR
    [MenuItem("Peanut Games/Rsa encrypt test")]
    public static void EncryptTest()
    {
        string key = string.Join("\n", testKey);
        string dataToEncrypt =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras vulputate libero ac libero ultricies dictum. Fusce elementum aliquet neque vel suscipit. Nulla scelerisque et libero ut hendrerit. Pellentesque et molestie ex. Integer libero ligula, efficitur in viverra et, pulvinar tincidunt augue. Suspendisse porta magna nec nibh semper convallis. Cras commodo lectus et nibh dapibus scelerisque.";
        List<string> encrypted = Encrypt(dataToEncrypt, key);
        EncryptedData d = new()
        {
            data = encrypted
        };
        Debug.Log(JsonUtility.ToJson(d));
    }

    [MenuItem("Peanut Games/Rsa decrypt test")]
    public static void DecryptTest()
    {
        string key = string.Join("\n", testKey);
        List<string> encrypted = new()
        {
            "cusJuCEfr8+v7iWW3pkybxzVSf1uBz0cd4oE5uliRBazaKW7ALGmSwvP/izDOEdXTTzHqU3fAd/MuiEioflL0ccbZ2KLw/60xy6i/QcL/F5jn5DzOyHUiEDmx2MKmwrSAlzs25rqx6oZC53C+9BCHPQh4L0dQ4wJ/GDChClxzeHhHqWMhi5UcKW/l5HLSHBqsXsvABUL4AbY+2ykI6dmPJvX+wb1CiD0R0LeNaGiEF1Dy8WUVLpLsRB/hwEbJqFNn7vJXr6rQYXuP0NRkTWXTzQ32cGvJ4l5vMpxBsQZHRM7BzObSlXqQogLuvEmrl01GldU6wptGEGnCVYzbhQmUQ==",
            "sWMw3k5rokoc5MiPP1KrC7Q7dZJi0XIssXSpSM16UNHBApce5G+usgSLgn3TlSPdQw5v1nAaa0OPoYIhra5rDvWWc+mfIwDAp8s6zSi5vkbZG+tGYrB37RZMm4qb4Ebls0k4iadnGgriM68/0jCEQTX14raoBTHSQE90AlMB5IpjzE6b4+RaWrKqIfoZjIxp4mr/5Xt4DymRNQ7mAm/EFEkRP2ubyI+x6BxMM15Nwf6rU33q0ZMAe1RNUP8vwTUETBH/EOwR4bzKb3z3u/E/HsMWiwLZFIEsCucrziKDVyosvP+PuMedVQGuMOKx04nu8U3n+HG+o/iqnyePBNmMMQ=="
        };
        string decrypted = Decrypt(encrypted, key);
        Debug.Log(decrypted);
    }
#endif
}