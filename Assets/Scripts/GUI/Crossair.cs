using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Crossair : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating("AnimateCrossair", 0, 1.5f);
    }

    void AnimateCrossair()
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine(AnimateCrossairNow());
        }
    }

    IEnumerator AnimateCrossairNow()
    {
        transform.DOLocalRotate(transform.eulerAngles + new Vector3(0, 0, 179), 0.75f).SetEase(Ease.Linear);
        transform.DOScale(Vector3.one * 1.2f, 0.75f).SetEase(Ease.Linear);

        yield return new WaitForSeconds(0.75f);

        transform.DOLocalRotate(transform.eulerAngles + new Vector3(0, 0, 179), 0.75f).SetEase(Ease.Linear);
        transform.DOScale(Vector3.one, 0.75f).SetEase(Ease.Linear);
    }
}