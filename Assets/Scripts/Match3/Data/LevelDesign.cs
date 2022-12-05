using System.Collections.Generic;
using UnityEngine;

namespace BubbleBots.Match3.Data
{
    [CreateAssetMenu(fileName = "Levels", menuName = "Bubble Bots/Create Level Design", order = 1)]
    public class LevelDesign : ScriptableObject
    {
        public List<LevelRow> rows;
    }

    [System.Serializable]
    public class LevelRow
    {
        public List<int> row;
        public int this[int i]
        {
            get => row[i];
            set => row[i] = value;
        }
    }
}

