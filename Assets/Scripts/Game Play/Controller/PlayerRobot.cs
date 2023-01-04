using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRobot : MonoBehaviour
{
    public GameObject HitEffect;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    private bool damageAnimationIsRunning = false;

    private Image robotImage;
    private int maxHp;
    private int currentHp;

    void Start()
    {
        robotImage = GetComponent<Image>();
        HitEffect.SetActive(false);
        robotImage.DOFade(1f, 0f);
        robotImage.transform.DOScale(1f, 0f);
        damageAnimationIsRunning = false;
    }

    private void SetHpText(int value, int max)
    {
        hpText.text = value + " / " + max;
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
        hpSlider.DOValue(0, 0.33f);
        SetHpText(0, maxHp);
        StartCoroutine(DieEffect());
    }

    IEnumerator DieEffect()
    {
        robotImage.DOFade(0.35f, 0.5f);
        robotImage.transform.DOScale(0.9f, 0.5f);
        yield return null;
    }

    internal virtual void Initialize()
    {
        robotImage = GetComponent<Image>();
        if (robotImage != null && currentHp != 0)
        {
            robotImage.DOFade(1f, 0f);
            robotImage.transform.DOScale(1f, 0f);
        }
    }

    public void SetRobotImage(Sprite sprite)
    {
        robotImage.sprite = sprite;
    }

    public void SetMaxHpTo(int maxHp)
    {
        this.maxHp = maxHp;
        hpSlider.minValue = 0;
        hpSlider.maxValue = maxHp;
    }

    public void SetHpTo(int hp)
    {
        currentHp = hp;
        hpSlider.value = hp;
        SetHpText(hp, maxHp);
    }

    public void DecreaseHpBy(int damage)
    {
        currentHp -= damage;
        hpSlider.DOValue(currentHp, 0.33f);
        SetHpText(currentHp, maxHp);
    }
}