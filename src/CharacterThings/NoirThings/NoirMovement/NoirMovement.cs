using RWCustom;
using TheFriend.RemixMenus;
using TheFriend.SlugcatThings;
using UnityEngine;
using static TheFriend.Plugin;
namespace TheFriend.CharacterThings.NoirThings;

public partial class NoirCatto
{
    public static void PlayerOnUpdateBodyMode(Player self)
    {
        if (!self.TryGetNoir(out var noirData)) return;

        if (self.bodyMode == Player.BodyModeIndex.Crawl)
        {
            //Crawl boost
            self.dynamicRunSpeed[0] *= NoirCrawlSpeedFac;
            self.dynamicRunSpeed[1] *= NoirCrawlSpeedFac;
        }

        //Crawl turn
        UpdateSlideCounter(noirData);
    }

    internal static void PlayerOnUpdateAnimation(Player self)
    {
        if (!self.TryGetNoir(out var noirData)) return;

        if (self.animation == Player.AnimationIndex.StandOnBeam)
        {
            if (noirData.CanCrawlOnBeam())  //Boost while crawling on horizontal pole
            {
                // self.dynamicRunSpeed[0] = (2.1f + self.slugcatStats.runspeedFac * 0.5f) * CrawlSpeedFac; //obsolete, hard assignment not good for compatibility
                // self.dynamicRunSpeed[1] = (2.1f + self.slugcatStats.runspeedFac * 0.5f) * CrawlSpeedFac; //use a += assignment instead
                self.dynamicRunSpeed[0] = (self.dynamicRunSpeed[0] + 0.82f) * NoirCrawlSpeedFac; //hardcoded for now, will definitely use runspeedfac here later, mhm
                self.dynamicRunSpeed[1] = (self.dynamicRunSpeed[1] + 0.82f) * NoirCrawlSpeedFac;
            }
        }

        if (self.animation == Player.AnimationIndex.CrawlTurn)
        {
            self.dynamicRunSpeed[0] /= 0.75f; // Game multiplies it by 0.75f
            self.dynamicRunSpeed[1] /= 0.75f;
        }
    }
    
    public static void PlayerOnMovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
    {
        if (!self.TryGetNoir(out var noirData))
        {
            orig(self, eu);
            return;
        }

        ResetPoleInputBlocker(noirData);
        ModifyLeapInput(self);
        PoleLeapUpdate(noirData);

        DirectlyToBeam1(noirData, out var dtbFlag);
        orig(self, eu); // ORIG HERE
        DirectlyToBeam2(noirData, dtbFlag);

        #region Standing
        if (noirData.LastAnimation == Player.AnimationIndex.LedgeCrawl && self.animation != Player.AnimationIndex.LedgeCrawl)
        {
            if (RemixMain.NoirDisableAutoCrouch.Value)
            {
                self.standing = false;
                noirData.StandCounter = 0;
            }
        }
        if (self.input[0].y > 0 || !noirData.Jumping && self.input[0].y == 0 && noirData.StandCounter is >= 1 and <= 10)
        {
            self.standing = true;
        }
        else
        {
            self.standing = false;
        }

        if (self.standing)
        {
            if (self.input[0].y == 0 && self.input[0].x != 0 && !RemixMain.NoirDisableAutoCrouch.Value) noirData.StandCounter++;
            else noirData.StandCounter = 1;
        }
        else if (!self.standing)
        {
            noirData.StandCounter = 0;
        }
        #endregion

        CrawlTurnUpdate(noirData);

        if (self.bodyMode != Player.BodyModeIndex.WallClimb && self.bodyMode != Player.BodyModeIndex.Default || self.bodyChunks[0].contactPoint.y != 0)
        {
            noirData.ClimbCounter = 0;
        }
        if (noirData.ClimbCooldown > 0)
        {
            noirData.ClimbCooldown--;
        }

        CustomCombatUpdate(noirData, eu);
        noirData.LastAnimation = self.animation;
    }

