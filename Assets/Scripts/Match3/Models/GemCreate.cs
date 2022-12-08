using UnityEngine;
using BubbleBots.Match3.Data;

namespace BubbleBots.Match3.Models
{
    public class GemCreate
    {
        public Vector2Int At { get; }
        public GemData GemData { get; }

        public GemCreate(Vector2Int at, GemData gemData)
        {
            At = at;
            GemData = gemData;
        }
    }
}
