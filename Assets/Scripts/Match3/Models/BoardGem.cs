namespace BubbleBots.Match3.Models
{
    public enum GemType
    {
        Normal,
        Special,
        Bubble
    }

    //represents a gem on the board
    public class BoardGem
    {
        private string id;
        private GemType gemType;
        
        public BoardGem(string _id, GemType _type = GemType.Normal)
        {
            id = _id;
            gemType = _type;
        }

        public void SetId(string _id)
        {
            id = _id;
        }

        public string GetId()
        {
            return id;
        }

        public bool IsSpecial()
        {
            return id.Equals("9") ||
                id.Equals("10") ||
                id.Equals("11") ||
                id.Equals("12") ||
                id.Equals("13");
        }

        public bool IsSwappable()
        {
            return !IsBubble();
        }
        public bool IsMatchable()
        {
            return !IsBubble();
        }
        public bool IsBubble()
        {
            return id.Equals("14");
        }

    }
}