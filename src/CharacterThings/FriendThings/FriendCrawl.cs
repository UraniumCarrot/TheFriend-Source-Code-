using RWCustom;
using UnityEngine;
using TheFriend.SlugcatThings;

namespace TheFriend.FriendThings;
// FriendCrawl code kindly given to me by Noir, thank you so much Noir!!! DO NOT use this code without his permission.
public class FriendCrawl
{
    public static void Apply()
    {
        On.PlayerGraphics.Update += PlayerGraphics_Update;
        On.SlugcatHand.EngageInMovement += SlugcatHand_EngageInMovement;
    }

    public static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);
        if (!self.player.TryGetFriend(out _) || !Configs.FriendPoleCrawl) return;

        var angle = Custom.AimFromOneVectorToAnother(self.player.bodyChunks[0].pos, self.player.bodyChunks[1].pos);
        var lastAngle = Custom.AimFromOneVectorToAnother(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[1].lastPos);
        var a2 = Custom.PerpendicularVector(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos);
        var a3 = Custom.PerpendicularVector(self.player.bodyChunks[0].pos, self.player.bodyChunks[1].pos);
        var flpDirNeg = self.player.flipDirection * -1;
        var dirVec = (self.player.bodyChunks[0].pos - self.player.bodyChunks[1].pos).normalized;

        //Adjusting draw positions slightly
        if (self.player.animation == Player.AnimationIndex.StandOnBeam && self.player.input[0].y < 1)
        {
            self.player.GetFriend().poleCrawlState = true;
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
        else self.player.GetFriend().poleCrawlState = false;
    }
    public static bool SlugcatHand_EngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
    {
        var player = (Player)self.owner.owner;

        if (player.slugcatStats.name != Plugin.FriendName || !Configs.FriendPoleCrawl) return orig(self);

        if (player.animation == Player.AnimationIndex.StandOnBeam && player.input[0].y < 1)
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
}
