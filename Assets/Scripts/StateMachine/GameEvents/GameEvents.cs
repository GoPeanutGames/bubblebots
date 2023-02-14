public static class GameEvents
{
    public const string Default = "default";

    // App events
    public const string AppStart = "app.start";

    //UI events
    public const string ButtonTap = "ui.button.tap";
    
    //JSLIB events
    public const string MetamaskSuccess = "matamask.login.success";
    public const string SignatureSuccess = "metamask.signature.success";

    //gameplay
    public const string ShowLevetText = "gameplay.show.levelText";
    public const string BubbleExploded = "gameplay.bubble.exploded";
    public const string BubblesUnclaimedUpdate = "gameplay.bubbles.unclaimed.update";

    //free mode gameplay
    public const string FreeModeSessionStarted = "freemode.session.start";
    public const string FreeModeEnemyChanged = "freemode.enemy.changed";
    public const string FreeModeLevelStart = "freemode.level.start";
    public const string FreeModeLevelComplete = "freemode.level.complete";
    public const string FreeModeEnemyRobotDamage = "freemode.enemy.robot.damage";
    public const string FreeModeEnemyRobotKilled = "freemode.enemy.robot.killed";
    public const string FreeModeEnemyRobotTargeted = "freemode.enemy.robot.target";
    public const string FreeModePlayerRobotDamage = "freemode.player.robot.damage";
    public const string FreeModePlayerRobotKilled = "freemode.player.robot.killed";
    public const string FreeModeLose = "freemode.lose";
    public const string FreeModeScoreUpdate = "freemode.score.update";


    //nether mode
    public const string NetherModeComplete = "nether.mode.complete";

    // server comms
    public const string UpdateSessionResponse = "server.session.update";
    

}