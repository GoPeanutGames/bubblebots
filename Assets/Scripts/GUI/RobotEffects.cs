using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RobotEffects : MonoBehaviour
{
    public GameObject Crossair;
    public GameObject HitEffect;
    public Image EnemyRobot;

    private bool damageAnimationIsRunning = false;

    Image robotImage;
    
    void Start()
    {
        robotImage = GetComponent<Image>();

        HitEffect.SetActive(false);
        Crossair.SetActive(false);

        Crossair.transform.localScale = Vector3.one;
        robotImage.DOFade(1f, 0f);
        robotImage.transform.DOScale(1f, 0f);
        damageAnimationIsRunning = false;
    }

    public virtual void Damage()
    {
        if (!damageAnimationIsRunning)
        {
            StartCoroutine(DamageEffect());
        }
    }

    IEnumerator DamageEffect()
    {
        damageAnimationIsRunning = true;
        GameObject hitObject = Instantiate(HitEffect, HitEffect.transform.position, HitEffect.transform.rotation, transform.parent);
        hitObject.SetActive(true);
        Destroy(hitObject, 2);

        robotImage.CrossFadeColor(Color.red, 0.35f, false, false);
        yield return new WaitForSeconds(0.35f);
        robotImage.CrossFadeColor(Color.white, 0.35f, false, false);
        damageAnimationIsRunning = false;
    }

    public void FadeOut()
    {
        robotImage.DOFade(0f, 0.33f);
    }

    public void FadeIn()
    {
        robotImage.DOFade(1f, 0.33f);
    }

    public virtual void Die()
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

    internal virtual void Initialize()
    {
        Crossair.transform.localScale = Vector3.one;
        robotImage = GetComponent<Image>();
        if (robotImage != null)
        {
            robotImage.DOFade(1f, 0f);
            robotImage.transform.DOScale(1f, 0f);
        }
    }

    public void SetRobotImage(Sprite sprite)
    {
        robotImage.sprite = sprite;
    }

    public void SetEnemyRobotImage(Sprite sprite)
    {
        EnemyRobot.sprite = sprite;
    }
}
