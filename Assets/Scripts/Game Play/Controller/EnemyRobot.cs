using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyRobot : MonoBehaviour
{
    public GameObject Crossair;
    public GameObject HitEffect;
    public Image robotImage;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    private bool damageAnimationIsRunning = false;
    private int maxHp;
    
    void Start()
    {

        HitEffect.SetActive(false);
        Crossair.SetActive(false);

        Crossair.transform.localScale = Vector3.one;
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

        yield return new WaitForSeconds(0.35f);
        damageAnimationIsRunning = false;
    }

    public virtual void Die()
    {
        SetHpTo(0);
        StartCoroutine(DieEffect());
    }

    IEnumerator DieEffect()
    {
        Crossair.transform.DOScale(0, 0.3f);

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
    }

    public void SetEnemyRobotImage(Sprite sprite)
    {
        robotImage.sprite = sprite;
    }
    
    public void SetMaxHpTo(int hp)
    {
        this.maxHp = hp;
        hpSlider.minValue = 0;
        hpSlider.maxValue = hp;
        hpSlider.value = hp;
        SetHpText(hp,hp);
    }
    
    public void SetHpTo(int currentHp)
    {
        hpSlider.DOValue(currentHp,0.33f);
        SetHpText(currentHp, maxHp);
    }
}
