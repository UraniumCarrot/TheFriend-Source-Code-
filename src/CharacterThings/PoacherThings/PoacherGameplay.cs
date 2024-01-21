using MoreSlugcats;
using UnityEngine;
using TheFriend.CharacterThings;
using TheFriend.Creatures.LizardThings.DragonRideThings;
using Random = UnityEngine.Random;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;

namespace TheFriend.PoacherThings;

public class PoacherGameplay
{
    public static void Apply()
    {
        On.Player.ObjectEaten += PlayerOnObjectEaten;
        On.Player.HeavyCarry += Player_HeavyCarry;
        On.DangleFruit.Update += DangleFruit_Update;
        On.LanternMouse.Update += LanternMouse_Update;
        On.MoreSlugcats.DandelionPeach.Update += DandelionPeach_Update;
    }
    public static readonly SlugcatStats.Name DragonName = Plugin.DragonName;

    #region main mechanics

    public static void PoacherJump(Player self)
    {
        float debuff = 1f;
        self.bodyChunks[0].vel.y -= debuff;
        self.bodyChunks[1].vel.y -= debuff;
    }
    
    public static void PoacherUpdate(Player self, bool eu)
    {
        if (self.input[0].y < 1 || !self.input[0].pckp) self.GetPoacher().isMakingPoppers = false;
        self.Hypothermia += self.HypothermiaGain * (Configs.PoacherFreezeFaster ? 1.2f : 0.2f);
        PoacherGameplay.PoacherEats(self);
        if (self.dangerGraspTime > 0)
        {
            self.stun = 0;
            if (self.input[0].thrw) self.ThrowToGetFree(eu);
            if (self.input[0].pckp) self.DangerGraspPickup(eu);
        }
    }

    public static void PoacherConstructor(Player self)
    {
        self.setPupStatus(true);
        self.GetPoacher().IsSkullVisible = true;
        if (Configs.PoacherBackspear) self.spearOnBack = new Player.SpearOnBack(self);
    }
    
    #endregion
    
