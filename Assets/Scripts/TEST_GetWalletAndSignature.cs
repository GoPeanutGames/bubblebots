using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TEST_GetWalletAndSignature : MonoBehaviour
{

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void RequestWalletAdress_test();

    [DllImport("__Internal")]
    private static extern void RequestSignature_test();
#endif



    // Start is called before the first frame update
    void Start()
    {
#if UNITY_WEBGL
        RequestWalletAdress_test();
        RequestSignature_test();
#endif
    }

}
