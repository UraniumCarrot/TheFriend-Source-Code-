namespace TheFriend.WorldChanges.WorldStates.General;

public static class QuickWorldData
{
    public static bool SolaceCampaign => RWCustom.Custom.rainWorld.GetSessionData(out var data) && data.TrueSolaceCampaign;
    public static bool ExpeditionGame => RWCustom.Custom.rainWorld.ExpeditionMode;

    public static bool HasMark => RWCustom.Custom.rainWorld.GetSessionData(out var data) && data.self.saveState.deathPersistentSaveData.theMark;
    public static bool HasRobo => RWCustom.Custom.rainWorld.GetSessionData(out var data) && data.self.saveState.hasRobo;

    public static int CycleNumber()
    {
        var _ = RWCustom.Custom.rainWorld.GetSessionData(out var data);
        return data.self.saveState.cycleNumber;
    }
}