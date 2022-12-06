using UnityEngine;

namespace BubbleBots.Data
{
    [CreateAssetMenu(fileName = "Gameplay", menuName = "Bubble Bots/Game Play/Create Free To Play Gameplay Data", order = 1)]
    public class FreeToPlayGameplayData : GameplayData
    {
        public int stopHpRefreshAfterLevel = 5;
    }
 }
