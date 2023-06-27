using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using TheFriend.SlugcatThings;

namespace TheFriend.WorldChanges;

public class SLOracleHandler
{
    public static void Apply()
    {
        On.SLOracleBehavior.UnconciousUpdate += SLOracleBehavior_UnconciousUpdate;
        On.SLOrcacleState.ForceResetState += SLOrcacleState_ForceResetState;
        On.SLOracleBehavior.Update += SLOracleBehavior_Update;
        On.SLOracleBehavior.Move += SLOracleBehavior_Move;
        On.SLOracleBehaviorNoMark.Update += SLOracleBehaviorNoMark_Update;
        //On.OracleGraphics.Update += OracleGraphics_Update;
        On.RainWorldGame.IsMoonActive += RainWorldGame_IsMoonActive;
        On.RainWorldGame.MoonHasRobe += RainWorldGame_MoonHasRobe;
        On.RainWorldGame.IsMoonHeartActive += RainWorldGame_IsMoonHeartActive;
    }

    // Simple Moon fixes
    public static void SLOracleBehavior_UnconciousUpdate(On.SLOracleBehavior.orig_UnconciousUpdate orig, SLOracleBehavior self)
    {
        orig(self);
        if (self.oracle.room.game.IsStorySession && FriendWorldState.SolaceWorldstate)
        {
            self.oracle.SetLocalGravity(1f);
            if (self.oracle.room.world.rainCycle.brokenAntiGrav.on)
            {
                self.oracle.room.world.rainCycle.brokenAntiGrav.counter = -1;
                self.oracle.room.world.rainCycle.brokenAntiGrav.to = 0f;
            }
            self.oracle.arm.isActive = false;
            self.moonActive = false;
        }
    }
    public static bool RainWorldGame_IsMoonActive(On.RainWorldGame.orig_IsMoonActive orig, RainWorldGame self)
    {
        orig(self);
        if (FriendWorldState.SolaceWorldstate && self.GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft > 0) return true;
        return orig(self);
    }
    public static bool RainWorldGame_IsMoonHeartActive(On.RainWorldGame.orig_IsMoonHeartActive orig, RainWorldGame self)
    {
        if (FriendWorldState.SolaceWorldstate) return true;
        return orig(self);
    }
    public static void SLOrcacleState_ForceResetState(On.SLOrcacleState.orig_ForceResetState orig, SLOrcacleState self, SlugcatStats.Name saveStateNumber)
    {
        orig(self, saveStateNumber);
        if (FriendWorldState.SolaceWorldstate) self.neuronsLeft = 7;
    }
    public static bool RainWorldGame_MoonHasRobe(On.RainWorldGame.orig_MoonHasRobe orig, RainWorldGame self)
    {
        if (FriendWorldState.SolaceWorldstate) return true;
        return orig(self);
    }

