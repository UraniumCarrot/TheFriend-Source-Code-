using UnityEngine;
using RWCustom;
using System;
using System.Linq;
using On.MoreSlugcats;
using TheFriend.Objects.DelugePearlObject;
using TheFriend.SlugcatThings;
using Random = UnityEngine.Random;

namespace TheFriend.CharacterThings.DelugeThings;

public class DelugeGameplay
{
    public static void Apply()
    {
        On.DeafLoopHolder.Update += DeafLoopHolderOnUpdate;
    }

    public static void DelugeUpdate(Player self, bool eu)
    {
        var scug = self.GetDeluge();

        if (scug.pearl != null)
        {
            var pearlData = scug.pearl.AbstractPearl.DelugePearlData();

            if (scug.pearl.grabbedBy.Any() && !scug.pearl.grabbedBy.Select(x => x.grabber).Contains(self))
            {
                if (self.dead && Custom.Dist(scug.pearl.firstChunk.pos, self.bodyChunks[1].pos) > PearlCWT.DelugePearl.BasePearlToButtDist) ;
                {
                    pearlData.tailConnection.active = false;
                    pearlData.buttConnection.active = false;
                }
                pearlData.buttConnection.weightSymmetry = 0.75f;
                pearlData.tailConnection.weightSymmetry = 0.25f;
            }
            else
            {
                pearlData.buttConnection.weightSymmetry = PearlCWT.DelugePearl.BaseButtConnectionAssymetry;
                pearlData.tailConnection.weightSymmetry = PearlCWT.DelugePearl.BaseTailConnectionAssymetry;
            }
            pearlData.tailConnection?.Update();
            pearlData.buttConnection?.Update();
        }

        if (self.dead && !self.Stunned)
        {
            self.deaf = 0;
            return;
        }
        
        scug.AmIIdling = DelugeIdleCheck(self);
        if (scug.GracePeriod > 0)
            scug.GracePeriod--;

        // LookTarget determination, see DelugeOverload for why
        if (scug.lookTarget is Creature || scug.lookTarget is Oracle || scug.lookTarget is OracleSwarmer)
            scug.lookTarget = (self.graphicsModule as PlayerGraphics)?.objectLooker.currentMostInteresting;
        else scug.lookTarget = null;
        
        scug.OverloadLooper++;
        if (scug.OverloadLooper > 5)
        { // Overload changes every five ticks, determines intensity of DelugeOverloadEffects. Overload IS NOT CHANGED during grace period, preventing effect intensifying
            if (scug.GracePeriod <= 0) DelugeOverload(self, eu);
            scug.OverloadLooper = 0;
        }
        DelugeOverloadEffects(self, scug.Overload, eu);

        if (scug.sprinting)
            DelugeSprint(self, scug, eu);
        else
        {
            scug.sprintParticleTimer = 0;
            if (scug.Exhaustion >= DelugeCWT.Deluge.ExhaustionStillnessThreshold)
            {
                if (scug.AmIIdling)
                {
                    scug.Exhaustion--;
                    if (self.bodyMode == Player.BodyModeIndex.Crawl)
                        scug.Exhaustion--; //  Exhaustion depletes twice as fast if Deluge lays down
                }
                else if (scug.Exhaustion < DelugeCWT.Deluge.ExhaustionLimit) scug.Exhaustion++;
            }
            else if (scug.Exhaustion > 0)
            {
                scug.Exhaustion--;
                if (self.bodyMode == Player.BodyModeIndex.Crawl) scug.Exhaustion--; //  Exhaustion depletes twice as fast if Deluge lays down
            }
        }

        if (scug.Exhaustion > DelugeCWT.Deluge.ExhaustionSiezeThreshold) // Add to Sieze if threshold is passed
            if (scug.Sieze < DelugeCWT.Deluge.SiezeLimit && !scug.AmIIdling) scug.Sieze++;
        if (scug.Sieze >= DelugeCWT.Deluge.SiezeLimit) DelugeSiezure(self, eu);

        if (scug.Sieze > 0)
        {
            if (scug.AmIIdling) scug.Sieze--;
            if (!scug.siezing) CharacterTools.HeadShiver(self.graphicsModule as PlayerGraphics, 0.5f);
            if (scug.Sieze < DelugeCWT.Deluge.SiezeLimit && scug.siezing) DelugeSiezeEffects(self, eu);
        }
        else scug.siezing = false;

        if (scug.sprinting || (scug.Sieze > 0 && !scug.siezing) || scug.Overload > DelugeCWT.Deluge.OverloadLimit*0.7f) self.GetGeneral().squint = true;
        else self.GetGeneral().squint = false; // Controls if Deluge is squeezing their eyes shut
    }

