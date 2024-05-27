using System.Collections.Generic;
using UnityEngine;

namespace BubbleBots.Data
{
    [CreateAssetMenu(fileName = "Gameplay", menuName = "Bubble Bots/Game Play/Create Levels Mode Gameplay Data", order = 1)]
    public class LevelsModeGameplayData : GameplayData
    {
        public int startHP;
        public int hpIncrement;

        public List<Sprite> backgrounds;
    }
 }
