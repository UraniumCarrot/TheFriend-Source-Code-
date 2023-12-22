using System;
using System.Reflection;
using System.Reflection.Emit;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MoreSlugcats;
using UnityEngine;
using MonoMod.RuntimeDetour;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace TheFriend.WorldChanges;

public class DangerTypes
{
    /*
     I'm having to hook so much stuff for FloodAndAerie to act like an actual flood.
     */
    
    
    public static void Apply()
    {
        new Hook(typeof(RoomRain).GetProperty(nameof(RoomRain.OutsidePushAround))!.GetGetMethod(), ChillyOutsidePushAround);
        new ILHook(typeof(RoomRain).GetProperty(nameof(RoomRain.FloodLevel))!.GetGetMethod(), ChillyFloodLevel);

        // Hooks that ensure FloodAndAerie is treated like FloodAndRain
        On.RoomRain.ctor += RoomRainOnctor;
        On.RoomRain.Update += RoomRainOnUpdate;
        On.AbstractCreature.DrainWorldDenFlooded += AbstractCreatureOnDrainWorldDenFlooded;
        On.Water.ctor += WaterOnctor;
        On.ShelterDoor.Update += ShelterDoorOnUpdate;
        IL.RoomCamera.Update += RoomCameraOnUpdate;
        
        // Minor hooks
        
    }

    public static void RoomCameraOnUpdate(ILContext il)
    { // Stops FloodAndBlizzard from getting deleted and remade every update
        try
        {
            var c = new ILCursor(il);
            c.GotoNext(i => i.MatchLdsfld<RoomRain.DangerType>("AerieBlizzard"));
            c.GotoNext(MoveType.Before, i => i.Match(OpCodes.Brfalse_S));
            ILLabel label = il.DefineLabel();
            c.MarkLabel(label);
            c.GotoPrev(MoveType.AfterLabel,i => i.MatchLdfld<RoomCamera>("blizzardGraphics"));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<RoomCamera, bool>>(self =>
            {
                if (self.room != null &&
                    self.blizzardGraphics == null &&
                    self.room.roomSettings.DangerType == DangerType.FloodAndAerie)
                    return true;
                return false;
            });
            c.Emit(OpCodes.Brtrue_S, label);

            c.GotoNext(MoveType.After, i => i.MatchStfld<Room>("blizzard"));
            ILLabel label3 = il.DefineLabel();
            c.MarkLabel(label3);
            c.GotoPrev(i => i.MatchStfld<RoomCamera>("blizzardGraphics"));
            c.GotoPrev(i => i.Match(OpCodes.Ldarg_0));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<RoomCamera, bool>>(self =>
            {
                if (self.room.roomSettings.DangerType == DangerType.FloodAndAerie)
                    return true;
                return false;
            });
            c.Emit(OpCodes.Brtrue, label3);
        }
        catch (Exception e) { Plugin.LogSource.LogError("Solace: IL hook RoomCameraUpdate failed!" + e); }
    }


