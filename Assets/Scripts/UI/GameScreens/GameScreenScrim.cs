using UnityEngine;
using UnityEngine.UI;

public class GameScreenScrim : GameScreen
{
    [SerializeField] private Button scrimButton;

    public void MakeScrimButtonInvisible()
    {
        ColorBlock colorBlock = scrimButton.colors;
        Color color = colorBlock.normalColor;
        color.a = 0f;
        colorBlock.normalColor = color;
        colorBlock.highlightedColor = color;
        colorBlock.pressedColor = color;
        colorBlock.selectedColor = color;
        colorBlock.disabledColor = color;
        colorBlock.fadeDuration = 0.0f;
        scrimButton.colors = colorBlock;
    }

    public void DisableScrimButton()
    {
        scrimButton.gameObject.SetActive(false);
    }
}
