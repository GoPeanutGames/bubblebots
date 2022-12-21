using System.Collections.Generic;
using BubbleBots.Match3.Data;
using UnityEngine;

namespace BubbleBots.Data
{
    [CreateAssetMenu(fileName = "Gameplay", menuName = "Bubble Bots/Game Play/Create Game Play Data", order = 1)]
    public class GameplayData : ScriptableObject
    {
        public List<LevelData> levels;
        public List<BubbleBotData> robotsAvailable;
        public Sprite backgroundSprite;
        public Sprite gamebackgroundSprite;
    }
 }
