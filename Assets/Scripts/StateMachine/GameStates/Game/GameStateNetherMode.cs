using System.Collections.Generic;
using BubbleBots.Data;
using BubbleBots.User;
using UnityEngine;

public class GameStateNetherMode : GameState
{
    private GameScreenRobotSelection gameScreenRobotSelection;
    private GameScreenGame gameScreenGame;
    private GameScreenSkinsInfoPopup _gameScreenSkinsInfoPopup;
    
    private NetherModeGameplayManager netherModeGameplayManager;

    public override string GetGameStateName()
    {
        return "game state nether mode";
    }
    
    private List<BubbleBotData> GetAvailableBots()
    {
        List<BubbleBotData> availableBots = new List<BubbleBotData>(GameSettingsManager.Instance.netherModeGameplayData.robotsAvailable);
        List<NFTImage> nftImages = UserManager.Instance.NftManager.GetAvailableNfts();
        foreach (NFTImage nftImage in nftImages)
        {
            NFTData data = UserManager.Instance.NftManager.GetCorrectNFTFromTokenId(nftImage.tokenId);
            string faction = data.attributes.Find((trait) => trait.trait_type == "Factions").value;
            string botName = data.attributes.Find((trait) => trait.trait_type == "Bots").value;
            BubbleBotData bot = ScriptableObject.CreateInstance<BubbleBotData>();
            bot.botName = botName;
            bot.id = data.edition;
            bot.robotSprite = nftImage.sprite;
            switch (faction)
            {
                case "Guardian":
                    bot.badgeSprite = UserManager.Instance.NftManager.guardianBadge;
                    bot.frameSprite = UserManager.Instance.NftManager.guardianFrame;
                    bot.labelSprite = UserManager.Instance.NftManager.guardianLabel;
                    bot.bgSprite = UserManager.Instance.NftManager.guardianBg;
                    break;
                case "Hunter":
                    bot.badgeSprite = UserManager.Instance.NftManager.hunterBadge;
                    bot.frameSprite = UserManager.Instance.NftManager.hunterFrame;
                    bot.labelSprite = UserManager.Instance.NftManager.hunterLabel;
                    bot.bgSprite = UserManager.Instance.NftManager.hunterBg;
                    break;
                case "Builder":
                    bot.badgeSprite = UserManager.Instance.NftManager.builderBadge;
                    bot.frameSprite = UserManager.Instance.NftManager.builderFrame;
                    bot.labelSprite = UserManager.Instance.NftManager.builderLabel;
                    bot.bgSprite = UserManager.Instance.NftManager.builderBg;
                    break;
            }
            availableBots.Add(bot);
        }
        return availableBots;
    }

