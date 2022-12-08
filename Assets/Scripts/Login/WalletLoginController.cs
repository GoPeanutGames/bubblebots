using System.Runtime.InteropServices;
using Beebyte.Obfuscator;
using BubbleBots.Server.Signature;
using UnityEngine;
using WalletConnectSharp.Unity;

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

    private async void RequestSignatureFromMetamask(string schema)
    {
        if (Application.isMobilePlatform)
        {
            string signature = await WalletConnect.ActiveSession.EthPersonalSign(tempAddress, schema);
            SignatureLoginSuccess(signature);
        }
        else
        {
            RequestSignature(schema, tempAddress);
        }
    }
    
    public void LoginWithMetamask()
    {
        if (Application.isMobilePlatform == false)
        {
            Login();
        }
        else
        {
            WalletConnect.Instance.OpenDeepLink();
        }
    }

    [SkipRename]
    public void MetamaskLoginSuccess(string address)
    {
        tempAddress = address;
        ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginAPI.Get, (schema) => { RequestSignatureFromMetamask(schema.ToString()); }, address);
    }

    [SkipRename]
    public void SignatureLoginSuccess(string signature)
    {
        SoundManager.Instance.PlayMetamaskSfx();
        loginController.InitSession(tempAddress, signature);
    }

    public void OnNewWalletSessionConnectedEventFromPlugin(WalletConnectUnitySession session)
    {
        string account = session.Accounts[0];
        MetamaskLoginSuccess(account);
    }
}