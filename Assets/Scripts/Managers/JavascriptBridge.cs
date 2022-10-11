using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JavascriptBridge : MonoBehaviour
{
    public void SetWalletAddress(string address)
    {
        Debug.Log("Wallet address is set as " + address);
    }
}
