using System.Collections.Generic;
using BubbleBots.Match3.Data;
using UnityEngine;

namespace BubbleBots.Data
{
    [CreateAssetMenu(fileName = "Gameplay", menuName = "Bubble Bots/Game Play/Create Game Play Data", order = 1)]
    public class GameplayData : ScriptableObject
    {
        [Header("Mode settings")]
        public List<LevelData> levels;
        public Sprite gamebackgroundSprite;
        [Header("Player robot settings")]
        public List<BubbleBotData> robotsAvailable;
        [Header("Enemy robot settings")]
        public List<BubbleBotData> enemyRobots;
    }
 }
