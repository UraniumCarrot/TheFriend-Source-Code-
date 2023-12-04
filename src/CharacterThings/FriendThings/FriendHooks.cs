using TheFriend.FriendThings;

namespace TheFriend.CharacterThings.FriendThings;

public class FriendHooks
{
    public static void Apply()
    {
        FriendGameplay.Apply();
        FriendGraphics.Apply();
        FriendCrawl.Apply();
        FriendCrawlTurn.Apply();
    }
}