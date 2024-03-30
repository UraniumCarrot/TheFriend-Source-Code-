using UnityEngine;
using System.Linq;
using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using TheFriend.FriendThings;
using TheFriend.Objects.LittleCrackerObject;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;
using Random = UnityEngine.Random;
using JollyColorMode = Options.JollyColorMode;
using TheFriend.SlugcatThings;
using TheFriend.WorldChanges.WorldStates.General;

namespace TheFriend.CharacterThings.FriendThings;

public class FriendGameplay
{
    #region movement
    public static void FriendLeapController(Player self, int timer)
    {
        if (self.GetFriend().longjump && self.input[0].y == 0) self.GetFriend().WantsUp = false;
        if (self.GetFriend().longjump && self.input[0].y > 0)
        {
            self.GetFriend().WantsUp = true;
            self.input[0].y = 0;
            self.input[0].x = 0;
        }
        if (self.input[0].y < 0 && self.superLaunchJump != 0)
        {
            self.superLaunchJump = 0;
            self.killSuperLaunchJumpCounter = 0;
            self.GetFriend().WantsUp = false;
        }
        if (!self.GetFriend().longjump) self.GetFriend().WantsUp = false;

        // Friend pole leap mechanics
        if (self.animation == ind.StandOnBeam && self.GetFriend().poleCrawlState)
        {
            if (self.input[0].y > 0) self.GetFriend().YesIAmLookingUpStopThinkingOtherwise = true;
            else self.GetFriend().YesIAmLookingUpStopThinkingOtherwise = false;
            if (self.input[0].y > 0) self.GetFriend().upwardpolejump = true;
            else self.GetFriend().upwardpolejump = false;
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


            if (self.input[0].jmp && timer < 20) { self.GetFriend().LetGoOfPoleJump = false; }
            else if (!self.input[0].jmp) { self.GetFriend().LetGoOfPoleJump = true; }
            if (!self.GetFriend().LetGoOfPoleJump)
            {
                if (self.input[0].x == 0) self.input[0].jmp = false;
                if (timer < 20) 
                {
                    if (self.input[0].x != 0 || self.input[0].y != 0) self.GetFriend().poleSuperJumpTimer = 0;
                    else self.GetFriend().poleSuperJumpTimer++; 
                }
            }
            if (self.GetFriend().LetGoOfPoleJump) 
            {
                if (timer >= 20) { self.GetFriend().polejump = true; self.GetFriend().DoingAPoleJump = true; self.Jump(); }
                self.GetFriend().poleSuperJumpTimer = 0;
            }
        }
        else { self.GetFriend().poleSuperJumpTimer = 0; self.GetFriend().LetGoOfPoleJump = false; }
    }
    public static void FriendJump1(Player self)
    {
        var crawlFlip = !self.standing && self.slideCounter > 0 && self.slideCounter < 10;
        
        // Friend improved jumps
        if (!crawlFlip &&
            self.animation != ind.Roll &&
            !self.standing &&
            Mathf.Abs(self.firstChunk.vel.x) > 3)
        {
            if (self.bodyChunks[1].contactPoint.y == 0 ||
                self.input.Count(i => i.jmp) == 9)
                return;
        }

        // Whiplash crawlturn jump
        if (crawlFlip)
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
            self.slideCounter = 0;
            self.jumpBoost = 0.0f;
            self.room.PlaySound(SoundID.Slugcat_Sectret_Super_Wall_Jump, self.mainBodyChunk, false, 1f, 1f);
            
            if ((!(self.input[0].y > 0) && Configs.FriendAutoCrouch))
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
    }

