using System.Collections.Generic;
using UnityEngine;

namespace BubbleBots.Match3.Data
{
    [System.Serializable]
    public class MatchOffsets
    {
        [SerializeField]
        public List<Vector2Int> offsetList;
    }

    [CreateAssetMenu(fileName = "Shapes", menuName = "Bubble Bots/Match3/Create match shape", order = 1)]
    public class MatchShape : ScriptableObject
    {
        [SerializeField]
        public List<MatchOffsets> offsets;

        public GemData matchOutcome = null;
    }
}
