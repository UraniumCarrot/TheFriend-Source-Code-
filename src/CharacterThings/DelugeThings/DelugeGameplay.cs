using UnityEngine;
using RWCustom;
using System;
using On.MoreSlugcats;
using Random = UnityEngine.Random;

namespace TheFriend.CharacterThings.DelugeThings;

public class DelugeGameplay
{
    public static void Apply()
    {
    }

    public static void DelugeUpdate(Player self, bool eu)
    {
        var scug = self.GetDeluge();
        if (self.dead && !self.Stunned) return;
        
        scug.AmIIdling = DelugeIdleCheck(self);
        if (scug.GracePeriod > 0)
            scug.GracePeriod--;

        DelugeOverloadEffects(self, scug.Overload, eu);

        if (scug.lookTarget is Creature || scug.lookTarget is Oracle || scug.lookTarget is OracleSwarmer)
            scug.lookTarget = (self.graphicsModule as PlayerGraphics)?.objectLooker.currentMostInteresting;
        else scug.lookTarget = null;
        
        scug.OverloadLooper++;
        if (scug.OverloadLooper > 5)
        {
            if (scug.GracePeriod <= 0) DelugeOverload(self, eu);
            scug.OverloadLooper = 0;
        }
        

        if (scug.sprintParticleTimer >= 5)
        {
            self.room?.AddObject(new WaterDrip(
                self.bodyChunks[1].pos + new Vector2(0f, -self.bodyChunks[1].rad + 1f), 
                Custom.DegToVec(-self.slideDirection * Mathf.Lerp(30f, 70f, Random.value)) * Mathf.Lerp(6f, 11f, Random.value), 
                false));
            self.room?.AddObject(new MouseSpark(
                self.firstChunk.pos,
                Custom.DegToVec(-self.slideDirection * Mathf.Lerp(10f, 50f, Random.value)) * Mathf.Lerp(4f, 8f, Random.value), 
                5f, 
                Color.white));
            scug.sprintParticleTimer = 0;
        }

        if (scug.sprinting)
        {
            self.bodyChunks[0].vel.x += 2.2f * self.input[0].x;
            self.bodyChunks[1].vel.x += 1.8f * self.input[0].x;
            if (scug.Exhaustion < 500) scug.Exhaustion++;
            if (scug.Exhaustion >= 400) scug.Exhaustion = 500;
            scug.sprintParticleTimer++;
            self.AerobicIncrease(1f);
        }
        else
        {
            scug.sprintParticleTimer = 0;
            if (scug.Exhaustion >= 100)
            {
                if (self.input[0].x == 0) scug.Exhaustion--;
            }
            else if (scug.Exhaustion > 0)
            {
                scug.Exhaustion--;
                if (self.bodyMode == Player.BodyModeIndex.Crawl) scug.Exhaustion--;
            }
        }

        if (scug.Exhaustion > 400)
        {
            self.Blink(5);
            if (scug.Sieze < 100) scug.Sieze += 1;
        }
        if (scug.Sieze == 100) DelugeSiezure(self, eu);

        if (scug.Sieze > 0)
        {
            self.Blink(5);
            if (self.input[0].x == 0) scug.Sieze--;
            if (scug.Sieze < 100 && scug.siezing) DelugeSiezeEffects(self, eu);
        }
        else scug.siezing = false;
    }

    public static void DelugeSiezure(Player self, bool eu)
    {
        self.Stun(80);
        self.AerobicIncrease(1);
        self.room.AddObject(new CreatureSpasmer(self, false, 25));
        self.GetDeluge().siezing = true;
    }
    public static void DelugeSiezeEffects(Player self, bool eu)
    {
        var scug = self.GetDeluge();
        if (self.bodyMode == Player.BodyModeIndex.Stand && self.animation == Player.AnimationIndex.None && scug.siezing)
        {
            self.bodyChunks[0].vel.x *= 0.7f;
            self.bodyChunks[1].vel.x *= 0.7f;
        }
        self.Blink(5);
        self.AerobicIncrease(1);
    }
    public static void DelugeSiezeJump(Player self)
    {
        var deluge = self.GetDeluge();
        if (deluge.Sieze > 0)
        { 
            self.Stun(20); 
            deluge.Sieze += 40;
            self.Blink(50);
        }
    }
    public static void DelugeOverload(Player self, bool eu)
    {
        var scug = self.GetDeluge();
        if (scug.lookTarget != null)
            scug.Overload -= 100;
        else if (scug.Overload < 1000 && scug.AmIIdling)
        {
            scug.Overload += 5;
        }

        if (scug.sprinting) scug.Overload -= 45;
        else if (!scug.AmIIdling) scug.Overload--;
        
        if (scug.Overload < 0) scug.Overload = 0;
        
    }