    // Moon behaviors
    public static int counter = 0;
    public static bool SLHasMarkCutsceneHappened;
    public static bool SLGaveMarkCutsceneHappened;
    public static bool IsShowingMedia;
    public static void SLOracleBehavior_Update(On.SLOracleBehavior.orig_Update orig, SLOracleBehavior self, bool eu)
    {
        try 
        {
            orig(self, eu);
        }
        catch (Exception e) { Debug.Log("Solace: Exception occured in SLOracleBehaviorUpdate orig" + e); }

        try 
        {
            if (FriendWorldState.SolaceWorldstate)
            {
                if (!(self?.oracle?.room?.game?.session as StoryGameSession).saveState.deathPersistentSaveData.theMark && counter <= 0) SLHasMarkCutsceneHappened = false;
                if (self.hasNoticedPlayer && !SLHasMarkCutsceneHappened) { counter = 1800; SLHasMarkCutsceneHappened = true; }
                if (counter > 0)
                {
                    counter--;
                    if (counter > 1) self.forceFlightMode = true;
                    self.oracle.SetLocalGravity(Mathf.Lerp(self.oracle.gravity, 1f, 0.2f));
                    self.floatyMovement = true;
                    if (counter > 1300) self.setMovementBehavior(SLOracleBehavior.MovementBehavior.ShowMedia);
                    if (counter <= 1300 && counter >= 300) 
                    { 
                        self.setMovementBehavior(SLOracleBehavior.MovementBehavior.KeepDistance);
                    }
                    if (counter > 300 && counter < 1300) 
                        self.lookPoint = ((self.showMediaPos.x <= self.oracle.room.PixelWidth * 0.85f) ? 
                            new Vector2(self.showMediaPos.x + 100f, self.showMediaPos.y + 150f) : 
                            new Vector2(self.showMediaPos.x - 100f, self.showMediaPos.y + 150f));
                    if (counter <= 300 && counter > 1) 
                    { 
                        self.setMovementBehavior(SLOracleBehavior.MovementBehavior.Meditate);
                        if (Random.value > 0.5f && counter < 240) self.player.firstChunk.vel.y += 0.3f;
                    }
                }

                if (counter <= 0)
                {
                    self.forceFlightMode = false;
                }

                switch (counter)
                {
                    /*case 1301:
                        IsShowingMedia = true;
                        self.displayImage = self.oracle.myScreen.AddImage("AIimg1_DM");
                        self.displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
                        self.displayImageTimer = 200;
                        Debug.Log("Solace: Moon mark cutscene phase -1 passed");
                        break;*/

                    case 1300:
                        IsShowingMedia = true;
                        self.oracle.room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                        self.displayImage = self.oracle.myScreen.AddImage("AIimg1_DM");
                        self.displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
                        self.displayImageTimer = 200;
                        Debug.Log("Solace: Moon mark cutscene phase 0 passed");
                        break;

                    case 1100:
                        self.oracle.room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                        self.displayImage = self.oracle.myScreen.AddImage("AIimg2_RIVEND");
                        self.displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
                        self.displayImageTimer = 200;
                        Debug.Log("Solace: Moon mark cutscene phase 1 passed");
                        break;

                    case 900:
                        self.oracle.room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                        self.displayImage = self.oracle.myScreen.AddImage("AIimg1");
                        self.displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
                        self.displayImageTimer = 200;
                        Debug.Log("Solace: Moon mark cutscene phase 2 passed");
                        break;

                    case 700:
                        self.oracle.room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                        self.displayImage = self.oracle.myScreen.AddImage("AIimg2");
                        self.displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
                        self.displayImageTimer = 200;
                        Debug.Log("Solace: Moon mark cutscene phase 3 passed");
                        break;

                    case 500:
                        self.oracle.room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                        self.displayImage = self.oracle.myScreen.AddImage("AIimg3");
                        self.displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
                        self.displayImageTimer = 200;
                        Debug.Log("Solace: Moon mark cutscene phase 4 passed");
                        break;

                    case 300:
                        IsShowingMedia = false;
                        Debug.Log("Solace: Moon mark cutscene phase 5 passed");
                        break;

                    case 240:
                        self.oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Telekenisis, self.player.firstChunk.pos, 1f, 1f);
                        self.player.Stun(200);
                        self.player.firstChunk.vel.x += 0.02f;
                        Debug.Log("Solace: Moon mark cutscene phase 6 passed");
                        break;

                    case < 100 and > 1:
                        self.oracle.spasms = 10;
                        break;

                    case 1:
                        (self.oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark = true;
                        self.Pain();
                        self.oracle.spasms = 44;
                        self.oracle.stun = Math.Max(self.oracle.stun, 183);
                        self.player.GetPoacher().JustGotMoonMark = true;
                        self.oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.player.firstChunk.pos, 1f, 1f);
                        self.oracle.room.AddObject(new Spark(self.player.mainBodyChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                        Debug.Log("Solace: Moon mark cutscene phase final passed");
                        break;
                }
            }
        }
        catch(Exception e) { Debug.Log("Solace: Exception occured in SLOracleBehaviorUpdate custom" + e); }
    }
    public static void SLOracleBehaviorNoMark_Update(On.SLOracleBehaviorNoMark.orig_Update orig, SLOracleBehaviorNoMark self, bool eu)
    {
        orig(self,eu);
        if (FriendWorldState.SolaceWorldstate && counter > 300 && counter < 1300)
        {
            self.lookPoint = ((self.showMediaPos.x <= self.oracle.room.PixelWidth * 0.85f) ?
                            new Vector2(self.showMediaPos.x + 100f, self.showMediaPos.y + 150f) :
                            new Vector2(self.showMediaPos.x - 100f, self.showMediaPos.y + 150f));
        }
    }

    public static void SLOracleBehavior_Move(On.SLOracleBehavior.orig_Move orig, SLOracleBehavior self)
    {
        orig(self);
        if (FriendWorldState.SolaceWorldstate && counter > 300 && counter < 1300)
        {
            self.lookPoint = ((self.showMediaPos.x <= self.oracle.room.PixelWidth * 0.85f) ?
                            new Vector2(self.showMediaPos.x + 100f, self.showMediaPos.y + 150f) :
                            new Vector2(self.showMediaPos.x - 100f, self.showMediaPos.y + 150f));
        }
    }

    /*public static void OracleGraphics_Update(On.OracleGraphics.orig_Update orig, OracleGraphics self)
    {
        orig(self);
        if (self?.oracle?.room?.game?.IsStorySession == true && FriendWorldState.SolaceWorldstate && IsShowingMedia && self.IsMoon)
        {
            self.lookDir *= new Vector2(1.4f,0f);
        }
    }

    public class SLOracleFriendBehaviors
    {
        public static SLOracleBehavior.MovementBehavior GiveMark = new SLOracleBehavior.MovementBehavior("GiveMark", register: true);
    }
    public static void SLOracleBehavior_Update(On.SLOracleBehavior.orig_Update orig, SLOracleBehavior self, bool eu)
    {
        if (self.hasNoticedPlayer && self.moonActive && self.player.slugcatStats.name == Plugin.FriendName)
        {
            self.movementBehavior = SLOracleFriendBehaviors.GiveMark;
        }
        if (self.movementBehavior == SLOracleFriendBehaviors.GiveMark)
        {
            self.movementBehavior = SLOracleBehavior.MovementBehavior.ShowMedia;
        } // Only used for Friend
        else orig(self, eu);
    }*/


    // Moon dialogue
}
