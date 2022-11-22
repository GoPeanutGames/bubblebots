namespace BubbleBots.Match3.Models
{
    public enum GemType
    {
        Normal,
        Special
    }

    //represents a gem on the board
    public class BoardGem
    {
        private int id;
        private GemType gemType;
        
        public BoardGem(int _id, GemType _type = GemType.Normal)
        {
            id = _id;
            gemType = _type;
        }

        public void SetId(int _id)
        {
            id = _id;
        }

        public int GetId()
        {
            return id;
        }

        public bool IsSpecial()
        {
            return id >= 9;
        }
                
    }
}