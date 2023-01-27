using System;

namespace BubbleBots.Server.Signature
{
    public enum SignatureLoginAPI
    {
        Get,
        Web3LoginCheck,
        GoogleLogin
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
    public class GoogleLoginResult
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