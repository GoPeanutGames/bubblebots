using UnityEngine;
using System.Runtime.InteropServices;
using WalletConnectSharp.Unity;
using Beebyte.Obfuscator;
using BubbleBots.Server.Signature;

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
            WalletConnect.Instance.OpenDeepLink();
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
            Debug.Log("req mobile sign");
            string signature = await WalletConnect.ActiveSession.EthPersonalSign(tempAddress, schema);
            SignatureLoginSuccess(signature);
        }
    }

    [SkipRename]
    public void MetamaskLoginSuccess(string address)
    {
        Debug.Log("metamask login success");
        tempAddress = address;

        SoundManager.Instance.PlayMetamaskEffect();

        ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginApi.Get, (schema) => {

            Debug.Log("got sign from server");
            RequestSignatureFromMetamask(schema);
        }, address);
    }

    [SkipRename]
    public void SignatureLoginSuccess(string signature)
    {
        Debug.Log("Signature login success");
        loginController.InitSession(tempAddress, signature);
    }

    public void OnNewWalletSessionConnectedEventFromPlugin(WalletConnectUnitySession session)
    {
        Debug.Log("Connected from plugin");
        string account = session.Accounts[0];
        MetamaskLoginSuccess(account);
    }
}
