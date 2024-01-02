using MoreSlugcats;
using UnityEngine;
using RWCustom;
using TheFriend.CharacterThings;
using TheFriend.SlugcatThings;
using Random = UnityEngine.Random;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;

namespace TheFriend.PoacherThings;

public class PoacherGameplay
{
    public static void Apply()
    {
        On.Player.ObjectEaten += PlayerOnObjectEaten;
        On.Player.Grabbed += Player_Grabbed;
        On.Player.Stun += Player_Stun;
        On.Player.HeavyCarry += Player_HeavyCarry;
        On.DangleFruit.Update += DangleFruit_Update;
        On.LanternMouse.Update += LanternMouse_Update;
        On.MoreSlugcats.DandelionPeach.Update += DandelionPeach_Update;
    }
    public static readonly SlugcatStats.Name DragonName = Plugin.DragonName;

    #region main mechanics

    public static void PoacherUpdate(Player self, bool eu)
    {
        if (self.input[0].y < 1 || !self.input[0].pckp) self.GetPoacher().isMakingPoppers = false;
        self.Hypothermia += self.HypothermiaGain * (Plugin.PoacherFreezeFaster() ? 1.2f : 0.2f);
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
        if (Plugin.PoacherBackspear()) self.spearOnBack = new Player.SpearOnBack(self);
    }
    
    #endregion
    
    #region misc mechanics
    public static void Player_Grabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp grasp)
    { 
        orig(self, grasp);
        if (!self.TryGetPoacher(out var poacher)) return;
        if (grasp.grabber is Lizard || grasp.grabber is Vulture || grasp.grabber is BigSpider || grasp.grabber is DropBug)
        { // Poacher skull flicker (from grabs)
            PoacherGraphics.PoacherFlicker(self);
        }
    }
    public static void Player_Stun(On.Player.orig_Stun orig, Player self, int st)
    { 
        if (self.TryGetPoacher(out var poacher) && self.stunDamageType == Creature.DamageType.Blunt && !self.Stunned)
        { // Poacher skull flicker (from rocks)
            if (self.bodyMode == bod.Crawl) { self.firstChunk.vel.y += 10; self.animation = ind.Flip; }
            PoacherGraphics.PoacherFlicker(self);
        }
        orig(self, st);
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
        if (Plugin.PoacherPupActs() == true)
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
        if (self.room?.abstractRoom.name == "VR1" || 
            self.room?.abstractRoom.name == "PUMP03" || 
            self.room?.abstractRoom.name == "PS1") 
            return orig(self,obj);
        if (obj is Creature young && young.Template.type == CreatureTemplateType.YoungLizard) return false;
        else if (obj is Lizard mother && mother.GetLiz() != null && mother.GetLiz().IsRideable) return true;
        if (self.TryGetPoacher(out var poacher))
        {
            if (obj is Creature crit && crit is not Hazer && crit is not VultureGrub && crit is not Snail && crit is not SmallNeedleWorm && crit is not TubeWorm) return orig(self, obj);
            else if (obj is DandelionPeach || obj is DangleFruit)
            {
                if (!Plugin.PoacherFoodParkour()) return false;
                else return true;
            }
            else return false;
        }
        else return orig(self, obj);
    }
    public static void DandelionPeach_Update(On.MoreSlugcats.DandelionPeach.orig_Update orig, DandelionPeach self, bool eu)
    { // Poacher food parkour
        orig(self, eu);
        if (!Plugin.PoacherFoodParkour()) return;
        if (self.room.abstractRoom.name == "VR1" || 
            self.room.abstractRoom.name == "PUMP03" || 
            self.room.abstractRoom.name == "PS1") 
            return;
        if (self.grabbedBy?.Count > 0)
        {
            for (int i = 0; i < self.grabbedBy.Count; i++)
            {
                if (self.grabbedBy[i].grabber is Player player && player.TryGetPoacher(out var poacher))
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
                if (self.grabbedBy[i].grabber is Player player && player.TryGetPoacher(out var poacher))
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
        if (!Plugin.PoacherFoodParkour()) return;
        if (self.room.abstractRoom.name == "VR1" || 
            self.room.abstractRoom.name == "PUMP03" || 
            self.room.abstractRoom.name == "PS1") 
            return;
        if (self.grabbedBy?.Count > 0)
        {
            for (int i = 0; i < self.grabbedBy.Count; i++)
            {
                if (self.grabbedBy[i].grabber is Player player && player.TryGetPoacher(out var poacher))
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