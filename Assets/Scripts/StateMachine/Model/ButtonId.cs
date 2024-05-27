using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ButtonId
{
    public const string DefaultId = "default";

    //access denied
    public const string AccessDeniedGoBack = "accessdenied.goback";
    
    //account disabled
    public const string AccountDisabledContactSupport = "accountdisabled.contactsupport";
    public const string AccountDisabledClose = "accountdisabled.close";
    
    //login
    public const string LoginSignInClose = "login.signin.close";
    public const string LoginSignInGoogle = "login.signin.google";
    public const string LoginSignInApple = "login.signin.apple";
    public const string LoginSignInMetamask = "login.signin.metamask";
    public const string LoginGoToSignUp = "login.goto.signup";
    public const string LoginSignInSubmit = "login.signin.submit";
    public const string LoginForgotPassword = "login.forgotpassword";
    public const string LoginResetPassSubmit = "login.resetpass.submit";
    public const string LoginResetPassGoBack = "login.resetpass.goback";
    public const string LoginSetNewPassSubmit = "login.setnewpass.submit";
    public const string LoginSetNewPassGoBack = "login.setnewpass.goback";
    public const string LoginSignUpSubmit = "login.signup.submit";
    public const string LoginSignUpGoBack = "login.signup.goback";
    
    public const string LoginCodeSubmit = "login.code.submit";
    public const string LoginCodeDidntReceive = "login.code.didntreceive";
    public const string LoginSetNewPassDidntReceiveCode = "login.setnewpass.didntreceivecode";

    //main menu bottom hud
    public const string MainMenuBottomHUDPlay = "mainmenu.bottomHUD.play";
    public const string MainMenuBottomHUDStore = "mainmenu.bottomHUD.store";
    public const string MainMenuBottomHUDHome = "mainmenu.bottomHUD.home";
    public const string MainMenuBottomHUDFriends = "mainmenu.bottomHUD.friends";
    public const string MainMenuBottomHUDBattlepass = "mainmenu.bottomHUD.battlepass";
    public const string MainMenuBottomHUDItems = "mainmenu.bottomHUD.items";
    public const string MainMenuBottomHUDLevels = "mainmenu.bottomHUD.levels";

    //levels map
    public const string LevelsMapPlay = "levelsmap.play";
    public const string LevelsMapBack = "levelsmap.back";

    //main menu side bar
    public const string MainMenuSideBarLeaderboard = "mainmenu.sidebar.leaderboard";
    public const string MainMenuSideBarSettings = "mainmenu.sidebar.settings";
    public const string MainMenuSideBarDashboard = "mainmenu.sidebar.dashboard";
    public const string MainMenuSideBarUsePoints = "mainmenu.sidebar.usepoints";
    public const string MainMenuSideBarMissions = "mainmenu.sidebar.missions";
    public const string MainMenuSideBarTutorial = "mainmenu.sidebar.tutorial";
    
    //main menu top hud
    public const string MainMenuTopHUDGemPlus = "mainmenu.topHUD.gems.plus";
    public const string HomeHeaderExplanator = "home.header.explanator";
    
    //options popup
    public const string OptionsChangePicture = "options.changepicture";
    public const string OptionsChangeName = "options.changename";
    public const string OptionsSave = "options.save";
    public const string OptionsSyncProgress = "options.syncprogress";
    public const string OptionsSignOut = "options.signout";
    public const string OptionsManageAccount = "options.manageaccount";
    public const string OptionsClose = "options.close";
    
    //manage account popup
    public const string ManageAccountClose = "manageaccount.close";
    public const string ManageAccountSignIn = "manageaccount.signin";
    public const string ManageAccountContact = "manageaccount.contact";
    public const string ManageAccountGoogleSignIn = "manageaccount.google.signin";
    public const string ManageAccountAppleSignIn = "manageaccount.apple.signin";
    public const string ManageAccountDelete = "manageaccount.delete";
    public const string ManageAccountSignOut = "manageaccount.signout";
    
    //delete account popup
    public const string DeleteAccountClose = "deleteaccount.close";
    public const string DeleteAccountProceed = "deleteaccount.proceed";
    
    //confirm delete account popup
    public const string DeleteAccountConfirmClose = "deleteaccountconfirm.close";
    public const string DeleteAccountConfirmProceed = "deleteaccountconfirm.proceed";
    public const string DeleteAccountConfirmNoCode = "deleteaccountconfirm.nocode";

    //leaderboard
    public const string LeaderboardFree = "leaderboard.free";
    public const string LeaderboardNether = "leaderboard.nether";
    public const string LeaderboardClose = "leaderboard.close";

    //mode select
    public const string ModeSelectCloseButton = "modeselect.close";
    public const string ModeSelectNethermode = "modeselect.nethermode";
    public const string ModeSelectFreeMode = "modeselect.freemode";
    public const string ModeSelectNetherModeTooltip = "modeselect.nethermode.tooltip";
    public const string ModeSelectNetherModeTooltipBack = "modeselect.nethermode.tooltip.back";
    public const string ModeSelectFreeModeTooltip = "modeselect.freemode.tooltip";
    public const string ModeSelectFreeModeTooltipBack = "modeselect.freemode.tooltip.back";

    //robot selection
    public const string RobotSelectionBackButton = "robotSelection.back";
    public const string RobotSelectionStartButton = "robotSelection.start";
    public const string RobotSelectionQuestionMark = "robotSelection.questionMark";
    public const string RobotSelectionSkinPopupClose = "robotSelection.skinPopup.close";
    
    //store
    public const string StoreTabGems = "store.tabs.gems";
    public const string StoreTabSkins = "store.tabs.skins";
    public const string StoreTabOffers = "store.tabs.offers";
    public const string StoreTabNuts = "store.tabs.nuts";
    public const string StoreClose = "store.close";
    public const string StoreSpecialOfferLeft = "store.specialoffers.left";
    public const string StoreSpecialOfferRight = "store.specialoffers.right";
    public const string StoreBuy = "store.buy";
    
    //confirm transaction
    public const string ConfirmTransactionClose = "confirmtransaction.close";
    public const string ConfirmTransactionBuy = "confirmtransaction.buy";

    //not enough gems 
    public const string NotEnoughGemsBack = "notenoughgems.back";
    public const string NotEnoughGemsBuy = "notenoughgems.buy";
    public const string NotLoggedIn = "notloggedin.login";
    public const string NotLoggedInClose = "notloggedin.close";

    //game
    public const string QuitGame = "game.quit";

    //level complete
    public const string LevelCompleteContinue = "level.complete.continue";

    //game end
    public const string GameEndGoToMainMenu = "game.end.mainmenu";

    //quit game menu
    public const string QuitGameMenuQuit = "quit.game.quit";
    public const string QuitGameMenuPlay = "quit.game.play";
    
    //change nickname popup
    public const string ChangeNicknameClose = "changenickname.close";
    public const string ChangeNicknameOk = "changenickname.ok";
    
    //explanator popup
    public const string ExplanatorPopupClose = "explanator.close";
    
    //battlepass popup
    public const string BattlepassPopupClose = "battlepass.close";
    public const string BattlepassPopupBuy = "battlepass.buy";
    public const string BattlepassPopupJoin = "battlepass.join";
    
    //save progress popup
    public const string SaveYourProgressPopupClose = "saveyourprogress.close";
    public const string SaveYourProgressPopupSave = "saveyourprogress.save";
    
    //sign in to buy
    public const string SignInToBuyPopupClose = "signintobuy.close";
    public const string SignInToBuyPopupSignIn = "signintobuy.signin";


    //not referred
    public const string NotReferredClose = "notreferred.close";
    public const string NotReferredGetReferral = "notreferred.getreferral";
}

public static class TypeUtilities
{
    public static List<T> GetAllPublicConstantValues<T>(this Type type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
            .Select(x => (T)x.GetRawConstantValue())
            .ToList();
    }
}

