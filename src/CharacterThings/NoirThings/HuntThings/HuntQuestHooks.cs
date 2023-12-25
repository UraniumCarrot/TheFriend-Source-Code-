using System.Linq;

namespace TheFriend.CharacterThings.NoirThings.HuntThings;

public partial class HuntQuestThings
{
    public static void Apply()
    {
        On.HUD.HUD.InitSinglePlayerHud += HUDOnInitSinglePlayerHud;
        On.Menu.KarmaLadder.ctor += KarmaLadderOnctor;
        On.Menu.KarmaLadderScreen.Singal += KarmaLadderScreenOnSingal;

        On.StoryGameSession.ctor += StoryGameSessionOnctor;
        On.RainWorldGame.Win += RainWorldGameOnWin;
        On.ProcessManager.RequestMainProcessSwitch_ProcessID += ProcessManagerOnRequestMainProcessSwitch_ProcessID;
        On.PlayerSessionRecord.AddEat += PlayerSessionRecordOnAddEat;
        On.Creature.Die += CreatureOnDie;
    }

    private static void CreatureOnDie(On.Creature.orig_Die orig, Creature self)
    {
        if (self.killTag == null)
        {
            if (self.grabbedBy.FirstOrDefault(x => x.grabber is Player)?.grabber is Player player)
                self.SetKillTag(player.abstractCreature);
        }
        orig(self);
    }

    private static void StoryGameSessionOnctor(On.StoryGameSession.orig_ctor orig, StoryGameSession self, SlugcatStats.Name savestatenumber, RainWorldGame game)
    {
        orig(self, savestatenumber, game);
        if (!game.rainWorld.ExpeditionMode && savestatenumber == Plugin.NoirName)
        {
            Master ??= new HuntQuestMaster();
            Master.StorySession = self;
            Master.LoadOrCreateQuests();
        }
    }

    private static void PlayerSessionRecordOnAddEat(On.PlayerSessionRecord.orig_AddEat orig, PlayerSessionRecord self, PhysicalObject eatenobject)
    {
        Master?.AddEat(eatenobject);
        orig(self, eatenobject);
    }
}