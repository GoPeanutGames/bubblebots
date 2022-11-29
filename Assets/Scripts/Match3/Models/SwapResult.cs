using System.Collections.Generic;
using UnityEngine;
using BubbleBots.Match3.Models;

namespace BubbleBots.Match3.Data
{

    public class NewSwapResult
    {
        public List<ExplodeEvent> explodeEvents;
    }


    public class SwapResult
    {
        public List<ExplodeEvent> explodeEvents;
        public ColorChangeEvent colorChangeEvent;

        public List<Vector2Int> toExplode;
        public List<GemCreate> toCreate;

        public List<int> swapedGemsIds;
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
    public class BombBlastEvent : ExplodeEvent
    {
        public Vector2Int bombPosition;
    }

    public class ColorBlastEvent : ExplodeEvent
    {
        public Vector2Int colorBlastPosition;
    }

    public class BoardBlastEvent : ExplodeEvent
    {
        public Vector2Int blastPosition;
    }

    public class ColorChangeEvent : ExplodeEvent
    {
        public int targetColor;
        public List<Vector2Int> toChange;
    }
}
