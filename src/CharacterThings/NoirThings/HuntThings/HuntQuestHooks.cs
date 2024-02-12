using System.Linq;

namespace TheFriend.CharacterThings.NoirThings.HuntThings;

public partial class HuntQuestThings
{
    internal static void CreatureOnDie(Creature self)
    {
        if (self.killTag == null)
        {
            if (self.grabbedBy.FirstOrDefault(x => x.grabber is Player)?.grabber is Player player)
                self.SetKillTag(player.abstractCreature);
        }
    }

    internal static void StoryGameSessionOnctor(StoryGameSession self, SlugcatStats.Name savestatenumber, RainWorldGame game)
    {
        if (!game.rainWorld.ExpeditionMode && savestatenumber == Plugin.NoirName)
        {
            Master ??= new HuntQuestMaster();
            Master.StorySession = self;
            Master.LoadOrCreateQuests();
        }
    }

    internal static void PlayerSessionRecordOnAddEat(On.PlayerSessionRecord.orig_AddEat orig, PlayerSessionRecord self, PhysicalObject eatenobject)
    {
        Master?.AddEat(eatenobject);
        orig(self, eatenobject);
    }
}