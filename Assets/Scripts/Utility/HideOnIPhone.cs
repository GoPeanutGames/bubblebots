using UnityEngine;

public class HideOnIPhone : MonoBehaviour
{
    void Start()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            this.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            this.gameObject.SetActive(false);
        }
    }
}