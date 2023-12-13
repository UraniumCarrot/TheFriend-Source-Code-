using System;
using System.Reflection;
using System.Reflection.Emit;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using MonoMod.RuntimeDetour;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace TheFriend.WorldChanges;

public class DangerTypes
{
    public static void Apply()
    {
        new Hook(typeof(RoomRain).GetProperty(nameof(RoomRain.OutsidePushAround))!.GetGetMethod(), ChillyOutsidePushAround);
        new ILHook(typeof(RoomRain).GetProperty(nameof(RoomRain.FloodLevel))!.GetGetMethod(), ChillyFloodLevel);
        
        On.RoomRain.ctor += RoomRainOnctor;
        On.RoomRain.Update += RoomRainOnUpdate;
    }

    public static void RoomRainOnUpdate(On.RoomRain.orig_Update orig, RoomRain self, bool eu)
    {
        orig(self, eu);
        if (self.dangerType == DangerType.FloodAndBlizzard)
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
        if (rm.roomSettings.DangerType == DangerType.FloodAndBlizzard)
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
        if (self.dangerType == DangerType.FloodAndBlizzard)
            return self.globalRain.OutsidePushAround * self.room.roomSettings.RainIntensity;
        else return orig(self);
    }
    public static void ChillyFloodLevel(ILContext il) // Makes FloodAndBlizzard inherently capable of flooding
    {
        try
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After, i => i.Match(OpCodes.Brfalse_S));
            ILLabel label = il.DefineLabel();
            c.MarkLabel(label);
            c.GotoPrev(MoveType.AfterLabel, i=> i.MatchLdfld<RoomRain>("dangerType"));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<RoomRain, bool>>(self =>
            {
                if (self.dangerType == DangerType.FloodAndBlizzard) return true;
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
}