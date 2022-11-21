using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RobotEffectsCommunity : RobotEffects
{
    public Image BloodGround;
    public GameObject[] BloodSplashPrefabsOnDeath;
    public GameObject BloodSplashPrefabOnDamage;

    public override void Damage()
    {
        base.Damage();
        GameObject.Instantiate(BloodSplashPrefabOnDamage, this.transform);
    }

    public override void Die()
    {
        base.Die();
        BloodGround.gameObject.SetActive(true);
        BloodGround.DOFade(1, 1);
        GameObject.Instantiate(BloodSplashPrefabsOnDeath[Random.Range(0, BloodSplashPrefabsOnDeath.Length)], this.transform);
    }

    internal override void Initialize()
    {
        base.Initialize();
        BloodGround.DOFade(0, 0);
        BloodGround.gameObject.SetActive(false);
    }
}
