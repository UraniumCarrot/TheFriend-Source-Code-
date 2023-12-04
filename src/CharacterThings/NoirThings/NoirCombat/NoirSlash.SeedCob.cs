using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.NoirThings;

public partial class NoirCatto
{
    public partial class CatSlash
    {
        private void HitCob(SeedCob seedCob)
        {
            StuffHit.Add(seedCob);

            if (!seedCob.AbstractCob.opened)
            {
                seedCob.Open();
                return;
            }

            for (var i = 0; i < 2; i++)
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

    //ILHooks
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