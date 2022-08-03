using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelInfo : MonoBehaviour
{
    public int LevelNumber = 1;
    public int UnlocksLevel = 2;
    public bool Locked = true;
    public string DataFile = "level1";

    Transform imgLocked;
    Transform btnLevel;

    void Start()
    {
        imgLocked = transform.Find("ImgLocked");
        btnLevel = transform.Find("BtnLevel");

        if (imgLocked != null)
        {
            imgLocked.gameObject.SetActive(Locked);
            btnLevel.GetComponent<Button>().interactable = !Locked;
            btnLevel.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LevelNumber.ToString();
        }
    }

    public void PlayLevel()
    {
        if(!Locked)
        {
            FindObjectOfType<GamePlayManager>().PrepareLevel(DataFile);
        }
    }
}
