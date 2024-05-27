using CodeStage.AntiCheat.ObscuredTypes;

public class FreeToPlaySessionData
{
    ObscuredLong score = 0;

    private int scoreMultiplier = 10;// hardcoded score multiplier

    private int robotsKilled = 0;

    private int potentialBubbles = 0;

    private int totalBubbles = 0;

    public void IncrementScore(int toAdd)
    {
        score += toAdd * scoreMultiplier;
    }

    public long GetScore()
    {
        return score;
    }

    public void IncrementRobotsKilled(int killed)
    {
        robotsKilled += killed;
    }

    public int GetRobotsKilled()
    {
        return robotsKilled;
    }

    public void SetPotentialBubbles(int value)
    {
        potentialBubbles = value;
    }

    public void AddPotentialBubbles(int value)
    {
        potentialBubbles += value;
    }

    public int GetPotentialBubbles()
    {
        return potentialBubbles;
    }

    public void AddTotalBubbles(int val)
    {
        totalBubbles += val;
    }

    public int GetTotalBubbles()
    {
        return totalBubbles;
    }

    public void ResetPotentialBubbles()
    {
        potentialBubbles = 0;
    }
}