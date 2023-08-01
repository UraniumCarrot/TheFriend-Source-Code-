using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using Solace.SlugcatThings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Solace.NoirThings;

public partial class NoirCatto
{
    //Pretty much a modified vanilla code to set slideCounter
    //slideCounter runs right after changing run direction
    private static void UpdateSlideCounter(NoirData noirData)
    {
        var self = noirData.Cat;
        if (self.bodyMode == Player.BodyModeIndex.Crawl || (self.animation == Player.AnimationIndex.StandOnBeam && noirData.CanCrawlOnBeam()))
        {
            if (self.slideCounter > 0)
            {
                self.slideCounter++;
                if (self.input[0].x == self.slideDirection)
                {
                    self.slideDirection = -self.input[0].x;
                    self.slideCounter = 1;
                    self.room.PlaySound(SoundID.Slugcat_Skid_On_Ground_Init, self.mainBodyChunk, false, 0.9f, 0.8f);
                    noirData.FrontCrawlFlip = true;
                }
                if (self.slideCounter > 20)
                {
                    self.slideCounter = 0;
                    noirData.FrontCrawlFlip = false;
                }

                if (self.input[0].x != 0)
                {
                    var num = -Mathf.Sin(self.slideCounter / 20f * 3.1415927f * 0.5f) + 0.5f;
                    var mainBodyChunk2 = self.mainBodyChunk;
                    mainBodyChunk2.vel.x = mainBodyChunk2.vel.x + (num * 3.5f * self.slideDirection - self.slideDirection * ((num < 0f) ? 0.8f : 0.5f) * (self.isSlugpup ? 0.25f : 1f));
                    var bodyChunk21 = self.bodyChunks[1];
                    bodyChunk21.vel.x = bodyChunk21.vel.x + (num * 3.5f * self.slideDirection + self.slideDirection * 0.5f);
                    if ((self.slideCounter == 4 || self.slideCounter == 7 || self.slideCounter == 11) && Random.value < Mathf.InverseLerp(0f, 0.5f, self.room.roomSettings.CeilingDrips) &&
                        self.animation != Player.AnimationIndex.StandOnBeam)
                    {
                        self.room.AddObject(new WaterDrip(self.bodyChunks[1].pos + new Vector2(0f, -self.bodyChunks[1].rad + 1f), Custom.DegToVec(self.slideDirection * Mathf.Lerp(30f, 70f, Random.value)) * Mathf.Lerp(6f, 11f, Random.value), false));
                    }
                }
            }
            else if (self.input[0].x != 0)
            {
                if (self.input[0].x != self.slideDirection)
                {
                    if (self.initSlideCounter > 10 && self.mainBodyChunk.vel.x > 0f == self.slideDirection > 0 && Mathf.Abs(self.mainBodyChunk.vel.x) > 1f)
                    {
                        self.slideCounter = 1;
                        self.room.PlaySound(SoundID.Slugcat_Skid_On_Ground_Init, self.mainBodyChunk, false, 1f, 0.9f);
                    }
                    else
                    {
                        self.slideDirection = self.input[0].x;
                    }

                    self.initSlideCounter = 0;
                    return;
                }

                if (self.initSlideCounter < 30)
                {
                    self.initSlideCounter++;
                }
            }
            else if (self.initSlideCounter > 0)
            {
                self.initSlideCounter--;
            }
        }
    }

    private static void PlayerILUpdateAnimation(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(
                i => i.MatchLdsfld<Player.AnimationIndex>("CrawlTurn"),
                i => i.MatchCall(out _),
                i => i.MatchBrfalse(out label)
            );
            c.GotoPrev(MoveType.Before, i => i.MatchLdarg(0));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(CustomCrawlTurn);
            c.Emit(OpCodes.Brtrue, label);
        }
        catch (Exception ex)
        {
            Solace.LogSource.LogError("ILHook failed - Noir's CrawlTurn");
            Solace.LogSource.LogError(ex);
        }
    }
    private static bool CustomCrawlTurn(Player self)
    {
        if (self.animation != Player.AnimationIndex.CrawlTurn) return false;
        if (!self.TryGetNoir(out var noirData)) return false;

        //If we're jumping, don't proceed with other code, it breaks the jump
        if (self.input[0].jmp)
        {
            //Also reset the anims so they don't break, too
            self.bodyMode = Player.BodyModeIndex.Default;
            self.animation = Player.AnimationIndex.None;
            return true;
        }

        //If our input does not match the facing direction of the slug
        if (self.input[0].x != 0 && (self.input[0].x > 0) == (self.bodyChunks[0].pos.x < self.bodyChunks[1].pos.x))
        {
            //Debug.Log($"CAT NOT FACING CORRECT DIRECTION");
            noirData.CrawlTurnCounter++;
            noirData.AfterCrawlTurnCounter = 0;

            //Temporarily turning off player's bodychunk push/pull
            self.bodyChunkConnections[0].active = false;

            //The back legs drag behind, initiating the turn
            self.bodyChunks[1].vel.x -= noirData.CrawlTurnCounter * 0.5f * self.flipDirection;

            return true;
        }

        if (self.input[0].x != 0)
        {
            noirData.AfterCrawlTurnCounter++;
        }

        if (noirData.AfterCrawlTurnCounter >= 3 || Custom.Dist(self.bodyChunks[0].pos, self.bodyChunks[1].pos) > 15f)
        {
            //Letting orig run after this
            //Debug.Log($"Resetting cat's anim...");
            self.bodyChunkConnections[0].active = true;
            return false;
        }
        return true;
    }

    private static void CrawlTurnUpdate(NoirData noirData)
    {
        var self = noirData.Cat;
        if (self.animation != Player.AnimationIndex.CrawlTurn)
        {
            noirData.CrawlTurnCounter = 0;
            noirData.AfterCrawlTurnCounter = 0;
        }

        if (noirData.LastAnimation == Player.AnimationIndex.CrawlTurn && self.animation != Player.AnimationIndex.CrawlTurn)
        {
            self.bodyChunkConnections[0].active = true; //Just to make sure we're not left in an unhinged state
        }
    }
}