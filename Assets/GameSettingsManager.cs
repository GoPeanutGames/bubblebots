using BubbleBots.Data;
using UnityEngine;

public class GameSettingsManager : MonoSingleton<GameSettingsManager>
{
    public FreeToPlayGameplayData freeModeGameplayData;
    public int freeModeEnemyDamage;
    public GameObject freemodeGameplayManager; //feels like a hack


    public NetherModeGameplayData netherModeGameplayData;
    public int netherModeEnemyDamage;
    public GameObject netherModeGameplayManager;
}
