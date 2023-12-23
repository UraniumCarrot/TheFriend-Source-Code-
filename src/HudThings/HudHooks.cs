using System.Collections.Generic;
using System.Linq;
using HUD;
using Menu;
using On.MoreSlugcats;
using SlugBase.SaveData;
using TheFriend.SlugcatThings;
using TheFriend.WorldChanges;
using UnityEngine;

namespace TheFriend.HudThings;

public class HudHooks
{
    public static void Apply()
    {
        On.Menu.SleepAndDeathScreen.GetDataFromGame += SleepAndDeathScreen_GetDataFromGame;
        On.HUD.RainMeter.Draw += RainMeter_Draw;
        On.HUD.RainMeter.ctor += RainMeter_ctor;
        On.HUD.RainMeter.Update += RainMeter_Update;
        
        On.HUD.HUD.InitSinglePlayerHud += HUDOnInitSinglePlayerHud;
    }

    public static void HUDOnInitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig(self, cam);
        if (Plugin.LizRep() && 
            ((self.owner as Player)?.room.world.game.StoryCharacter == Plugin.FriendName || 
             (self.owner as Player)?.room.world.game.StoryCharacter == Plugin.DragonName || 
             Plugin.LizRepAll())) 
            self.AddPart(new LizardRepHud.LizardUI(self, self.fContainers[1], self.owner as Player));
    }

    public static readonly SlugcatStats.Name FriendName = Plugin.FriendName;
    public static readonly SlugcatStats.Name DragonName = Plugin.DragonName;
    
    public static void SleepAndDeathScreen_GetDataFromGame(On.Menu.SleepAndDeathScreen.orig_GetDataFromGame orig, Menu.SleepAndDeathScreen self, Menu.KarmaLadderScreen.SleepDeathScreenDataPackage package)
    { // Improved sleep screen
        orig(self, package);
        if (self.IsSleepScreen || self.IsDeathScreen || self.IsStarveScreen)
        {
            if ((package.characterStats.name == FriendName || package.characterStats.name == DragonName || package.characterStats.name == Plugin.NoirName) && self.IsSleepScreen)
            {
                if (self.soundLoop != null) self.soundLoop.Destroy();
                self.mySoundLoopID = MoreSlugcats.MoreSlugcatsEnums.MSCSoundID.Sleep_Blizzard_Loop;
            }

            if (package.saveState.miscWorldSaveData.GetSlugBaseData().TryGet("MothersKilledInRegionStr", out List<string> killedInRegion) && killedInRegion.Any())
            {
                if (self.pages[0].subObjects.FirstOrDefault(i => i is MoreSlugcats.CollectiblesTracker) is not MoreSlugcats.CollectiblesTracker tracker) return;
                self.pages[0].subObjects.Add(new MotherKillTracker(
                    self, 
                    self.pages[0],
                    new Vector2(self.manager.rainWorld.options.ScreenSize.x - 50f + (1366f - self.manager.rainWorld.options.ScreenSize.x) / 2f, 
                        self.manager.rainWorld.options.ScreenSize.y - 15f),
                    package.saveState,
                    self.container, 
                    tracker));
            }
        }
    } 
    public static void RainMeter_Update(On.HUD.RainMeter.orig_Update orig, RainMeter self)
    { // Makes solace rain timer function like Saint's
        orig(self);
        if (self.hud.owner is Player && 
             FriendWorldState.SolaceWorldstate &&
            self.hud.map.RegionName != "HR") self.halfTimeShown = true;
    } 
    public static void RainMeter_ctor(On.HUD.RainMeter.orig_ctor orig, RainMeter self, HUD.HUD hud, FContainer fContainer)
    { // Makes solace rain timer function like Saint's
        orig(self, hud, fContainer);
        if (self.hud.owner is Player &&
             FriendWorldState.SolaceWorldstate &&
            self.hud.map.RegionName != "HR") self.halfTimeShown = true;
    } 
    public static void RainMeter_Draw(On.HUD.RainMeter.orig_Draw orig, RainMeter self, float timeStacker)
    { // Makes rain timer visible or not
        orig(self, timeStacker);
        var owner = self.hud.owner as Player;

        if (!Plugin.ShowCycleTimer())
        {
            if (self.hud.map.RegionName != "HR")
            {
                if (FriendWorldState.SolaceWorldstate)
                {
                    owner.GetGeneral().RainTimerExists = false;
                    for (int i = 0; i < self.circles.Length; i++)
                    {
                        self.circles[i].rad = 0;
                        self.circles[i].lastRad = 0;
                        self.circles[i].fade = 0;
                        self.circles[i].lastFade = 0;
                        self.circles[i].sprite.scale = 0;
                    }
                }
                else if (owner.slugcatStats.name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)
                    owner.GetGeneral().RainTimerExists = false;
                else owner.GetGeneral().RainTimerExists = true;
            }
        }
        else
        {
            if (owner.slugcatStats.name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                for (int i = 0; i < self.circles.Length; i++)
                    self.circles[i].Draw(timeStacker);
            }
            owner.GetGeneral().RainTimerExists = true;
        }
        if (self.hud.map.RegionName == "HR") 
            owner.GetGeneral().RainTimerExists = true;
    } 
}
