using UnityEngine;
using System.Runtime.InteropServices;
using WalletConnectSharp.Unity;
using Beebyte.Obfuscator;
using BubbleBots.Server.Signature;
using WalletConnectSharp.Core.Models;

public class WalletLoginController : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Login();

    [DllImport("__Internal")]
    private static extern void RequestSignature(string schema, string address);

    public LoginController loginController;
    private string tempAddress;

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

    private async void RequestSignatureFromMetamask(string schema)
    {
        if(Application.isMobilePlatform == false)
        {
            RequestSignature(schema.ToString(), tempAddress);
        }
        else
        {
            string signature = await WalletConnect.Instance.Session.EthPersonalSign(tempAddress, schema);
            SignatureLoginSuccess(signature);
        }

    }

    [SkipRename]
    public void MetamaskLoginSuccess(string address)
    {
        tempAddress = address;

        SoundManager.Instance.PlayMetamaskEffect();

        ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginApi.Get, (schema) => {
            RequestSignatureFromMetamask(schema);
        }, address);
    }

    [SkipRename]
    public void SignatureLoginSuccess(string signature)
    {
        loginController.InitSession(tempAddress, signature);
    }

    public void OnNewWalletSessionConnectedEventFromPlugin(WalletConnectUnitySession session)
    {
        string account = session.Accounts[0];
        MetamaskLoginSuccess(account);
    }
}