    #region FloodAndBlizzardHooks
    public static void ShelterDoorOnUpdate(On.ShelterDoor.orig_Update orig, ShelterDoor self, bool eu)
    {
        orig(self, eu);
        if (self.room != null && 
            self.room.game.globalRain.drainWorldFlood > 0f && 
            !self.room.game.globalRain.drainWorldFloodFlag && 
            self.room.roomSettings.DangerType == DangerType.FloodAndAerie && 
            self.room.waterObject == null &&
            self.room.game.Players.Count > 0)
        {
            self.room.AddWater();
            self.room.waterObject.fWaterLevel = self.room.game.Players[0].Room.realizedRoom.roomRain.FloodLevel;
            Plugin.LogSource.LogWarning("Water add");
        }
    }
    public static void WaterOnctor(On.Water.orig_ctor orig, Water self, Room room, int waterlevel)
    {
        Plugin.LogSource.LogWarning("Waterctor called");
        orig(self, room, waterlevel);
        Plugin.LogSource.LogWarning("Waterctor orig done");
        if (room.roomRain != null &&
            room.roomRain.globalRain != null &&
            room.roomSettings.DangerType == DangerType.FloodAndAerie)
        {
            self.fWaterLevel = self.originalWaterLevel + room.roomRain.globalRain.flood;
            self.fWaterLevel = self.originalWaterLevel;
            Plugin.LogSource.LogWarning("Waterctor hooked");
        }
    }
    public static bool AbstractCreatureOnDrainWorldDenFlooded(On.AbstractCreature.orig_DrainWorldDenFlooded orig, AbstractCreature self)
    {
        bool test = orig(self);
        if (test) return orig(self);
        if (self.world == null || self.Room == null || (self.world != null && self.world.game.globalRain.drainWorldFlood == 0f))
            return false;
        if (self.creatureTemplate.doesNotUseDens)
            return false;
        if (self.Room == self.world.offScreenDen)
            return false;
        
        if (self.pos.abstractNode > -1 && self.creatureTemplate.waterRelationship != CreatureTemplate.WaterRelationship.Amphibious && self.creatureTemplate.waterRelationship != CreatureTemplate.WaterRelationship.WaterOnly && self.world.game.globalRain.drainWorldFlood > 0f)
            if (self.Room.realizedRoom != null &&
                self.Room.realizedRoom.roomSettings.DangerType == DangerType.FloodAndAerie &&
                self.Room.realizedRoom.shortCutsReady &&
                self.world.game.globalRain.DrainWorldPositionFlooded(self.Room.realizedRoom.ShortcutLeadingToNode(self.pos.abstractNode).startCoord))
                return true;
        return false;
    }
    public static void RoomRainOnUpdate(On.RoomRain.orig_Update orig, RoomRain self, bool eu)
    {
        orig(self, eu);
        if (self.room == null) return;
        if (self.dangerType == DangerType.FloodAndAerie)
        {
            self.intensity =  Mathf.Lerp(self.intensity, self.globalRain.Intensity * ChillyRoomRainFloodShake(self.room, self.globalRain.flood), 0.2f);
            bool flag = self.globalRain.drainWorldFlood > 0f;
            if (self.room.world.rainCycle.TimeUntilRain <= 0 || flag || self.room.defaultWaterLevel > -1 ||
                self.room.waterInverted || self.room.waterFlux != null)
            {
                if (self.room.waterObject != null)
                {
                    self.room.waterObject.fWaterLevel = Mathf.Lerp(self.room.waterObject.fWaterLevel, self.FloodLevel, ModManager.MSC ? self.globalRain.floodLerpSpeed : 0.2f);
                    if (!ModManager.MMF || self.room.roomSettings.RumbleIntensity > 0f)
                    {
                        self.room.waterObject.GeneralUpsetSurface(Mathf.InverseLerp(0f, 0.5f, self.globalRain.Intensity) * 4f);
                    }
                }
                else if (self.globalRain.deathRain != null || self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxMaxLevel) > 0f)
                {
                    self.room.AddWater();
                    self.room.waterObject!.fWaterLevel = self.FloodLevel;
                }
            }
            if (self.normalRainSound != null)
                self.normalRainSound.Volume = 0;
            if (self.heavyRainSound != null)
                self.heavyRainSound.Volume = 0;
        }
    }
    public static void RoomRainOnctor(On.RoomRain.orig_ctor orig, RoomRain self, GlobalRain globalrain, Room rm)
    {
        orig(self, globalrain, rm);
        if (self.room == null) return;
        if (rm.roomSettings.DangerType == DangerType.FloodAndAerie)
        {
            if (rm.waterObject != null)
                rm.waterObject.fWaterLevel = rm.waterObject.originalWaterLevel + globalrain.flood;
            self.floodingSound.VolumeGroup = 3;
            self.distantDeathRainSound.sound = SoundID.Death_Rain_Approaching_Heard_From_Underground_LOOP;
            self.normalRainSound = null;
            self.heavyRainSound = null;
        }
    }
    
    public delegate float orig_OutsidePushAround(RoomRain self);
    public static float ChillyOutsidePushAround(orig_OutsidePushAround orig, RoomRain self)
    {
        if (self.dangerType == DangerType.FloodAndAerie)
            return self.globalRain.OutsidePushAround * self.room.roomSettings.RainIntensity;
        else return orig(self);
    }
    public static void ChillyFloodLevel(ILContext il) // Makes FloodAndAerie inherently capable of flooding
    {
        try
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After, i => i.Match(OpCodes.Brfalse_S));
            ILLabel label = il.DefineLabel();
            c.MarkLabel(label);
            c.GotoNext(MoveType.AfterLabel, i=> i.MatchLdfld<RoomRain>("dangerType"));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<RoomRain, bool>>(self =>
            {
                if (self.dangerType == DangerType.FloodAndAerie) return true;
                return false;
            });
            c.Emit(OpCodes.Brtrue_S, label);
        }
        catch(Exception e) { Plugin.LogSource.LogError(e); }
    }
    public static float ChillyRoomRainFloodShake(Room room, float globalFloodLevel)
    {
        float y = room.world.RoomToWorldPos(new Vector2(0f, (float)room.abstractRoom.size.y * 20f), room.abstractRoom.index).y;
        float num = 1f - Mathf.InverseLerp(y, y + 800f, globalFloodLevel);
        return 0.75f + num * 0.25f;
    }
    #endregion
}