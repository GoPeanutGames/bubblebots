using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class MetamaskManager : MonoSingleton<MetamaskManager>
{

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void BuyBundle(int bundleId, bool isDev);
#endif

    private Action _onCompleteCallback;
    private Action<string> _onFailCallback;

    public void BuyStoreBundle(int bundleId, bool isDev, Action onComplete, Action<string> onFail)
    {
        Debug.Log($"{nameof(MetamaskManager)}::{nameof(BuyStoreBundle)}");
        _onCompleteCallback = onComplete;
        _onFailCallback = onFail;
#if UNITY_WEBGL
        BuyBundle(bundleId, isDev);
#endif
    }

    public void StoreBundleBuySuccess()
    {
        Debug.Log($"{nameof(MetamaskManager)}::{nameof(StoreBundleBuySuccess)}");
        _onCompleteCallback?.Invoke();
    }

    public void StoreBundleBuyFail()
    {
        Debug.Log($"{nameof(MetamaskManager)}::{nameof(StoreBundleBuyFail)}");
        _onFailCallback?.Invoke("error");
    }

    public void StoreBundleBuyFailBalance()
    {
        Debug.Log($"{nameof(MetamaskManager)}::{nameof(StoreBundleBuyFailBalance)}");
        _onFailCallback?.Invoke("balance");
    }
}