using System;

namespace BubbleBots.Server.Signature
{
    public enum SignatureLoginAPI
    {
        Get,
        Web3LoginCheck,
        GoogleLogin,
        EmailPassSignUp,
        Login1stStep,
        Login2ndStep
    }

    [Serializable]
    public class GetLoginSchema
    {
        public DomainValue domain;
        public metaType message;
    }

    [Serializable]
    public class DomainValue
    {
        public int version;
        public int chainId;
        public string verifyingContract;
        public string name;
    }

    [Serializable]
    public class metaType
    {
        public string description;
        public string signer;
    }

    [Serializable]
    public class PostWeb3Login
    {
        public string address;

        public string signature;
    }

    [Serializable]
    public class ResponseWeb3Login
    {
        public bool status;
    }

    [Serializable]
    public class GoogleLogin
    {
        public string accessToken;
    }

    [Serializable]
    public class EmailPassSignUp
    {
        public string email;
        public string password;
    }
    
    [Serializable]
    public class Login2ndStep
    {
        public string email;
        public string password;
        public string twoFaCode;
    }


    [Serializable]
    public class LoginResult
    {
        public string jwt;
        public User user;
        public PostWeb3Login web3Info;
    }

    [Serializable]
    public class User
    {
        public string _id;
        public string email;
        public string firstName;
        public string lastName;
    }
}