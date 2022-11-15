using UnityEngine;
using System.Runtime.InteropServices;
using WalletConnectSharp.Unity;

public class WalletManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Login();

    public delegate void WalletCallback(object param);

    public static WalletManager Instance { get; private set; }

    private void Awake()
    {
        
        Instance = this;
    }

    public void LoginWithMetamask()
    {
        if (Application.isMobilePlatform == false)
        {
            Login();
        }
        else
        {
            Application.OpenURL(WalletConnect.Instance.ConnectURL);
            //TODO: mobile login
        }
    }

    public void MetamaskLoginSuccess()
    {
        SoundManager.Instance.PlayMetamaskEffect();
    }
}
