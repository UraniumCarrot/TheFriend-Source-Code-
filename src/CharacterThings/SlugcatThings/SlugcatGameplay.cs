﻿using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using SlugBase;
using TheFriend.CharacterThings.BelieverThings;
using TheFriend.CharacterThings.DelugeThings;
using TheFriend.CharacterThings.FriendThings;
using TheFriend.DragonRideThings;
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
        
        IL.Player.MovementUpdate += PlayerOnMovementUpdate;
    }

    public static void PlayerOnMovementUpdate(ILContext il)
    {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.After,
            i => i.MatchCallOrCallvirt<Player>("get_playerState"),
            i => i.MatchLdfld<PlayerState>(nameof(PlayerState.isPup)),
            i => i.MatchBrtrue(out _),
            i => i.MatchLdcI4(out _));

        cursor.MoveAfterLabels();

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((int originalDistance, Player self) => 
            (self.TryGetFriend(out _) && Configs.CharHeight) ? originalDistance+4 : 
            (self.TryGetPoacher(out _) && Configs.CharHeight) ? originalDistance-3 :
            originalDistance);
    }

    public static readonly SlugcatStats.Name FriendName = Plugin.FriendName;
    public static readonly SlugcatStats.Name DragonName = Plugin.DragonName;
    
    public static void PlayerOnThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        var mine = self.grasps[grasp].grabbed as BoomMine;
        if (self.TryGetBeliever(out _)) 
            BelieverGameplay.PacifistThrow(self, grasp, eu);
        
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
        if (self.TryGetPoacher(out _))
            DragonCrafts.PoacherQuickCraft(self);
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
    { 
        if (obj is Lizard liz)
        { // Lizard grabability for dragonriding and young lizards
            var grab = DragonRiding.LizardGrabability(self, liz);
        
            if (grab == Player.ObjectGrabability.TwoHands)
                return orig(self, obj);
            else return grab;
        }
        
        if (self.TryGetPoacher( out var poacher) && poacher.IsInIntro && obj is Weapon) return Player.ObjectGrabability.CantGrab;
        if (obj is FakePlayerEdible edible) return edible.grabability;
        return orig(self, obj);
    }
    public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        
        if (self.GetGeneral().iHaveSenses) 
            SensoryHolograms.PlayerSensesUpdate(self);
        
        if (self?.room == null) { Debug.Log("Solace: Player returned null, cancelling PlayerUpdate code"); return; }
        
        if (self.TryGetPoacher(out _))
            PoacherGameplay.PoacherUpdate(self, eu);
        
        if (self.TryGetFriend(out _))
            FriendGameplay.FriendUpdate(self, eu);
        
        if (self.TryGetDeluge(out _))
            DelugeGameplay.DelugeUpdate(self, eu);
        
        //var coord = self.abstractCreature.pos;
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
            
            DragonRiding.DragonRiderPoint(self);
        }
    }
    public static void Player_GraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
    {
        orig(self, actuallyViewed, eu);
        try
        {
            DragonRiding.DragonRiderSpearPoint(self);
        }
        catch (Exception e) { Debug.Log("Solace: Exception occurred in Player.GraphicsModuleUpdated" + e); }
    }
    
    public static bool SlugcatStats_SlugcatCanMaul(On.SlugcatStats.orig_SlugcatCanMaul orig, SlugcatStats.Name slugcatNum)
    {
        if (slugcatNum == FriendName)
            return true; // Friend maul
        else
            return orig(slugcatNum);
    }
    public static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    { // Friend and Poacher backspears, Poacher cutscene preparation
        orig(self, abstractCreature, world);
        try
        {
            if (self.TryGetFriend(out _))
                FriendGameplay.FriendConstructor(self);
            
            if (self.TryGetPoacher(out _))
                PoacherGameplay.PoacherConstructor(self);
            
            if (self.TryGetDeluge(out _))
                DelugeGameplay.DelugeConstructor(self);
        }
        catch (Exception e) { Debug.Log("Solace: Player.ctor hook failed" + e); }
    }
    public static void Player_UpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
    { // Friend fast crawl
        orig(self);
        if (self.TryGetFriend(out _))
            FriendGameplay.FriendMovement(self);
    }
    public static void Player_Jump(On.Player.orig_Jump orig, Player self)
    {
        bool isFriend = self.TryGetFriend(out _);
        
        if (isFriend) 
            FriendGameplay.FriendJump1(self);
        
        orig(self);
        
        if (self.TryGetPoacher(out _) && Configs.PoacherJumpNerf)
            PoacherGameplay.PoacherJump(self);
        else if (isFriend)
            FriendGameplay.FriendJump2(self);
        else if (self.TryGetDeluge(out _)) 
            DelugeGameplay.DelugeSiezeJump(self);
    }
    public static void Player_UpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self)
    { 
        orig(self);
        if (self.TryGetFriend(out _))
            FriendGameplay.FriendLedgeFix(self); 
    }
    public static void Player_WallJump(On.Player.orig_WallJump orig, Player self, int direction)
    { 
        orig(self, direction);
        if (self.TryGetFriend(out _)) 
            FriendGameplay.FriendWalljumpFix(self);
    }
    public static void SlugcatStats_ctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
    { // Friend unnerfs
        orig(self, slugcat, malnourished);
        if (slugcat == FriendName)
            FriendGameplay.FriendStats(self);
    }
    public static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    {
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

        if (self.TryGetFriend(out _)) 
            FriendGameplay.FriendLeapController(self, timer);

        if (self.TryGetDeluge(out _)) 
            DelugeGameplay.DelugeSprintCheck(self);
    }
}
