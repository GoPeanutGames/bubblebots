using System.Collections;
using UnityEngine;

public class OrbDamageMissile : MonoBehaviour
{
    public GameObject impactParticle; // Effect spawned when projectile hits a collider
    public GameObject projectileParticle; // Effect attached to the gameobject as child

    public void Init()
    {
        projectileParticle.SetActive(true);
    }

    public void Explode()
    {
        projectileParticle.SetActive(false);
        impactParticle.SetActive(true);
    }
}
