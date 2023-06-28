using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using SlugBase;
using MoreSlugcats;
using TheFriend.WorldChanges;
using UnityEngine;
using Random = UnityEngine.Random;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;


namespace TheFriend.SlugcatThings;

public class SlugcatGameplay
{
    public static void Apply()
    {
        On.Player.Update += Player_Update;
        On.Player.Stun += Player_Stun;
        On.Weapon.HitThisObject += Weapon_HitThisObject;
        On.Player.WallJump += Player_WallJump;
        On.Player.Jump += Player_Jump;
        On.Player.ctor += Player_ctor;
        On.Player.Grabability += Player_Grabability;
        On.Player.GrabUpdate += Player_GrabUpdate;
        On.Player.Grabbed += Player_Grabbed;
        On.Player.UpdateBodyMode += Player_UpdateBodyMode;
        On.Player.UpdateAnimation += Player_UpdateAnimation;
        On.Player.HeavyCarry += Player_HeavyCarry;
        On.Player.checkInput += Player_checkInput;
        On.SlugcatStats.SlugcatCanMaul += SlugcatStats_SlugcatCanMaul;
        On.SlugcatStats.ctor += SlugcatStats_ctor;

        // Misc gameplay changes
        On.DangleFruit.Update += DangleFruit_Update;
        On.LanternMouse.Update += LanternMouse_Update;
        On.MoreSlugcats.DandelionPeach.Update += DandelionPeach_Update;
    }
    public static readonly SlugcatStats.Name FriendName = Plugin.FriendName;
    public static readonly SlugcatStats.Name DragonName = Plugin.DragonName;

