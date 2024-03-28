using TheFriend.WorldChanges.WorldStates.General;
using UnityEngine;

namespace TheFriend.WorldChanges.Oracles.LooksToTheMoon;

public partial class SLOracle
{
    public static bool RainWorldGame_IsMoonActive(On.RainWorldGame.orig_IsMoonActive orig, RainWorldGame self)
    {
        if (QuickWorldData.SolaceCampaign && self.GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft > 0) return true;
        return orig(self);
    }
    public static bool RainWorldGame_IsMoonHeartActive(On.RainWorldGame.orig_IsMoonHeartActive orig, RainWorldGame self)
    {
        if (QuickWorldData.SolaceCampaign) return true;
        return orig(self);
    }
    public static void SLOrcacleState_ForceResetState(On.SLOrcacleState.orig_ForceResetState orig, SLOrcacleState self, SlugcatStats.Name saveStateNumber)
    { // This is the ONE moon fix that CANNOT be done with a check to QuickWorldData. It is performed TOO EARLY.
        orig(self, saveStateNumber);
        if (saveStateNumber == Plugin.DragonName || saveStateNumber == Plugin.FriendName || saveStateNumber == Plugin.NoirName)
        {
            Plugin.LogSource.LogWarning("Solace: Reset moon neuron count to 7 for solace campaign");
            self.neuronsLeft = 7;
        }
    }
    public static bool RainWorldGame_MoonHasRobe(On.RainWorldGame.orig_MoonHasRobe orig, RainWorldGame self)
    {
        if (QuickWorldData.SolaceCampaign) return true;
        return orig(self);
    }
}