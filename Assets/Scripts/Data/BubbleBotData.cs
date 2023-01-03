using UnityEngine;

namespace BubbleBots.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "Bubble Bots/Data/Create Bubble Bot Data", order = 1)]
    public class BubbleBotData : ScriptableObject
    {
        public int id;
        public string botName;
        public Sprite bgSprite;
        public Sprite robotSprite;
        public Sprite asEnemySprite;
        public Sprite frameSprite;
        public Sprite badgeSprite;
        public Sprite labelSprite;

        public Sprite robotSelection;
    }
}