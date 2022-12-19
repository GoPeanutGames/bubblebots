using BubbleBots.Server.Player;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameStateFreeMode : GameState
{
    private GameScreenRobotSelection gameScreenRobotSelection;
    private GameScreenGame gameScreenGame;
    private GameScreenLevelComplete gameScreenLevelComplete;
    private GameScreenGameEnd gameScreenGameEnd;
    private GameScreenQuitToMainMenu gameScreenQuitToMainMenu;

    private FreeToPlayGameplayManager freeToPlayGameplayManager;

    [DllImport("__Internal")]
    private static extern void Premint();

    public override string GetGameStateName()
    {
        return "game state free mode";
    }

    public override void Enable()
    {
        //SceneManager.LoadScene("FreeToPlayMode");
        gameScreenRobotSelection = Screens.Instance.PushScreen<GameScreenRobotSelection>();
        gameScreenRobotSelection.PopulateSelectionList();
        Screens.Instance.SetBackground(GameSettingsManager.Instance.freeModeGameplayData.backgroundSprite);
        Screens.Instance.HideGameBackground();
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        SoundManager.Instance?.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayRobotSelectMusicNew();
            SoundManager.Instance.FadeInMusic();
        });
        UserManager.CallbackWithResources += ResourcesReceived;
    }

    private void OnGameEvent(GameEventData data)
    {
        if (data.eventName == GameEvents.ButtonTap)
        {
            OnButtonTap(data);
        }
        else if (data.eventName == GameEvents.FreeModeLevelComplete)
        {
            gameScreenLevelComplete = Screens.Instance.PushScreen<GameScreenLevelComplete>();
            gameScreenLevelComplete.SetMessage("You earned " + (data as GameEventLevelComplete).numBubblesWon.ToString() + " bubbles!");
            gameScreenLevelComplete.SetButtonText("Continue");
        }
        else if (data.eventName == GameEvents.FreeModeLose)
        {
            gameScreenGameEnd = Screens.Instance.PushScreen<GameScreenGameEnd>();
            gameScreenGameEnd.SetMessage("You earned " + (data as GameEventFreeModeLose).numBubblesWon.ToString() + " Bubbles and lost "
                + (data as GameEventFreeModeLose).lastLevelPotentialBubbles + " Bubbles for failing to complete the last level!");
        }
        else if (data.eventName == GameEvents.UpdateSessionResponse)
        {
            if (freeToPlayGameplayManager != null)
            {
                freeToPlayGameplayManager.OnNewBubblesCount((data as GameEventUpdateSession).bubbles);
            }
        }
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString customButtonData = data as GameEventString;
        switch (customButtonData.stringData)
        {

            case ButtonId.RobotSelectionStartButton:
                StartPlay();
                break;
            case ButtonId.LevelCompleteContinue:
                freeToPlayGameplayManager.StartNextLevel();
                Screens.Instance.PopScreen(gameScreenLevelComplete);
                break;
            case ButtonId.QuitGame:
                ShowQuitGameMenu();
                break;
            case ButtonId.QuitGameMenuPlay:
                ContinuePlaying();
                break;
            case ButtonId.RobotSelectionBackButton:
            case ButtonId.GameEndGoToMainMenu:
            case ButtonId.QuitGameMenuQuit:
                GoToMainMenu();
                break;
            default:
                break;
        }
    }

    private void ContinuePlaying()
    {
        Screens.Instance.PopScreen(gameScreenQuitToMainMenu);
    }

    private void ShowQuitGameMenu()
    {
        if (freeToPlayGameplayManager.CanShowQuitPopup())
        {
            gameScreenQuitToMainMenu = Screens.Instance.PushScreen<GameScreenQuitToMainMenu>();
        }
    }

    private void PremintPressed()
    {
#if !UNITY_EDITOR
                Premint();
#endif
        GoToMainMenu();
        //        Menu.gameObject.SetActive(true);
        //        Menu.GetComponent<CanvasGroup>().DOFade(1, 0.35f);
        //        if (UserManager.PlayerType == PlayerType.Guest)
        //        {
        //            //Menu.transform.Find("PlayerLogin").gameObject.SetActive(true);
        //            SceneManager.LoadScene("Login");
        //        }
        //        else
        //        {
        //            Menu.transform.Find("PlayerInfo").gameObject.SetActive(true);
        //            Menu.DisplayHighScores();
        //            Menu.ReverseHighScoreButtons();
        //        }

        //        gameObject.SetActive(false);
    }

    private void GoToMainMenu()
    {
        if (freeToPlayGameplayManager != null)
        {
            GameObject.Destroy(freeToPlayGameplayManager.gameObject);
            freeToPlayGameplayManager = null;
        }

        if (gameScreenGame != null)
        {
            gameScreenGame.GetComponent<GUIGame>().DestroyExplosionEffects();
        }

        stateMachine.PushState(new GameStateMainMenu());
        SoundManager.Instance.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayStartMusicNew();
            SoundManager.Instance.FadeInMusic();
        });
    }

    private void StartPlay()
    {
        gameScreenGame = Screens.Instance.PushScreen<GameScreenGame>();
        freeToPlayGameplayManager = GameObject.Instantiate(GameSettingsManager.Instance.freemodeGameplayManager).GetComponent<FreeToPlayGameplayManager>();

        freeToPlayGameplayManager.gameplayData = GameSettingsManager.Instance.freeModeGameplayData;
        freeToPlayGameplayManager.enemyDamage = GameSettingsManager.Instance.freeModeEnemyDamage;
        freeToPlayGameplayManager.serverGameplayController = ServerGameplayController.Instance;

        freeToPlayGameplayManager.StartSession(gameScreenRobotSelection.GetSelectedBots());
        UserManager.CallbackWithResources += ResourcesReceived;
        UserManager.Instance.GetPlayerResources();
        Screens.Instance.SetGameBackground(GameSettingsManager.Instance.freeModeGameplayData.gamebackgroundSprite);
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        //Screens.Instance.PopScreen<GameScreenMainMenuTopHUD>();
    }

    private void ResourcesReceived(GetPlayerWallet wallet)
    {
        if (freeToPlayGameplayManager != null)
        {
            freeToPlayGameplayManager.SetCanSapwnBubbles(wallet.energy > 9);
        }
    }


    public override void Disable()
    {
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        Screens.Instance.PopScreen(gameScreenGameEnd);
        Screens.Instance.PopScreen(gameScreenGame);
        Screens.Instance.PopScreen(gameScreenQuitToMainMenu);
        Screens.Instance.PopScreen(gameScreenLevelComplete);
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        UserManager.CallbackWithResources -= ResourcesReceived;
    }
}
