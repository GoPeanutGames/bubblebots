using System.Collections.Generic;
using BubbleBots.Data;
using UnityEngine;
using UnityEngine.UI;

public class GameScreenRobotSelection : GameScreen
{
    public List<BubbleBots.Data.BubbleBotData> availableBots;

    [SerializeField] private GameObject selectionListRoot;
    [SerializeField] private GameObject selectionRobotPrefab;
    [SerializeField] private Button startButton;
    [SerializeField] private List<RobotSelectedUIElement> playerRobotsImages;

    private List<RobotSelectionUIElement> selectableRobots;

    private List<int> selectedRobots;
    


    public List<BubbleBotData> GetSelectedBots()
    {
        List<BubbleBotData> selectedBots = new List<BubbleBotData>();
        for (int i = 0; i < selectedRobots.Count; ++i)
        {
            selectedBots.Add(availableBots.Find(x => x.id == selectedRobots[i]));
        }
        return selectedBots;
    }

    public void PopulateSelectionList()
    {
        selectedRobots = new List<int>();
        selectableRobots = new List<RobotSelectionUIElement>();
        for (int i = 0; i < availableBots.Count; ++i)
        {
            GameObject selectionObject = Instantiate(selectionRobotPrefab, selectionListRoot.transform);
            selectionObject.SetActive(true);
            selectionObject.GetComponent<RobotSelectionUIElement>().Setup(availableBots[i], OnRobotPressed);
            selectableRobots.Add(selectionObject.GetComponent<RobotSelectionUIElement>());
        }
    }

    private RobotSelectionUIElement GetRobotSelectionUIElementByRobotId(int id)
    {
        return selectableRobots.Find(x => x.GetId() == id);
    }

    private void OnRobotPressed(int id)
    {
        if (selectedRobots.Count == 3)
        {
            return;
        }
        selectedRobots.Add(id);
        GetRobotSelectionUIElementByRobotId(id).DisableButton();
        PopulatePlayerSelectedRobots();
    }

    private void OnRobotDeselected(int id)
    {
        selectedRobots.Remove(id);
        GetRobotSelectionUIElementByRobotId(id).EnableButton();
        PopulatePlayerSelectedRobots();
    }

    private void PopulatePlayerSelectedRobots()
    {
        for (int i = 0; i < playerRobotsImages.Count; ++i)
        {
            playerRobotsImages[i].HideImage();
        }

        for (int i = 0; i < selectedRobots.Count; ++i)
        {
            playerRobotsImages[i].ShowImage(GetDataForId(selectedRobots[i]), OnRobotDeselected);
        }

        startButton.interactable = selectedRobots.Count == 3;
    }

    private BubbleBots.Data.BubbleBotData GetDataForId(int id)
    {
        return availableBots.Find(x => x.id == id);
    }
}
