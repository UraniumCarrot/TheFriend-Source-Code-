using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using TheFriend.SlugcatThings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.CharacterThings.NoirThings;

public partial class NoirCatto
{
    public enum SlashType
    {
        Default,
        AirSlash,
        AirSlash2,
    }
    
    public class AbstractCatSlash : AbstractPhysicalObject
    {
        public readonly Player Owner;
        public readonly SlashType SlashType;
        public readonly int HandUsed;

        public AbstractCatSlash(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, Player owner, int handUsed, SlashType slashType = SlashType.Default) : base(world, type, realizedObject, pos, ID)
        {
            Owner = owner;
            SlashType = slashType;
            HandUsed = handUsed;
        }
    }

    public partial class CatSlash : Weapon
    {
        public const int BaseSlashCooldown = 40;
        public const int BaseAutoSlashCooldown = BaseSlashCooldown / 2;

        public readonly Player Owner;
        public readonly SlashType SlashType;
        public readonly int HandUsed;
        private readonly NoirData noirData;
        
        public readonly int Direction;
        private int MaxLifetime = 40;
        private int lifeTime;
        private float Radius = 30f;
        private float startingAngle;
        private float traveledAngle;
        private float targetAngle = 180f;

        const float AirSlashTargetAngle = 360f;

        private readonly List<PhysicalObject> StuffHit = new List<PhysicalObject>();

        public override bool HeavyWeapon => true;

        public CatSlash(AbstractPhysicalObject abstractPhysicalObject, World world, Player owner, int handUsed, SlashType slashType) : base(abstractPhysicalObject, world)
        {
            Owner = owner;
            SlashType = slashType;
            HandUsed = handUsed;
            noirData = owner.GetNoir();
            Direction = owner.flipDirection;

            bodyChunks = new[] { new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f) };
            bodyChunkConnections = Array.Empty<BodyChunkConnection>();
            airFriction = 0.999f;
            gravity = 0f;
            bounce = 0f;
            surfaceFriction = 0.999f;
            waterFriction = 0.999f;
            waterRetardationImmunity = 1f;
            buoyancy = 0f;
            exitThrownModeSpeed = float.MinValue;
            rotation = Vector2.zero;
            Direction = Owner.flipDirection;
            tailPos = firstChunk.pos;
            firstChunk.loudness = 9f;
            soundLoop = new ChunkDynamicSoundLoop(firstChunk);

            switch (slashType)
            {
                case SlashType.AirSlash:
                case SlashType.AirSlash2:
                    var angle = Custom.AimFromOneVectorToAnother(Owner.bodyChunks[0].pos, Owner.bodyChunks[1].pos);
                    MaxLifetime = 80;
                    startingAngle = angle;
                    if (slashType is SlashType.AirSlash2)
                    {
                        startingAngle += 180f;
                    }
                    targetAngle = AirSlashTargetAngle;
                    break;
                default:
                    startingAngle = 0f;
                    if (owner.animation == Player.AnimationIndex.BellySlide)
                    {
                        Radius += 10f;
                    }
                    break;
            }

            firstChunk.pos = GetSpawnPosition();
            firstChunk.lastPos = firstChunk.pos;
            tailPos = firstChunk.pos;

            collisionLayer = 0; //default = 2;
            firstChunk.collideWithTerrain = false;
            firstChunk.collideWithObjects = true;

            mode = Mode.Thrown;
            thrownBy = owner;
            throwDir = new IntVector2(Direction, 0);
            firstChunk.vel.x = Direction * Radius * 0.6f;
            firstChunk.vel = Custom.RotateAroundOrigo(firstChunk.vel, startingAngle * Direction);
        }

        private Vector2 GetSpawnPosition()
        {
            return Owner.firstChunk.pos + Custom.RotateAroundOrigo(new Vector2(Radius * Direction, 0f), (-90f + startingAngle) * Direction);
        }

