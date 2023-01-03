using System;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Models;

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
    
    public sealed class EthSignTypeDataV4: JsonRpcRequest
    {
        [JsonProperty("params")]
        private string[] _parameters;

        public EthSignTypeDataV4(string address, string schema) : base()
        {
            this.Method = "eth_signTypedData";

            this._parameters = new string[] { address, schema };
        }
    }
}