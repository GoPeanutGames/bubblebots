using UnityEngine;

public class HideOnIPhone : MonoBehaviour
{
    void Start()
    {
#if UNITY_IOS
        this.gameObject.SetActive(false);
#endif
    }

    private void OnEnable()
    {
#if UNITY_IOS
        this.gameObject.SetActive(false);
#endif
    }
}