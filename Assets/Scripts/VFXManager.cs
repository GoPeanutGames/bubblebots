using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoSingleton<VFXManager>
{
    public GameObject ExplosionEffect;
    public GameObject LineExplosionEffect;
    public GameObject ColorExplosionEffect;
    public GameObject ColorChangingEffect;

    public List<GameObject> enemyBullets;

    public GameObject defaultMissile;
    public GameObject redMissile;
    public GameObject blueMissile;
    public GameObject greenMissile;
    public GameObject yellowMissile;
    public GameObject purpleMissile;

    public GameObject GetMissileForId(string id)
    {
        switch (id)
        {
            case "1":
                return purpleMissile;
            case "2":
                return redMissile;
            case "3":
                return blueMissile;
            case "4":
                return yellowMissile;
            case "5":
                return greenMissile;
           
            default:
                break;
        }

        return defaultMissile;
    }
}
