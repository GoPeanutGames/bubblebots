using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class RobotEffects : MonoBehaviour
{
    public GameObject Crossair;
    public GameObject HitEffect;

    Image robotImage;
    
    void Start()
    {
        robotImage = GetComponent<Image>();

        HitEffect.SetActive(false);
        Crossair.SetActive(false);

        Crossair.transform.localScale = Vector3.one;
        robotImage.DOFade(1f, 0f);
        robotImage.transform.DOScale(1f, 0f);
    }

    public void Damage()
    {
        StartCoroutine(DamageEffect());
    }

    IEnumerator DamageEffect()
    {
        GameObject hitObject = Instantiate(HitEffect, HitEffect.transform.position, HitEffect.transform.rotation, transform.parent);
        hitObject.SetActive(true);
        Destroy(hitObject, 2);

        robotImage.CrossFadeColor(Color.red, 0.35f, false, false);
        yield return new WaitForSeconds(0.35f);
        robotImage.CrossFadeColor(Color.white, 0.35f, false, false);
    }

    public void FadeOut()
    {
        robotImage.DOFade(0f, 0.33f);
    }

    public void FadeIn()
    {
        robotImage.DOFade(1f, 0.33f);
    }

    public void YourTurn()
    {
        Crossair.SetActive(false);
    }

    public void Die()
    {
        StartCoroutine(DieEffect());
    }

    IEnumerator DieEffect()
    {
        Crossair.transform.DOScale(0, 0.3f);
        robotImage.DOFade(0.35f, 0.5f);
        robotImage.transform.DOScale(0.9f, 0.5f);

        yield return new WaitForSeconds(0.3f);

        Crossair.SetActive(false);
    }

    public void SetTarget()
    {
        Crossair.SetActive(true);
    }

    public void ClearTarget()
    {
        Crossair.SetActive(false);
    }

    internal void Initialize()
    {
        Crossair.transform.localScale = Vector3.one;
        if (robotImage != null)
        {
            robotImage.DOFade(1f, 0f);
            robotImage.transform.DOScale(1f, 0f);
        }
    }
}
