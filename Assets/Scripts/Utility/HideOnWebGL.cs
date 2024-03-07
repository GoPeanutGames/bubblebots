using UnityEngine;

public class HideOnWebGL : MonoBehaviour
{
    void Start()
    {
#if UNITY_WEBGL
        this.gameObject.SetActive(false);
#endif
    }

    private void OnEnable()
    {
#if UNITY_WEBGL
        this.gameObject.SetActive(false);
#endif
    }
}