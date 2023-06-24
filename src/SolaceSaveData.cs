using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;


namespace TheFriend;
/*
public class SolaceSaveData
{
    // Solace save data revolves around passage and OptionsInterface abuse!
    // Huge thanks to WillowWisp for coming up with the idea
    public static WinState.EndgameID SolaceWorldData = new WinState.EndgameID("SolaceWorldData", true);
    public static Menu.MenuScene.SceneID Endgame_SolaceWorldData = new Menu.MenuScene.SceneID("Endgame_SolaceWorldData", true);

    public static void Apply()
    {
        On.WinState.CycleCompleted += WinState_CycleCompleted;
        On.WinState.CreateAndAddTracker += WinState_CreateAndAddTracker;
        On.WinState.DeathModifyTracker += WinState_DeathModifyTracker;

        On.WinState.PassageDisplayName += WinState_PassageDisplayName;
        On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
        On.Menu.CustomEndGameScreen.GetDataFromSleepScreen += CustomEndGameScreen_GetDataFromSleepScreen;
    }

    #region actual passages
    public static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, Menu.MenuScene self)
    {
        orig(self);
    }
    public static string WinState_PassageDisplayName(On.WinState.orig_PassageDisplayName orig, WinState.EndgameID ID)
    {
        return orig(ID);
    }
    public static void CustomEndGameScreen_GetDataFromSleepScreen(On.Menu.CustomEndGameScreen.orig_GetDataFromSleepScreen orig, Menu.CustomEndGameScreen self, WinState.EndgameID endGameID)
    {
        orig(self, endGameID);
    }
    #endregion

    // Save data for this specific slugcat savefile, gets destroyed when the save is reset
    public static WinState.EndgameTracker WinState_CreateAndAddTracker(On.WinState.orig_CreateAndAddTracker orig, WinState.EndgameID ID, List<WinState.EndgameTracker> endgameTrackers)
    {
        WinState.EndgameTracker tracker = null;
        if (ID == SolaceWorldData) tracker = new WinState.BoolArrayTracker(SolaceWorldData, 7);
        else return orig(ID, endgameTrackers);

        if (tracker != null && endgameTrackers != null)
        {
            bool trackerIsInList = false;
            for (int i = 0; i < endgameTrackers.Count; i++)
            {
                trackerIsInList = true;
                if (endgameTrackers[i].ID == ID) { endgameTrackers[i] = tracker; break; }
            }
            if (!trackerIsInList) endgameTrackers.Add(tracker);
        }
        return tracker;
    }

    public static void WinState_CycleCompleted(On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
    {
        orig(self, game);
        WinState.BoolArrayTracker tracker = self.GetTracker(SolaceWorldData,true) as WinState.BoolArrayTracker;
        if (tracker != null)
        {

        }
    }
    public static void WinState_DeathModifyTracker(On.WinState.orig_DeathModifyTracker orig, WinState self, WinState.EndgameTracker tracker)
    { // Only some save data should be getting reset if slugcat dies
        orig(self, tracker);
    }

    public static void SetSolaceData(WinState.BoolArrayTracker self)
    {

    }
}

// Permanent save data, will be used for dummy achievement tracking and per-file events. Currently unused
public partial class Options
{
    public static Configurable<bool> SolaceFriendOEAchievement; // Live Well
    public static Configurable<bool> SolacePoacherOEAchievement; // A New Beginning
    public static Configurable<bool> SolaceFriendBadAscensionAchievement; // Betrayal
    public static Configurable<bool> SolaceFriendGoodAscensionAchievement; // Allegiance
    public static Configurable<bool> SolacePoacherBadAscensionAchievement; // Trapped
    public static Configurable<bool> SolacePoacherGoodAscensionAchievement; // Forgiveness
    public static Configurable<bool> SolacePebblesAchievement; // Alone No More
    public static Configurable<bool> SolacePebblesStolenEnlightenmentAchievement; // Broken Mind

    public void SolaceData()
    {
        SolaceFriendOEAchievement = this.config.Bind<bool>("SolaceFriendOEAchievement", SolaceFriendOEAchievement.Value, new ConfigAcceptableList<bool>(true, false));
        SolacePoacherOEAchievement = this.config.Bind<bool>("SolacePoacherOEAchievement", SolacePoacherOEAchievement.Value, new ConfigAcceptableList<bool>(true, false));
        SolaceFriendBadAscensionAchievement = this.config.Bind<bool>("SolaceFriendBadAscensionAchievement", SolaceFriendBadAscensionAchievement.Value, new ConfigAcceptableList<bool>(true, false));
        SolaceFriendGoodAscensionAchievement = this.config.Bind<bool>("SolaceFriendGoodAscensionAchievement", SolaceFriendGoodAscensionAchievement.Value, new ConfigAcceptableList<bool>(true, false));
        SolacePoacherBadAscensionAchievement = this.config.Bind<bool>("SolacePoacherBadAscensionAchievement", SolacePoacherBadAscensionAchievement.Value, new ConfigAcceptableList<bool>(true, false));
        SolacePoacherGoodAscensionAchievement = this.config.Bind<bool>("SolacePoacherGoodAscensionAchievement", SolacePoacherGoodAscensionAchievement.Value, new ConfigAcceptableList<bool>(true, false));
        SolacePebblesAchievement = this.config.Bind<bool>("SolacePebblesAchievement", SolacePebblesAchievement.Value, new ConfigAcceptableList<bool>(true, false));
        SolacePebblesStolenEnlightenmentAchievement = this.config.Bind<bool>("SolacePebblesStolenEnlightenmentAchievement", SolacePebblesStolenEnlightenmentAchievement.Value, new ConfigAcceptableList<bool>(true, false));
    }
}*/
