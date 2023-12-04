using System.Runtime.CompilerServices;
using Color = UnityEngine.Color;
using UnityEngine;

namespace TheFriend.FriendThings;

public static class FriendCWT
{
    public class Friend
    {
        public bool poleCrawlState;
        public bool longjump;
        public bool polejump;
        public bool upwardpolejump;
        public bool DoingAPoleJump;
        public bool LetGoOfPoleJump;
        public bool YesIAmLookingUpStopThinkingOtherwise;
        public bool WantsUp;
        public int poleSuperJumpTimer;
        public bool HighJumped;
        public int CrawlTurnCounter;
        public int AfterCrawlTurnCounter;
        public Player.AnimationIndex LastAnimation;
        public Overseer Iggy; // Yellow / 1
        public Overseer Wiggy; // Blue / 0
        public bool TriedSpawningOverseerInThisRoom;
        public Friend(Player player)
        {
            
        }
    }
    public static readonly ConditionalWeakTable<Player, Friend> CWT = new();
    public static Friend GetFriend(this Player player) => CWT.GetValue(player, _ => new(player));

    public static bool TryGetFriend(this Player player, out Friend data)
    {
        if (player.SlugCatClass == Plugin.FriendName)
        {
            data = player.GetFriend();
            return true;
        }
        data = null;
        return false;
    }
    
}