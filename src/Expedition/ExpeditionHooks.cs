namespace TheFriend.Expedition;

public class ExpeditionHooks
{
    public static void Apply()
    {
        ExpeditionGeneral.Apply();
        ExpeditionBurdens.Apply();
    }
}