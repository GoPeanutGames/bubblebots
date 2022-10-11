using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class WalletManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Login();

    public delegate void WalletCallback(object param);

    public void LoginWithMetamask(WalletCallback onSuccess, WalletCallback onFailure)
    {
        Login();
    }
}
