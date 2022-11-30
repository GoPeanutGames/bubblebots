using System.Collections.Generic;
using UnityEngine;

namespace BubbleBots.Data
{
    [CreateAssetMenu(fileName = "Levels", menuName = "Bubble Bots/Create Level Data", order = 1)]
    public class LevelData : ScriptableObject
    {
        public int width;
        public int height;
        public List<int> gemSet;
        public int waves;
    }
}