    public static void DelugeSprint(Player self, DelugeCWT.Deluge scug, bool eu)
    {
        self.bodyChunks[0].vel.x += 2.2f * self.input[0].x;
        self.bodyChunks[1].vel.x += 1.8f * self.input[0].x;
        if (scug.Exhaustion < DelugeCWT.Deluge.ExhaustionLimit) 
            scug.Exhaustion++;
        scug.sprintParticleTimer++;
        self.AerobicIncrease(1f);
        
        if (scug.sprintParticleTimer >= 5)
        { // Cosmetic effect that loops every 5 ticks
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
    }
    
    public static void DelugeSiezure(Player self, bool eu)
    {
        self.Stun(80);
        self.AerobicIncrease(1);
        self.room.AddObject(new CreatureSpasmer(self, false, 25));
        self.GetDeluge().siezing = true;
    }
    public static void DelugeSiezeEffects(Player self, bool eu)
    {   // These occur while Deluge is recovering from siezing
        var scug = self.GetDeluge();
        self.Blink(5);
        if (self.bodyMode == Player.BodyModeIndex.Stand && self.animation == Player.AnimationIndex.None && scug.siezing)
        {
            self.bodyChunks[0].vel.x *= 0.7f;
            self.bodyChunks[1].vel.x *= 0.7f;
        }
        self.AerobicIncrease(1);
    }
    public static void DelugeSiezeJump(Player self)
    {   // Penalty for trying to be movementful while under DelugeSiezeEffects
        var scug = self.GetDeluge();
        if (scug.Sieze > 0 && scug.siezing)
        { 
            self.Stun(20); 
            scug.Sieze += Mathf.RoundToInt(DelugeCWT.Deluge.SiezeLimit*0.4f);
            self.Blink(50);
        }
    }
    public static void DelugeOverload(Player self, bool eu)
    {
        /*
         Deluge gains overload while not moving and not focusing on anything
         By moving or having something interesting to look at, they lose overload
         */
        var limit = DelugeCWT.Deluge.OverloadLimit;
        var scug = self.GetDeluge();
        if (scug.lookTarget != null)
            scug.Overload -= Mathf.RoundToInt(limit*0.05f);
        else if (scug.Overload < limit && scug.AmIIdling)
            scug.Overload += Mathf.RoundToInt(limit*0.005f);
        

        if (scug.sprinting) scug.Overload -= Mathf.RoundToInt(limit*0.045f);
        else if (!scug.AmIIdling) scug.Overload--;
        
        if (scug.Overload < 0) scug.Overload = 0;
    }

    public static void DelugeOverloadEffects(Player self, int intensity, bool eu)
    {
        if (self.room != null && self.room.game.IsArenaSession) return;
        float percent = (float)intensity / (float)DelugeCWT.Deluge.OverloadLimit;

        self.deaf = Mathf.RoundToInt(Mathf.Lerp(1f, 90f, percent));
    }

    public static void DeafLoopHolderOnUpdate(On.DeafLoopHolder.orig_Update orig, DeafLoopHolder self, bool eu)
    { // Consider this a part of DelugeOverloadEffects...
        orig(self, eu);
        if (self.player.TryGetDeluge(out var scug) && self.deafLoop != null)
        {
            var deafener = self.deafLoop;
            var intensity = scug.Overload;
            float percent = (float)intensity / (float)DelugeCWT.Deluge.OverloadLimit;

            deafener.sound = DelugeSounds.DelugeHeartbeatSine;
            deafener.Volume = Mathf.Lerp(0.1f, DelugeCWT.Deluge.OverloadIntensity, percent);
        }
    }
    
    public static bool DelugeIdleCheck(Player self)
    { // Is the Deluge doing something?
        var scug = self.GetDeluge();
        if (self.stun > 0 && !self.dead) return true;

        if ((self.firstChunk.vel.magnitude + self.bodyChunks[1].vel.magnitude) / 2 > 3) return false;
        else return true;
    }

    public static void DelugeConstructor(Player self)
    { // Deluge is the loudest motherfucker besides Artificer. Funny bauble go tinktink
        self.slugcatStats.visualStealthInSneakMode = 0;
        self.slugcatStats.generalVisibilityBonus = 1;
        self.GetGeneral().iHaveSenses = true;
    }
    
    public static void DelugeSprintCheck(Player self)
    { // TODO: Allow any player to have their own sprint keybind
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