        public override void PlaceInRoom(Room placeRoom)
        {
            placeRoom.AddObject(this);

            if (noirData.MovementBonus > 0 || noirData.CombinedBonus >= 4) placeRoom.PlaySound(NoirCatto.SlashSND, firstChunk);
            else placeRoom.PlaySound(SoundID.Slugcat_Throw_Rock, firstChunk, false, 0.5f, 1.2f);
        }

        private bool markedToRemove;
        public override void Update(bool eu)
        {
            lifeTime++;
            if (lifeTime >= MaxLifetime)
            {
                Destroy();
                return;
            }

            AirSlashUpdate();
            TryDeflect();
            RotationUpdate();

            base.Update(eu);
            var posAdjustment = Owner.firstChunk.pos - Owner.firstChunk.lastPos;
            firstChunk.pos = Vector2.Lerp(firstChunk.pos, firstChunk.pos + posAdjustment, 1f);

            MovePaw();

            soundLoop.sound = SoundID.None;
        }
        private void RotationUpdate()
        {
            if (mode == Mode.Free) return;

            var ang = Mathf.Acos(firstChunk.vel.magnitude / (Radius));
            ang *= Mathf.Rad2Deg;
            ang = 90 - ang;

            firstChunk.vel = Custom.RotateAroundOrigo(firstChunk.vel, ang * Direction);
            traveledAngle += ang;
            if (traveledAngle >= targetAngle)
            {
                Destroy();
            }
        }
        private void AirSlashUpdate()
        {
            if (SlashType is SlashType.AirSlash or SlashType.AirSlash2)
            {
                if (!noirData.Jumping && !markedToRemove)
                {
                    if (traveledAngle + 30f < AirSlashTargetAngle)
                    {
                        targetAngle = traveledAngle + 30f; //Don't go full cycle if already on ground
                    }
                    markedToRemove = true;
                }
            }
        }
        private void TryDeflect()
        {
            for (var i = 0; i < room?.physicalObjects.Length; i++)
            {
                for (var j = 0; j < room?.physicalObjects[i].Count; j++)
                {
                    if (room?.physicalObjects[i][j] is Weapon wep && wep.mode == Mode.Thrown)
                    {
                        if (wep is CatSlash slash && slash.Owner == Owner) break;
                        if (!Custom.DistLess(wep.firstChunk.pos, firstChunk.pos, wep.firstChunk.vel.magnitude + Radius)) break;

                        if (SlashType is SlashType.AirSlash or SlashType.AirSlash2)
                        {
                            HitAnotherThrownWeapon(wep);
                        }
                        else
                        {
                            var wepDir = Custom.AimFromOneVectorToAnother(wep.firstChunk.lastPos, wep.firstChunk.pos);
                            var thisDir = Custom.AimFromOneVectorToAnother(Owner.firstChunk.pos, firstChunk.pos);

                            const int maxAngle = 90;
                            if (wepDir + thisDir <= maxAngle && wepDir + thisDir >= -maxAngle)
                            {
                                HitAnotherThrownWeapon(wep);
                            }
                        }
                    }
                }
            }
        }
        private void MovePaw()
        {
            if (traveledAngle < targetAngle)
            {
                var noirGraphics = (PlayerGraphics)Owner.graphicsModule;
                noirGraphics.hands[HandUsed].mode = Limb.Mode.HuntAbsolutePosition;
                noirGraphics.hands[HandUsed].huntSpeed = 1000f;
                noirGraphics.hands[HandUsed].quickness = 100f;
                noirGraphics.hands[HandUsed].reachingForObject = true;
                noirGraphics.hands[HandUsed].absoluteHuntPos = this.firstChunk.pos;
            }
        }


        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            var directionAndMomentum = firstChunk.vel * Custom.PerpendicularVector(firstChunk.vel) * firstChunk.mass;

            if (result.obj == null)
            {
                return false;
            }

            if (StuffHit.Contains(result.obj)) return false;


            if (result.obj is SeedCob seedCob) HitCob(seedCob);

