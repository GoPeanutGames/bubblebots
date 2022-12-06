using UnityEngine;

namespace BubbleBots.Match3.Models
{
    public class GemCreate
    {
        public Vector2Int At { get; }
        public int Id { get; }

        public GemCreate(Vector2Int at, int id)
        {
            At = at;
            Id = id;
        }
    }
}
