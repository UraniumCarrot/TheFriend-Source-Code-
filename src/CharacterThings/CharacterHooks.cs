using TheFriend.CharacterThings.BelieverThings;
using TheFriend.CharacterThings.DelugeThings;
using TheFriend.CharacterThings.FriendThings;
using TheFriend.CharacterThings.NoirThings;
using TheFriend.CharacterThings.NoirThings.HuntThings;
using TheFriend.PoacherThings;
using TheFriend.SlugcatThings;

namespace TheFriend.CharacterThings;

public class CharacterHooks
{
    public static void Apply()
    {
        PoacherHooks.Apply();
        FriendHooks.Apply();
        NoirCatto.Apply();
        HuntQuestThings.Apply();
        BelieverHooks.Apply();
        DelugeHooks.Apply();
        
        SlugcatGameplay.Apply();
        SlugcatGraphics.Apply();
        SensoryHolograms.Apply();
    }
}