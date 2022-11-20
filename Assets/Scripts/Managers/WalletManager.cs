using UnityEngine;
using System.Runtime.InteropServices;
using WalletConnectSharp.Unity;
using WalletConnectSharp.Core.Models;

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
        }
    }

    public void MetamaskLoginSuccess()
    {
        SoundManager.Instance.PlayMetamaskEffect();
    }

    public void OnNewWalletSessionConnectedEventFromPlugin(WalletConnectUnitySession session)
    {
        MetamaskLoginSuccess();
        string account = session.Accounts[0];
        //TODO: REFACTOR - UnityEvent to launch this, GUIMenu listens for it
        //TODO: bad for performance, but no other way until other things are refactored
        GameObject.FindObjectOfType<GUIMenu>().InitSession(account);
    }
}
