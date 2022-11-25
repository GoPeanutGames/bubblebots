using UnityEngine;
using System.Runtime.InteropServices;
using WalletConnectSharp.Unity;
using Beebyte.Obfuscator;

public class WalletLoginController : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Login();

    public LoginController loginController;

    private void Start()
    {
        WalletConnect.Instance.NewSessionConnected.AddListener(OnNewWalletSessionConnectedEventFromPlugin);
    }

    private void OnDestroy()
    {
        WalletConnect.Instance.NewSessionConnected.RemoveListener(OnNewWalletSessionConnectedEventFromPlugin);
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

    [SkipRename]
    public void MetamaskLoginSuccess(string address)
    {
        currentWalletAddress = account;
        SoundManager.Instance.PlayMetamaskEffect();
        loginController.InitSession(address);
    }

    public void OnNewWalletSessionConnectedEventFromPlugin(WalletConnectUnitySession session)
    {
        string account = session.Accounts[0];
        MetamaskLoginSuccess(account);
    }

    public void OnNewWalletSessionConnectedEventFromPlugin(WalletConnectUnitySession session)
    {
        string account = session.Accounts[0];
        MetamaskLoginSuccess(account);
    }

    public string GetWalletAddress()
    {
        return currentWalletAddress;
    }

    public void SetWalletAddress(string address)
    {
        currentWalletAddress = address;
    }
}
