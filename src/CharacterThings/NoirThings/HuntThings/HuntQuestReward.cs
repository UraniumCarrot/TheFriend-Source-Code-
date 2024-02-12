using RWCustom;
using UnityEngine;

namespace TheFriend.CharacterThings.NoirThings.HuntThings;

public partial class HuntQuestThings
{
    public enum RewardPhase
    {
        Normal,
        IncreaseKarmaCap,
        SleepScreen
    }

    internal static void RainWorldGameOnWin(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished)
    {
        if (self.manager.upcomingProcess != null)
        {
            orig(self, malnourished);
            return;
        }

        var flag = false;
        if (!self.rainWorld.ExpeditionMode && self.StoryCharacter == Plugin.NoirName && Master != null)
        {
            if (Master.NextRewardPhase == RewardPhase.IncreaseKarmaCap)
            {
                self.GetStorySession.saveState.deathPersistentSaveData.karmaCap++;
            }
            flag = true;
        }
        orig(self, malnourished);
        if (flag) Master.SaveQuestProgress(); //Save after orig so malnourished is fetched properly
    }

    internal static void ProcessManagerOnRequestMainProcessSwitch_ProcessID(On.ProcessManager.orig_RequestMainProcessSwitch_ProcessID orig, ProcessManager self, ProcessManager.ProcessID id)
    {
        if (id == ProcessManager.ProcessID.MainMenu || id == ProcessManager.ProcessID.SlugcatSelect)
        {
            Master = null;
        }
        if (Master == null)
        {
            orig(self, id);
            return;
        }

        if (id == ProcessManager.ProcessID.SleepScreen && Master.NextRewardPhase == RewardPhase.IncreaseKarmaCap)
        {
            Master.NextRewardPhase = RewardPhase.SleepScreen;
            id = ProcessManager.ProcessID.GhostScreen;
        }

        orig(self, id);
    }

    internal static void KarmaLadderScreenOnSingal(On.Menu.KarmaLadderScreen.orig_Singal orig, Menu.KarmaLadderScreen self, Menu.MenuObject sender, string message)
    {
        if (self is Menu.GhostEncounterScreen && message is "CONTINUE" && Master != null && Master.NextRewardPhase == RewardPhase.SleepScreen)
        {
            Master.NextRewardPhase = RewardPhase.Normal;
            self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SleepScreen);
        }
        else
            orig(self, sender, message);
    }

    internal static void KarmaLadderOnctor(On.Menu.KarmaLadder.orig_ctor orig, Menu.KarmaLadder self, Menu.Menu menu, Menu.MenuObject owner, Vector2 pos, HUD.HUD hud, IntVector2 displaykarma, bool reinforced)
    {
        if (Master != null && Master.NextRewardPhase == RewardPhase.SleepScreen)
        {
            displaykarma.x = displaykarma.y - 1;
        }
        orig(self, menu, owner, pos, hud, displaykarma, reinforced);
    }
}