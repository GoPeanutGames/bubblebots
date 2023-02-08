using UnityEngine;

public class HideOnAndroid : MonoBehaviour
{
    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            this.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            this.gameObject.SetActive(false);
        }
    }
}