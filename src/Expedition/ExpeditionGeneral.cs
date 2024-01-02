using Expedition;
using MoreSlugcats;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using On.Menu;

namespace TheFriend.Expedition;

public class ExpeditionGeneral
{
    public static void Apply()
    {
        On.Menu.UnlockDialog.TogglePerk += UnlockDialogOnTogglePerk;
    }

    public static void UnlockDialogOnTogglePerk(UnlockDialog.orig_TogglePerk orig, Menu.UnlockDialog self, string message)
    {
        var player = ExpeditionData.slugcatPlayer;
        if (message == "unl-crafting")
            if (player == Plugin.DragonName)
            {
                self.PlaySound(SoundID.MENU_Error_Ping);
                return;
            }
        if (message == "unl-backspear")
            if ((player == Plugin.DragonName && Configs.PoacherBackspear) ||
                (player == Plugin.FriendName && Configs.FriendBackspear))
            {
                self.PlaySound(SoundID.MENU_Error_Ping);
                return;
            }
        orig(self, message);
    }
}