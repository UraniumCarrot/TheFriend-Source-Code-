using System;
using System.Linq;
using RWCustom;
using SlugBase;
using TheFriend.CharacterThings.BelieverThings;
using TheFriend.CharacterThings.FriendThings;
using TheFriend.FriendThings;
using TheFriend.Objects.BoomMineObject;
using TheFriend.Objects.FakePlayerEdible;
using TheFriend.PoacherThings;
using UnityEngine;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;


namespace TheFriend.SlugcatThings;

public class SlugcatGameplay
{
    public static void Apply()
    {
        On.Player.Update += Player_Update;
        On.Player.ThrowObject += PlayerOnThrowObject;
        On.Weapon.HitThisObject += Weapon_HitThisObject;
        On.Player.WallJump += Player_WallJump;
        On.Player.Jump += Player_Jump;
        On.Player.ctor += Player_ctor;
        On.Player.Grabability += Player_Grabability;
        On.Player.GrabUpdate += Player_GrabUpdate;
        On.Player.UpdateBodyMode += Player_UpdateBodyMode;
        On.Player.UpdateAnimation += Player_UpdateAnimation;
        On.Player.checkInput += Player_checkInput;
        On.SlugcatStats.SlugcatCanMaul += SlugcatStats_SlugcatCanMaul;
        On.SlugcatStats.ctor += SlugcatStats_ctor;

        // Misc gameplay changes

    }

    public static readonly SlugcatStats.Name FriendName = Plugin.FriendName;
    public static readonly SlugcatStats.Name DragonName = Plugin.DragonName;
    
