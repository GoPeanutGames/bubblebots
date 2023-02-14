using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoSingleton<VFXManager>
{
    public GameObject ExplosionEffect;
    public GameObject LineExplosionEffect;
    public GameObject ColorExplosionEffect;
    public GameObject ColorChangingEffect;

    public List<GameObject> enemyBullets;

    public GameObject bullet;
}