    public static void DelugeOverloadEffects(Player self, int intensity, bool eu)
    {
        var scug = self.GetDeluge();
        var percent = intensity * 0.001f;

        if (self.dead && !self.Stunned)
        {
            self.deaf = 0;
            return;
        }
        
        self.deaf = Mathf.RoundToInt(Mathf.Lerp(1f, 90f, percent));
        if (self.deafLoopHolder != null)
            if (self.deafLoopHolder.deafLoop != null)
            {
                var deafener = self.deafLoopHolder.deafLoop;
                deafener.sound = NoirThings.NoirCatto.DelugeHeartbeatSine;
                deafener.Volume = Mathf.Lerp(0.1f, 2f, percent);
            }
    }

    public static bool DelugeIdleCheck(Player self)
    {
        var scug = self.GetDeluge();
        if (self.stun > 0 && !self.dead) return true;

        if (self.dead) return false;
        if (scug.lookTarget != null) return false;
        if (scug.sprinting) return false;
        
        if (self.animation == Player.AnimationIndex.Flip) return false;
        if (self.animation == Player.AnimationIndex.BellySlide) return false;
        if (self.bodyMode == Player.BodyModeIndex.WallClimb && self.firstChunk.vel.y > 0) return true;
        
        if (self.animation != Player.AnimationIndex.CorridorTurn && 
            self.bodyMode == Player.BodyModeIndex.CorridorClimb &&
            Mathf.Abs(self.firstChunk.vel.x) < 2 && 
            Mathf.Abs(self.firstChunk.vel.y) < 2) return true;

        if (self.input[0].x == 0 && self.input[0].y == 0) // Both X and Y are 0
        {
            if (self.bodyMode == Player.BodyModeIndex.Stand) return true;
            if (self.bodyMode == Player.BodyModeIndex.Crawl) return true;

            if (self.animation == Player.AnimationIndex.DeepSwim) return true;
            if (self.animation == Player.AnimationIndex.GetUpOnBeam) return false;
            if (self.animation == Player.AnimationIndex.HangFromBeam) return true;
            if (self.animation == Player.AnimationIndex.HangUnderVerticalBeam) return true;
            if (self.animation == Player.AnimationIndex.VineGrab) return true;
            if (self.animation == Player.AnimationIndex.ZeroGSwim) return true;
            if (self.animation == Player.AnimationIndex.ZeroGPoleGrab) return true;
            return false;
        }
        else if (self.input[0].x == 0 && self.input[0].y != 0) // Only X is 0
        {
            if (self.bodyMode == Player.BodyModeIndex.Stand) return true;
            if (self.animation == Player.AnimationIndex.StandOnBeam) return true;
            if (self.animation == Player.AnimationIndex.BeamTip) return true;
            if (self.input[0].y >= 0 && self.animation == Player.AnimationIndex.SurfaceSwim) return true;
            return false;
        }
        else if (self.input[0].y == 0 && self.input[0].x != 0) // Only Y is 0
        {
            if (self.animation == Player.AnimationIndex.ClimbOnBeam) return true;
            return false;
        }
        if (self.animation == Player.AnimationIndex.None) return false;

        return false;
    }
    
    
    public static void DelugeSprintCheck(Player self)
    {
        var deluge = self.GetDeluge();
        if (self.Malnourished) return;
        if (self.bodyMode == Player.BodyModeIndex.Stand && self.animation == Player.AnimationIndex.None)
            if (Input.GetKey(KeyCode.LeftShift))
                if (deluge.Sieze <= 0 && self.input[0].x != 0) 
                    deluge.sprinting = true;
                else  deluge.sprinting = false;
            else  deluge.sprinting = false;
        else  deluge.sprinting = false;
    }
    
    
}