using RWCustom;
using TheFriend.SlugcatThings;
using UnityEngine;

namespace TheFriend.NoirThings;

public partial class NoirCatto //Animations + drawing position adjustments are here
{
    private static void PlayerGraphicsOnUpdate(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);
        if (!self.player.TryGetNoir(out var noirData)) return;

        PoleCrawl(noirData);
        LandingHelp(noirData);
        AdjustCrawlGraphics(noirData); //Also includes Tail Lift
        EarsUpdate(noirData);
        MoveEars(noirData);
        LookDirection(noirData);

        noirData.LastHeadRotation = self.head.connection.Rotation;
    }

    private static bool SlugcatHandOnEngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
    {
        var player = (Player)self.owner.owner;
        if (!player.TryGetNoir(out var noirData)) return orig(self);

        if (noirData.CanCrawlOnBeam())
        {
            //Crawl anim code while on beams!
            self.mode = Limb.Mode.HuntAbsolutePosition;
            self.huntSpeed = 12f;
            self.quickness = 0.7f;
            if ((self.limbNumber == 0 || (Mathf.Abs(((PlayerGraphics)self.owner).hands[0].pos.x - self.owner.owner.bodyChunks[0].pos.x) < 10f && ((PlayerGraphics)self.owner).hands[0].reachedSnapPosition)) && !Custom.DistLess(self.owner.owner.bodyChunks[0].pos, self.absoluteHuntPos, 29f))
            {
                self.FindGrip(self.owner.owner.room, self.connection.pos + new Vector2(((Player)self.owner.owner).flipDirection * 20f, 0f), self.connection.pos + new Vector2(((Player)self.owner.owner).flipDirection * 20f, 0f), 100f, new Vector2(self.owner.owner.bodyChunks[0].pos.x + ((Player)self.owner.owner).flipDirection * 28f, self.owner.owner.room.MiddleOfTile(self.owner.owner.bodyChunks[0].pos).y - 10f), 2, 1, false);
            }
            return false;
        }
        return orig(self);
    }

    private static void PlayerOnGraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyviewed, bool eu)
    {
        orig(self, actuallyviewed, eu);
        if (!actuallyviewed) return;
        if (!self.TryGetNoir(out var noirData)) return;

        for (var i = 0; i < self.grasps.Length; i++)
        {
            var grasp = self.grasps[i];
            if (grasp?.grabbed is Weapon wep)
            {
                if (noirData.CanCrawlOnBeam())
                {
                    if (ModManager.CoopAvailable && self.jollyButtonDown && self.handPointing == i) continue;
                    var rotation = Custom.DirVec(self.bodyChunks[1].pos, Vector2.Lerp(grasp.grabbed.bodyChunks[0].pos, self.bodyChunks[0].pos, 0.8f));
                    wep.setRotation = rotation; //Rotate weapons accordingly when crawling on poles
                }
            }
        }
    }

    //------------------------
    private static void PoleCrawl(NoirData noirData)
    {
        var self = (PlayerGraphics)noirData.Cat.graphicsModule;
        var angle = Custom.AimFromOneVectorToAnother(self.player.bodyChunks[0].pos, self.player.bodyChunks[1].pos);
        var a2 = Custom.PerpendicularVector(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos);
        var a3 = Custom.PerpendicularVector(self.player.bodyChunks[0].pos, self.player.bodyChunks[1].pos);
        var flpDirNeg = self.player.flipDirection * -1;

        //Adjusting draw positions slightly
        if (noirData.CanCrawlOnBeam())
        {
            if (self.player.input[0].x != 0)
            {
                self.drawPositions[0, 0].y = Mathf.Lerp(self.drawPositions[0, 1].y + 5, self.drawPositions[0, 0].y + 5, 1f);
                self.drawPositions[1, 0].y = Mathf.Lerp(self.drawPositions[1, 1].y + 4, self.drawPositions[1, 0].y + 4, 1f);
                self.drawPositions[1, 0].x = Mathf.Lerp(self.drawPositions[1, 1].x + 2 * flpDirNeg, self.drawPositions[1, 0].x + 2 * flpDirNeg, 1f);

                self.head.pos.y = Mathf.Lerp(self.head.lastPos.y + 5f, self.head.pos.y + 5f, 1f);
                self.head.pos.x = Mathf.Lerp(self.head.lastPos.x + 1 * flpDirNeg, self.head.pos.x + 1 * flpDirNeg, 1f);
            }
            else
            {
                self.drawPositions[0, 0].y = Mathf.Lerp(self.drawPositions[0, 1].y + 2, self.drawPositions[0, 0].y + 2, 1f);
                self.drawPositions[1, 0].y = Mathf.Lerp(self.drawPositions[1, 1].y + 3, self.drawPositions[1, 0].y + 3, 1f);
                self.drawPositions[1, 0].x = Mathf.Lerp(self.drawPositions[1, 1].x + 1 * flpDirNeg, self.drawPositions[1, 0].x + 1 * flpDirNeg, 1f);

                self.head.pos.y = Mathf.Lerp(self.head.lastPos.y + 2.5f, self.head.pos.y + 2.5f, 1f);
                self.head.pos.x = Mathf.Lerp(self.head.lastPos.x + 1 * flpDirNeg, self.head.pos.x + 1 * flpDirNeg, 1f);
            }

            //Damping
            if (self.player.input[0].x == 0)
            {
                //The last number is the modifier - the bigger the number, the less floppage
                self.player.bodyChunks[0].vel = Vector2.Lerp(self.player.bodyChunks[0].vel, Vector2.zero, self.player.bodyChunks[0].vel.magnitude * self.player.bodyChunks[0].vel.magnitude * 0.006f);
            }

            //Forcing bodychunks to rotate so the player is aligned horizontally
            switch (angle)
            {
                case > 0 and < 90: //Left Down->Middle
                    if (self.player.flipDirection != -1) break;
                    self.player.bodyChunks[0].vel += a2 * 1;
                    self.player.bodyChunks[1].vel += a3 * 1;
                    break;
                case > 90 and < 180: //Left Up->Middle
                    if (self.player.flipDirection != -1) break;
                    if (self.player.input[0].x != 0)
                    {
                        self.player.bodyChunks[0].vel -= a2 * self.player.bodyChunks[0].vel.y;
                        self.player.bodyChunks[1].vel -= a3 * self.player.bodyChunks[0].vel.y;
                    }
                    else
                    {
                        if (angle > 130)
                        {
                            self.player.bodyChunks[0].vel -= a2 * self.player.bodyChunks[0].vel.y * 0.75f;
                            self.player.bodyChunks[1].vel -= a3 * self.player.bodyChunks[0].vel.y * 0.75f;
                        }
                        else
                        {
                            self.player.bodyChunks[0].vel -= a2;
                            self.player.bodyChunks[1].vel -= a3;
                        }
                    }
                    break;

                case < 0 and > -90: //Right Down->Middle
                    if (self.player.flipDirection != 1) break;
                    self.player.bodyChunks[0].vel -= a2 * 1;
                    self.player.bodyChunks[1].vel -= a3 * 1;
                    break;
                case < -90 and > -180: //Right Up->Middle
                    if (self.player.flipDirection != 1) break;
                    if (self.player.input[0].x != 0)
                    {
                        self.player.bodyChunks[0].vel += a2 * self.player.bodyChunks[0].vel.y;
                        self.player.bodyChunks[1].vel += a3 * self.player.bodyChunks[0].vel.y;
                    }
                    else
                    {
                        if (angle < -130)
                        {
                            self.player.bodyChunks[0].vel += a2 * self.player.bodyChunks[0].vel.y * 0.75f;
                            self.player.bodyChunks[1].vel += a3 * self.player.bodyChunks[0].vel.y * 0.75f;
                        }
                        else
                        {
                            self.player.bodyChunks[0].vel += a2;
                            self.player.bodyChunks[1].vel += a3;
                        }
                    }
                    break;
            }

        }
    }
    private static void LandingHelp(NoirData noirData)
    {
        if (!noirData.JumpInitiated) return;
        if (noirData.Cat.input[0].x == 0) return;

        var self = (PlayerGraphics)noirData.Cat.graphicsModule;
        var angle = Custom.AimFromOneVectorToAnother(self.player.bodyChunks[0].pos, self.player.bodyChunks[1].pos);
        var lastAngle = Custom.AimFromOneVectorToAnother(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[1].lastPos);
        var a2 = Custom.PerpendicularVector(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos);
        var a3 = Custom.PerpendicularVector(self.player.bodyChunks[0].pos, self.player.bodyChunks[1].pos);
        var flpDirNeg = self.player.flipDirection * -1;

        //Debug.Log($"You can be my devil or my {angle}");
        var angelMod = 180 - angle * flpDirNeg;
        var diff = (lastAngle - angle) * flpDirNeg * 0.1f;
        if (diff < 0f) diff = 0f;
        var mod = 0.01f;

        switch (angle)
        {
            case > 0 and < 180: //Left Down->Middle
                if (self.player.flipDirection != -1) break;
                self.player.bodyChunks[0].vel += a2 * angelMod * mod * diff;
                self.player.bodyChunks[1].vel += a3 * angelMod * mod * diff;
                break;
            case < 0 and > -180: //Right Down->Middle
                if (self.player.flipDirection != 1) break;
                self.player.bodyChunks[0].vel -= a2  * angelMod * mod * diff;
                self.player.bodyChunks[1].vel -= a3  * angelMod * mod * diff;
                break;
        }
    }
    private static void AdjustCrawlGraphics(NoirData noirData)
    {
        var self = (PlayerGraphics)noirData.Cat.graphicsModule;
        if (self.player.bodyMode == Player.BodyModeIndex.Crawl && self.player.animation == Player.AnimationIndex.None)
        {
            if (self.player.input[0].x != 0)
            {
                self.drawPositions[0, 0].y = Mathf.Lerp(self.drawPositions[0, 1].y + 0.5f, self.drawPositions[0, 0].y + 0.5f, 1f); //Closer to head
                self.drawPositions[1, 0].y = Mathf.Lerp(self.drawPositions[1, 1].y + 2.5f, self.drawPositions[1, 0].y + 2.5f, 1f); //Closer to tail

                var diff = self.drawPositions[1, 0].y - self.head.pos.y;
                var lastDiff = self.drawPositions[1, 1].y - self.head.lastPos.y;
                
                self.head.pos.y = Mathf.Lerp(self.head.lastPos.y + 1.5f + lastDiff, self.head.pos.y + 1.5f + diff, 1f);

                for (var i = 0; i < self.tail.Length; i++)
                {
                    self.tail[i].pos.y = Mathf.Lerp(self.tail[i].lastPos.y + 0.75f, self.tail[i].pos.y + 0.75f, 1f);
                    
                    if (self.tail[i].pos.y <= self.head.pos.y + ((i < self.tail.Length / 2) ? 3f : 6f))
                        self.tail[i].vel.y += i * ((i < self.tail.Length / 2) ? (0.72f / self.tail.Length) : (0.8f / self.tail.Length)) * self.player.room.gravity;
                }
               
            }
            else
            {
                self.drawPositions[0, 0].y = Mathf.Lerp(self.drawPositions[0, 1].y + 0, self.drawPositions[0, 0].y + 0, 1f);
                self.drawPositions[1, 0].y = Mathf.Lerp(self.drawPositions[1, 1].y + 1, self.drawPositions[1, 0].y + 1, 1f);

                self.head.pos.y = Mathf.Lerp(self.head.lastPos.y + 0f, self.head.pos.y + 0f, 1f);
                
                for (var i = 0; i < self.tail.Length; i++)
                {
                    self.tail[i].pos.y = Mathf.Lerp(self.tail[i].lastPos.y + 0.25f, self.tail[i].pos.y + 0.25f, 1f);
                    self.tail[i].vel.x *= 0.5f;

                    if (self.tail[i].pos.y <= self.head.pos.y + ((i < self.tail.Length / 2) ? 5f : 8f))
                        self.tail[i].vel.y += i * ((i < self.tail.Length / 2) ? (0.4f / self.tail.Length) : (0.8f / self.tail.Length)) * self.player.room.gravity;
                }
            }
        }
        // Tailtip!
        else if (!self.player.State.dead && self.player.bodyMode != Player.BodyModeIndex.Stunned &&
                 (self.player.bodyChunks[1].contactPoint != new IntVector2(0, 0) || self.player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam) &&
                 self.player.animation != Player.AnimationIndex.Roll && self.player.animation != Player.AnimationIndex.BellySlide &&
                 self.player.bodyMode != Player.BodyModeIndex.Swimming)
        {
            for (var i = 0; i < self.tail.Length; i++)
            {
                self.tail[i].pos.y = Mathf.Lerp(self.tail[i].lastPos.y + 0.25f, self.tail[i].pos.y + 0.25f, 1f);
                self.tail[i].vel.x *= 0.5f;

                if (self.tail[i].pos.y <= self.drawPositions[1, 0].y + ((i < self.tail.Length / 2) ? 5f : 10f))
                    self.tail[i].vel.y += i * ((i < self.tail.Length / 2) ? (0.4f / self.tail.Length) : (0.8f / self.tail.Length)) * self.player.room.gravity
                        * (noirData.OnVerticalBeam() && !noirData.OnHorizontalBeam() ? 0.75f : 1f);
            }
        }
    }

    private static void MoveEars(NoirData noirData)
    {
        var self = (PlayerGraphics)noirData.Cat.graphicsModule;
        var earL = noirData.Ears[0];
        var earR = noirData.Ears[1];

        for (var i = 0; i < 2; i++)
        {
            noirData.Ears[i][0].vel.x *= 0.5f;
            noirData.Ears[i][0].vel.y += self.player.EffectiveRoomGravity * 0.5f;
            noirData.Ears[i][1].vel.x *= 0.3f;
            noirData.Ears[i][1].vel.y += self.player.EffectiveRoomGravity * 0.3f;
        }

        if ((self.player.animation == Player.AnimationIndex.None && self.player.input[0].x != 0) ||
            (self.player.animation == Player.AnimationIndex.StandOnBeam && self.player.input[0].x != 0) ||
            self.player.bodyMode == Player.BodyModeIndex.Crawl || noirData.CanCrawlOnBeam() ||
            self.player.animation != Player.AnimationIndex.None && self.player.animation != Player.AnimationIndex.Flip && !noirData.OnAnyBeam())
        {
            if (noirData.FlipDirection == 1)
            {
                noirData.EarsFlip[0] = 1;
                noirData.EarsFlip[1] = -1;
            }
            else
            {
                noirData.EarsFlip[0] = -1;
                noirData.EarsFlip[1] = 1;
            }

            if ((self.player.bodyMode == Player.BodyModeIndex.Crawl || noirData.CanCrawlOnBeam()) && self.player.input[0].x == 0)
            {
                var noirFlpDirNeg = noirData.FlipDirection * -1;
                if (noirData.FlipDirection == 1)
                {
                    earL[0].vel.x += 0.45f * noirFlpDirNeg;
                    earL[1].vel.x += 0.45f * noirFlpDirNeg;
                    earR[0].vel.x += 0.35f * noirFlpDirNeg;
                    earR[1].vel.x += 0.35f * noirFlpDirNeg;

                    if (self.player.superLaunchJump >= 20 || noirData.SuperCrawlPounce >= 20)
                    {
                        earL[0].vel.x += 0.5f * noirFlpDirNeg;
                        earL[1].vel.x += 0.5f * noirFlpDirNeg;
                        earR[0].vel.x += 0.5f * noirFlpDirNeg;
                        earR[1].vel.x += 0.5f * noirFlpDirNeg;
                    }

                    if (noirData.SuperCrawlPounce >= 20)
                    {
                        earL[0].vel.y -= 0.35f;
                        earL[1].vel.y -= 0.35f;
                        earR[0].vel.y -= 0.35f;
                        earR[1].vel.y -= 0.35f;
                    }
                }
                else
                {
                    earL[0].vel.x += 0.35f * noirFlpDirNeg;
                    earL[1].vel.x += 0.35f * noirFlpDirNeg;
                    earR[0].vel.x += 0.45f * noirFlpDirNeg;
                    earR[1].vel.x += 0.45f * noirFlpDirNeg;

                    if (self.player.superLaunchJump >= 20 || noirData.SuperCrawlPounce >= 20)
                    {
                        earL[0].vel.x += 0.5f * noirFlpDirNeg;
                        earL[1].vel.x += 0.5f * noirFlpDirNeg;
                        earR[0].vel.x += 0.5f * noirFlpDirNeg;
                        earR[1].vel.x += 0.5f * noirFlpDirNeg;
                    }

                    if (noirData.SuperCrawlPounce >= 20)
                    {
                        earL[0].vel.y -= 0.35f;
                        earL[1].vel.y -= 0.35f;
                        earR[0].vel.y -= 0.35f;
                        earR[1].vel.y -= 0.35f;
                    }
                }
            }
        }
        else
        {
            noirData.EarsFlip[0] = 1;
            noirData.EarsFlip[1] = 1;

            //Push ears to the side when idle
            earL[1].vel.x -= 0.5f;
            earR[1].vel.x += 0.5f;
        }
    }

    private static void LookDirection(NoirData noirData)
    {
        var self = (PlayerGraphics)noirData.Cat.graphicsModule;
        var earL = noirData.Ears[0];
        var earR = noirData.Ears[1];

        var interesting = self.objectLooker.currentMostInteresting;

        //Angle ears when pouncing
        if (self.player.superLaunchJump >= 20 || noirData.SuperCrawlPounce >= 20)
        {
            self.lookDirection *= 0.2f;
            self.lookDirection += new Vector2(noirData.UnchangedInput[0].x, noirData.UnchangedInput[0].y);

            var ymod = noirData.UnchangedInput[0].y >= 0 ? noirData.UnchangedInput[0].y : noirData.UnchangedInput[0].y * 0.5f;
            earL[0].vel -= new Vector2(noirData.UnchangedInput[0].x, ymod) * 0.75f;
            earL[1].vel -= new Vector2(noirData.UnchangedInput[0].x, ymod) * 0.75f;
            earR[0].vel -= new Vector2(noirData.UnchangedInput[0].x, ymod) * 0.75f;
            earR[1].vel -= new Vector2(noirData.UnchangedInput[0].x, ymod) * 0.75f;
        }
        else //Angle ears according to lookDirection
        {
            var strength = 0.2f;
            if (interesting != null)
            {
                var ceiling = interesting is Creature crit && !crit.dead ||
                                  interesting is not PlayerCarryableItem and not Creature ||
                                  interesting is Weapon wep && wep.mode == Weapon.Mode.Thrown ? 0f : 0.5f;
                strength = (1f - Vector2.Distance(self.head.pos, interesting.firstChunk.pos).Map(0f, 500f, ceiling, 1f, true));
            }

            var ymod = self.lookDirection.y >= 0 ? self.lookDirection.y : 0f;
            earL[0].vel -= new Vector2(self.lookDirection.x, ymod).normalized * 0.25f * strength;
            earL[1].vel -= new Vector2(self.lookDirection.x, ymod).normalized * 0.50f * strength;
            earR[0].vel -= new Vector2(self.lookDirection.x, ymod).normalized * 0.25f * strength;
            earR[1].vel -= new Vector2(self.lookDirection.x, ymod).normalized * 0.50f * strength;
        }
    }

}