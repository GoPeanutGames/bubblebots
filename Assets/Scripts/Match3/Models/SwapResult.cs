using System.Collections.Generic;
using UnityEngine;
using BubbleBots.Match3.Models;

namespace BubbleBots.Match3.Data
{
    public class SwapResult
    {
        public List<ExplodeEvent> explodeEvents;

        public List<Vector2Int> toExplode;
        public List<GemCreate> toCreate;
    }


    public class ExplodeEvent
    {
        public List<Vector2Int> toExplode;
        public List<GemCreate> toCreate;
    }

    public class LineBlastExplodeEvent : ExplodeEvent
    {
        public Vector2Int lineBlastStartPosition;
    }

    public class ColumnBlastEvent : ExplodeEvent
    {
        public Vector2Int columnBlastStartPosition;
    }


    public class HammerBlastEvent : ExplodeEvent
    {

    }
}
