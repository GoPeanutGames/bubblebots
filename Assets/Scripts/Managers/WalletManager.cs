using UnityEngine;
using System.Runtime.InteropServices;
using WalletConnectSharp.Unity;
using Beebyte.Obfuscator;

public class WalletManager : MonoSingleton<WalletManager>
{
    public delegate void WalletCallback(object param);

    [DllImport("__Internal")]
    private static extern void Login();

    private string currentWalletAddress = "";

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

    [SkipRename]
    public void MetamaskLoginSuccess(string account)
    {
        currentWalletAddress = account;
        SoundManager.Instance.PlayMetamaskEffect();
        GameObject.FindObjectOfType<GUIMenu>().InitSession(account);
    }

    public void OnNewWalletSessionConnectedEventFromPlugin(WalletConnectUnitySession session)
    {
        string account = session.Accounts[0];
        //TODO: REFACTOR - UnityEvent to launch this, GUIMenu listens for it
        //TODO: bad for performance, but no other way until other things are refactored
        MetamaskLoginSuccess(account);
    }

    public string GetWalletAddress()
    {
        return currentWalletAddress;
    }
}
