using System;

namespace BubbleBots.Data
{
    public enum RobotPosition {Left, Middle, Right}
    
    [Serializable]
    public class EnemyRobotData
    {
        public BubbleBotData robot;
        public RobotPosition position;
    }
}