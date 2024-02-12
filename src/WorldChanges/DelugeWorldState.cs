using TheFriend.CharacterThings.DelugeThings;
using UnityEngine;
using System;
using BepInEx.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Kittehface.Framework20;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MoreSlugcats;
using UnityEngine.Assertions.Must;
using Random = UnityEngine.Random;
using ColdRoom = On.MoreSlugcats.ColdRoom;
using Logger = UnityEngine.Logger;

namespace TheFriend.WorldChanges;

public class DelugeWorldState
{
    public static bool DelugeWorld(RainWorldGame game)
    {
        if (game?.StoryCharacter == Plugin.DelugeName && game != null)
        { Deluge = true; return true; }
        { Deluge = false; return false; }
    }
    public static bool Deluge;
    
    public static bool FlyAIOnRoomNotACycleHazard(On.FlyAI.orig_RoomNotACycleHazard orig, Room room)
    {
        if (room.roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard && Deluge)
            if (room.world.rainCycle.CycleProgression < 0.2f) return true;
            else return false;
        return orig(room);
    }
    public static void WaterOnInitiateSprites(On.Water.orig_InitiateSprites orig, Water self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam)
    {
        orig(self, sleaser, rcam);
        if (self.room == null) return;
        if ((self.room.roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard || self.room.roomSettings.DangerType == DangerType.FloodAndAerie) && Deluge)
            sleaser.sprites[0].shader = self.room.game.rainWorld.Shaders["WaterSlush"];
    }
    public static void DustPuffOnUpdate(On.RoofTopView.DustpuffSpawner.DustPuff.orig_Update orig, RoofTopView.DustpuffSpawner.DustPuff self, bool eu)
    {
        orig(self, eu);
        if (!Deluge || self.room == null) return;
        self.lifeTime =  Mathf.Lerp(40f, 120f, Random.value) * Mathf.Lerp(0.5f, 1.5f, self.size)/self.room.world.rainCycle.CycleProgression;
    }

    public static void ColdBreathOnUpdate(ColdRoom.ColdBreath.orig_Update orig, MoreSlugcats.ColdRoom.ColdBreath self, bool eu)
    {
        orig(self, eu);
        if (!Deluge || self.room == null) return;
        self.startAlpha = self.room.world.rainCycle.CycleProgression;
    }

    public static void RoomOnUpdate(On.Room.orig_Update orig, Room self)
    {
        orig(self);
        if (!Deluge) return;
        if (self.roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard || 
            self.roomSettings.DangerType == DangerType.FloodAndAerie)
        {
            if (self.world.rainCycle == null) return;
            self.roomSettings.RainIntensity = self.world.rainCycle.CycleProgression;
            if (self.snowSources.Count > 0)
                for (int i = 0; i < self.snowSources.Count(); i++)
                    self.snowSources[i].intensity = self.world.rainCycle.CycleProgression - 0.015f;
        }
    }
    
    public static void RoomOnctor(On.Room.orig_ctor orig, Room self, RainWorldGame game, World world, AbstractRoom abstractroom)
    {
        orig(self, game, world, abstractroom);
        if (!Deluge)
        {
            return;
        }
        var saints = new RoomSettings(abstractroom.name, world.region, false, false, MoreSlugcatsEnums.SlugcatStatsName.Saint);
        if (saints.filePath != null && !self.snow)
        {
            for (int i = 0; i < saints.placedObjects.Count; i++)
                if (saints.placedObjects[i].type == PlacedObject.Type.SnowSource || 
                    saints.placedObjects[i].type == PlacedObject.Type.LanternOnStick ||
                    saints.placedObjects[i].type == PlacedObject.Type.LocalBlizzard)
                    self.roomSettings.placedObjects.Add(saints.placedObjects[i]);
            if (world.region.name != "UG")
                for (int i = 0; i < saints.effects.Count; i++)
                    self.roomSettings.effects.Add(saints.effects[i]);
            if (world.region.name == "SB")
                self.roomSettings.pal = saints.pal;
        }
    }

    public static void DelugeRoomSettings(Room self, RainWorldGame game)
    {
        if (self.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard || 
            self.roomSettings.DangerType == RoomRain.DangerType.Rain) 
            self.roomSettings.DangerType = RoomRain.DangerType.AerieBlizzard;
        
        else if ((self.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain || 
                  self.roomSettings.DangerType == RoomRain.DangerType.Flood) && self.snow)
            self.roomSettings.DangerType = DangerType.FloodAndAerie;
        
        else if (self.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain)
            self.roomSettings.DangerType = RoomRain.DangerType.Flood;
    }
    
    public static void CreatureOnHypothermiaUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            c.GotoNext(i => i.MatchCallvirt<RainCycle>("get_CycleProgression"));
            c.GotoNext(MoveType.After, i => i.Match(OpCodes.Ble_Un));
            ILLabel label = il.DefineLabel();
            c.MarkLabel(label);
            c.GotoPrev(MoveType.AfterLabel ,i => i.MatchLdsfld<ModManager>("MSC"));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Creature, bool>>(crit =>
            {
                if (crit.room == null || crit.room.blizzardGraphics == null || !Deluge) return false;
                return true;
            });
            c.Emit(OpCodes.Brtrue_S, label);
        }
        catch (Exception e) { Plugin.LogSource.LogError(e); }
    }
}