using UnityEngine;

namespace BubbleBots.Match3.Models
{
    public class GemMove
    {
        public Vector2Int From { get; }
        public Vector2Int To { get; }

        public GemMove(Vector2Int to, Vector2Int from)
        {
            From = from;
            To = to;
        }
    }
}
