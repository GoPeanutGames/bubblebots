using BubbleBots.Data;
using UnityEngine;

public class GameSettingsManager : MonoSingleton<GameSettingsManager>
{
    public FreeToPlayGameplayData freeToPlayGameplayData;
    public int freeToPlayEnemyDamage;
    public GameObject freeToPlayGameplayManager; //feels like a hack
}