    public static void PlayerOnThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        var mine = self.grasps[grasp].grabbed as BoomMine;
        if (self.TryGetBeliever(out var believer))
        {
            BelieverGameplay.PacifistThrow(self, grasp, eu);
        }
        orig(self, grasp, eu);
        if (mine != null) mine.ExplodeTimer = 5;  // Boommine cooldown reduction if thrown
    }
    public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    { // Makes player ride lizard
        orig(self, eu);
        if (self == null) return;

        for (int i = 0; i < 2; i++)
        {
            if (self.grasps[i]?.grabbed is Lizard liz && 
                liz.GetLiz() != null && 
                liz.GetLiz().IsRideable && 
                !liz.dead && 
                !liz.Stunned && 
                liz.AI?.LikeOfPlayer(liz.AI?.tracker?.RepresentationForCreature(self.abstractCreature, true)) > 0)
            {
                if (!liz.GetLiz().boolseat0)
                {
                    self.grasps[i].Release();
                    self.GetGeneral().isRidingLizard = true;
                    DragonRiding.DragonRidden(liz, self);
                    liz.GetLiz().boolseat0 = true;
                    self.GetGeneral().dragonSteed = liz;
                    liz.GetLiz().rider = self;
                }
            }
        }
        // Poacher poppers quickcraft
        if (self.TryGetPoacher(out var poacher))
        {
            if (self.craftingObject && self.GetPoacher().isMakingPoppers && 
                self.grasps.Count(i => i.grabbed is FirecrackerPlant) == 1 && 
                self.swallowAndRegurgitateCounter < 70) 
                self.swallowAndRegurgitateCounter = 70;
        }
    }
    public static bool Weapon_HitThisObject(On.Weapon.orig_HitThisObject orig, Weapon self, PhysicalObject obj)
    { // Lizard mount will not be hit by owner's weapons
        if (obj is Lizard liz &&
            liz.GetLiz().rider != null && 
            self.thrownBy is Player pl && 
            pl.GetGeneral().dragonSteed == liz) 
            return false;
        else return orig(self, obj);
    }
    
    public static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    { // Lizard grabability for dragonriding and young lizards
        if (obj is Lizard liz)
        {
            if (Plugin.LizRide() && liz.Template.type != CreatureTemplateType.YoungLizard)
            {
                if (liz.GetLiz().IsRideable)
                {
                    if (liz.Template?.type != CreatureTemplateType.MotherLizard && 
                        liz.AI?.DynamicRelationship(self?.abstractCreature).type != CreatureTemplate.Relationship.Type.Attacks && 
                        liz.AI?.DynamicRelationship(self?.abstractCreature).type != CreatureTemplate.Relationship.Type.Eats && 
                        liz.AI?.friendTracker?.friend != null && 
                        liz.AI?.friendTracker?.friendRel?.like < 0.5f && 
                        !liz.dead && 
                        !liz.Stunned) 
                        return Player.ObjectGrabability.CantGrab;
                    if ((liz.GetLiz().rider != null || 
                         self.GetGeneral().grabCounter > 0 || 
                         liz.AI?.LikeOfPlayer(liz.AI?.tracker?.RepresentationForCreature(self?.abstractCreature, true)) < 0) && 
                        !liz.dead && 
                        !liz.Stunned) 
                        return Player.ObjectGrabability.CantGrab;
                    self.GetGeneral().grabCounter = 15;
                    return Player.ObjectGrabability.OneHand;
                }
            }
            else if (liz.Template.type == CreatureTemplateType.YoungLizard)
            {
                for (int i = 0; i < self?.grasps?.Length; i++) 
                    if ((self.grasps[i]?.grabbed as Creature)?.Template?.type == CreatureTemplateType.YoungLizard) 
                        return Player.ObjectGrabability.CantGrab;
                return Player.ObjectGrabability.OneHand;
            }
            return orig(self, obj);
        }
        if (self.TryGetPoacher( out var poacher) && poacher.IsInIntro && obj is Weapon) return Player.ObjectGrabability.CantGrab;
        if (obj is FakePlayerEdible edible) return edible.grabability;
        return orig(self, obj);
    }
    public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self?.room == null) { Debug.Log("Solace: Player returned null, cancelling PlayerUpdate code"); return; }
        
        var coord = self.abstractCreature.pos;
        //Debug.Log("Your room coordinate is: room " + coord.room + ", x " + coord.x + ", y " + coord.y + ", abstractNode " + coord.abstractNode);
        // Moon mark
        if (self.GetGeneral().JustGotMoonMark && !self.GetGeneral().MoonMarkPassed)
        {
            self.Stun(20);
            self.GetGeneral().MarkExhaustion = (int)((1 / self.slugcatStats.bodyWeightFac) * 200); self.GetGeneral().MoonMarkPassed = true;
        }
        if (self.GetGeneral().MarkExhaustion > 0 && self.GetGeneral().JustGotMoonMark)
        {
            self.GetGeneral().MarkExhaustion--;
            self.exhausted = true;
            self.aerobicLevel = (self.slugcatStats.bodyWeightFac < 0.5f) ? 1.5f : 1.1f;
            (self.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * 0.2f;
        }

        // Poacher
        if (self.TryGetPoacher(out var poacher))
        {
            PoacherGameplay.PoacherUpdate(self, eu);
        }

        // Dragonriding
        if (self.GetGeneral().grabCounter > 0) // Stops player from getting slam dunked into the floor when they dismount
        {
            self.GetGeneral().grabCounter--;
            for (int i = 0; i < 2; i++)
                if (self.bodyChunks[i].vel.y < -20) self.bodyChunks[i].vel.y = -20;
        }
        if (self.GetGeneral().isRidingLizard && (self.GetGeneral().dragonSteed as Lizard).GetLiz() != null)
        {
            var liz = self.GetGeneral()?.dragonSteed as Lizard;
            try
            {
                self.standing = true;
                if (liz != null)
                {
                    if (liz.animation != Lizard.Animation.Lounge &&
                        liz.animation != Lizard.Animation.PrepareToLounge &&
                        liz.animation != Lizard.Animation.ShootTongue &&
                        liz.animation != Lizard.Animation.Spit &&
                        liz.animation != Lizard.Animation.HearSound &&
                        liz.animation != Lizard.Animation.PreyReSpotted &&
                        liz.animation != Lizard.Animation.PreySpotted &&
                        liz.animation != Lizard.Animation.ThreatReSpotted &&
                        liz.animation != Lizard.Animation.ThreatSpotted) liz.JawOpen = 0;
                    DragonRiding.DragonRiderSafety(self, self.GetGeneral().dragonSteed, (self.GetGeneral().dragonSteed as Lizard).GetLiz().seat0);
                    if ((self.GetGeneral().UnchangedInputForLizRide[0].y < 0 && self.input[0].pckp) ||
                        (self.GetGeneral().dragonSteed as Lizard)?.AI?.LikeOfPlayer((self.GetGeneral().dragonSteed as Lizard)?.AI?.tracker?.RepresentationForCreature(self.abstractCreature, true)) <= 0 ||
                        self.dead ||
                        self.Stunned ||
                        (self.room != self.GetGeneral()?.dragonSteed?.room && self.room != null))
                        DragonRiding.DragonRideReset(self.GetGeneral().dragonSteed, self);
                }
            }
            catch (Exception e) { Debug.Log("Solace: Exception occurred in Player.Update LizRide" + e); }

            // Pointing (ONWARDS, STEED!)
            var oldinput = self.GetGeneral().UnchangedInputForLizRide;
            Vector2 pointPos = new Vector2(oldinput[0].x * 50, oldinput[0].y * 50) + self.bodyChunks[0].pos;
            var graph = self.graphicsModule as PlayerGraphics;
            var hand = ((pointPos - self.mainBodyChunk.pos).x < 0 || self?.grasps[0]?.grabbed is Spear) ? 0 : 1;
            if (self?.grasps[1]?.grabbed is Spear) hand = 1;
            var nothand = (hand == 1) ? 0 : 1;

            for (int i = 0; i < 2; i++)
            {
                if (self?.grasps[i]?.grabbed is Spear && self?.grasps[0]?.grabbed != self?.grasps[1]?.grabbed) hand = i;
            }
            try
            {
                graph.LookAtPoint(pointPos, 0f);
                graph.hands[hand].absoluteHuntPos = pointPos;
                if (self.GetGeneral().dragonSteed != null) graph.hands[nothand].absoluteHuntPos = self.GetGeneral().dragonSteed.firstChunk.pos;
                graph.hands[hand].reachingForObject = true;
                graph.hands[nothand].reachingForObject = true;
            }
            catch (Exception) { Debug.Log("Solace: Harmless exception happened in Player.Update riderHand"); }
        }

        // Friend stuff
        if (self.TryGetFriend(out var friend))
        {
            FriendGameplay.FriendUpdate(self, eu);
        }
    }
    public static void Player_GraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
    { // Spear pointing while on a lizard
        orig(self, actuallyViewed, eu);
        try
        {
            if (self != null && self.GetGeneral().dragonSteed != null && self.GetGeneral().isRidingLizard)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (self.grasps[i] != null && self.grasps[i]?.grabbed != null && self.grasps[i]?.grabbed is Weapon wep)
                    {
                        float rotation = i == 1 ? self.GetGeneral().pointDir1 + 90 : self.GetGeneral().pointDir0 + 90f;
                        Vector2 vec = Custom.DegToVec(rotation);
                        (wep).setRotation = vec;
                        (wep).rotationSpeed = 0f;
                    }
                }
            }
        }
        catch (Exception e) { Debug.Log("Solace: Exception occurred in Player.GraphicsModuleUpdated" + e); }
    }
    
    public static bool SlugcatStats_SlugcatCanMaul(On.SlugcatStats.orig_SlugcatCanMaul orig, SlugcatStats.Name slugcatNum)
    {
        if (SlugBaseCharacter.TryGet(slugcatNum, out var chara) && Plugin.MaulEnabled.TryGet(chara, out var canMaul) && canMaul)
            return true; // Friend maul
        else
            return orig(slugcatNum);
    }
    public static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    { // Friend and Poacher backspears, Poacher cutscene preparation
        orig(self, abstractCreature, world);
        try
        {
            if (self.TryGetFriend(out var friend) && Plugin.FriendBackspear())
            {
                self.spearOnBack = new Player.SpearOnBack(self);
            }
            if (self.TryGetPoacher(out var poacher))
            {
                self.setPupStatus(true);
                self.GetPoacher().IsSkullVisible = true;
                if (Plugin.PoacherBackspear()) self.spearOnBack = new Player.SpearOnBack(self);
            }
        }
        catch (Exception e) { Debug.Log("Solace: Player.ctor hook failed" + e); }
    }
    public static void Player_UpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
    { // Friend fast crawl
        orig(self);
        if (self.TryGetFriend(out var friend))
            FriendGameplay.FriendMovement(self);
    }
    public static void Player_Jump(On.Player.orig_Jump orig, Player self)
    {
        bool isFriend = self.TryGetFriend(out var friend);
        
        if (isFriend) FriendGameplay.FriendJump1(self);
        
        orig(self);
        
        if (isFriend) FriendGameplay.FriendJump2(self);
    }
    public static void Player_UpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self)
    { 
        orig(self);
        if (self.TryGetFriend(out var friend))
        { // Attempted ledgegrab fix, and increased polewalk speed
            if (self.animation == ind.LedgeGrab && self.input[0].y < 1) { self.standing = false; self.bodyMode = bod.Crawl; }
            if (self.animation == ind.StandOnBeam && self.input[0].y < 1 && Plugin.PoleCrawl())
            {
                self.dynamicRunSpeed[0] = 2.1f + (self.slugcatStats.runspeedFac * 0.5f) * 4.5f;
                self.dynamicRunSpeed[1] = 2.1f + (self.slugcatStats.runspeedFac * 0.5f) * 4.5f;
            }
        }
    }
    public static void Player_WallJump(On.Player.orig_WallJump orig, Player self, int direction)
    { 
        orig(self, direction);
        if (self.TryGetFriend(out var friend)) self.standing = false; // Walljump fix
    }
    public static void SlugcatStats_ctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
    { // Friend unnerfs
        orig(self, slugcat, malnourished);
        if (slugcat == FriendName && Plugin.FriendUnNerf())
        {
            self.poleClimbSpeedFac = 6f;
            self.runspeedFac = 0.8f;
        }
    }
    public static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    { // Friend leap mechanics
        var timer = self.GetFriend().poleSuperJumpTimer;
        orig(self);
        
        //Moving all inputs one slot up
        for (var i = self.GetGeneral().UnchangedInputForLizRide.Length - 1; i > 0; i--)
        {
            self.GetGeneral().UnchangedInputForLizRide[i] = self.GetGeneral().UnchangedInputForLizRide[i - 1];
        }
        //Copying original unmodified input
        self.GetGeneral().UnchangedInputForLizRide[0] = self.input[0];
        // Elliot note: Thanks Noir <3

        if (self.GetGeneral().isRidingLizard)
        {
            self.input[0].y = 0;
            self.input[0].x = 0;
            self.input[0].jmp = false;
        }

        if (self.TryGetFriend(out var friend)) FriendGameplay.FriendLeapController(self, timer);
        
    }
}
