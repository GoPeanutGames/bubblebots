using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

public static class ButtonId
{
    public const string DefaultId = "default";

    //login
    public const string LoginGuest = "login.guest";
    public const string LoginGuestPlay = "login.guest.play";
    public const string LoginMetamask = "login.metamask";


    //main menu bottom hud
    public const string MainMenuBottomHUDPlay = "mainmenu.bottomHUD.play";


    //mode select
    public const string ModeSelectBackButton = "modeselect.back";
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
