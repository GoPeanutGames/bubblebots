namespace BubbleBots.Match3.Models
{
    public class BoardCell
    {
        public bool empty;
        public BoardGem gem;

        public BoardCell()
        {
            empty = false;
        }

        public void SetGem(BoardGem _gem) {
            gem = _gem;
            empty = false;
        }

    }
}