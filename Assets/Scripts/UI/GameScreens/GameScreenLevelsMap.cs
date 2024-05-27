using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class GameScreenLevelsMap : GameScreen
{
    public TMPro.TextMeshProUGUI levelsMapPlayButtonText;
    public Button playButton;
    public List<GameObject> levelPins;

    public ScrollRect levelScrollRect;
    public UILineRenderer uiLineRenderer;

    public void SetPlayButtonText(string text)
    {
        levelsMapPlayButtonText.text = text;
    }


    public void SetCurrentLevel(int currentLevel, int totalLevels)
    {
        //levelScrollRect.normalizedPosition = new Vector2(0, 0);
        levelScrollRect.normalizedPosition = new Vector2(0, (float)currentLevel / totalLevels);
        for (int i = 0; i < levelPins.Count; ++i)
        {
            levelPins[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = (i + 1).ToString();
            levelPins[i].GetComponent<Button>().interactable = i + 1 <= currentLevel;
            if (i + 1 < currentLevel)
            {
                levelPins[i].GetComponentInChildren<Image>().color = levelPins[i].GetComponent<Button>().colors.highlightedColor;
            }
        }
    }

}
