using System.Collections;
using System.Collections.Generic;
using BubbleBots.Gameplay.Models;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameScreenGame : GameScreen
{
    public List<PlayerRobot> playerRobotVisuals;
    public List<EnemyRobot> enemyRobotVisuals;
    public TextMeshProUGUI playerNameText;

    public void InitialiseEnemyRobots()
    {
        foreach (EnemyRobot enemyRobotVisual in enemyRobotVisuals)
        {
            enemyRobotVisual.Initialize();
        }
    }
    
    public void SetPlayerRobots(PlayerRoster roster)
    {
        for (int bot = 0; bot < roster.bots.Count; bot++)
        {
            playerRobotVisuals[bot].SetMaxHpTo(roster.bots[bot].maxHp);
            playerRobotVisuals[bot].SetHpTo(roster.bots[bot].hp);
            playerRobotVisuals[bot].Initialize();
            playerRobotVisuals[bot].SetRobotImage(roster.bots[bot].bubbleBotData);
        }
    }

    public void SetEnemyRobots(List<BubbleBot> enemyBots)
    {
        for (int bot = 0; bot < enemyBots.Count; bot++)
        {
            enemyRobotVisuals[bot].SetMaxHpTo(enemyBots[bot].maxHp);
            enemyRobotVisuals[bot].Initialize();
            enemyRobotVisuals[bot].SetEnemyRobotImage(enemyBots[bot].bubbleBotData.asEnemySprite);
        }
    }
    public void DamageEnemyRobotAndSetHp(int index, int hp)
    {
        enemyRobotVisuals[index].Damage();
        enemyRobotVisuals[index].SetHpTo(hp);
    }

    public void DamagePlayerRobotAndSetHp(int index, int currentEnemy, int damage)
    {
        playerRobotVisuals[index].DecreaseHpBy(damage);
        GameObject bullet = Instantiate(VFXManager.Instance.enemyBullets[currentEnemy], VFXManager.Instance.enemyBullets[currentEnemy].transform.parent);
        bullet.transform.position = enemyRobotVisuals[currentEnemy].transform.position;
        bullet.gameObject.SetActive(true);
        bullet.transform.DOMove(new Vector3(playerRobotVisuals[index].transform.position.x, playerRobotVisuals[index].transform.position.y, bullet.transform.position.z), 0.25f).SetEase(Ease.Linear);
        StartCoroutine(HideAndDestroyAfter(bullet, 0.21f, 1, index));
    }
    public void KillPlayerRobot(int index, int currentEnemy)
    {
        playerRobotVisuals[index].Die();
        GameObject bullet = Instantiate(VFXManager.Instance.enemyBullets[currentEnemy], VFXManager.Instance.enemyBullets[currentEnemy].transform.parent);
        bullet.transform.position = VFXManager.Instance.enemyBullets[currentEnemy].transform.position;
        bullet.gameObject.SetActive(true);
        bullet.transform.DOMove(new Vector3(playerRobotVisuals[index].transform.position.x, playerRobotVisuals[index].transform.position.y, bullet.transform.position.z), 0.25f).SetEase(Ease.Linear);
        StartCoroutine(HideAndDestroyAfter(bullet, 0.21f, 1, index));
    }

    public void KillEnemyRobot(int index)
    {
        enemyRobotVisuals[index].Die();
    }

    public void TargetEnemyRobot(int index)
    {
        foreach (EnemyRobot enemyRobot in enemyRobotVisuals)
        {
            enemyRobot.ClearTarget();
        }
        enemyRobotVisuals[index].SetTarget();
    }

    public EnemyRobot GetTargetedRobot()
    {
        foreach (EnemyRobot enemyRobot in enemyRobotVisuals)
        {
            if (enemyRobot.IsTargeted())
            {
                return enemyRobot;
            }
        }
        return null;
    }

    IEnumerator HideAndDestroyAfter(GameObject target, float timeToHide, float timeToDestroy, int id)
    {
        yield return new WaitForSeconds(timeToHide);
        playerRobotVisuals[id].Damage();

        target.SetActive(false);

        yield return new WaitForSeconds(timeToDestroy);

        Destroy(target);
    }

    public void SetPlayerName(string name)
    {
        playerNameText.text = name;
    }
}