using System.Collections.Generic;
using UnityEngine;

namespace BubbleBots.Match3.Data
{
    [CreateAssetMenu(fileName = "Levels", menuName = "Bubble Bots/Create Level Data", order = 1)]
    public class LevelData : ScriptableObject
    {
        public int width;
        public int height;
        public List<GemData> gemSet;
        public List<GemData> gemSetSpecials;
        public GemData bubbleGem;
        public int waves;

        public Sprite background;

        [Range(0, 100)]
        public float bubbleSpawnChance = 0;


        public GemData GetGemData(string id)
        {
            for (int i = 0; i < gemSet.Count; ++i)
            {
                if (gemSet[i].gemId == id)
                {
                    return gemSet[i];
                }
            }

            for (int i = 0; i < gemSetSpecials.Count; ++i)
            {
                if (gemSetSpecials[i].gemId == id)
                {
                    return gemSetSpecials[i];
                }
            }

            if (id == bubbleGem.gemId)
            {
                return bubbleGem;
            }

            return null;
        }
    }
}
