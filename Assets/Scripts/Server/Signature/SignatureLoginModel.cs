using System;

namespace BubbleBots.Server.Signature
{
    public enum SignatureLoginAPI
    {
        Get
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
}