using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using TheFriend.Objects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.PoacherThings;

public static class LanternSpearCWTs
{
    public static readonly ConditionalWeakTable<AbstractSpear, SpearData> SpearDeets = new ConditionalWeakTable<AbstractSpear, SpearData>();
    public static readonly ConditionalWeakTable<AbstractPhysicalObject, LanternData> LanternDeets = new ConditionalWeakTable<AbstractPhysicalObject, LanternData>();

    public static SpearData GetSpearData(this AbstractSpear abstractSpear) => SpearDeets.GetValue(abstractSpear, _ => new(abstractSpear));
    public class SpearData(AbstractSpear owner)
    {
        public readonly AbstractSpear Owner = owner;
        public AbstractPhysicalObject AttachedLantern;
        public AbstractPhysicalObject.AbstractObjectStick StickTogether;
        public float LanternDistance;
        public float LanternRotation;
        public bool FirstTimePlace = true;

        public void AttachLantern(Lantern lantern)
        {
            if (AttachedLantern != null)
            {
                Plugin.LogSource.LogError("LanternSpear: Lantern is already attached!!!");
                return;
            }

            AttachedLantern = lantern.abstractPhysicalObject;
            AttachedLantern.GetLanternData().AttachedSpear = Owner;
            StickTogether = new GenericObjectStick(Owner, AttachedLantern);
            LanternDistance = Random.Range(-1f, 1f) * Random.Range(0f, 20f); //Total spear length is 50f, 25f from the middle.
            LanternRotation = Random.value * 360f;
        }

        public void DetachLantern()
        {
            if (AttachedLantern == null) return;
            StickTogether.Deactivate();
            StickTogether = null;
            AttachedLantern.GetLanternData().AttachedSpear = null;
            AttachedLantern = null;
        }
    }


    public static LanternData GetLanternData(this AbstractPhysicalObject abstractLantern)
    {
        if (abstractLantern.type != AbstractPhysicalObject.AbstractObjectType.Lantern)
        {
            Plugin.LogSource.LogError("Cannot get LanternData - AbstractPhysicalObject is NOT a Lantern!!!");
            return null;
        }

        return LanternDeets.GetValue(abstractLantern, _ => new(abstractLantern));
    }
    public static bool TryGetLanternData(this AbstractPhysicalObject abstractLantern, out LanternData lanternData)
    {
        if (abstractLantern.type != AbstractPhysicalObject.AbstractObjectType.Lantern)
        {
            lanternData = null;
            return false;
        }

        lanternData = LanternDeets.GetValue(abstractLantern, _ => new(abstractLantern));
        return true;
    }
    public class LanternData(AbstractPhysicalObject abstractLantern)
    {
        public readonly AbstractPhysicalObject Owner = abstractLantern;
        public AbstractSpear AttachedSpear;
        public void DetachSpear() => AttachedSpear?.GetSpearData().DetachLantern();
    }
}

public static class LanternSpear
{
    public static void FromExisting(Spear spear, Lantern lantern)
    {
        lantern.AllGraspsLetGoOfThisObject(true);

        var spearData = spear.abstractSpear.GetSpearData();
        spearData.AttachLantern(lantern);
    }

    public static bool AreConnected(PhysicalObject one, PhysicalObject two)
    {
        if (one is Spear spear1 && two.abstractPhysicalObject.TryGetLanternData(out var lanternData2))
        {
            if (lanternData2?.AttachedSpear == spear1.abstractSpear)
                return true;
        }
        if (two is Spear spear2 && one.abstractPhysicalObject.TryGetLanternData(out var lanternData1))
        {
            if (lanternData1?.AttachedSpear == spear2.abstractSpear)
                return true;
        }
        return false;
    }

    public static void SpearOnUpdate(Spear spear, bool eu)
    {
        var spearData = spear.abstractSpear.GetSpearData();
        if (spearData.AttachedLantern == null) return;

        if (spearData.AttachedLantern.realizedObject is not Lantern lantern)
        {
            spearData.AttachedLantern.RealizeInRoom();
            return;
        }

        if (spear.stuckInWall != null && lantern.firstChunk.vel.sqrMagnitude > 500f) //We were likely hit by a huge force, like an explosion, detach lantern
        {
            spearData.DetachLantern();
            return;
        }

        //Positions updated here
        var spearAngle = Custom.VecToDeg(spear.rotation);
        lantern.firstChunk.pos = Custom.RotateAroundVector(spear.firstChunk.pos + new Vector2(spearData.LanternDistance, 0f), spear.firstChunk.pos, spearAngle + 90f);
        lantern.firstChunk.vel = spear.firstChunk.vel; //not accurate, but should be good enough
        lantern.rotation = Custom.DegToVec(Custom.AimFromOneVectorToAnother(lantern.firstChunk.pos, spear.firstChunk.pos) + spearData.LanternRotation);
    }

