using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GUITile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    public int X = 0;
    public int Y = 0;
    public string Key = "";

    Match3GameplayManager gamePlayManager;

    private void Start()
    {
        gamePlayManager = FindObjectOfType<Match3GameplayManager>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (gamePlayManager.InputLocked())
        {
            return;
        }

        //gamePlayManager.SetDownTile(X, Y);
        gamePlayManager.TouchTile(X, Y);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (gamePlayManager.InputLocked())
        {
            return;
        }
        gamePlayManager.SwapTile(X, Y);

        //gamePlayManager.MoveOverTile(X, Y);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (gamePlayManager.InputLocked())
        {
            return;
        }

        gamePlayManager.ZeroReleasedTiles();
    }
}
