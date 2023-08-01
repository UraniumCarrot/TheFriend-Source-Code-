using System;
using System.Runtime.CompilerServices;
using MoreSlugcats;
using RWCustom;
using SlugBase.SaveData;
using UnityEngine;
using Random = UnityEngine.Random;
using Solace.SlugcatThings;
using MoreSlugcatsEnums = IL.MoreSlugcats.MoreSlugcatsEnums;

namespace Solace.WorldChanges;

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
        
        On.SLOracleBehaviorHasMark.InitateConversation += SLOracleBehaviorHasMarkOnInitateConversation;
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
    /*public static int counter = 0;
    public static bool SLHasMarkCutsceneHappened;
    public static bool SLGaveMarkCutsceneHappened;
    public static bool IsShowingMedia;*/
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
                var moondata = self.MoonCutsceneData();
                var room = self.oracle.room;
                int stage = self.MoonCutsceneData().stage;
                // Moon mark cutscene
                if (!(self?.oracle?.room?.game?.session as StoryGameSession).saveState.deathPersistentSaveData.theMark)
                {
                    var grav = room.world.rainCycle.brokenAntiGrav;
                    int counter = moondata.counter;
                    if (counter <= 0 &&
                        self.hasNoticedPlayer)
                        moondata.counter = 1800;
                    if (counter > 0)
                        moondata.counter--;
                    if (room.game.rainWorld.progression.miscProgressionData.GetSlugBaseData()
                            .TryGet("SolaceMarkCutsceneHasBeenSeen", out bool a) && a)
                    {
                        moondata.counter = 301;
                    }

                    self.forceFlightMode = true;
                    self.oracle.SetLocalGravity(Mathf.Lerp(self.oracle.gravity, 1f, 0.2f));
                    self.floatyMovement = true;
                        
                    if (counter == 1799 && grav.on)
                        room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_Off, 0f, room.game.cameras[1].room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG), 1f);
                    grav.on = false;
                    grav.counter = 10;
                    grav.progress = 0f;
                    grav.from = 0f;
                    grav.to = 0f;

                    switch (stage)
                    {
                        case 0: // Examining player
                            self.setMovementBehavior(SLOracleBehavior.MovementBehavior.ShowMedia);
                            if (Random.value > 0.92f) self.armsProtest = true;
                            else self.armsProtest = false;
                            break;
                        case 1: // Researching slugcats
                            self.setMovementBehavior(SLOracleBehavior.MovementBehavior.KeepDistance);
                            self.lookPoint = ((self.showMediaPos.x <= room.PixelWidth * 0.85f) ? 
                                new Vector2(self.showMediaPos.x + 100f, self.showMediaPos.y + 150f) : 
                                new Vector2(self.showMediaPos.x - 100f, self.showMediaPos.y + 150f));
                            break;
                        case 2: // Applying mark
                            self.setMovementBehavior(SLOracleBehavior.MovementBehavior.Meditate);
                            if (Random.value > 0.5f && counter < 240) self.player.firstChunk.vel.y += 0.3f;
                            for (int i = 0; i < self.oracle.mySwarmers.Count; i++)
                            {
                                var swarmer = self.oracle.mySwarmers[i];
                                var dist = 250f;
                                Vector2 startPos = swarmer.firstChunk.pos;
                                Vector2 endPos = swarmer.firstChunk.pos + Custom.DirVec(self.oracle.firstChunk.pos,startPos)*dist;

                                var lightning = new LightningBolt(startPos, endPos, 1, 0.5f, 0.3f, 1, 0.33f, false);
                                if (Random.value > 0.96f + counter*0.0002f)
                                {
                                    room.AddObject(lightning);
                                    room.AddObject(new Explosion.ExplosionLight(startPos, 0.4f,1f,30,new Color(0.2f,1f,0f)));
                                    room.PlaySound(SoundID.SS_Mycelia_Spark,swarmer.firstChunk.pos,3f,1.4f - Random.value * 0.4f);
                                    room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous,swarmer.firstChunk.pos,0.4f,1f);
                                    lightning.type = 0;
                                }
                            }
                            break;
                    }
                    switch (counter)
                    {
                        case 1300:
                            room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                            self.displayImage = self.oracle.myScreen.AddImage("AIimg1_DM");
                            self.displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
                            self.displayImageTimer = 200;
                            moondata.stage = 1;
                            break;
                        
                        case 1100:
                            room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                            self.displayImage = self.oracle.myScreen.AddImage("AIimg2_RIVEND");
                            self.displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
                            self.displayImageTimer = 200;
                            break;
                        
                        case 900:
                            room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                            self.displayImage = self.oracle.myScreen.AddImage("AIimg1");
                            self.displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
                            self.displayImageTimer = 200;
                            break;
                        
                        case 700:
                            room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                            self.displayImage = self.oracle.myScreen.AddImage("AIimg2");
                            self.displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
                            self.displayImageTimer = 200;
                            break;

                        case 500:
                            room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                            self.displayImage = self.oracle.myScreen.AddImage("AIimg3");
                            self.displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
                            self.displayImageTimer = 200;
                            break;
                        
                        case 300:
                            moondata.stage = 2;
                            break;

                        case 240:
                            room.PlaySound(SoundID.SS_AI_Give_The_Mark_Telekenisis, self.player.firstChunk.pos, 1f, 1f);
                            self.player.Stun(240);
                            self.player.firstChunk.vel.x += 0.02f;
                            break;

                        case < 100 and > 1:
                            if (Random.value > 1-(counter*0.01f)) self.oracle.spasms = 5;
                            break;

                        case 1:
                            (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark = true;
                            self.Pain();
                            self.oracle.spasms = 44;
                            self.oracle.stun = Math.Max(self.oracle.stun, 183);
                            self.player.GetPoacher().JustGotMoonMark = true;
                            room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.player.firstChunk.pos, 1f, 1f);
                            room.AddObject(new Spark(self.player.mainBodyChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                            room.game.rainWorld.progression.miscProgressionData.GetSlugBaseData().Set("SolaceMarkCutsceneHasBeenSeen", true);
                            room.game.GetStorySession.saveState.deathPersistentSaveData.GetSlugBaseData().Set("SolaceMoonTalk", true);
                            break;
                    }
                }

                if ((self?.oracle?.room?.game?.session as StoryGameSession).saveState.deathPersistentSaveData.theMark &&
                    room.game.GetStorySession.saveState.deathPersistentSaveData.GetSlugBaseData().TryGet("SolaceMoonTalk", out bool b) &&
                    b && self.oracle.stun <= 0)
                {
                    self.forceFlightMode = false;
                    if (moondata.speechCounter <= 0) 
                        moondata.speechCounter = 200;
                    if (moondata.speechCounter > 1) 
                        moondata.speechCounter--;
                    if (moondata.speechCounter == 1)
                    {
                        self.holdKnees = true;
                        self.movementBehavior = SLOracleBehavior.MovementBehavior.Meditate;
                    }
                }
                /*
                if (!(self?.oracle?.room?.game?.session as StoryGameSession).saveState.deathPersistentSaveData.theMark && counter <= 0) SLHasMarkCutsceneHappened = false;
                if (self.hasNoticedPlayer && !SLHasMarkCutsceneHappened) { counter = 1800; SLHasMarkCutsceneHappened = true; }
                if (counter > 0)
                {
                    if (counter == 1799 && self.oracle.room.world.rainCycle.brokenAntiGrav.on == true)
                        self.oracle.room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_Off, 0f, self.oracle.room.game.cameras[1].room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG), 1f);
                    self.oracle.room.world.rainCycle.brokenAntiGrav.on = false;
                    self.oracle.room.world.rainCycle.brokenAntiGrav.counter = 10;
                    self.oracle.room.world.rainCycle.brokenAntiGrav.progress = 0f;
                    self.oracle.room.world.rainCycle.brokenAntiGrav.from = 0f;
                    self.oracle.room.world.rainCycle.brokenAntiGrav.to = 0f;


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
                        for (int i = 0; i < self.oracle.mySwarmers.Count; i++)
                        {
                            var swarmer = self.oracle.mySwarmers[i];
                            var dist = 250f;
                            Vector2 startPos = swarmer.firstChunk.pos;
                            Vector2 endPos = swarmer.firstChunk.pos + Custom.DirVec(self.oracle.firstChunk.pos,startPos)*dist;

                            var lightning = new LightningBolt(startPos, endPos, 1, 0.5f, 0.3f, 1, 0.33f, false);
                            if (Random.value > 0.96f + counter*0.0002f)
                            {
                                self.oracle.room.AddObject(lightning);
                                self.oracle.room.AddObject(new Explosion.ExplosionLight(startPos, 0.4f,0.5f,30,new Color(0.2f,1f,0f)));
                                self.oracle.room.PlaySound(SoundID.SS_Mycelia_Spark,swarmer.firstChunk.pos,3f,1.4f - Random.value * 0.4f);
                                self.oracle.room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous,swarmer.firstChunk.pos,0.4f,1f);
                                lightning.type = 0;
                            }
                        }
                    }
                }

                if (counter <= 0)
                {
                    self.forceFlightMode = false;
                }

                switch (counter)
                {
                    case 1301:
                        IsShowingMedia = true;
                        self.displayImage = self.oracle.myScreen.AddImage("AIimg1_DM");
                        self.displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
                        self.displayImageTimer = 200;
                        Debug.Log("Solace: Moon mark cutscene phase -1 passed");
                        break;

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
                }*/
            }
        }
        catch(Exception e) { Debug.Log("Solace: Exception occured in SLOracleBehaviorUpdate custom" + e); }
    }
    public static void SLOracleBehaviorNoMark_Update(On.SLOracleBehaviorNoMark.orig_Update orig, SLOracleBehaviorNoMark self, bool eu)
    {
        orig(self,eu);
        int counter = self.MoonCutsceneData().counter;
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
        int counter = self.MoonCutsceneData().counter;
        if (FriendWorldState.SolaceWorldstate && counter > 300 && counter < 1300)
        {
            self.lookPoint = ((self.showMediaPos.x <= self.oracle.room.PixelWidth * 0.85f) ?
                            new Vector2(self.showMediaPos.x + 100f, self.showMediaPos.y + 150f) :
                            new Vector2(self.showMediaPos.x - 100f, self.showMediaPos.y + 150f));
        }
    }
    // Moon dialogue
    public static void SLOracleBehaviorHasMarkOnInitateConversation(On.SLOracleBehaviorHasMark.orig_InitateConversation orig, SLOracleBehaviorHasMark self)
    {
        orig(self);
        if (!FriendWorldState.SolaceWorldstate || !self.oracle.room.game.IsStorySession) return;
        if (self.currentConversation.id == Conversation.ID.MoonFirstPostMarkConversation)
            self.currentConversation = new SLOracleBehaviorHasMark.MoonConversation(Conversation.ID.MoonFirstPostMarkConversation,self,SLOracleBehaviorHasMark.MiscItemType.NA);
    }
}

public static class MoonCutscene
{
    public class Moon
    {
        public int stage;
        public int counter;
        public int speechCounter;
        public Moon()
        {
        }
    }
    public static readonly ConditionalWeakTable<SLOracleBehavior, Moon> CWT = new();
    public static Moon MoonCutsceneData(this SLOracleBehavior moon) => CWT.GetValue(moon, _ => new());

}
