using System.Collections.Generic;
using BubbleBots.Data;
using BubbleBots.Server.Player;
using BubbleBots.User;
using UnityEngine;

public class GameStateLevelsMode : GameState
{
    private GameScreenRobotSelection gameScreenRobotSelection;
    private GameScreenGame gameScreenGame;
    private GameScreenSkinsInfoPopup _gameScreenSkinsInfoPopup;
    
    private LevelsGameplayManager levelsGameplayManager;

    public override string GetGameStateName()
    {
        return "game state levels mode";
    }

    private List<BubbleBotData> GetAvailableBots()
    {
        List<BubbleBotData> availableBots = new List<BubbleBotData>(GameSettingsManager.Instance.freeModeGameplayData.robotsAvailable);
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

    public override void Enable()
    {
        UserManager.CallbackWithResources += ResourcesReceived;
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
                "<color=#FFCB5E>" + (data as GameEventLevelComplete).numBubblesWon.ToString() + "</color> Points!",
                ButtonId.LevelCompleteContinue,
                "Continue",
                () => levelsGameplayManager.StartNextLevel()) //todo: probably can do this better -> refactor
            );
        }
        else if (data.eventName == GameEvents.FreeModeLose)
        {
            SoundManager.Instance.PlayBattleLostSfx();
            stateMachine.PushState(new GameStateWonPopup(
                "<color=#FFCB5E>" + (data as GameEventFreeModeLose).numBubblesWon.ToString() + "</color> Points from previous levels!",
                ButtonId.GameEndGoToMainMenu, 
                "Go to home"));
        }
        else if (data.eventName == GameEvents.UpdateSessionResponse)
        {
            if (levelsGameplayManager != null)
            {
                levelsGameplayManager.OnNewBubblesCount((data as GameEventUpdateSession).bubbles);
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
            case ButtonId.QuitGame:
                ShowQuitGameMenu();
                break;
            case ButtonId.RobotSelectionQuestionMark:
                OpenSkinPopup();
                break;
            case ButtonId.RobotSelectionSkinPopupClose:
                CloseSkinPopup();
                break;
            case ButtonId.RobotSelectionBackButton:
                ShowQuitRobotSelect();
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

    private void ShowQuitRobotSelect()
    {
        stateMachine.PopState();
    }
    
    private void ShowQuitGameMenu()
    {
        if (levelsGameplayManager.CanShowQuitPopup())
        {
            stateMachine.PushState(new GameStateQuitPopup("You will sacrifice your <color=#FFCB5E>Points</color> this round... if you exit this game. Are you sure you want to quit?"));
        }
    }

    private void GoToMainMenu()
    {
        if (levelsGameplayManager != null)
        {
            GameObject.Destroy(levelsGameplayManager.gameObject);
            levelsGameplayManager = null;
        }

        if (gameScreenGame != null)
        {
            gameScreenGame.GetComponent<GUIGame>().DestroyExplosionEffects();
        }

        if (UserManager.PlayerType == PlayerType.Guest)
        {
            UserManager.TimesPlayed++;
        }
        stateMachine.PushState(new GameStateHome(true));
        SoundManager.Instance.FadeOutMusic();
    }

    private void StartPlay()
    {
        SoundManager.Instance.PlayBattleStartSfx();
        gameScreenGame = Screens.Instance.PushScreen<GameScreenGame>();
        gameScreenGame.SetPlayerName(UserManager.Instance.GetPlayerUserName());
        levelsGameplayManager = GameObject.Instantiate(GameSettingsManager.Instance.levelsGameplayManager).GetComponent<LevelsGameplayManager>();

        levelsGameplayManager.gameplayData = GameSettingsManager.Instance.levelsGameplayData;
        levelsGameplayManager.enemyDamage = GameSettingsManager.Instance.freeModeEnemyDamage;
        levelsGameplayManager.serverGameplayController = ServerGameplayController.Instance;

        levelsGameplayManager.StartSession(gameScreenRobotSelection.GetSelectedBots());
        UserManager.Instance.GetPlayerResources();
        Screens.Instance.SetGameBackground(GameSettingsManager.Instance.freeModeGameplayData.gamebackgroundSprite);
        Screens.Instance.PopScreen(gameScreenRobotSelection);
    }

    private void ResourcesReceived(GetPlayerWallet wallet)
    {
        if (levelsGameplayManager != null)
        {
            levelsGameplayManager.SetCanSpawnBubbles(wallet.energy > 0);
        }
    }


    public override void Disable()
    {
        UserManager.CallbackWithResources -= ResourcesReceived;
    }

    public override void Exit()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        Screens.Instance.PopScreen(gameScreenGame);
        GoToMainMenu();
    }
}
