using Beebyte.Obfuscator;
using UnityEngine;

public class JSLibConnectionManager : MonoBehaviour
{
    [SkipRename]
    public void MetamaskLoginSuccess(string address)
    {
        GameEventString metamaskLoginEventData = new()
        {
            eventName = GameEvents.MetamaskSuccess,
            stringData = address
        };
        GameEventsManager.Instance.PostEvent(metamaskLoginEventData);
    }

    [SkipRename]
    public void SignatureLoginSuccess(string signature)
    {
        GameEventString metamaskSignatureEventData = new()
        {
            eventName = GameEvents.SignatureSuccess,
            stringData = signature
        };
        GameEventsManager.Instance.PostEvent(metamaskSignatureEventData);
    }
}
