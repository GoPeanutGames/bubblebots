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
    public const string LoginMetamask = "login.metamask";

    //main menu bottom hud
    public const string MainMenuBottomHUDPlay = "mainmenu.bottomHUD.play";
    public const string MainMenuBottomHUDStore = "mainmenu.bottomHUD.store";
    public const string MainMenuBottomHUDHome = "mainmenu.bottomHUD.home";
    
    //main menu top hud
    public const string MainMenuTopHUDChangeNickname = "mainmenu.topHUD.changenickname";
    public const string MainMenuTopHUDGemPlus = "mainmenu.topHUD.gems.plus";

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
