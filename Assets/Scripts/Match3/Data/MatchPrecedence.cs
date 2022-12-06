using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleBots.Match3.Data
{
    [CreateAssetMenu(fileName = "Matches", menuName = "Bubble Bots/Match3/Create match precedence list", order = 1)]
    public class MatchPrecedence : ScriptableObject
    {
        public List<MatchShape> matches;
    }
}
