using System;
using UnityEngine.Serialization;

namespace BubbleBots.Server.Signature
{
    public enum SignatureLoginAPI
    {
        Get,
        Web3LoginCheck,
        GoogleLogin,
        AppleLogin,
        EmailPassSignUp,
        Login1StStep,
        Login2NdStep,
        AutoLoginGet,
        ResetPassword,
        SetNewPass
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
    public class GoogleLoginData
    {
        public string accessToken;
    }
    
    [Serializable]
    public class AppleLoginData
    {
        public string appleToken;
    }

    [Serializable]
    public class EmailPassSignUp
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class ResetPassData
    {
        public string email;
    }
    
    [Serializable]
    public class SetNewPassData
    {
        public string newPassword;
        public string token;
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
        public string token;
        public User user;
        public PostWeb3Login web3Info;
    }

    [Serializable]
    public class User
    {
        public string _id;
        public string email;
    }
}