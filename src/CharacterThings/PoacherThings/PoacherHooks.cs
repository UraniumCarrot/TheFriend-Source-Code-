namespace TheFriend.PoacherThings;

public class PoacherHooks
{
    public static void Apply()
    {
        PoacherGameplay.Apply();
        PoacherGraphics.Apply();
        FirecrackerFix.Apply();
        DragonCrafts.Apply();
        DragonClassFeatures.Apply();
    }
}