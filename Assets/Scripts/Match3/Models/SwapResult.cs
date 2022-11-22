using System.Collections.Generic;
using UnityEngine;
using BubbleBots.Match3.Models;

namespace BubbleBots.Match3.Data
{
    public class SwapResult
    {
        public List<Vector2Int> toExplode;
        public List<GemCreate> toCreate;
    }
}