    public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    { // Makes player ride lizard
        orig(self, eu);
        for (int i = 0; i < 2; i++)
        {
            if (self?.grasps[i]?.grabbed is Lizard liz && liz.GetLiz() != null && liz.GetLiz().IsRideable && !liz.dead && !liz.Stunned && liz?.AI?.LikeOfPlayer(liz?.AI?.tracker?.RepresentationForCreature(self?.abstractCreature, true)) > 0)
            {
                if (!liz.GetLiz().boolseat0)
                {
                    self.grasps[i].Release();
                    self.GetPoacher().isRidingLizard = true;
                    DragonRiding.DragonRidden(liz, self);
                    liz.GetLiz().boolseat0 = true;
                    self.GetPoacher().dragonSteed = liz;
                    liz.GetLiz().rider = self;
                }
            }
        }
        // Poacher poppers quickcraft
        if (self.slugcatStats.name == DragonName)
        {
            if (self.craftingObject && self.GetPoacher().isMakingPoppers && self.grasps.Count(i => i.grabbed is FirecrackerPlant) == 1 && self.swallowAndRegurgitateCounter < 70) self.swallowAndRegurgitateCounter = 70;
        }
    }
    public static bool Weapon_HitThisObject(On.Weapon.orig_HitThisObject orig, Weapon self, PhysicalObject obj)
    { // Lizard mount will not be hit by owner's weapons
        if (obj is Lizard liz && liz.GetLiz() != null && liz.GetLiz().IsBeingRidden && self.thrownBy is Player pl && pl.GetPoacher().dragonSteed == liz) return false;
        else return orig(self, obj);
    }
    public static void Player_Grabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp grasp)
    { // Poacher skull flicker (from grabs)
        orig(self, grasp);
        if ((grasp.grabber is Lizard || grasp.grabber is Vulture || grasp.grabber is BigSpider || grasp.grabber is DropBug) && self.slugcatStats.name == DragonName)
        {
            self.GetPoacher().flicker = Custom.IntClamp(200 / 3, 3, 15);
            self.room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, self.firstChunk, false, 1, 1);
        }
    }
    public static void Player_Stun(On.Player.orig_Stun orig, Player self, int st)
    { // Poacher skull flicker (from rocks)
        if (self.slugcatStats.name == DragonName && self.stunDamageType == Creature.DamageType.Blunt && !self.Stunned)
        {
            if (self.bodyMode == bod.Crawl) { self.firstChunk.vel.y += 10; self.animation = ind.Flip; }
            self.GetPoacher().flicker = Custom.IntClamp(200 / 3, 3, 15);
            self.room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, self.firstChunk, false, 1, 1);
        }
        orig(self, st);
    }
    public static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    { // Lizard grabability for dragonriding and young lizards
        if (obj is Lizard liz)
        {
            if (Plugin.LizRide() && liz.Template.type != CreatureTemplateType.YoungLizard)
            {
                if (liz.GetLiz() != null && liz.GetLiz().IsRideable)
                {
                    if (liz?.Template?.type != CreatureTemplateType.MotherLizard && liz?.AI?.DynamicRelationship(self?.abstractCreature).type != CreatureTemplate.Relationship.Type.Attacks && liz?.AI?.DynamicRelationship(self?.abstractCreature).type != CreatureTemplate.Relationship.Type.Eats && liz?.AI?.friendTracker?.friend != null && liz?.AI?.friendTracker?.friendRel?.like < 0.5f && !liz.dead && !liz.Stunned) return Player.ObjectGrabability.CantGrab;
                    if ((liz.GetLiz().IsBeingRidden || self.GetPoacher().grabCounter > 0 || liz?.AI?.LikeOfPlayer(liz?.AI?.tracker?.RepresentationForCreature(self?.abstractCreature, true)) < 0) && !liz.dead && !liz.Stunned) return Player.ObjectGrabability.CantGrab;
                    self.GetPoacher().grabCounter = 15;
                    return Player.ObjectGrabability.OneHand;
                }
            }
            else if (liz.Template.type == CreatureTemplateType.YoungLizard)
            {
                for (int i = 0; i < self?.grasps?.Count(); i++) if ((self?.grasps[i]?.grabbed as Creature)?.Template?.type == CreatureTemplateType.YoungLizard) return Player.ObjectGrabability.CantGrab;
                return Player.ObjectGrabability.OneHand;
            }
            return orig(self, obj);
        }
        if (self.slugcatStats.name == DragonName && self.GetPoacher().IsInIntro && obj is Weapon) return Player.ObjectGrabability.CantGrab;
        return orig(self, obj);
    }
    public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self == null || self.room == null) { Debug.Log("Solace: Player returned null, cancelling PlayerUpdate code"); return; }

        // Moon mark
        if (self.GetPoacher().JustGotMoonMark && !self.GetPoacher().MoonMarkPassed)
        {
            Debug.Log("Solace: PlayerUpdate JustGotMoonMark check passed");
            self.Stun(20);
            self.GetPoacher().MarkExhaustion = (int)((1 / self.slugcatStats.bodyWeightFac) * 200); self.GetPoacher().MoonMarkPassed = true;
        }
        if (self.GetPoacher().MarkExhaustion > 0 && self.GetPoacher().JustGotMoonMark)
        {
            Debug.Log("Solace: PlayerUpdate MarkExhaustion check passed");
            self.GetPoacher().MarkExhaustion--;
            self.exhausted = true;
            self.aerobicLevel = (self.slugcatStats.bodyWeightFac < 0.5f) ? 1.5f : 1.1f;
            (self.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * 0.2f;
        }

        // Poacher
        if (self.slugcatStats.name == DragonName)
        {
            if (self.input[0].y < 1 || !self.input[0].pckp) self.GetPoacher().isMakingPoppers = false;
            self.Hypothermia += self.HypothermiaGain * (Plugin.PoacherFreezeFaster() ? 1.2f : 0.2f);
            FamineWorld.PoacherEats(self);
            if (self.dangerGraspTime > 0)
            {
                self.stun = 0;
                if (self.input[0].thrw) self.ThrowToGetFree(eu);
                if (self.input[0].pckp) self.DangerGraspPickup(eu);
            }
        }

        // Dragonriding
        if (self.GetPoacher().grabCounter > 0) // Stops player from getting slam dunked into the floor when they dismount
        {
            self.GetPoacher().grabCounter--;
            for (int i = 0; i < 2; i++)
                if (self.bodyChunks[i].vel.y < -20) self.bodyChunks[i].vel.y = -20;
        }
        if (self.GetPoacher().isRidingLizard && (self.GetPoacher().dragonSteed as Lizard).GetLiz() != null)
        {
            var liz = self?.GetPoacher()?.dragonSteed as Lizard;
            try
            {
                self.standing = true;
                if (liz.animation != Lizard.Animation.Lounge &&
                    liz.animation != Lizard.Animation.PrepareToLounge &&
                    liz.animation != Lizard.Animation.ShootTongue &&
                    liz.animation != Lizard.Animation.Spit &&
                    liz.animation != Lizard.Animation.HearSound &&
                    liz.animation != Lizard.Animation.PreyReSpotted &&
                    liz.animation != Lizard.Animation.PreySpotted &&
                    liz.animation != Lizard.Animation.ThreatReSpotted &&
                    liz.animation != Lizard.Animation.ThreatSpotted) liz.JawOpen = 0;
                DragonRiding.DragonRiderSafety(self, self.GetPoacher().dragonSteed, (self.GetPoacher().dragonSteed as Lizard).GetLiz().seat0);
                if ((self?.input[0].y < 0 && self.input[0].pckp) ||
                    (self?.GetPoacher()?.dragonSteed as Lizard).AI?.LikeOfPlayer((self?.GetPoacher()?.dragonSteed as Lizard).AI?.tracker?.RepresentationForCreature(self?.abstractCreature, true)) <= 0 ||
                    self.dead ||
                    self.Stunned ||
                    (self?.room != self?.GetPoacher()?.dragonSteed?.room && self.room != null))
                    DragonRiding.DragonRideReset(self.GetPoacher().dragonSteed, self);
            }
            catch (Exception e) { Debug.Log("Solace: Exception occurred in Player.Update LizRide" + e); }

            // Pointing (ONWARDS, STEED!)
            Vector2 pointPos = new Vector2(self.input[0].x * 50, self.input[0].y * 50) + self.bodyChunks[0].pos;
            var graph = self.graphicsModule as PlayerGraphics;
            var hand = ((pointPos - self.mainBodyChunk.pos).x < 0 || self?.grasps[0]?.grabbed is Spear) ? 0 : 1;
            if (self?.grasps[1]?.grabbed is Spear) hand = 1;
            var nothand = (hand == 1) ? 0 : 1;

            for (int i = 0; i < 2; i++)
            {
                if (self?.grasps[i]?.grabbed is Spear && !(self?.grasps[0]?.grabbed == self?.grasps[1]?.grabbed)) hand = i;
            }
            try
            {
                graph.LookAtPoint(pointPos, 0f);
                graph.hands[hand].absoluteHuntPos = pointPos;
                if (self.GetPoacher().dragonSteed != null) graph.hands[nothand].absoluteHuntPos = self.GetPoacher().dragonSteed.firstChunk.pos;
                graph.hands[hand].reachingForObject = true;
                graph.hands[nothand].reachingForObject = true;
            }
            catch (Exception) { Debug.Log("Solace: Harmless exception happened in Player.Update riderHand"); }
        }

        // Friend stuff
        if (self.slugcatStats.name == FriendName)
        {
            AbstractCreature guide0;
            AbstractCreature guide1;
            if (self?.room?.world?.overseersWorldAI?.playerGuide == null && !self.room.world.game.IsArenaSession)
            {
                WorldCoordinate pos = new WorldCoordinate(self.room.world.offScreenDen.index, -1, -1, 0);
                guide0 = new AbstractCreature(self.room.game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, pos, new EntityID());
                guide1 = new AbstractCreature(self.room.game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, pos, new EntityID());
                self.room.world.GetAbstractRoom(pos).entitiesInDens.Add(guide0);
                self.room.world.GetAbstractRoom(pos).entitiesInDens.Add(guide1);
                guide0.ignoreCycle = true;
                guide1.ignoreCycle = true;
                (guide0.abstractAI as OverseerAbstractAI).SetAsPlayerGuide(0);
                (guide1.abstractAI as OverseerAbstractAI).SetAsPlayerGuide(1);
                (guide0.abstractAI as OverseerAbstractAI).BringToRoomAndGuidePlayer(self.room.abstractRoom.index);
                (guide1.abstractAI as OverseerAbstractAI).BringToRoomAndGuidePlayer(self.room.abstractRoom.index);
                if (self.GetPoacher().Wiggy == null) self.GetPoacher().Wiggy = guide0.realizedCreature as Overseer;
                if (self.GetPoacher().Iggy == null) self.GetPoacher().Iggy = guide1.realizedCreature as Overseer;
            }
            // overseer code made with HUGE help from Leo, creator of the Lost!
            if (self.animation != ind.RocketJump && self.GetPoacher().HighJumped) self.GetPoacher().HighJumped = false;
        }
    }
    public static void Player_GraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
    { // Spear pointing while on a lizard
        orig(self, actuallyViewed, eu);
        try
        {
            if (self != null && self.GetPoacher().dragonSteed != null && self.GetPoacher().isRidingLizard)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (self?.grasps[i] != null && self?.grasps[i]?.grabbed != null && self?.grasps[i]?.grabbed is Weapon)
                    {
                        float rotation = (i == 1) ? self.GetPoacher().pointDir1 + 90 : self.GetPoacher().pointDir0 + 90f;
                        Vector2 vec = Custom.DegToVec(rotation);
                        (self?.grasps[i]?.grabbed as Weapon).setRotation = vec; //new Vector2(self.input[0].x*10, self.input[0].y*10);
                        (self?.grasps[i]?.grabbed as Weapon).rotationSpeed = 0f;
                    }
                }
            }
        }
        catch (Exception e) { Debug.Log("Solace: Exception occurred in Player.GraphicsModuleUpdated" + e); }
    }
    public static bool Player_HeavyCarry(On.Player.orig_HeavyCarry orig, Player self, PhysicalObject obj)
    { // Allows Poacher to carry things that they couldn't usually
        if (obj is Creature young && young.Template.type == CreatureTemplateType.YoungLizard) return false;
        else if (obj is Lizard mother && mother.GetLiz() != null && mother.GetLiz().IsRideable) return true;
        if (self.slugcatStats.name == DragonName)
        {
            if (obj is Creature crit && crit is not Hazer && crit is not VultureGrub && crit is not Snail && crit is not SmallNeedleWorm && crit is not TubeWorm) return orig(self, obj);
            else if (obj is MoreSlugcats.DandelionPeach || obj is DangleFruit)
            {
                if (!Plugin.PoacherFoodParkour()) return false;
                else return true;
            }
            else return false;
        }
        else return orig(self, obj);
    }
    public static void DandelionPeach_Update(On.MoreSlugcats.DandelionPeach.orig_Update orig, MoreSlugcats.DandelionPeach self, bool eu)
    { // Poacher food parkour
        orig(self, eu);
        if (!Plugin.PoacherFoodParkour()) return;
        if (self.grabbedBy.Count > 0)
        {
            for (int i = 0; i < self.grabbedBy.Count; i++)
            {
                if (self.grabbedBy[i].grabber is Player player && player.slugcatStats.name == DragonName)
                {
                    if (player.animation == ind.None && player.bodyMode != bod.Stand) { self.firstChunk.mass = 0.34f; }
                    else self.firstChunk.mass = 0f;
                }
            }
        }
        else self.firstChunk.mass = 0.34f;
    }
    public static void LanternMouse_Update(On.LanternMouse.orig_Update orig, LanternMouse self, bool eu)
    { // Fixes Poacher unable to use lantern mice
        orig(self, eu);
        if (self.grabbedBy.Count > 0)
        {
            for (int i = 0; i < self.grabbedBy.Count; i++)
            {
                if (self.grabbedBy[i].grabber is Player player && player.slugcatStats.name == DragonName)
                {
                    if (player.animation == ind.None && player.bodyMode != bod.Stand) { self.bodyChunks[0].mass = 0.2f; self.bodyChunks[1].mass = 0.2f; }
                    else { self.bodyChunks[0].mass = 0f; self.bodyChunks[1].mass = 0f; }
                }
            }
        }
        else { self.bodyChunks[0].mass = 0.4f / 2f; self.bodyChunks[1].mass = 0.4f / 2f; }
    }
    public static void DangleFruit_Update(On.DangleFruit.orig_Update orig, DangleFruit self, bool eu)
    { // Poacher food parkour
        orig(self, eu);
        if (!Plugin.PoacherFoodParkour()) return;
        if (self.grabbedBy.Count > 0)
        {
            for (int i = 0; i < self.grabbedBy.Count; i++)
            {
                if (self.grabbedBy[i].grabber is Player player && player.slugcatStats.name == DragonName)
                {
                    if (player.animation == ind.None && player.bodyMode != bod.Stand) { self.firstChunk.mass = 0.2f; }
                    else self.firstChunk.mass = 0f;
                }
            }
        }
        else self.firstChunk.mass = 0.2f;
    }
    public static bool SlugcatStats_SlugcatCanMaul(On.SlugcatStats.orig_SlugcatCanMaul orig, SlugcatStats.Name slugcatNum)
    { // Friend maul
        if (SlugBase.SlugBaseCharacter.TryGet(slugcatNum, out var chara) && Plugin.MaulEnabled.TryGet(chara, out var canMaul) && canMaul)
            return true;
        else
            return orig(slugcatNum);
    }
    public static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    { // Friend and Poacher backspears, Poacher cutscene preparation
        orig(self, abstractCreature, world);
        try
        {
            if (self.slugcatStats.name == FriendName && Plugin.FriendBackspear() == true && self != null)
            {
                self.spearOnBack = new Player.SpearOnBack(self);
            }
            if (self.slugcatStats.name == DragonName && self != null)
            {
                self.setPupStatus(true);
                self.GetPoacher().isPoacher = true;
                self.GetPoacher().IsSkullVisible = true;
                if (Plugin.PoacherBackspear() == true) self.spearOnBack = new Player.SpearOnBack(self);
            }
        }
        catch (Exception e) { Debug.Log("Solace: Player.ctor hook failed" + e); }
    }
    public static void Player_UpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
    { // Friend fast crawl
        orig(self);

        if (!self.standing && Plugin.SuperCrawl.TryGet(self, out var boost))
        {
            if (self.superLaunchJump >= 20) self.GetPoacher().longjump = true;
            if (self.bodyMode == bod.Crawl)
            {
                self.dynamicRunSpeed[0] += boost;
                self.dynamicRunSpeed[1] += boost;
            }
            if (self.bodyMode == bod.Default)
            {
                if (!self.GetPoacher().HighJumped)
                {
                    self.dynamicRunSpeed[0] += boost - 1f;
                    self.dynamicRunSpeed[1] += boost - 1f;
                }
            }
            if (self.GetPoacher().longjump == true && self.superLaunchJump == 0)
            {
                if (self.GetPoacher().WantsUp)
                {
                    self.animation = ind.RocketJump;
                    self.GetPoacher().HighJumped = true;
                    self.bodyChunks[0].vel.y *= 4;
                    self.bodyChunks[0].vel.x *= 0.3f;
                    self.bodyChunks[1].vel.x *= 0.3f;
                }
                else
                {
                    self.bodyChunks[0].vel.x *= 1.1f;
                    self.bodyChunks[1].vel.x *= 1.1f;
                }
                self.GetPoacher().longjump = false;
            }
        }
        if (self.slugcatStats.name != FriendName) return;

        // Friend pole leap
        if (self.GetPoacher().polejump)
        {
            if (self.GetPoacher().upwardpolejump)
            {
                self.bodyChunks[0].vel.y += 21;
                self.bodyChunks[0].vel.x += self.flipDirection*1.5f;
                self.bodyChunks[1].vel.x += self.flipDirection*1.5f;
            }
            else
            {
                self.bodyChunks[0].vel.y += 10;
                self.bodyChunks[0].vel.x += (self.flipDirection * 9) + (self.input[0].x * 2);
                self.bodyChunks[1].vel.x += (self.flipDirection * 9) + (self.input[0].x * 2);
                self.GetPoacher().upwardpolejump = false;
            }
            self.animation = ind.RocketJump;
            self.room.PlaySound(SoundID.Slugcat_Super_Jump, self.firstChunk);
            self.room.PlaySound(SoundID.Slugcat_Skid_On_Ground_Init, self.firstChunk);

            self.room.AddObject(new WaterDrip(self.bodyChunks[1].pos + new Vector2(0f, -self.bodyChunks[1].rad + 1f), Custom.DegToVec(self.slideDirection * Mathf.Lerp(30f, 70f, Random.value)) * Mathf.Lerp(6f, 11f, Random.value), false));

            self.GetPoacher().polejump = false;
        }
        if (self.GetPoacher().DoingAPoleJump && !self.GetPoacher().upwardpolejump && self.input[0].y > 0) { self.animation = ind.None; self.standing = true; }
        else if (self.GetPoacher().upwardpolejump) self.standing = false;
        if (self.feetStuckPos != null || self.animation != ind.RocketJump) { self.GetPoacher().DoingAPoleJump = false; self.GetPoacher().upwardpolejump = false; }
    }
    public static void Player_Jump(On.Player.orig_Jump orig, Player self)
    { 
        var crawlFlip = !self.standing && self.slideCounter > 0 && self.slideCounter < 10;
        
        // Friend improved jumps
        if (!crawlFlip &&
            self.animation != ind.Roll &&
            !self.standing &&
            self.slugcatStats.name == FriendName &&
            Mathf.Abs(self.firstChunk.vel.x) > 3)
        {
            if (self.bodyChunks[1].contactPoint.y == 0 ||
                self.input.Count(i => i.jmp) == 9)
                return;
        }

        // Whiplash crawlturn jump
        if (self.SlugCatClass == FriendName && crawlFlip)
        {
            var flipDirNeg = self.flipDirection * -1;
            
            self.animation = ind.Flip;
            self.room.AddObject(new ExplosionSpikes(self.room, self.bodyChunks[1].pos + new Vector2(0.0f, -self.bodyChunks[1].rad), 8, 7f, 5f, 5.5f, 40f, new Color(1f, 1f, 1f, 0.5f)));
            int num3 = 1;
            for (int index = 1; index < 4 && !self.room.GetTile(self.bodyChunks[0].pos + new Vector2(index * -flipDirNeg * 15f, 0.0f)).Solid && !self.room.GetTile(self.bodyChunks[0].pos + new Vector2(index * -flipDirNeg * 15f, 20f)).Solid; ++index)
              num3 = index;
            self.bodyChunks[0].pos += new Vector2(flipDirNeg * (float) -(num3 * 15.0 + 8.0), 14f);
            self.bodyChunks[1].pos += new Vector2(flipDirNeg * (float) -(num3 * 15.0 + 2.0), 0.0f);
            self.bodyChunks[0].vel = new Vector2(flipDirNeg * -7f, 10f);
            self.bodyChunks[1].vel = new Vector2(flipDirNeg * -7f, 11f);
            self.flipFromSlide = true;
            self.whiplashJump = false;
            self.jumpBoost = 0.0f;
            self.room.PlaySound(SoundID.Slugcat_Sectret_Super_Wall_Jump, self.mainBodyChunk, false, 1f, 1f);
            
            if ((!(self.input[0].y > 0) && Plugin.FriendAutoCrouch()))
            {
                self.standing = false;
            }
            
            #region whiplash grab
            if (self.pickUpCandidate == null || !self.CanIPickThisUp(self.pickUpCandidate) || self.grasps[0] != null && self.grasps[1] != null || self.Grabability(self.pickUpCandidate) != Player.ObjectGrabability.OneHand && self.Grabability(self.pickUpCandidate) != Player.ObjectGrabability.BigOneHand)
              return;
            int graspUsed = self.grasps[0] == null ? 0 : 1;
            for (int index = 0; index < self.pickUpCandidate.grabbedBy.Count; ++index)
            {
              self.pickUpCandidate.grabbedBy[index].grabber.GrabbedObjectSnatched(self.pickUpCandidate.grabbedBy[index].grabbed, self);
              self.pickUpCandidate.grabbedBy[index].grabber.ReleaseGrasp(self.pickUpCandidate.grabbedBy[index].graspUsed);
            }
            self.SlugcatGrab(self.pickUpCandidate, graspUsed);
            if (self.pickUpCandidate is PlayerCarryableItem)
              (self.pickUpCandidate as PlayerCarryableItem).PickedUp(self);
            if (self.pickUpCandidate.graphicsModule == null)
              return;
            self.pickUpCandidate.graphicsModule.BringSpritesToFront();
            #endregion
            
            return;
        }
        
        orig(self);
        if (Plugin.SuperJump.TryGet(self, out var power))
        {
            if (Plugin.FriendUnNerf()) self.jumpBoost += 3f;
            else if (self.bodyMode == bod.Crawl) self.jumpBoost *= 1f + (power / 2);
            else self.jumpBoost += (power + 0.25f) * (self.animation == ind.StandOnBeam ? 0 : 1);

            if ((!(self.input[0].y > 0) && Plugin.FriendAutoCrouch()))
            {
                self.standing = false;
            }

        }
    }
    public static void Player_UpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self)
    { // Attempted ledgegrab fix, and increased polewalk speed
        orig(self);
        if (self.slugcatStats.name == FriendName)
        {
            if (self.animation == ind.LedgeGrab && self.input[0].y < 1) { self.standing = false; self.bodyMode = bod.Crawl; }
            if (self.animation == ind.StandOnBeam && self.input[0].y < 1 && Plugin.PoleCrawl() == true)
            {
                self.dynamicRunSpeed[0] = 2.1f + (self.slugcatStats.runspeedFac * 0.5f) * 4.5f;
                self.dynamicRunSpeed[1] = 2.1f + (self.slugcatStats.runspeedFac * 0.5f) * 4.5f;
            }
        }
    }
    public static void Player_WallJump(On.Player.orig_WallJump orig, Player self, int direction)
    { // Walljump fix
        orig(self, direction);
        if (self.slugcatStats.name == FriendName) self.standing = false;
    }
    public static void SlugcatStats_ctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
    { // Friend unnerfs
        orig(self, slugcat, malnourished);
        if (slugcat == FriendName && Plugin.FriendUnNerf() == true) self.poleClimbSpeedFac = 6f;
        if (slugcat == FriendName && Plugin.FriendUnNerf() == true) self.runspeedFac = 0.8f;
    }
    public static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    { // Friend leap mechanics
        var timer = self.GetPoacher().poleSuperJumpTimer;
        orig(self);
        if (self.slugcatStats.name != FriendName) return;

        if (self.GetPoacher().longjump && self.input[0].y == 0) self.GetPoacher().WantsUp = false;
        if (self.GetPoacher().longjump && self.input[0].y > 0)
        {
            self.GetPoacher().WantsUp = true;
            self.input[0].y = 0;
            self.input[0].x = 0;
        }
        if (self.input[0].y < 0 && self.superLaunchJump != 0)
        {
            self.superLaunchJump = 0;
            self.killSuperLaunchJumpCounter = 0;
            self.GetPoacher().WantsUp = false;
        }
        if (!self.GetPoacher().longjump) self.GetPoacher().WantsUp = false;

        // Friend pole leap mechanics
        if (self.animation == ind.StandOnBeam && self.GetPoacher().poleCrawlState)
        {
            if (self.input[0].y > 0) self.GetPoacher().YesIAmLookingUpStopThinkingOtherwise = true;
            else self.GetPoacher().YesIAmLookingUpStopThinkingOtherwise = false;
            if (self.input[0].y > 0) self.GetPoacher().upwardpolejump = true;
            else self.GetPoacher().upwardpolejump = false;
            if (timer >= 20)
            {
                for (int i = 0; i < self.input.Length; i++)
                {
                    self.input[i].x = 0;
                    if (self.input[i].y >= 0) 
                    { 
                        self.input[i].y = 0; 
                    }
                }
            }


            if (self.input[0].jmp && timer < 20) { self.GetPoacher().LetGoOfPoleJump = false; }
            else if (!self.input[0].jmp) { self.GetPoacher().LetGoOfPoleJump = true; }
            if (!self.GetPoacher().LetGoOfPoleJump)
            {
                if (self.input[0].x == 0) self.input[0].jmp = false;
                if (timer < 20) 
                {
                    if (self.input[0].x != 0 || self.input[0].y != 0) self.GetPoacher().poleSuperJumpTimer = 0;
                    else self.GetPoacher().poleSuperJumpTimer++; 
                }
            }
            if (self.GetPoacher().LetGoOfPoleJump) 
            {
                if (timer >= 20) { self.GetPoacher().polejump = true; self.GetPoacher().DoingAPoleJump = true; self.Jump(); }
                self.GetPoacher().poleSuperJumpTimer = 0;
            }
        }
        else { self.GetPoacher().poleSuperJumpTimer = 0; self.GetPoacher().LetGoOfPoleJump = false; }
    }
}
