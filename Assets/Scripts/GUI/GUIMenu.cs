using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;

public class GUIMenu : MonoBehaviour
{
    public Image MenuImage;
    public Image MapImage;
    public Image GameImage;
    public Image WinDialogImage;
    public Image WinNoMoreMoves;
    public TextMeshProUGUI TxtStatus;
    public Button PlayButton;
    public GameObject Robot1;
    public GameObject Robot2;

    GamePlayManager gamePlayManager = null;

    private void Awake()
    {
        Robot1.SetActive(false);
        Robot2.SetActive(false);
        //PlayButton.gameObject.SetActive(true);
        //TxtStatus.gameObject.SetActive(false);
        //GameImage.gameObject.SetActive(false);

        gamePlayManager = FindObjectOfType<GamePlayManager>();
    }

    public void SwitchToMap()
    {
        MenuImage.GetComponent<CanvasGroup>().DOFade(0, 0.5f);

        StartCoroutine(TurnOffGUI());
    }

    public void SwitchToMultiplayer(string levelFile, int levelNumber)
    {
        MenuImage.gameObject.SetActive(true);
        MenuImage.GetComponent<CanvasGroup>().DOFade(0, 0f);
        MenuImage.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

        StartCoroutine(StartMultiplayerSequence(levelFile, levelNumber));
    }

    IEnumerator StartMultiplayerSequence(string levelFile, int levelNumber)
    {
        DisplayStatusText("");

        yield return new WaitForSeconds(1);
        DisplayStatusText("FINDING OPPONENT...");

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.63f, 1.14f));
        DisplayStatusText("OPPONENT FOUND...");

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.32f, 0.49f));
        DisplayStatusText("CONNECTING...");

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.63f, 1.14f));
        DisplayStatusText("STARTING THE GAME...");

        MapImage.gameObject.SetActive(false);
        MenuImage.GetComponent<CanvasGroup>().DOFade(0, 0.5f);

        StartCoroutine(TurnOffGUI());
        StartCoroutine(TurnOnPlay(levelFile, levelNumber));
    }

    IEnumerator TurnOffGUI()
    {
        //MenuImage.raycastTarget = false;
        yield return new WaitForSeconds(0.6f);

        MenuImage.gameObject.SetActive(false);
    }

    IEnumerator TurnOnPlay(string levelFile, int levelNumber)
    {
        //yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();

        GameImage.gameObject.SetActive(true);
        GameImage.GetComponent<CanvasGroup>().DOFade(0, 0f);
        GameImage.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

        Robot1.SetActive(true);
        //Robot2.SetActive(true);

        gamePlayManager?.StartLevel(levelFile, levelNumber);
    }

    public void DisplayStatusText(string message)
    {
        PlayButton.gameObject.SetActive(false);
        TxtStatus.gameObject.SetActive(true);

        TxtStatus.text = message;
    }

    public void DisplayWin()
    {
        WinDialogImage.gameObject.SetActive(true);
    }

    public void HideWin()
    {
        Robot1.SetActive(false);
        Robot2.SetActive(false);

        WinDialogImage.gameObject.SetActive(false);
        MapImage.gameObject.SetActive(true);
    }

    public void DisplayNoMoreMoves()
    {
        WinNoMoreMoves.gameObject.SetActive(true);
    }

    public void HideNoMoreMoves()
    {
        WinNoMoreMoves.gameObject.SetActive(false);
    }

    internal void UnlockLevel(int numLevel)
    {
        Transform level = MapImage.transform.Find("Level" + numLevel);

        if(level != null)
        {
            level.GetComponent<LevelInfo>().Locked = false;
            level.transform.Find("ImgLocked").gameObject.SetActive(false);
            level.transform.Find("BtnLevel").GetComponent<Button>().interactable = true;

            MenuImage.gameObject.SetActive(false);
        }
    }
}
