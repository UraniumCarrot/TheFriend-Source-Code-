using HUD;
using TheFriend.SlugcatThings;

namespace TheFriend.HudThings;

public class HudHooks
{
    public static void Apply()
    {
        On.Menu.SleepAndDeathScreen.GetDataFromGame += SleepAndDeathScreen_GetDataFromGame;
        On.HUD.RainMeter.Draw += RainMeter_Draw;
        On.HUD.RainMeter.ctor += RainMeter_ctor;
        On.HUD.RainMeter.Update += RainMeter_Update;
    }
    public static readonly SlugcatStats.Name FriendName = Plugin.FriendName;
    public static readonly SlugcatStats.Name DragonName = Plugin.DragonName;

    public static void SleepAndDeathScreen_GetDataFromGame(On.Menu.SleepAndDeathScreen.orig_GetDataFromGame orig, Menu.SleepAndDeathScreen self, Menu.KarmaLadderScreen.SleepDeathScreenDataPackage package)
    { // Improved sleep screen
        orig(self, package);
        if (self.IsSleepScreen && (package.characterStats.name == FriendName || package.characterStats.name == DragonName))
        {
            if (self.soundLoop != null) self.soundLoop.Destroy();
            self.mySoundLoopID = MoreSlugcats.MoreSlugcatsEnums.MSCSoundID.Sleep_Blizzard_Loop;
        }
    } 
    public static void RainMeter_Update(On.HUD.RainMeter.orig_Update orig, RainMeter self)
    { // Makes solace rain timer function like Saint's
        orig(self);
        if ( self.hud.owner is Player pl && 
             (pl.slugcatStats.name == FriendName ||
             pl.slugcatStats.name == DragonName) &&
            self.hud.map.RegionName != "HR") self.halfTimeShown = true;
    } 
    public static void RainMeter_ctor(On.HUD.RainMeter.orig_ctor orig, RainMeter self, HUD.HUD hud, FContainer fContainer)
    { // Makes solace rain timer function like Saint's
        orig(self, hud, fContainer);
        if ( self.hud.owner is Player pl &&
             (pl.slugcatStats.name == FriendName ||
            pl.slugcatStats.name == DragonName) &&
            self.hud.map.RegionName != "HR") self.halfTimeShown = true;
    } 
    public static void RainMeter_Draw(On.HUD.RainMeter.orig_Draw orig, RainMeter self, float timeStacker)
    { // Makes rain timer visible or not
        orig(self, timeStacker);
        var owner = self.hud.owner;
        if (owner is Player pl && pl.slugcatStats.name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint) // if showing timer is true
        {
            for (int i = 0; i < self.circles.Length; i++)
            {
                if (Plugin.ShowCycleTimer()) { self.circles[i].Draw(timeStacker);
                    (owner as Player).GetPoacher().RainTimerExists = true; }
                else self.circles[i].sprite.scale = 0;
            }
        }
        if (owner is Player pla && 
            (pla.slugcatStats.name == FriendName || 
             pla.slugcatStats.name == DragonName) &&
            self.hud.map.RegionName != "HR" && !Plugin.ShowCycleTimer()) // if showing timer is false
        {
            pla.GetPoacher().RainTimerExists = false;
            for (int i = 0; i < self.circles.Length; i++)
            {
                self.circles[i].rad = 0;
                self.circles[i].lastRad = 0;
                self.circles[i].fade = 0;
                self.circles[i].lastFade = 0;
                self.circles[i].sprite.scale = 0;
            }
        }
        else (owner as Player).GetPoacher().RainTimerExists = true;
    } 
}
