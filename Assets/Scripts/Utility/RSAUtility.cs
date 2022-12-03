using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using System;
using System.IO;
using System.Text;

public class RSAUtility
{
    public static string Decrypt(string encryptedData)
    {
        string publicKey = Environment.GetEnvironmentVariable("PUBLIC_KEY");
        byte[] bytesToDecrypt = Convert.FromBase64String(encryptedData);
        Pkcs1Encoding engine = new Pkcs1Encoding(new RsaEngine());
        using (StringReader txtReader = new StringReader(publicKey))
        {
            AsymmetricKeyParameter keyParam = (AsymmetricKeyParameter)new PemReader(txtReader).ReadObject();
            engine.Init(false, keyParam);
        }
        string decrypted = Encoding.UTF8.GetString(engine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));
        return decrypted;
    }

    public static string Encrypt(string data)
    {
        string publicKey = Environment.GetEnvironmentVariable("PUBLIC_KEY");
        byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(data);
        Pkcs1Encoding engine = new Pkcs1Encoding(new RsaEngine());
        using(StringReader txtReader = new StringReader(publicKey))
        {
            AsymmetricKeyParameter keyParam = (AsymmetricKeyParameter)new PemReader(txtReader).ReadObject();
            engine.Init(true, keyParam);
        }
        string encrypted = Convert.ToBase64String(engine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
        return encrypted;
    }
}
