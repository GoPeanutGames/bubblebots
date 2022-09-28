using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float Speed = 1f;
    RectTransform rect;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        rect.GetComponent<RectTransform>().Rotate(Vector3.forward * Speed * Time.deltaTime);
    }
}
