using System.Linq;
using RWCustom;
using TheFriend.SlugcatThings;
using UnityEngine;

namespace TheFriend.NoirThings;

public abstract partial class NoirCatto // Noir master class
{
    public class NoirData
    {
        public readonly Player Cat;
        public readonly Player.InputPackage[] UnchangedInput;
        public Player.AnimationIndex LastAnimation;
        public Player.AnimationIndex SpearThrownAnimation;

        public readonly TailSegment[][] Ears = new[]
        {
            new TailSegment[2],
            new TailSegment[2]
        };
        public readonly int[] EarsFlip = new[] { 1, 1 };
        public Vector2 LastHeadRotation;
        public bool CallingAddToContainerFromOrigInitiateSprites;

        public int StandCounter;
        public int CrawlTurnCounter;
        public int AfterCrawlTurnCounter;
        public int SuperCrawlPounce;
        public int Ycounter;
        public int ClimbCounter;
        public int ClimbCooldown;
        public bool Jumping;
        public bool JumpInitiated;
        public bool LastJumpFromHorizontalBeam;
        public bool FrontCrawlFlip;

        public readonly int[] SlashCooldown = new[] { 0, 0 };
        public int AirSlashCooldown;
        public int AutoSlashCooldown;
        public int CombinedBonus => ComboBonus + MovementBonus + RotundnessBonus;
        public int ComboBonus;
        public int MovementBonus;
        public int RotundnessBonus;
        public int ComboTimer;
        public int rjumpTimer;

        public bool GraspsAllNull;
        public bool GraspsAnyNull;
        public bool GraspsFirstNull;

        public float MeowPitch = 1f;

        public int FlipDirection
        {
            get
            {
                if (Mathf.Abs(Cat.bodyChunks[0].pos.x - Cat.bodyChunks[1].pos.x) < 2f)
                {
                    return Cat.flipDirection;
                }
                else
                {
                    return Cat.bodyChunks[0].pos.x > Cat.bodyChunks[1].pos.x ? 1 : -1;
                }
            }
        }

        private Player.AnimationIndex lastAnimationInternal;
        private Player.BodyModeIndex lastBodyModeInternal;
        public NoirData(Player cat)
        {
            Cat = cat;
            UnchangedInput = new Player.InputPackage[cat.input.Length];
        }

        #region uh beam stuff I guess
        public bool OnVerticalBeam()
        {
            return Cat.bodyMode == Player.BodyModeIndex.ClimbingOnBeam;
        }
        public bool OnHorizontalBeam()
        {
            return Cat.animation == Player.AnimationIndex.HangFromBeam || Cat.animation == Player.AnimationIndex.StandOnBeam;
        }
        public bool OnAnyBeam()
        {
            return OnVerticalBeam() || OnHorizontalBeam();
        }
        public bool WasOnVerticalBeam()
        {
            return lastBodyModeInternal == Player.BodyModeIndex.ClimbingOnBeam;
        }
        public bool WasOnHorizontalBeam()
        {
            return lastAnimationInternal == Player.AnimationIndex.HangFromBeam || lastAnimationInternal == Player.AnimationIndex.StandOnBeam;
        }
        public bool WasOnAnyBeam()
        {
            return WasOnVerticalBeam() || WasOnHorizontalBeam();
        }
        #endregion

        #region more beam stuff
        public bool CanCrawlOnBeam()
        {
            var graphics = (PlayerGraphics)Cat.graphicsModule;
            if (graphics == null || Cat.room == null) return false;

            var beamLengthL = 0;
            var beamLengthR = 0;
            while (Cat.room.GetTile(Cat.room.GetTilePosition(graphics.legs.pos + new Vector2(beamLengthR, 0f))).horizontalBeam)
            {
                beamLengthR++;
            }
            while (Cat.room.GetTile(Cat.room.GetTilePosition(graphics.legs.pos + new Vector2(-beamLengthL, 0f))).horizontalBeam)
            {
                beamLengthL++;
            }

            //Debug.Log($"BEAM LENGTH - LEFT: {beamLengthL}, RIGHT: {beamLengthR}, TOTAL: {beamLengthL + beamLengthR}");

            return (Cat.animation == Player.AnimationIndex.StandOnBeam && beamLengthL + beamLengthR > 40 && Ycounter < YcounterTreshold);
        }

        public bool CanGrabBeam()
        {
            var graphics = (PlayerGraphics)Cat.graphicsModule;
            if (graphics == null || Cat.room == null) return false;

            Vector2 pos;
            if (Cat.room.GetTile(Cat.room.GetTilePosition(Cat.bodyChunks[0].pos)).horizontalBeam) pos = Cat.bodyChunks[0].pos;
            else if (Cat.room.GetTile(Cat.room.GetTilePosition(Cat.bodyChunks[1].pos)).horizontalBeam) pos = Cat.bodyChunks[1].pos;
            else if (Cat.room.GetTile(Cat.room.GetTilePosition(graphics.hands[0].pos)).horizontalBeam) pos = graphics.hands[0].pos;
            else if (Cat.room.GetTile(Cat.room.GetTilePosition(graphics.hands[1].pos)).horizontalBeam) pos = graphics.hands[1].pos;
            else return false;

            // Debug.Log("Found a beam tile!");

            var beamLengthL = 0;
            var beamLengthR = 0;
            while (Cat.room.GetTile(Cat.room.GetTilePosition(pos + new Vector2(beamLengthR, 0f))).horizontalBeam)
            {
                beamLengthR++;
            }
            while (Cat.room.GetTile(Cat.room.GetTilePosition(pos + new Vector2(-beamLengthL, 0f))).horizontalBeam)
            {
                beamLengthL++;
            }

            // Debug.Log($"BEAM LENGTH - LEFT: {beamLengthL}, RIGHT: {beamLengthR}, TOTAL: {beamLengthL + beamLengthR}");

            return beamLengthL + beamLengthR > 40;
        }
        #endregion

