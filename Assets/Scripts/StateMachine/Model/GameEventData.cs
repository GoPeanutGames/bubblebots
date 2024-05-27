using System.Collections.Generic;
using BubbleBots.Gameplay.Models;

public class GameEventData
{
    public string eventName;
}

public class GameEventString: GameEventData
{
    public string stringData;
}

public class GameEventStore : GameEventString
{
    public int bundleId;
}

public class GameEventInt : GameEventData
{
    public int intData;
}

public class GameEventLevelStart : GameEventData
{
    public List<BubbleBot> enemies;
    public PlayerRoster playerRoster;
}

public class GameEventLevelComplete : GameEventData
{
    public int numBubblesWon;
    public int lastLevelPotentialBubbles;
}

public class GameEventEnemyRobotDamage : GameEventData
{
    public int index;
    public int enemyRobotNewHp;
}


public class GameEventUpdateRoster : GameEventData
{
    public PlayerRoster playerRoster;
}

public class GameEventPlayerRobotKilled : GameEventData
{
    public int id; // as in order in list for ui. needs refactoring
    public int enemyIndex; // as in order in list for ui. needs refactoring
}

public class GameEventPlayerRobotDamage : GameEventData
{
    public int id; // as in order in list for ui. needs refactoring
    public int enemyIndex; // as in order in list for ui. needs refactoring
    public int damage;
}

public class GameEventFreeModeLose : GameEventData
{
    public int score;
    public int numBubblesWon;
    public int lastLevelPotentialBubbles;
}

public class GameEventEnemyRobotKilled : GameEventData
{
    public int id;
}

public class GameEventEnemyRobotTargeted : GameEventData
{
    public int id; // as in order in list for ui. needs refactoring
}

public class GameEventScoreUpdate: GameEventData
{
    public int score;
}

public class GameEventShowLevelText : GameEventData
{
    public float duration;
    public float fadeDuration;
}

public class GameEventBubbleExploded : GameEventData
{
    public int posX;
    public int posY;
}

public class GameEventUpdateSession : GameEventData
{
    public int bubbles;
}