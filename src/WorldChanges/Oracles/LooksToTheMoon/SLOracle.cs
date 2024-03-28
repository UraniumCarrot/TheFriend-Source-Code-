using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using SlugBase.SaveData;
using TheFriend.WorldChanges.WorldStates.General;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.WorldChanges.Oracles.LooksToTheMoon;

public partial class SLOracle
{
    public static void SLOracleBehavior_Update(On.SLOracleBehavior.orig_Update orig, SLOracleBehavior self, bool eu)
    {
        orig(self, eu);
        if (!self.TryMoonData(out var data)) return;
        if (QuickWorldData.SolaceCampaign)
            SolaceSLOracleGiveMark(self, data);
        if (data.restMode) SLStopFlight(self);
    }
    public static void SLOracleBehaviorHasMark_Update(On.SLOracleBehaviorHasMark.orig_Update orig, SLOracleBehaviorHasMark self, bool eu)
    {
        orig(self, eu);
    }
    public static void SLOracleBehaviorNoMark_Update(On.SLOracleBehaviorNoMark.orig_Update orig, SLOracleBehaviorNoMark self, bool eu)
    {
        orig(self,eu);
    }
    public static void SLOracleBehavior_Move(On.SLOracleBehavior.orig_Move orig, SLOracleBehavior self)
    {
        orig(self);
    }

