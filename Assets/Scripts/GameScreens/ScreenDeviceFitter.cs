using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenDeviceFitter : MonoBehaviour
{
    public GameObject DesktopCanvas;
    public GameObject MobileCanvas;
    public GameObject UIRoot;
    
    private void Start()
    {
        if (Application.isMobilePlatform)
        {
            UIRoot.transform.parent = MobileCanvas.transform;
            DesktopCanvas.SetActive(false);
            RectTransform rTransformUI = UIRoot.GetComponent<RectTransform>();
            rTransformUI.anchorMax = new Vector2(1, 1);
            rTransformUI.anchorMin = new Vector2(0, 0);
            rTransformUI.offsetMax = new Vector2(0, 0);
            rTransformUI.offsetMin = new Vector2(0, 0);
            rTransformUI.localScale = new Vector3(1, 1, 1);
        }
    }
}
