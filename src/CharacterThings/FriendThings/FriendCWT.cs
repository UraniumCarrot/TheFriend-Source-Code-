using System.Runtime.CompilerServices;
using MoreSlugcats;

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
        public Friend(AbstractCreature player)
        {
            
        }
    }
    public static readonly ConditionalWeakTable<AbstractCreature, Friend> CWT = new();
    public static Friend GetFriend(this Player player) => CWT.GetValue(player.abstractCreature, _ => new(player.abstractCreature));

    public static bool TryGetFriend(this AbstractCreature crit, out Friend data)
    {
        var template = crit.creatureTemplate.type;
        if (template == CreatureTemplate.Type.Slugcat || template == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            if (crit.realizedCreature is Player player && player.SlugCatClass == Plugin.FriendName)
            {
                data = player.GetFriend();
                return true;
            }
        data = null;
        return false;
    }
    public static bool TryGetFriend(this Player player, out Friend data) => player.abstractCreature.TryGetFriend(out data);
    public static bool TryGetFriend(this Creature player, out Friend data) => player.abstractCreature.TryGetFriend(out data);
}