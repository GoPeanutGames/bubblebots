using BubbleBots.Gameplay.Models;
using System.Collections.Generic;

public class GameEventData
{
    public string eventName;
}

public class GameEventString: GameEventData
{
    public string stringData;
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

public class GameEventEnemyRobotDamage : GameEventData
{
    public int enemyRobotNewHp;
}

public class GameEventPlayerRobotKilled : GameEventData
{
    public int id; // as in order in list for ui. needs refactoring
}

public class GameEventPlayerRobotDamage : GameEventData
{
    public int id; // as in order in list for ui. needs refactoring
    public int damage;
}

public class GameEventFreeModeLose : GameEventData
{
    public int score;
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

public class GameEventUpdateUnclaimedBubbles : GameEventData 
{
    public int balance;
}