namespace TheFriend.CharacterThings.DelugeThings;

public class DelugeHooks
{
    public static void Apply()
    {
        DelugeGameplay.Apply();
        DelugeGraphics.Apply();
    }
}