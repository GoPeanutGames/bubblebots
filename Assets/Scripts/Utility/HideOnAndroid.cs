using UnityEngine;

public class HideOnAndroid : MonoBehaviour
{
    void Start()
    {
#if UNITY_ANDROID
        this.gameObject.SetActive(false);
#endif
    }

    private void OnEnable()
    {
#if UNITY_ANDROID
        this.gameObject.SetActive(false);
#endif
    }
}