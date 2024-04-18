using Beebyte.Obfuscator;
using UnityEngine;

public class JSLibConnectionManager : MonoBehaviour
{
    [SkipRename]
    public void MetamaskLoginSuccess(string address)
    {
        GameEventString metamaskLoginEventData = new()
        {
            eventName = GameEvents.MetamaskSuccess,
            stringData = address
        };
        GameEventsManager.Instance.PostEvent(metamaskLoginEventData);
    }

    [SkipRename]
    public void SignatureLoginSuccess(string signature)
    {
        GameEventString metamaskSignatureEventData = new()
        {
            eventName = GameEvents.SignatureSuccess,
            stringData = signature
        };
        GameEventsManager.Instance.PostEvent(metamaskSignatureEventData);
    }

    [SkipRename]
    public void BundleBuySuccess()
    {
        Debug.Log($"{nameof(JSLibConnectionManager)}::{nameof(BundleBuySuccess)}");
        MetamaskManager.Instance.StoreBundleBuySuccess();
    }

    [SkipRename]
    public void BundleBuyFail()
    {
        Debug.Log($"{nameof(JSLibConnectionManager)}::{nameof(BundleBuyFail)}");
        MetamaskManager.Instance.StoreBundleBuyFail();
    }
    
    [SkipRename]
    public void BundleBuyFailBalance()
    {
        Debug.Log($"{nameof(JSLibConnectionManager)}::{nameof(BundleBuyFailBalance)}");
        MetamaskManager.Instance.StoreBundleBuyFailBalance();
    }


    [SkipRename]
    public void RequestWalletAdressSuccess(string address)
    {
        Debug.Log("JSLIB : got wallet adress: " + address);
    }

    [SkipRename]
    public void RequestSignatureSuccess(string signature)
    {
        Debug.Log("JSLIB : got wallet adress: " + signature);
    }

}
