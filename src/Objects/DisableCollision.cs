using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TheFriend.PoacherThings;
using UnityEngine;

namespace TheFriend.Objects;

public static class DisableCollision
{
    #region RoomUpdate, for objects
    public static void RoomILUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            var index3_local_index = 0;
            var index4_local_index = 0;
            var index5_local_index = 0;

            c.GotoNext(MoveType.Before,
                i => i.MatchLdfld<BodyChunk>(nameof(BodyChunk.pos)),
                i => i.MatchCallOrCallvirt<Vector2>(nameof(Vector2.Distance))
            );

            c.GotoPrev(MoveType.Before,
                i => i.MatchLdfld<Room>(nameof(Room.physicalObjects)),
                i => i.MatchLdloc(out index3_local_index),
                i => i.MatchLdelemRef(),
                i => i.MatchLdloc(out index5_local_index)
            );
            c.GotoPrev(MoveType.Before,
                i => i.MatchLdfld<Room>(nameof(Room.physicalObjects)),
                i => i.MatchLdloc(out index3_local_index),
                i => i.MatchLdelemRef(),
                i => i.MatchLdloc(out index4_local_index)
            );

            c.GotoPrev(MoveType.After,
                i => i.MatchCallOrCallvirt(typeof(RWCustom.Custom), nameof(RWCustom.Custom.DistLess)),
                i => i.MatchBrfalse(out label) //label to the end of the loop
            );

            //we are now inside the triple-quadruple-layer room.physicalObjects loop
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, index3_local_index);
            c.Emit(OpCodes.Ldloc, index4_local_index);
            c.Emit(OpCodes.Ldloc, index5_local_index);

            c.EmitDelegate( (Room self, int index3, int index4, int index5) =>
            {
                var one = self.physicalObjects[index3][index4];
                var two = self.physicalObjects[index3][index5];
                //Here check for conditions for your items, if one is true, we skip the collision check1

                if (SkipLanternSpears(one, two)) return true;

                return false;
            });

            c.Emit(OpCodes.Brtrue, label);

        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError("ILHook failed - Room Update");
            Plugin.LogSource.LogError(ex);
        }
    }

    private static bool SkipLanternSpears(PhysicalObject one, PhysicalObject two)
    {
        //In case a creature is holding the spear, the lantern would collide with creature and make it fly. Let's prevent that
        if (one is Creature && two.abstractPhysicalObject.TryGetLanternData(out var lanternData2))
        {
            if (lanternData2.AttachedSpear != null)
                return true;
        }
        if (two is Creature && one.abstractPhysicalObject.TryGetLanternData(out var lanternData1))
        {
            if (lanternData1.AttachedSpear != null)
                return true;
        }
        return false;
    }
    #endregion

    #region SharedPhysics, for weapons
    public static SharedPhysics.CollisionResult SharedPhysicsOnTraceProjectileAgainstBodyChunks(On.SharedPhysics.orig_TraceProjectileAgainstBodyChunks orig, SharedPhysics.IProjectileTracer projtracer, Room room, Vector2 lastpos, ref Vector2 pos, float rad, int collisionlayer, PhysicalObject exemptobject, bool hitappendages)
    {
        var result = orig(projtracer, room, lastpos, ref pos, rad, collisionlayer, exemptobject, hitappendages);
        var emptyResult = new SharedPhysics.CollisionResult(null, null, null, false, pos);

        if (projtracer is Spear spear && result.obj is Lantern lantern && LanternSpear.AreConnected(spear, lantern)) //Skip lantern spears so they don't hit themselves
            return emptyResult;

        return result;
    }
    #endregion
}