    public static void FriendJump2(Player self)
    {
        if (Configs.FriendUnNerf) self.jumpBoost += 3f;
        else if (self.bodyMode == bod.Crawl) self.jumpBoost *= 1f + (0.5f / 2);
        else self.jumpBoost += (0.5f + 0.25f) * (self.animation == ind.StandOnBeam ? 0 : 1);

        if ((!(self.input[0].y > 0) && Configs.FriendAutoCrouch))
        {
            self.standing = false;
        }
    }
    public static void FriendMovement(Player self)
    {
        float boost = 3.5f;
        if (!self.standing)
        {
            if (self.superLaunchJump >= 20) self.GetFriend().longjump = true;
            if (self.bodyMode == bod.Crawl)
            {
                self.dynamicRunSpeed[0] += boost;
                self.dynamicRunSpeed[1] += boost;
            }
            if (self.bodyMode == bod.Default)
            {
                if (!self.GetFriend().HighJumped)
                {
                    self.dynamicRunSpeed[0] += boost - 1f;
                    self.dynamicRunSpeed[1] += boost - 1f;
                }
            }
            if (self.GetFriend().longjump && self.superLaunchJump == 0)
            {
                if (self.GetFriend().WantsUp)
                {
                    self.animation = ind.RocketJump;
                    self.GetFriend().HighJumped = true;
                    self.bodyChunks[0].vel.y *= 4;
                    self.bodyChunks[0].vel.x *= 0.3f;
                    self.bodyChunks[1].vel.x *= 0.3f;
                }
                else
                {
                    self.bodyChunks[0].vel.x *= 1.1f;
                    self.bodyChunks[1].vel.x *= 1.1f;
                }
                self.GetFriend().longjump = false;
            }
        }
        // Friend pole leap
        if (self.GetFriend().polejump)
        {
            if (self.GetFriend().upwardpolejump)
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
                self.GetFriend().upwardpolejump = false;
            }
            self.animation = ind.RocketJump;
            self.room.PlaySound(SoundID.Slugcat_Super_Jump, self.firstChunk);
            self.room.PlaySound(SoundID.Slugcat_Skid_On_Ground_Init, self.firstChunk);

            self.room.AddObject(new WaterDrip(self.bodyChunks[1].pos + new Vector2(0f, -self.bodyChunks[1].rad + 1f), Custom.DegToVec(self.slideDirection * Mathf.Lerp(30f, 70f, Random.value)) * Mathf.Lerp(6f, 11f, Random.value), false));

            self.GetFriend().polejump = false;
        }
        if (self.GetFriend().DoingAPoleJump && !self.GetFriend().upwardpolejump && self.input[0].y > 0) { self.animation = ind.None; self.standing = true; }
        else if (self.GetFriend().upwardpolejump) self.standing = false;
        if (self.feetStuckPos != null || self.animation != ind.RocketJump) { self.GetFriend().DoingAPoleJump = false; self.GetFriend().upwardpolejump = false; }
    }
    #endregion
    public static void FriendUpdate(Player self, bool eu)
    {
        var data = self.GetFriend();

        if (data.voice != null)
        {
            data.voice.Update();
            if (data.TalkEffectCounter > 0)
            {
                data.TalkEffectCounter--;
                FriendVoiceCommands.FriendTalk(self.graphicsModule as PlayerGraphics);
            }
            if (data.VoiceCooldown > 0) data.VoiceCooldown--;
            if (!self.room.game.IsArenaSession)
            {
                CharacterTools.InputTapCheck(self, "y", out int taps);
                if (data.VoiceCooldown <= 0)
                    if (taps > 1)
                        data.VoiceCooldown = 80;
        
                if (data.VoiceCooldown > 30 && taps > 0)
                    FriendVoiceCommands.FriendCommandingBark(self, data, taps);
            }
            if (self.Stunned && self.grabbedBy.Any()) FriendVoiceCommands.FriendWhimperWhileStunLoop(self, data);
            else data.voice.pitchOverride = null;
        }
        AbstractCreature guide0;
        AbstractCreature guide1;
        if (self.room.world.overseersWorldAI?.playerGuide == null && !self.room.world.game.IsArenaSession && QuickWorldData.SolaceCampaign)
        {   // overseer code made with HUGE help from Leo, creator of the Lost!
            WorldCoordinate pos = new WorldCoordinate(self.room.world.offScreenDen.index, -1, -1, 0);
            guide0 = new AbstractCreature(self.room.game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, pos, new EntityID());
            guide1 = new AbstractCreature(self.room.game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, pos, new EntityID());
            self.room.world.GetAbstractRoom(pos).entitiesInDens.Add(guide0);
            self.room.world.GetAbstractRoom(pos).entitiesInDens.Add(guide1);
            guide0.ignoreCycle = true;
            guide1.ignoreCycle = true;
            (guide0.abstractAI as OverseerAbstractAI)!.SetAsPlayerGuide(0);
            (guide1.abstractAI as OverseerAbstractAI)!.SetAsPlayerGuide(1);
            (guide0.abstractAI as OverseerAbstractAI)!.BringToRoomAndGuidePlayer(self.room.abstractRoom.index);
            (guide1.abstractAI as OverseerAbstractAI)!.BringToRoomAndGuidePlayer(self.room.abstractRoom.index);
            if (data.Wiggy == null) data.Wiggy = guide0.realizedCreature as Overseer;
            if (data.Iggy == null) data.Iggy = guide1.realizedCreature as Overseer;
        }
        if (self.animation != ind.RocketJump && data.HighJumped) data.HighJumped = false;
    }

    #region World's smallest adjustments
    public static void FriendConstructor(Player self)
    {
        if (Configs.FriendBackspear)
            self.spearOnBack = new Player.SpearOnBack(self);
        self.GetFriend().voice = new FriendVoice(self);
    }
    public static void FriendStats(SlugcatStats self)
    {
        if (Configs.FriendUnNerf)
        {
            self.poleClimbSpeedFac = 6f;
            self.runspeedFac = 0.8f;
        }
    }
    public static void FriendLedgeFix(Player self)
    { // Attempted ledgegrab fix, and increased polewalk speed
        if (self.animation == ind.LedgeGrab && self.input[0].y < 1) { self.standing = false; self.bodyMode = bod.Crawl; }
        if (self.animation == ind.StandOnBeam && self.input[0].y < 1 && Configs.FriendPoleCrawl)
        {
            self.dynamicRunSpeed[0] = 2.1f + (self.slugcatStats.runspeedFac * 0.5f) * 4.5f;
            self.dynamicRunSpeed[1] = 2.1f + (self.slugcatStats.runspeedFac * 0.5f) * 4.5f;
        }
    }

    public static void FriendWalljumpFix(Player self)
    {
        self.standing = false;
    }

    #endregion
    
    /*Surrounding IL***************************************************************************************
    // if (animation == AnimationIndex.Flip && flag && input[0].x == 0)
	IL_010d: ldarg.0
	IL_010e: ldfld class Player/AnimationIndex Player::animation
	
	---- First TryGotoNext Lands here ----
	IL_0113: ldsfld class Player/AnimationIndex Player/AnimationIndex::Flip
	IL_0118: call bool class ExtEnum`1<class Player/AnimationIndex>::op_Equality(class ExtEnum`1<!0>, class ExtEnum`1<!0>)
	
	---- Second TryGotoNext Lands here ----
	IL_011d: ldloc.1
	        ^---- MoveAfterLabels lands here? ----
	IL_011e: and
	IL_011f: brfalse.s IL_0163
    *******************************************************************************************************/
    public static void Player_ThrowObject(ILContext il)
    {
        var cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdsfld<Player.AnimationIndex>("Flip")))
        {
            Plugin.LogSource.LogError($"Failed to Hook Player_ThrowObject, Pt1!");
            return;
        }

        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloc(1)))
        {
            Plugin.LogSource.LogError($"Failed to Hook Player_ThrowObject, Pt2!");
            return;
        }

        cursor.MoveAfterLabels();

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate<Func<Player, bool>>(player =>
        {
            return (player.TryGetFriend(out _) &&
                    player.bodyChunks[1].ContactPoint.y != -1 && 
                    !(player.bodyMode == Player.BodyModeIndex.Crawl || 
                      player.standing)) 
                   || 
                   (player.grasps[0]?.grabbed is LittleCracker ||
                    (player.grasps[0]?.grabbed == null && player.grasps[1]?.grabbed is LittleCracker));
        });
        cursor.Emit(OpCodes.Or);
    }

}