    public static void PlayerOnThrownSpear(Player self, Spear spear)
    {
        var spearData = spear.abstractSpear.GetSpearData();
        if (spearData.AttachedLantern != null && spearData.AttachedLantern.realizedObject is Lantern lantern)
        {
            var massSlowDown= spear.TotalMass / (lantern.TotalMass * 0.75f);
            if (massSlowDown < 1f)
                spear.firstChunk.vel *= massSlowDown; //Spear is weighted down, slow it's velocity a little

            if (spear.exitThrownModeSpeed > 10f)
                spear.exitThrownModeSpeed = 10f;
        }
    }

    public static bool CantGrab(PhysicalObject physicalObject) //For the Player.ObjectGrabability hook
    {
        return physicalObject is Lantern lantern && lantern.abstractPhysicalObject.GetLanternData().AttachedSpear != null;
    }

    #region SaveData
    public const string LANTERNSPEARS = nameof(LANTERNSPEARS);
    public static Dictionary<string, (float lanternDistance, float lanternRotation)> LanternSpears = new();

    public static void SavePositions(StoryGameSession session)
    {
        SaveThings.SolaceCustom.SaveStorySpecific(LANTERNSPEARS, LanternSpears, session);
    }

    public static void LoadPositions(StoryGameSession session)
    {
        if (SaveThings.SolaceCustom.LoadStorySpecific(LANTERNSPEARS, out Dictionary<string, (float lanternDistance, float lanternRotation)> lanternSpears, session))
            LanternSpears = lanternSpears;
    }


    public static void SpearOnChangeMode(On.Spear.orig_ChangeMode orig, Spear self, Weapon.Mode newmode)
    {
        orig(self, newmode);
        var spearData = self.abstractSpear.GetSpearData();

        if (self.stuckInWall != null && spearData.AttachedLantern != null)
        {
            LanternSpears[self.abstractSpear.ID.ToString()] = (spearData.LanternDistance, spearData.LanternRotation);
            self.abstractSpear.stuckInWallCycles = 99999;
        }
        else if (!spearData.FirstTimePlace)
            LanternSpears.Remove(self.abstractSpear.ID.ToString());
    }

    public static void RoomOnShortCutsReady(On.Room.orig_ShortCutsReady orig, Room self)
    {
        orig(self);
        foreach (var spear in self.abstractRoom.entities.OfType<AbstractSpear>().ToArray())
        {
            var spearId = spear.ID.ToString();
            var spearData = spear.GetSpearData();
            if (LanternSpears.ContainsKey(spearId) && spearData.FirstTimePlace)
            {
                var abstractLantern = new AbstractPhysicalObject(self.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, spear.pos, self.game.GetNewID());
                self.abstractRoom.entities.Add(abstractLantern);
                abstractLantern.RealizeInRoom();

                spearData.AttachLantern((Lantern)abstractLantern.realizedObject);
                spearData.LanternDistance = LanternSpears[spearId].lanternDistance;
                spearData.LanternRotation = LanternSpears[spearId].lanternRotation;
                spearData.FirstTimePlace = false;
            }
        }
    }

    public static void CheckShelter(RainWorldGame self)
    {
        foreach (var cam in self.cameras)
        {
            if (cam?.room == null) continue;
            if (cam.room.abstractRoom.shelter)
            {
                foreach (var spear in cam.room.abstractRoom.entities.Where(x => x is AbstractSpear).ToArray())
                {
                    var spearData = ((AbstractSpear)spear).GetSpearData();
                    if (spearData.AttachedLantern != null)
                    {
                        LanternSpears[spear.ID.ToString()] = (spearData.LanternDistance, spearData.LanternRotation); //Save lanternSpear data

                        var lantern = spearData.AttachedLantern;
                        spearData.DetachLantern();
                        cam.room.abstractRoom.entities.Remove(lantern); //Prevent game from saving the lantern, we will add it on cycle start ourselves
                    }
                }
            }
        }
    }
    #endregion
    
    #region Misc IL Code
    public static void WeaponILUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            var index3_local_index = 0;

            c.GotoNext(MoveType.After, i => i.MatchCallOrCallvirt(typeof(Custom), nameof(Custom.Log)));
            label = il.DefineLabel(c.Next);

            c.GotoPrev(i => i.MatchCallOrCallvirt<AbstractPhysicalObject.AbstractObjectStick>(nameof(AbstractPhysicalObject.AbstractObjectStick.Deactivate)));
            c.GotoPrev(i => i.MatchLdloc(out index3_local_index));
            c.GotoPrev(MoveType.AfterLabel, i => i.MatchLdarg(0));

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, index3_local_index);
            c.EmitDelegate((Weapon self, int index3) =>
            {
                var stick = self.abstractPhysicalObject.stuckObjects[index3];
                return stick is GenericObjectStick &&
                       stick.A.TryGetLanternData(out var lanternDataA) && lanternDataA.AttachedSpear != null ||
                       stick.B.TryGetLanternData(out var lanternDataB) && lanternDataB.AttachedSpear != null;
            });

            c.Emit(OpCodes.Brtrue, label); //Weapon.Update will automatically remove every stick other than first, I don't want him to do that for LanternSpears
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError("ILHook failed - LanternSpear_WeaponILUpdate");
            Plugin.LogSource.LogError(ex);
        }
    }
    #endregion
}