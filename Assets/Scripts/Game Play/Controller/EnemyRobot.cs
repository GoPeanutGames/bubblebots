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

    public bool IsTargeted()
    {
        return Crossair.activeInHierarchy;
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
        HitEffect.SetActive(true);
        HitEffect.GetComponent<ParticleSystem>().Play();

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
        robotImage.DOFade(0.5f, 0.3f);
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
        robotImage.color = new Color(1,1,1,1);
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
