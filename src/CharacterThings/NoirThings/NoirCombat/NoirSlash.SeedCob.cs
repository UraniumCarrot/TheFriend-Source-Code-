using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using Random = UnityEngine.Random;
using static SeedCob;

namespace TheFriend.CharacterThings.NoirThings;

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
    public static void SeedCobOnPlaceInRoom(On.SeedCob.orig_PlaceInRoom orig, SeedCob self, Room placeroom)
    {
        orig(self, placeroom);
        var seedCobData = self.AbstractCob.GetSeedCobData();
        if (seedCobData.SeedsPopped != null)
            self.seedsPopped = seedCobData.SeedsPopped;
    }

    public static void AbstractPhysicalObjectOnAbstractize(On.AbstractPhysicalObject.orig_Abstractize orig, AbstractPhysicalObject self, WorldCoordinate coord)
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
    
    /*Surrounding IL***************************************************************************************
    // if (AbstractCob.dead || !(open > 0.8f))
            v---- GotoNext Lands here ----
	IL_0870: ldarg.0
	IL_0871: call instance class SeedCob/AbstractSeedCob SeedCob::get_AbstractCob()
	IL_0876: ldfld bool SeedCob/AbstractSeedCob::dead
	IL_087b: brtrue IL_0b52

	IL_0880: ldarg.0
	IL_0881: ldfld float32 SeedCob::open
	IL_0886: ldc.r4 0.8
	IL_088b: ble.un IL_0b52
    *******************************************************************************************************/
    internal static void SeedCobILUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;

            c.GotoNext(MoveType.AfterLabel,
                i => i.MatchLdarg(0),
                i => i.MatchGetterCall<SeedCob>(nameof(SeedCob.AbstractCob)),
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
    
    /*Surrounding IL***************************************************************************************
	// if (room.physicalObjects[0][num7] == this || !room.physicalObjects[0][num7].canBeHitByWeapons)
	IL_105d: br IL_14f4
	// loop start (head: IL_14f4)
		IL_1062: ldarg.0
		IL_1063: ldfld class Room UpdatableAndDeletable::room
		IL_1068: ldfld class [mscorlib]System.Collections.Generic.List`1<class PhysicalObject>[] Room::physicalObjects
		IL_106d: ldc.i4.0
		IL_106e: ldelem.ref
		IL_106f: ldloc.s 20
		IL_1071: callvirt instance !0 class [mscorlib]System.Collections.Generic.List`1<class PhysicalObject>::get_Item(int32)
		IL_1076: ldarg.0
		IL_1077: beq IL_14ee

        ---- GotoPrev Lands Here ----
		IL_107c: ldarg.0
		IL_107d: ldfld class Room UpdatableAndDeletable::room
		IL_1082: ldfld class [mscorlib]System.Collections.Generic.List`1<class PhysicalObject>[] Room::physicalObjects
		IL_1087: ldc.i4.0
		IL_1088: ldelem.ref
		IL_1089: ldloc.s 20
		IL_108b: callvirt instance !0 class [mscorlib]System.Collections.Generic.List`1<class PhysicalObject>::get_Item(int32)
		IL_1090: ldfld bool PhysicalObject::canBeHitByWeapons
		IL_1095: brfalse IL_14ee
		---- GotoNext Lands Here ----
    *******************************************************************************************************/
    internal static void WeaponILUpdate(ILContext il)
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
    
    /*Surrounding IL****************************************************************************************
    IL_004c: ldloca.s 5
    IL_004e: call instance !0 valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class PhysicalObject>::get_Current()
    IL_0053: stloc.s 6
    // if (item == exemptObject || !item.canBeHitByWeapons || (projTracer != null && !projTracer.HitThisObject(item)))
    IL_0055: ldloc.s 6
    IL_0057: ldarg.s exemptObject
    IL_0059: beq IL_0352
    
    ---- GotoPrev Lands Here ----
    IL_005e: ldloc.s 6
    IL_0060: ldfld bool PhysicalObject::canBeHitByWeapons
    IL_0065: brfalse IL_0352
    ---- GotoNext Lands Here ----

    // (no C# code)
    IL_006a: ldarg.0
    IL_006b: brfalse.s IL_007a
    *******************************************************************************************************/
    internal static void SharedPhysicsILTraceProjectileAgainstBodyChunks(ILContext il)
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