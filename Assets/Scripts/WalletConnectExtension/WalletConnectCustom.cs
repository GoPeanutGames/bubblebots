using UnityEngine;
using WalletConnectSharp.Unity;

public class WalletConnectCustom : WalletConnect
{
    protected override void SetupEvents()
    {
        base.SetupEvents();
        if (Application.isMobilePlatform)
        {
            Session.OnSend += (sender, session) => OpenMobileWallet();
        }
    }

    public override void OpenMobileWallet()
    {
        if (Application.isMobilePlatform)
        {
            var signingURL = ConnectURL.Split('@')[0];
            Application.OpenURL(signingURL);
        }
    }

    public override void OpenDeepLink()
    {
        if (!ActiveSession.ReadyForUserPrompt)
        {
            Debug.LogError("WalletConnectUnity.ActiveSession not ready for a user prompt" +
                           "\nWait for ActiveSession.ReadyForUserPrompt to be true");

            return;
        }

        if (Application.isMobilePlatform)
        {
            Debug.Log("[WalletConnect] Opening URL: " + ConnectURL);
            Application.OpenURL(ConnectURL);
        }
    }
}