            if (result.obj is Creature crit)
            {
                if (crit == Owner) return false;
                if (((ModManager.CoopAvailable && !Custom.rainWorld.options.friendlyFire) ||
                     room.game.IsArenaSession && !room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.spearsHitPlayers) &&
                    crit is Player) return false;
                if (crit is TubeWorm or JetFish && crit.grabbedBy.Any(x => x.grabber == Owner)) return false; //todo Add tamed lizards

                var smallCrit = Owner.IsSmallerThanMe(crit);

                var baseStunValue = Options.NoirBuffSlash.Value ? 20f : 10f;
                var baseDamageValue = Options.NoirBuffSlash.Value ? 0.2f : 0.1f;

                var stunBonus = baseStunValue * (noirData.CombinedBonus + 1);
                if (smallCrit) stunBonus += 20f;

                var damage = 0.2f + baseDamageValue * (noirData.CombinedBonus + 1);
                if (crit is Player) damage *= 2f;

                var hitShield = false;
                if (crit is Lizard liz && result.chunk.index == 0)
                {
                    if (liz.HitHeadShield(directionAndMomentum))
                    {
                        hitShield = true;
                        if (SlashType is not (SlashType.AirSlash or SlashType.AirSlash2))
                        {
                            Owner.Stun(30);
                            noirData.SlashCooldown[0] = 50;
                            noirData.SlashCooldown[1] = 50;
                            room?.PlaySound(NoirCatto.MeowFrustratedSND, Owner.firstChunk, false, 1f, noirData.MeowPitch);
                        }
                    }
                }

                if (crit is Player pl)
                {
                    if (pl.SlugCatClass == Plugin.DragonName) //Don't scratch up Poacher
                    {
                        //ToDo: tried directly using Poacher's hook, no worky, please reimplement something better, don't be lazy Noir
                    }
                }

                if (hitShield)
                {
                    damage *= 0.5f;
                    stunBonus *= 0.5f;
                }
                crit.Violence(firstChunk, directionAndMomentum, result.chunk, result.onAppendagePos, Creature.DamageType.Stab, damage, stunBonus);
                StuffHit.Add(crit);
                noirData.ClawHit();
                if (!smallCrit) firstChunk.vel = firstChunk.vel * 0.5f + Custom.DegToVec(90f) * 0.1f * firstChunk.vel.magnitude;
            }

            else if (result.chunk != null)
            {
                //change direction to perpendicular?
                result.chunk.vel += firstChunk.vel * firstChunk.mass / result.chunk.mass;
            }
            else if (result.onAppendagePos != null)
            {
                //change direction to perpendicular?
                ((IHaveAppendages)result.obj).ApplyForceOnAppendage(result.onAppendagePos, firstChunk.vel * firstChunk.mass);
            }

