using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIRobotSelection : MonoBehaviour
{
    public GameObject PnlMenuUI;
    public Button BtnStart;
    public Sprite[] RobotSprites;

    int totalSelectedRobots = 0;
    int selectedRobot1 = -1;
    int selectedRobot2 = -1;
    int selectedRobot3 = -1;

    void Start()
    {
        
    }

    public void BackToMenu()
    {
        PnlMenuUI.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Play()
    {
        PnlMenuUI.SetActive(true);
        PnlMenuUI.GetComponent<GUIMenu>().SetSelectedRobots(selectedRobot1, selectedRobot2, selectedRobot3);
        PnlMenuUI.GetComponent<GUIMenu>().StartLevel1();
        gameObject.SetActive(false);
    }

    public void SelectRobot(int order)
    {
        if(order < 6 && totalSelectedRobots != 3)
        {
            if(selectedRobot1 == order || selectedRobot2 == order || selectedRobot3 == order)
            {
                return;
            }

            SelectRobotImage(order);

            totalSelectedRobots = Mathf.Clamp(totalSelectedRobots + 1, 1, 3);

            BtnStart.interactable = totalSelectedRobots == 3;
        }
    }

    private void SelectRobotImage(int order)
    {
        int selectOrder = 0;
        if (selectedRobot1 == -1)
        {
            selectOrder = 1;
        }
        else if (selectedRobot2 == -1)
        {
            selectOrder = 2;
        }
        else if (selectedRobot3 == -1)
        {
            selectOrder = 3;
        }

        Transform button = transform.Find("BtnRobot" + selectOrder);
        button.transform.Find("Plus").gameObject.SetActive(false);
        button.transform.Find("Cross").gameObject.SetActive(true);
        button.transform.Find("Avatar").GetComponent<Image>().sprite = RobotSprites[order];
        button.transform.Find("Avatar").GetComponent<Image>().color = new Color(1, 1, 1, 1);

        if (selectedRobot1 == -1)
        {
            selectedRobot1 = order;
        }
        else if (selectedRobot2 == -1)
        {
            selectedRobot2 = order;
        }
        else if (selectedRobot3 == -1)
        {
            selectedRobot3 = order;
        }
    }

    public void DeselectRobot(int order)
    {
        switch(order)
        {
            case 0:
                selectedRobot1 = -1;
                break;
            case 1:
                selectedRobot2 = -1;
                break;
            case 2:
                selectedRobot3 = -1;
                break;
            default:
                return;
        }

        Transform button = transform.Find("BtnRobot" + (order + 1));
        button.transform.Find("Plus").gameObject.SetActive(true);
        button.transform.Find("Cross").gameObject.SetActive(false);
        button.transform.Find("Avatar").GetComponent<Image>().sprite = null;
        button.transform.Find("Avatar").GetComponent<Image>().color = new Color(1, 1, 1, 0);

        totalSelectedRobots = Mathf.Clamp(totalSelectedRobots - 1, 0, 2);
        BtnStart.interactable = totalSelectedRobots == 3;
    }
}