    private static bool IsStuckOrWedged(Player player)
    {
        return patch_Player.IsStuckOrWedged(player);
    }
    internal static void PlayerOnJump(On.Player.orig_Jump orig, Player self)
    {
        if (!self.TryGetNoir(out var noirData) || RotundWorld && IsStuckOrWedged(self))
        {
            orig(self);
            return;
        }

        noirData.LastJumpFromHorizontalBeam = self.animation == Player.AnimationIndex.StandOnBeam;

        #region Jump() consts
        var num1 = Mathf.Lerp(1f, 1.15f, self.Adrenaline);
        if (self.grasps[0] != null && self.HeavyCarry(self.grasps[0].grabbed) && !(self.grasps[0].grabbed is Cicada))
            num1 += Mathf.Min(Mathf.Max(0.0f, self.grasps[0].grabbed.TotalMass - 0.2f) * 1.5f, 1.3f);

        var flip = !self.standing && self.slideCounter > 0 && self.slideCounter < 10;
        var longJump = self.superLaunchJump;
        var ymod = noirData.UnchangedInput[0].y >= 0 ? 1 : -1;
        #endregion

        if (self.animation == Player.AnimationIndex.StandOnBeam)
        {
            self.slideDirection = -self.flipDirection; //Fix flip direction
        }

        #region Pole pounce
        var forcePounce = false;
        if (noirData.CanCrawlOnBeam() && noirData.SuperCrawlPounce >= 19) //We're checking one input late, hence why it's 19 not 20
        {
            forcePounce = true;
            noirData.SuperCrawlPounce = 0;

            var num5 = 9f;
            var num4 = self.bodyChunks[0].pos.x > self.bodyChunks[1].pos.x ? 1 : -1;
            self.simulateHoldJumpButton = 6;

            self.bodyMode = Player.BodyModeIndex.Default;
            self.animation = Player.AnimationIndex.None;

            self.bodyChunks[0].pos.y += 6f * ymod;

            if (self.bodyChunks[0].ContactPoint.y == -1)
            {
                self.bodyChunks[0].vel.y += 3f * num1 * ymod;
            }

            self.bodyChunks[1].vel.y += 4f * num1 * ymod;
            self.jumpBoost = 6f;

            if (self.bodyChunks[0].pos.x > self.bodyChunks[1].pos.x == num4 > 0)
            {
                self.bodyChunks[0].vel.x += num4 * num5 * num1;
                self.bodyChunks[1].vel.x +=num4 * num5 * num1;
                self.room.PlaySound(SoundID.Slugcat_Super_Jump, self.mainBodyChunk, false, 1f, 1f);
            }
            goto nope;
        }
        #endregion

        if ((!self.standing && self.bodyChunks[1].contactPoint.y == 0 && self.animation != Player.AnimationIndex.Roll) || //The run thingy fix (slug goes into small running anim if it lands into crawl right after jmp input)
            self.bodyMode == Player.BodyModeIndex.WallClimb ||
            self.bodyMode == Player.BodyModeIndex.Crawl &&
            (self.animation == Player.AnimationIndex.None || self.animation == Player.AnimationIndex.CrawlTurn) &&
            self.input[0].x != 0 && longJump < 20)
        {
            //Modifier constants
            var xMod = 1f;
            var xModPos = 5f;
            var yMod = flip ? 10.5f : 9f;
            var yModPos = 5f;

            if (self.bodyMode == Player.BodyModeIndex.WallClimb)
            {
                if (noirData.ClimbCooldown > 0) return;
                if (noirData.ClimbCounter >= 1)
                {
                    yMod = 7f;
                    noirData.ClimbCooldown = 27;
                }
                if (noirData.ClimbCounter >= 2)
                {
                    yMod = 5f;
                }
                xMod = 0f;
                xModPos = 0f;
            }

            if (flip && noirData.FrontCrawlFlip)
            {
                xMod = 5f;
                noirData.FrontCrawlFlip = false;
            }

            self.bodyChunks[1].pos += new Vector2(xModPos * self.flipDirection, yModPos);
            self.bodyChunks[0].pos = self.bodyChunks[1].pos + new Vector2(xModPos * self.flipDirection, yModPos);
            self.bodyChunks[1].vel += new Vector2(self.flipDirection * xMod, yMod) * num1;
            self.bodyChunks[0].vel += new Vector2(self.flipDirection * xMod, yMod) * num1;

            if (flip)
            {
                self.flipFromSlide = true;
                self.animation = Player.AnimationIndex.Flip;
                self.slideDirection = -self.flipDirection; //Fix flip direction
                self.slideCounter = 0;
            }
            self.room.PlaySound(flip ? SoundID.Slugcat_Flip_Jump : SoundID.Slugcat_Normal_Jump, self.mainBodyChunk, false, 1f, 1f);

            noirData.Jumping = true;
            if (self.bodyMode == Player.BodyModeIndex.WallClimb)
            {
                noirData.ClimbCounter++;
            }
            return;
        }

        orig(self); // <-- ORIG here
        nope:

        //Pounce changes
        if ((!self.standing && self.animation == Player.AnimationIndex.None && longJump >= 20) || forcePounce)
        {
            if (noirData.UnchangedInput[0].y > 0)
            {
                self.bodyChunks[0].vel += new Vector2(0f, 10.0f);
                self.bodyChunks[1].vel += new Vector2(0f, 4.5f);

                if (noirData.UnchangedInput[0].x == 0)
                {
                    self.bodyChunks[0].vel.x *= 0.25f;
                    self.bodyChunks[1].vel.x *= 0.25f;
                }
                else
                {
                    self.bodyChunks[0].vel.x *= 0.75f;
                    self.bodyChunks[1].vel.x *= 0.75f;
                }
            }
            else if (noirData.UnchangedInput[0].y < 0 && forcePounce)
            {
                self.bodyChunks[0].vel += new Vector2(0f, -10.0f);
                self.bodyChunks[1].vel += new Vector2(0f, -4.5f);

                if (noirData.UnchangedInput[0].x == 0)
                {
                    self.bodyChunks[0].vel.x *= 0.25f;
                    self.bodyChunks[1].vel.x *= 0.25f;
                }
                else
                {
                    self.bodyChunks[0].vel.x *= 1f;
                    self.bodyChunks[1].vel.x *= 1f;
                }
            }

            else if (noirData.UnchangedInput[0].x > 0)
            {
                self.bodyChunks[0].vel.x *= 1.25f;
                self.bodyChunks[0].vel.x *= 1.25f;
            }
        }

        noirData.Jumping = true;
    }
}