    #region food
    public static void PlayerOnObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible eatenobject)
    {
        orig(self,eatenobject);

        if (!self.TryGetPoacher(out var poacher)) return;
        if (eatenobject is GlowWeed) { poacher.favoriteFoodTimer = 100; Debug.Log("Poacher loves it!"); }
        if (eatenobject is Hazer) { poacher.favoriteFoodTimer = 50; }
        if (eatenobject is DangleFruit fruit && !WorldChanges.FamineWorld.IsDiseased(fruit)) { poacher.favoriteFoodTimer = 50; }
        if (eatenobject is LillyPuck) { poacher.favoriteFoodTimer = -50; }
        if (eatenobject is JellyFish) { poacher.favoriteFoodTimer = -100; }
        if (eatenobject is FireEgg) { poacher.favoriteFoodTimer = -600; Debug.Log("Poacher hated this food so much they died! Just kidding, it was full of super acid."); }

    }

    public static void PoacherEats(Player self)
    {
        if (!self.TryGetPoacher(out var poacher)) return;
        if (poacher.sleepCounter < 300 && self.bodyMode == Player.BodyModeIndex.Crawl && !self.input[0].AnyInput) poacher.sleepCounter++;
        if (poacher.sleepCounter >= 300) (self.graphicsModule as PlayerGraphics).blink = 5;
        if (self.bodyMode != Player.BodyModeIndex.Crawl || self.input[0].AnyInput) poacher.sleepCounter = 0;
        if (poacher.favoriteFoodTimer == 0) return;
        if (Configs.PoacherPupActs)
        {
            if (poacher.favoriteFoodTimer > 0 && !self.Stunned && !self.Malnourished)
            {
                poacher.favoriteFoodTimer--;
                self.slugcatStats.runspeedFac = 1.5f;
                self.slugcatStats.poleClimbSpeedFac = 1.3f;
                self.dynamicRunSpeed[0] *= 2f;
                self.dynamicRunSpeed[1] *= 2f;
                if (self.bodyMode == Player.BodyModeIndex.Stand) self.input[0].jmp = Random.value < 0.1;
                if (self.bodyMode == Player.BodyModeIndex.Crawl) self.jumpBoost *= 1.2f;
                else self.jumpBoost *= 1.05f;
            }
            if (poacher.favoriteFoodTimer < 0 && !self.Stunned && !self.Malnourished)
            {
                poacher.favoriteFoodTimer++;
                self.exhausted = true;
                CharacterTools.HeadShiver(self.graphicsModule as PlayerGraphics, 0.2f);
                self.slugcatStats.runspeedFac = 0.7f;
                self.slugcatStats.poleClimbSpeedFac = 0.7f;
                if (poacher.favoriteFoodTimer < -500) self.Die();
            }
            if (self.dead) { poacher.favoriteFoodTimer = 0; }
            if (poacher.favoriteFoodTimer == 0 && !self.Stunned && !self.Malnourished) { self.slugcatStats.runspeedFac = 1f; self.slugcatStats.poleClimbSpeedFac = 1f; }
        }
    }
    #endregion
    #region item carrying
    public static bool Player_HeavyCarry(On.Player.orig_HeavyCarry orig, Player self, PhysicalObject obj)
    { // Allows Poacher to carry things that they couldn't usually
        if (obj.TryGetLiz(out var data))
        {
            if (data.myTemplate.type == CreatureTemplateType.YoungLizard) return false;
            else if (data.DoILikeYou(self)) return true;
        }
        if (self.TryGetPoacher(out _))
        {
            if (obj is Creature crit && crit is not Hazer && crit is not VultureGrub && crit is not Snail && crit is not SmallNeedleWorm && crit is not TubeWorm) return orig(self, obj);
            else if (obj is DandelionPeach || obj is DangleFruit)
                return Configs.PoacherFoodParkour;
            else return false;
        }
        else return orig(self, obj);
    }
    public static void DandelionPeach_Update(On.MoreSlugcats.DandelionPeach.orig_Update orig, DandelionPeach self, bool eu)
    { // Poacher food parkour
        orig(self, eu);
        if (!Configs.PoacherFoodParkour) return;
        if (self.grabbedBy?.Count > 0)
        {
            for (int i = 0; i < self.grabbedBy.Count; i++)
            {
                if (self.grabbedBy[i].grabber is Player player && player.TryGetPoacher(out _))
                {
                    if (player.animation == ind.None && player.bodyMode != bod.Stand && player.bodyMode != bod.Swimming && player.Submersion == 0) { self.firstChunk.mass = 0.34f; }
                    else self.firstChunk.mass = 0.0001f;
                }
            }
        }
        else self.firstChunk.mass = 0.34f;
    }
    public static void LanternMouse_Update(On.LanternMouse.orig_Update orig, LanternMouse self, bool eu)
    { // Fixes Poacher unable to use lantern mice
        orig(self, eu);
        if (self.grabbedBy?.Count > 0)
        {
            for (int i = 0; i < self.grabbedBy.Count; i++)
            {
                if (self.grabbedBy[i].grabber is Player player && player.TryGetPoacher(out _))
                {
                    if (player.animation != ind.None && player.bodyMode != bod.Stand && player.bodyMode != bod.Swimming && player.Submersion == 0) { self.bodyChunks[0].mass = 0.2f; self.bodyChunks[1].mass = 0.2f; }
                    else { self.bodyChunks[0].mass = 0.0001f; self.bodyChunks[1].mass = 0.0001f; }
                }
            }
        }
        else { self.bodyChunks[0].mass = 0.4f / 2f; self.bodyChunks[1].mass = 0.4f / 2f; }
    }
    public static void DangleFruit_Update(On.DangleFruit.orig_Update orig, DangleFruit self, bool eu)
    { // Poacher food parkour
        orig(self, eu);
        if (!Configs.PoacherFoodParkour) return;
        if (self.grabbedBy?.Count > 0)
        {
            for (int i = 0; i < self.grabbedBy.Count; i++)
            {
                if (self.grabbedBy[i].grabber is Player player && player.TryGetPoacher(out _))
                {
                    if (player.animation == ind.None && player.bodyMode != bod.Stand && player.bodyMode != bod.Swimming && player.Submersion == 0) { self.firstChunk.mass = 0.2f; }
                    else self.firstChunk.mass = 0.0001f;
                }
            }
        }
        else self.firstChunk.mass = 0.2f;
    }
    #endregion
}