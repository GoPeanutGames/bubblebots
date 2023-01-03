using System;
using System.Runtime.InteropServices;
using WalletConnectSharp.Core.Models.Ethereum;
using System.Threading.Tasks;
using BubbleBots.Server.Signature;
using WalletConnectSharp.Core;


public class MetamaskManager : MonoSingleton<MetamaskManager>
{
    public static EthChainData mumbaiChain = new EthChainData()
    {
        chainId = "0x13881",
        chainName = "Matic Mumbai",
        rpcUrls = new[] { "https://rpc-mumbai.maticvigil.com" },
        blockExplorerUrls = new[] { "https://mumbai.polygonscan.com/" },
        nativeCurrency = new NativeCurrency()
        {
            decimals = 18,
            name = "MATIC",
            symbol = "MATIC"
        }
    };
    
    public static EthChainData polygonChain = new EthChainData()
    {
        chainId = "0x89",
        chainName = "Polygon Mainnet",
        rpcUrls = new[] { "https://polygon-rpc.com" },
        blockExplorerUrls = new[] { "https://polygonscan.com/" },
        nativeCurrency = new NativeCurrency()
        {
            decimals = 18,
            name = "MATIC",
            symbol = "MATIC"
        }
    };
    
    [DllImport("__Internal")]
    private static extern void BuyBundle(int bundleId, bool isDev);

    private Action _onCompleteCallback;
    private Action<string> _onFailCallback;

    public void BuyStoreBundle(int bundleId, bool isDev, Action onComplete, Action<string> onFail)
    {
        _onCompleteCallback = onComplete;
        _onFailCallback = onFail;
#if !UNITY_EDITOR
        BuyBundle(bundleId, isDev);
#else
        StoreBundleBuySuccess();
#endif
    }

    public void StoreBundleBuySuccess()
    {
        _onCompleteCallback?.Invoke();
    }

    public void StoreBundleBuyFail()
    {
        _onFailCallback?.Invoke("error");
    }
    
    public void StoreBundleBuyFailBalance()
    {
        _onFailCallback?.Invoke("balance");
    }

    public static async Task<string> EthSignForMobile(WalletConnectSession activeSession, string address, string schema)
    {
        var request = new EthSignTypeDataV4(address, schema);

        var response = await activeSession.Send<EthSignTypeDataV4, EthResponse>(request);

        return response.Result;
    }

}