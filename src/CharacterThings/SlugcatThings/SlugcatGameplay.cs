﻿using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using TheFriend.CharacterThings.FriendThings;
using TheFriend.Creatures.LizardThings.DragonRideThings;
using TheFriend.FriendThings;
using TheFriend.Objects.BoomMineObject;
using TheFriend.Objects.FakePlayerEdible;
using TheFriend.Objects.SolaceScarfObject;
using TheFriend.PoacherThings;
using UnityEngine;


namespace TheFriend.SlugcatThings;

public class SlugcatGameplay
{
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
        orig(self, grasp, eu);
        if (mine != null) mine.ExplodeTimer = 5;  // Boommine cooldown reduction if thrown
    }
    public static void Player_GrabUpdate(Player self, bool eu)
    { // Makes player ride lizard
        for (int i = 0; i < 2; i++)
            if (self.grasps[i] != null && self.grasps[i].grabbed.TryGetLiz(out var data))
                if (data.RideEnabled &&
                    data.DoILikeYou(self))
                {
                    self.GetGeneral().dragonSteed = self.grasps[i]?.grabbed as Lizard;
                    self.GetGeneral().isRidingLizard = true;
                    if (!data.mainRiders.Contains(self)) data.mainRiders.Add(self);
                }

        // Poacher poppers quickcraft
        if (self.TryGetPoacher(out _))
            DragonCrafts.PoacherQuickCraft(self);
    }

    public static Player.ObjectGrabability? Player_Grabability(Player self, PhysicalObject obj)
    { 
        if (obj is SolaceScarf scarf) 
            if (scarf.wearer != null) return Player.ObjectGrabability.CantGrab;
            else return Player.ObjectGrabability.OneHand;
        if (obj is Lizard liz)
        { // Lizard grabability for dragonriding and young lizards
            var grab = LizardRideFixes.LizardGrabability(self, liz);
            
            if (grab == Player.ObjectGrabability.TwoHands)
                return null;
            else return grab;
        }
        
        if (obj is Player pl && pl.GetGeneral().dragonSteed != null) return Player.ObjectGrabability.CantGrab;
        if (self.TryGetPoacher(out var poacher) && poacher.IsInIntro && obj is Weapon) return Player.ObjectGrabability.CantGrab;
        if (obj is FakePlayerEdible edible) return edible.grabability;
        return null;
    }
    public static void Player_Update(Player self, bool eu)
    {
        if (self.GetGeneral().iHaveSenses) 
            SensoryHolograms.PlayerSensesUpdate(self);
        
        if (self?.room == null) { Debug.Log("Solace: Player returned null, cancelling PlayerUpdate code"); return; }
        
        if (self.TryGetPoacher(out _))
            PoacherGameplay.PoacherUpdate(self, eu);
        
        if (self.TryGetFriend(out _))
            FriendGameplay.FriendUpdate(self, eu);
        
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
            (self.graphicsModule as PlayerGraphics)!.head.vel += Custom.RNV() * 0.2f;
        }
        
        // Dragonriding
        if (self.GetGeneral().isRidingLizard && self.GetGeneral().dragonSteed != null)
        {
            var liz = self.GetGeneral().dragonSteed;
            var myIndex = liz.Liz().mainRiders.IndexOf(self);
            var seat = liz.Liz().seats[myIndex];
            try
            {
                liz.JawOpen = 0;
                DragonRiding.DragonRiderSafety(self, liz, seat.pos, eu);
                if ((self.GetGeneral().UnchangedInputForLizRide[0].y < 0 && self.input[0].pckp) ||
                    !liz.Liz().DoILikeYou(self) ||
                    (self.room != liz.room && self.room != null))
                    DragonRiding.DragonRideReset(liz, self);
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
        }
        catch (Exception e) { Debug.Log("Solace: Player.ctor hook failed" + e); }
    }
    public static void Player_UpdateBodyMode(Player self)
    { // Friend fast crawl
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
    }
    public static void Player_UpdateAnimation(Player self)
    { 
        if (self.TryGetFriend(out _))
            FriendGameplay.FriendLedgeFix(self); 
    }
    public static void Player_WallJump(On.Player.orig_WallJump orig, Player self, int direction)
    { 
        orig(self, direction);
        if (self.TryGetFriend(out _)) 
            FriendGameplay.FriendWalljumpFix(self);
    }
    public static void SlugcatStats_ctor(SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
    { // Friend unnerfs
        if (slugcat == FriendName)
            FriendGameplay.FriendStats(self);
    }
    public static void Player_checkInput(int timer, Player self)
    {
        //Moving all inputs one slot up
        for (var i = self.GetGeneral().UnchangedInputForLizRide.Length - 1; i > 0; i--)
            self.GetGeneral().UnchangedInputForLizRide[i] = self.GetGeneral().UnchangedInputForLizRide[i - 1];
        
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
    }
}