        #region CanSlash
        public bool CanSlashInpt
        {
            get
            {
                if (Cat.animation == Player.AnimationIndex.Flip)
                {
                    if (Cat.input[0].x == 0 && Cat.input[0].y == 0) return true;
                }
                else
                {
                    if (Cat.input[0].x == 0) return true;
                }
                return false;
            }
        }
        public bool CanSlash
        {
            get
            {
                if (Cat.input[0].thrw && !Cat.input[1].thrw ||
                    Cat.input[0].thrw && Options.NoirAutoSlash.Value && AutoSlashCooldown == 0)
                {
                    if (!Options.NoirAltSlashConditions.Value)
                    {
                        if (GraspsAllNull ||
                            GraspsAnyNull && (CanSlashInpt || !Cat.IsObjectThrowable(Cat.grasps[0]?.grabbed) || !Cat.IsObjectThrowable(Cat.grasps[1]?.grabbed)))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (GraspsAllNull || GraspsFirstNull ||
                            GraspsAnyNull && !Cat.IsObjectThrowable(Cat.grasps[0]?.grabbed))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        #endregion

        #region CustomCombat
        public void ClawHit()
        {
            ComboTimer = 40 * 3; //multiplier is number of seconds
            ComboBonus += 1;
        }

        private void CombatUpdate()
        {
            SlashCooldown[0].Tick();
            SlashCooldown[1].Tick();
            AirSlashCooldown.Tick();
            AutoSlashCooldown.Tick();
            ComboTimer.Tick();
            if (ComboTimer == 0 && ComboBonus > 0)
            {
                ComboBonus = 0;
            }

            if (Cat.animation == Player.AnimationIndex.RocketJump) rjumpTimer++;
            else rjumpTimer = 0;
            if (Cat.animation == Player.AnimationIndex.BellySlide || rjumpTimer >= 15) MovementBonus = 2;
            else MovementBonus = 0;

            if (Plugin.RotundWorld)
            {
                RotundnessBonus = (int)((Cat.bodyChunks[1].mass - DefaultFirstChunkMass) * 15f);
            }
        }
        #endregion

        public void Update()
        {
            CombatUpdate();

            if (Jumping)
            {
                if (Cat.mainBodyChunk.pos.y < Cat.mainBodyChunk.lastPos.y && Cat.animation != Player.AnimationIndex.RocketJump &&
                    !JumpInitiated && !Cat.standing &&
                    Cat.bodyMode == Player.BodyModeIndex.Default && Cat.animation == Player.AnimationIndex.None)
                {
                    //Change anim to RocketJump at the peak of the jump
                    Cat.animation = Player.AnimationIndex.RocketJump;
                    JumpInitiated = true;
                }
                
                if (Cat.bodyChunks[1].lastContactPoint == new IntVector2(0, 0) && Cat.bodyChunks[1].contactPoint != new IntVector2(0, 0) || !WasOnAnyBeam() && OnAnyBeam())
                {
                    Jumping = false;
                    JumpInitiated = false;
                    if (Cat.animation == Player.AnimationIndex.RocketJump)
                    {
                        if (OnVerticalBeam()) Cat.animation = Player.AnimationIndex.ClimbOnBeam;
                        if (OnHorizontalBeam()) Cat.animation = Player.AnimationIndex.HangFromBeam;
                        Cat.animation = Player.AnimationIndex.None;
                    }
                }
            }

            if (Plugin.RotundWorld)
            {
                MeowPitch = 1f - (Cat.bodyChunks[1].mass - DefaultFirstChunkMass) * 0.65f;
                if (MeowPitch < 0.15f) MeowPitch = 0.15f;
            }
            
            lastBodyModeInternal = Cat.bodyMode;
            lastAnimationInternal = Cat.animation;
            GraspsAllNull = Cat.grasps.All(x => x is null);
            GraspsAnyNull = Cat.grasps.Any(x => x is null);
            GraspsFirstNull = Cat.grasps[0] == null;
        }

        public void UpdateRealTime()
        {
            MeowUpdate(this);
        }
    }

    public const float DefaultFirstChunkMass = 0.315f;

    private static void PlayerOnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (!self.TryGetNoir(out var noirData)) return;
        noirData.Update();
    }
    
    private static void RainWorldOnUpdate(On.RainWorld.orig_Update orig, RainWorld self)
    {
        orig(self);
        if (self.processManager?.currentMainLoop is not RainWorldGame game) return;
        if (game.GamePaused) return;
        foreach (var absPlayer in game.Players)
        {
            var player = (Player)absPlayer.realizedCreature;
            if (player == null) continue;

            if (!player.TryGetNoir(out var noirData)) continue;
            noirData.UpdateRealTime();
        }
    }
}
