using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using Random = UnityEngine.Random;
using static SeedCob;

namespace TheFriend.NoirThings;

public static class SeedCobCWT
{
    public static readonly ConditionalWeakTable<AbstractSeedCob, SeedCobData> SeedCobDeets = new ConditionalWeakTable<AbstractSeedCob, SeedCobData>();

    public static SeedCobData GetSeedCobData(this AbstractSeedCob seedCob) => SeedCobDeets.GetValue(seedCob, _ => new(seedCob));
    public class SeedCobData
    {
        public AbstractSeedCob Owner;
        public bool[] SeedsPopped;
        public SeedCobData(AbstractSeedCob seedCob)
        {
            Owner = seedCob;
        }
    }
}

public partial class NoirCatto
{
    public partial class CatSlash
    {
        public const int MaxSeedsToTake = 6;
        private void HitCob(SeedCob seedCob)
        {
            StuffHit.Add(seedCob);

            if (!seedCob.AbstractCob.opened)
            {
                seedCob.Open();
                return;
            }

            var seedsTotal = seedCob.seedsPopped.Length;
            var step = seedsTotal / MaxSeedsToTake;
            var seedsAvailable = seedCob.seedsPopped.Count(s => s) / step;
            if (seedsAvailable <= 0) return;

            for (var i = 0; i < step; i++)
            {
                for (var j = 0; j < seedsTotal; j++)
                {
                    if (seedCob.seedsPopped[j])
                    {
                        seedCob.seedsPopped[j] = false;
                        break;
                    }
                }
            }

            //for (var i = 0; i < 2; i++) //Spawn seeds
            {
                var absSeed = new AbstractConsumable(seedCob.room.world, MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.Seed, null,
                    seedCob.room.GetWorldCoordinate(seedCob.placedPos), seedCob.room.game.GetNewID(), -1, -1, null);
                seedCob.room.abstractRoom.AddEntity(absSeed);
                absSeed.RealizeInRoom();
                var realSeed = absSeed.realizedObject;
                realSeed.firstChunk.HardSetPosition(firstChunk.pos + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
                realSeed.firstChunk.vel = firstChunk.vel * 0.5f;
            }
        }
    }

    //Saving and restoring from abstract state
    private static void SeedCobOnPlaceInRoom(On.SeedCob.orig_PlaceInRoom orig, SeedCob self, Room placeroom)
    {
        orig(self, placeroom);
        var seedCobData = self.AbstractCob.GetSeedCobData();
        if (seedCobData.SeedsPopped != null)
            self.seedsPopped = seedCobData.SeedsPopped;
    }

    private static void AbstractPhysicalObjectOnAbstractize(On.AbstractPhysicalObject.orig_Abstractize orig, AbstractPhysicalObject self, WorldCoordinate coord)
    {
        if (self is SeedCob.AbstractSeedCob abstractSeedCob)
        {
            if (abstractSeedCob.realizedObject is SeedCob seedCob)
            {
                abstractSeedCob.GetSeedCobData().SeedsPopped = seedCob.seedsPopped;
            }
        }
        orig(self, coord);
    }

    //ILHooks
    //Disabling the ability to eat from popcorn plants which do not have seeds
    private static void SeedCobILUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;

            c.GotoNext(MoveType.AfterLabel,
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt<SeedCob>(typeof(SeedCob).GetGetterMethodName(nameof(SeedCob.AbstractCob))),
                i => i.MatchLdfld<AbstractSeedCob>(nameof(AbstractSeedCob.dead)),
                i => i.MatchBrtrue(out label)
            );

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((SeedCob self) =>
            {
                var seedsAvailable = self.seedsPopped.Count(s => s);
                return seedsAvailable <= 0;
            });
            c.Emit(OpCodes.Brtrue, label);
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError("ILHook failed - SeedCob Update");
            Plugin.LogSource.LogError(ex);
        }
    }

    //Skipping the canBeHitByWeapons check
    private static void WeaponILUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            var poLocalIndex = 0;

            c.GotoNext(MoveType.After,
                i => i.MatchLdloc(out poLocalIndex),
                i => i.MatchCallOrCallvirt(out _),
                i => i.MatchLdfld<PhysicalObject>(nameof(PhysicalObject.canBeHitByWeapons)),
                i => i.MatchBrfalse(out _));

            label = il.DefineLabel(c.Next);

            c.GotoPrev(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<UpdatableAndDeletable>(nameof(UpdatableAndDeletable.room)));

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, poLocalIndex);
            c.EmitDelegate((Weapon weapon, int poIndex) =>
            {
                var target = weapon.room.physicalObjects[0][poIndex];

                //If target is of our interest, we skip the canBeHitByWeapons check
                //I have since found out this is useless for SeedCob I think, (it checks for loose objects)
                //but may be used for other stuff in the future...
                if (weapon is CatSlash &&
                    target is SeedCob seedCob && !seedCob.AbstractCob.dead)
                {
                    return true;
                }

                return false;
            });
            c.Emit(OpCodes.Brtrue, label);
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError("ILHook failed - Weapon Update");
            Plugin.LogSource.LogError(ex);
        }
    }

    private static void SharedPhysicsILTraceProjectileAgainstBodyChunks(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            var poLocalIndex = 0;

            c.GotoNext(MoveType.After,
                i => i.MatchLdloc(out poLocalIndex),
                i => i.MatchLdfld<PhysicalObject>(nameof(PhysicalObject.canBeHitByWeapons)),
                i => i.MatchBrfalse(out _)
            );

            label = il.DefineLabel(c.Next);

            c.GotoPrev(MoveType.Before,
                i => i.MatchLdloc(out _),
                i => i.MatchLdfld<PhysicalObject>(nameof(PhysicalObject.canBeHitByWeapons))
            );

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, poLocalIndex);
            c.EmitDelegate((SharedPhysics.IProjectileTracer projTracer, PhysicalObject target) =>
            {
                //If target is of our interest, we skip the canBeHitByWeapons check
                if (projTracer is CatSlash &&
                    target is SeedCob seedCob && !seedCob.AbstractCob.dead)
                {
                    return true;
                }
                return false;
            });
            c.Emit(OpCodes.Brtrue, label);
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError("ILHook failed - SharedPhysics.TraceProjectileAgainstBodyChunks");
            Plugin.LogSource.LogError(ex);
        }
    }
}