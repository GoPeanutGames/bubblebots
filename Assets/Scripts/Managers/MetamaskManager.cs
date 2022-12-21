using System;
using System.Runtime.InteropServices;

public class MetamaskManager : MonoSingleton<MetamaskManager>
{
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
}