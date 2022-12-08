using UnityEngine;
namespace BubbleBots.Match3.Data
{
    [CreateAssetMenu(fileName = "Gems", menuName = "Bubble Bots/Create Gem Data", order = 1)]
    public class GemData : ScriptableObject
    {
        public string gemId;
        public Sprite gemSprite;
    }
}