    public override void Enter()
    {
        gameScreenRobotSelection = Screens.Instance.PushScreen<GameScreenRobotSelection>();
        List<BubbleBotData> bots = GetAvailableBots();
        gameScreenRobotSelection.PopulateSelectionList(bots);
        SoundManager.Instance.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayRobotSelectMusic();
            SoundManager.Instance.FadeInMusic();
        });
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
    }

    private void OnGameEvent(GameEventData data)
    {
        if (data.eventName == GameEvents.ButtonTap)
        {
            OnButtonTap(data);
        }
        else if (data.eventName == GameEvents.FreeModeSessionStarted)
        {
            gameScreenGame.InitialiseEnemyRobots();
        }
        else if (data.eventName == GameEvents.FreeModeLevelStart)
        {
            GameEventLevelStart eventLevelStart = data as GameEventLevelStart;
            gameScreenGame.SetPlayerRobots(eventLevelStart.playerRoster);
            gameScreenGame.SetEnemyRobots(eventLevelStart.enemies);
        }
        else if (data.eventName == GameEvents.FreeModeEnemyRobotDamage)
        {
            GameEventEnemyRobotDamage eventEnemyRobotDamage = data as GameEventEnemyRobotDamage;
            gameScreenGame.DamageEnemyRobotAndSetHp(eventEnemyRobotDamage.index, eventEnemyRobotDamage.enemyRobotNewHp);
        }
        else if (data.eventName == GameEvents.FreeModePlayerRobotDamage)
        {
            GameEventPlayerRobotDamage eventPlayerRobotDamage = data as GameEventPlayerRobotDamage;
            gameScreenGame.DamagePlayerRobotAndSetHp(eventPlayerRobotDamage.id, eventPlayerRobotDamage.enemyIndex,eventPlayerRobotDamage.damage);
        }
        else if (data.eventName == GameEvents.FreeModePlayerRobotKilled)
        {
            GameEventPlayerRobotKilled eventPlayerRobotKilled = data as GameEventPlayerRobotKilled;
            gameScreenGame.KillPlayerRobot(eventPlayerRobotKilled.id, eventPlayerRobotKilled.enemyIndex);
        }
        else if (data.eventName == GameEvents.FreeModeEnemyRobotKilled)
        {
            GameEventEnemyRobotKilled eventEnemyRobotKilled = data as GameEventEnemyRobotKilled;
            gameScreenGame.KillEnemyRobot(eventEnemyRobotKilled.id);
        }
        else if (data.eventName == GameEvents.FreeModeEnemyRobotTargeted)
        {
            GameEventEnemyRobotTargeted eventEnemyRobotTargeted = data as GameEventEnemyRobotTargeted;
            gameScreenGame.TargetEnemyRobot(eventEnemyRobotTargeted.id);
        }
        else if (data.eventName == GameEvents.FreeModeEnemyChanged)
        {
            GameEventInt eventEnemyRobotTargeted = data as GameEventInt;
            gameScreenGame.TargetEnemyRobot(eventEnemyRobotTargeted.intData);
        }
        else if (data.eventName == GameEvents.FreeModeLevelComplete)
        {
            stateMachine.PushState(new GameStateWonPopup(
                "<color=#FFCB5E>" + (data as GameEventLevelComplete).lastLevelPotentialBubbles.ToString() + "</color> Bubbles!",
                ButtonId.LevelCompleteContinue,
                "Continue",
                ()=>netherModeGameplayManager.StartNextLevel()
                ));
        }
        else if (data.eventName == GameEvents.FreeModeLose)
        {
            SoundManager.Instance.PlayBattleLostSfx();
            stateMachine.PushState(new GameStateWonPopup(
                "<color=#FFCB5E>" + (data as GameEventFreeModeLose).numBubblesWon.ToString() + "</color> Bubbles from previous levels!",
                ButtonId.GameEndGoToMainMenu,
                "Go to home"
            ));
        }
        else if (data.eventName == GameEvents.UpdateSessionResponse)
        {
            netherModeGameplayManager.OnNewBubblesCount((data as GameEventUpdateSession).bubbles);
        } 
        else if (data.eventName == GameEvents.NetherModeComplete)
        {
            stateMachine.PushState(new GameStateWonPopup(
                "You earned <color=#FFCB5E>" + (data as GameEventInt).intData + "</color> Bubbles and extra <color=#FFCB5E>10,000</color> Bubbles for completing all levels!",
                ButtonId.GameEndGoToMainMenu,
                "Go to home"
            ));
        }
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString customButtonData = data as GameEventString;
        switch (customButtonData.stringData)
        {
            case ButtonId.RobotSelectionBackButton:
                ShowQuit();
                break;
            case ButtonId.RobotSelectionQuestionMark:
                OpenSkinPopup();
                break;
            case ButtonId.RobotSelectionSkinPopupClose:
                CloseSkinPopup();
                break;
            case ButtonId.RobotSelectionStartButton:
                StartPlay();
                break;
            case ButtonId.QuitGame:
                ShowQuitGameMenu();
                break;
        }
    }

    private void OpenSkinPopup()
    {
        _gameScreenSkinsInfoPopup = Screens.Instance.PushScreen<GameScreenSkinsInfoPopup>();
        _gameScreenSkinsInfoPopup.StartOpen();
    }

    private void CloseSkinPopup()
    {
        _gameScreenSkinsInfoPopup.StartClose();
    }
    
    private void ShowQuitGameMenu()
    {
        if (netherModeGameplayManager.CanShowQuitPopup())
        {
            ShowQuit();
        }
    }
    
    private void ShowQuit()
    {
        stateMachine.PushState(new GameStateQuitPopup("You will not get your Gem back\nif you go back to the previous menu.\nAre you sure you want to go back?"));
    }
    
    private void GoToMainMenu()
    {
        if (netherModeGameplayManager != null)
        {
            GameObject.Destroy(netherModeGameplayManager.gameObject);
            netherModeGameplayManager = null;
        }

        if (gameScreenGame != null)
        {
            gameScreenGame.GetComponent<GUIGame>().DestroyExplosionEffects();
        }

        stateMachine.PushState(new GameStateHome());
        SoundManager.Instance.FadeOutMusic();
    }

    private void StartPlay()
    {
        SoundManager.Instance.PlayBattleStartSfx();
        gameScreenGame = Screens.Instance.PushScreen<GameScreenGame>();
        gameScreenGame.SetPlayerName(UserManager.Instance.GetPlayerUserName());
        netherModeGameplayManager = GameObject.Instantiate(GameSettingsManager.Instance.netherModeGameplayManager).GetComponent<NetherModeGameplayManager>();

        netherModeGameplayManager.gameplayData = GameSettingsManager.Instance.netherModeGameplayData;
        netherModeGameplayManager.enemyDamage = GameSettingsManager.Instance.netherModeEnemyDamage;
        netherModeGameplayManager.serverGameplayController = ServerGameplayController.Instance;

        netherModeGameplayManager.StartSession(gameScreenRobotSelection.GetSelectedBots());

        Screens.Instance.PopScreen(gameScreenRobotSelection);
        Screens.Instance.PopScreen<GameScreenHomeHeader>();
        Screens.Instance.HideGameBackground();
    }

    public override void Exit()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        Screens.Instance.PopScreen(gameScreenGame);
        GoToMainMenu();
    }
}