    public static void SolaceSLOracleGiveMark(SLOracleBehavior self, SLOracleCWT.Moon data)
    {
        if (data.stage != MoonScene.MoonStageless)
        {
            SLStopGravity(self);
            if (Custom.rainWorld.progression.currentSaveState.miscWorldSaveData.GetSlugBaseData()
                    .TryGet("SolaceMarkCutsceneHasBeenSeen", out bool a) && a && self.hasNoticedPlayer)
                data.DestroyStage(MoonScene.MoonTalk);
            
            self.forceFlightMode = true;
            self.oracle.SetLocalGravity(Mathf.Lerp(self.oracle.gravity, 1f, 0.2f));
            self.floatyMovement = true;
        }
        
        switch (data.stage.value)
        {
            case nameof(MoonScene.MoonStageless): // Stage 0
                data.stageCounter = 0;
                data.ChangeStage(MoonScene.MoonExamine, self.hasNoticedPlayer);
                break;
            case nameof(MoonScene.MoonExamine): // Stage 1
                self.setMovementBehavior(SLOracleBehavior.MovementBehavior.KeepDistance);
                //SLTalk(self);
                data.ChangeStage(MoonScene.MoonResearch,10);
                break;
            case nameof(MoonScene.MoonResearch): //Stage 2
                self.setMovementBehavior(SLOracleBehavior.MovementBehavior.ShowMedia);
                SolaceSLOracleMarkScreenStage(self, data);
                self.lookPoint = ((self.showMediaPos.x <= self.oracle.room.PixelWidth * 0.85f) ?
                    new Vector2(self.showMediaPos.x + 100f, self.showMediaPos.y + 150f) :
                    new Vector2(self.showMediaPos.x - 100f, self.showMediaPos.y + 150f));
                data.ChangeStage(MoonScene.MoonTalk,data.miniStage == 5);
                break;
            case nameof(MoonScene.MoonTalk): //Stage 3
                data.ChangeStage(MoonScene.MoonGiveMark,5);
                break;
            
            case nameof(MoonScene.MoonGiveMark): //Stage 4
                self.setMovementBehavior(SLOracleBehavior.MovementBehavior.Meditate);
                for (int i = 0; i < self.oracle.mySwarmers.Count; i++)
                {
                    var swarmer = self.oracle.mySwarmers[i];
                    var dist = 250f;
                    Vector2 startPos = swarmer.firstChunk.pos;
                    Vector2 endPos = swarmer.firstChunk.pos + Custom.DirVec(self.oracle.firstChunk.pos,startPos)*dist;

                    var lightning = new LightningBolt(startPos, endPos, 1, 0.5f, 0.3f, 1, 0.33f, false);
                    self.armsProtest = true;
                    if (Random.value < data.stageCounter / (740f * 20))
                    {
                        self.oracle.spasms = 5;
                        self.oracle.room.AddObject(lightning);
                        self.oracle.room.AddObject(new Explosion.ExplosionLight(startPos, 0.4f, 1f, 30, new Color(0.2f, 1f, 0f)));
                        self.oracle.room.PlaySound(SoundID.SS_Mycelia_Spark, swarmer.firstChunk.pos, 3f, 1.4f - Random.value * 0.4f);
                        self.oracle.room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, swarmer.firstChunk.pos, 0.4f, 1f);
                        lightning.type = 0;
                    }
                }

                switch (data.stageCounter)
                {
                    case 500:
                        self.oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Telekenisis, self.player.firstChunk.pos, 1f, 1f);
                        break;
                    case > 739: SolaceSLOracleMarkEnd(self, data); break;
                }
                break;
        }
        data.stageCounter++;
    }
    public static void SolaceSLOracleMarkScreenStage(SLOracleBehavior self, SLOracleCWT.Moon data)
    {
        bool justStarted = data.stageCounter == 0;
        switch (data.miniStage)
        {
            case 0:
                if (justStarted)
                {
                    self.oracle.room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                    self.displayImage = self.oracle.myScreen.AddImage("AIimg1_DM");
                    self.displayImage.setAlpha = 0.91f + Random.value * 0.06f;
                    self.displayImageTimer = 200;
                }
                data.ChangeMiniStage(1,5f);
                break;
            case 1:
                if (justStarted)
                {
                    self.oracle.room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                    self.displayImage = self.oracle.myScreen.AddImage("AIimg2_RIVEND");
                    self.displayImage.setAlpha = 0.91f + Random.value * 0.06f;
                    self.displayImageTimer = 200;
                }
                data.ChangeMiniStage(2,5f);
                break;
            case 2:
                if (justStarted)
                {
                    self.oracle.room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                    self.displayImage = self.oracle.myScreen.AddImage("AIimg1");
                    self.displayImage.setAlpha = 0.91f + Random.value * 0.06f;
                    self.displayImageTimer = 200;
                }
                data.ChangeMiniStage(3,5f);
                break;
            case 3:
                if (justStarted)
                {
                    self.oracle.room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                    self.displayImage = self.oracle.myScreen.AddImage("AIimg2");
                    self.displayImage.setAlpha = 0.91f + Random.value * 0.06f;
                    self.displayImageTimer = 200;
                }
                data.ChangeMiniStage(4,5f);
                break;
            case 4:
                if (justStarted)
                {
                    self.oracle.room.PlaySound(SoundID.SS_AI_Text, self.player.firstChunk.pos, 1.5f, 1f);
                    self.displayImage = self.oracle.myScreen.AddImage("AIimg3");
                    self.displayImage.setAlpha = 0.91f + Random.value * 0.06f;
                    self.displayImageTimer = 200;
                }
                data.ChangeMiniStage(5,5f);
                break;
            case 5: return;
        }
    }
    public static void SolaceSLOracleMarkEnd(SLOracleBehavior self, SLOracleCWT.Moon data)
    {
        OracleTools.OracleGrantMarks(self, 60);
        
        self.Pain();
        self.oracle.spasms = 44;
        self.oracle.stun = Math.Max(self.oracle.stun, 183);
        self.oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.player.firstChunk.pos, 1f, 1f);
        self.oracle.room.game.rainWorld.progression.currentSaveState.miscWorldSaveData.GetSlugBaseData().Set("SolaceMarkCutsceneHasBeenSeen", true);
        self.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.GetSlugBaseData().Set("SolaceMoonTalk", true);
        self.oracle.oracleBehavior = new SLOracleBehaviorHasMark(self.oracle);
        data.restMode = true;
        data.DestroyStage();
    }
}