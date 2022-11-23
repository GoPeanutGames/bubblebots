using UnityEngine;

public class CheatDetectionController : MonoBehaviour
{
    public void HackDetected()
    {
        Debug.LogError("HACK DETECTED!");
    }
}
