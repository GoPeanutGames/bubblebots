using System;

namespace BubbleBots.Server.Signature
{
    public enum SignatureLoginAPI
    {
        Get,
        Web3LoginCheck
    }

    [Serializable]
    public class GetLoginSchema
    {
        public DomainValue domain;
        public MessageValue message;
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
    public class MessageValue
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
}