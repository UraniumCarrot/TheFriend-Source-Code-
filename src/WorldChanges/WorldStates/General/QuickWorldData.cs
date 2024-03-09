using Expedition;

namespace TheFriend.WorldChanges.WorldStates.General;

public static class QuickWorldData
{
    public static bool SolaceCampaign => RWCustom.Custom.rainWorld.GetSessionData(out var data) && data.TrueSolaceCampaign;
    
    public static bool NaturalFamines => RWCustom.Custom.rainWorld.GetSessionData(out var data) && data.NaturalFamines;
    public static bool UnnaturalFamines => RWCustom.Custom.rainWorld.GetSessionData(out var data) && data.UnnaturalFamines;
    public static bool FaminesExist => NaturalFamines || UnnaturalFamines || GuaranteedFamined;
    public static bool GuaranteedFamined => RWCustom.Custom.rainWorld.ExpeditionMode &&
                                            ExpeditionGame.activeUnlocks.Contains(Expedition.ExpeditionBurdens.famine);
    
    public static bool HasMark => RWCustom.Custom.rainWorld.GetSessionData(out var data) && data.self.saveState.deathPersistentSaveData.theMark;
    public static bool HasRobo => RWCustom.Custom.rainWorld.GetSessionData(out var data) && data.self.saveState.hasRobo;

    public static int CycleNumber()
    {
        var _ = RWCustom.Custom.rainWorld.GetSessionData(out var data);
        return data.self.saveState.cycleNumber;
    }

    public static void RainWorldGameOnctor(RainWorldGame self, ProcessManager manager)
    {
        self.GetSessionData(out _);
    }
}