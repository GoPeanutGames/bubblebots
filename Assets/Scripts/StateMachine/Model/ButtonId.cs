using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ButtonId
{
    public const string DefaultId = "default";

    //login
    public const string LoginGuest = "login.guest";
    public const string LoginGuestPlay = "login.guest.play";
    public const string LoginMobileDownload = "login.mobile.download";
    public const string LoginGoogle = "login.google";
    public const string LoginEmailPass = "login.emailpass";
    public const string LoginEmailPassSignUp = "login.emailpass.signup";
    public const string LoginEmailPassLogin = "login.emailpass.login";
    public const string LoginEmailPassLoginSubmit = "login.emailpass.login.submit";
    public const string LoginEmailPassSignUpSubmit = "login.emailpass.signup.submit";
    public const string LoginEmailPassSignUpLogin2ndStep = "login.emailpass.signuplogin.2nd.submit";

    //main menu bottom hud
    public const string MainMenuBottomHUDPlay = "mainmenu.bottomHUD.play";
    public const string MainMenuBottomHUDStore = "mainmenu.bottomHUD.store";
    public const string MainMenuBottomHUDHome = "mainmenu.bottomHUD.home";
    public const string MainMenuBottomHUDFriends = "mainmenu.bottomHUD.friends";
    public const string MainMenuBottomHUDNetherpass = "mainmenu.bottomHUD.netherpass";
    public const string MainMenuBottomHUDItems = "mainmenu.bottomHUD.items";
    
    //main menu top hud
    public const string MainMenuTopHUDChangeNickname = "mainmenu.topHUD.changenickname";
    public const string MainMenuTopHUDGemPlus = "mainmenu.topHUD.gems.plus";
    public const string MainMenuTopHUDPremint = "mainmenu.topHUD.premint";
    public const string MainMenuTopHUDLeaderboard = "mainmenu.topHUD.leaderboard";

    //leaderboard
    public const string LeaderboardFree = "leaderboard.free";
    public const string LeaderboardNether = "leaderboard.nether";
    public const string LeaderboardClose = "leaderboard.close";
    
    //coming soon
    public const string ComingSoonGenericClose = "comingsoon.generic.close";
    public const string ComingSoonNetherpassClose = "comingsoon.netherpass.close";
    public const string ComingSoonItemsClose = "comingsoon.items.close";
    
    //mode select
    public const string ModeSelectBackButton = "modeselect.back";
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

    //game
    public const string QuitGame = "game.quit";

    //level complete
    public const string LevelCompleteContinue = "level.complete.continue";

    //game end
    public const string GameEndPremint = "game.end.premint";
    public const string GameEndGoToMainMenu = "game.end.mainmenu";

    //quit game menu
    public const string QuitGameMenuQuit = "quit.game.quit";
    public const string QuitGameMenuPlay = "quit.game.play";
    
    //change nickname popup
    public const string ChangeNicknameClose = "changenickname.close";
    public const string ChangeNicknameOk = "changenickname.ok";
    
    //premint popup
    public const string PremintOk = "premint.ok";
    public const string PremintClose = "premint.close";
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