            room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, firstChunk, false, 0.30f, 0.65f);
            if (result.chunk != null)
            {
                room.AddObject(new ExplosionSpikes(room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
            }

            return true;
        }

        public override void HitSomethingWithoutStopping(PhysicalObject obj, BodyChunk chunk, Appendage appendage)
        {
            if (obj is Creature crit)
            {
                crit.SetKillTag(Owner.abstractCreature);
                if (Owner.IsSmallerThanMe(crit)) crit.Die();
                else crit.Stun(80);
            }

            obj.HitByWeapon(this);
            noirData.ClawHit();
        }

        public override void WeaponDeflect(Vector2 inbetweenPos, Vector2 deflectDir, float bounceSpeed)
        {
            firstChunk.pos = Vector2.Lerp(firstChunk.pos, inbetweenPos, 0.5f);
            firstChunk.vel = deflectDir * bounceSpeed * 0.5f;
            noirData.ClawHit();
            if (SlashType is SlashType.Default)
            {
                ChangeMode(Mode.Free);
                room?.PlaySound(MeowFrustratedSND, Owner.firstChunk, false, 1f, noirData.MeowPitch);
            }
        }

        public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
        }

        public override void PickedUp(Creature upPicker)
        {
        }

        public override void ChangeMode(Mode newMode)
        {
            if (newMode == mode)
                return;
            if (newMode == Mode.Thrown || newMode == Mode.StuckInWall)
                ChangeCollisionLayer(0);
            else
                ChangeCollisionLayer(DefaultCollLayer);
            if (newMode != Mode.Thrown)
            {
                throwModeFrames = -1;
                firstFrameTraceFromPos = new Vector2?();
            }

            if (newMode == Mode.Free)
            {
                var extraTime = 5;
                if (lifeTime < MaxLifetime - extraTime)
                {
                    lifeTime = MaxLifetime - extraTime;
                }
            }

            firstChunk.goThroughFloors = true;
            thrownClosestToCreature = null;
            closestCritDist = float.MaxValue;
            mode = newMode;
        }

        #region Graphics

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[1] = new FSprite("Pebble" + Random.Range(1, 15).ToString());

            TriangleMesh.Triangle[] tris =
            {
                new TriangleMesh.Triangle(0, 1, 2),
                new TriangleMesh.Triangle(3, 4, 5)
            };
            var triangleMesh = new TriangleMesh("Futile_White", tris, false);
            sLeaser.sprites[0] = triangleMesh;
            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Background"); //Render behind player
            for (var i = sLeaser.sprites.Length - 1; i >= 0; --i)
            {
                sLeaser.sprites[i].RemoveFromContainer();
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[1].isVisible = false; //Debug flag
            var vector2_1 = Vector2.Lerp(this.firstChunk.lastPos, this.firstChunk.pos, timeStacker);
            if (this.vibrate > 0)
                vector2_1 += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
            sLeaser.sprites[1].x = vector2_1.x - camPos.x;
            sLeaser.sprites[1].y = vector2_1.y - camPos.y;
            var p2 = Vector3.Slerp((Vector3)this.lastRotation, (Vector3)this.rotation, timeStacker);
            sLeaser.sprites[1].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0.0f, 0.0f), (Vector2)p2);


            var firstChunkPosLerpd = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            var tailPosLerpd = Vector2.Lerp(tailPos, firstChunk.lastPos, timeStacker);

            var perpendicularVector = Custom.PerpendicularVector((firstChunkPosLerpd - tailPosLerpd).normalized);

            var vel = firstChunk.vel.magnitude * 0.25f;

            var direction = (tailPosLerpd - firstChunkPosLerpd).normalized;
            var newTailPos = tailPosLerpd + Custom.RotateAroundOrigo(new Vector2(0f, vel), Custom.VecToDeg(direction));
            var newLeadingPos = firstChunkPosLerpd + Custom.RotateAroundOrigo(new Vector2(0f, -vel), Custom.VecToDeg(direction));


            ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(0, firstChunkPosLerpd + perpendicularVector * 2.5f - camPos);
            ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(1, firstChunkPosLerpd - perpendicularVector * 2.5f - camPos);
            ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(2, newTailPos - camPos);

            ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(3, firstChunkPosLerpd + perpendicularVector * 2.5f - camPos);
            ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(4, firstChunkPosLerpd - perpendicularVector * 2.5f - camPos);
            ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(5, newLeadingPos - camPos);

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            color = Color.white;
            sLeaser.sprites[1].color = Color.red;
            if (SlashType is SlashType.Default)
            {
                sLeaser.sprites[0].color = Color.Lerp(Color.white, Color.red, noirData.CombinedBonus / 5f);
            }

            if (SlashType is SlashType.AirSlash or SlashType.AirSlash2)
            {
                var blue = new Color(0.573f, 0.878f, 1f);
                var purple = new Color(0.667f, 0.435f, 0.714f);
                sLeaser.sprites[0].color = Color.Lerp(blue, purple, noirData.CombinedBonus / 5f);
            }
        }

        #endregion
    }
}