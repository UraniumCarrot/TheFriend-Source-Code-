using System.Collections.Generic;
using Noise;
using RWCustom;
using TheFriend.CharacterThings.DelugeThings;
using TheFriend.SlugcatThings;
using UnityEngine;

namespace TheFriend.Objects.DelugePearlObject;

public class DelugePearlMechanics
{
    public static void Apply()
    {
        On.Player.Grabability += PlayerOnGrabability;
        On.Player.NewRoom += PlayerOnNewRoom;
        On.Player.Update += PlayerOnUpdate;
        On.DataPearl.Update += DataPearlOnUpdate;
        On.DataPearl.ctor += DataPearlOnctor;
        On.PhysicalObject.Grabbed += PhysicalObjectOnGrabbed;
        On.SaveState.SessionEnded += SaveStateOnSessionEnded;
    }

    public static void PlayerOnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self.slugcatStats.name == Plugin.DelugeName && 
            self.GetDeluge().pearl == null && 
            self.room != null && 
            !self.GetDeluge().PearlWasTaken)
            MakeDelugePearl(self, self.room);
    }

    public static void SaveStateOnSessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newmalnourished)
    {
        var room = game.world.abstractRooms;
        var players = game.Players;
        try
        {
            for (int i = 0; i < room.Length; i++)
            {
                for (int a = 0; a < room[i].entities.Count; a++)
                {
                    if ((room[i].entities[a] as AbstractPhysicalObject) is DataPearl.AbstractDataPearl pearl)
                    {
                        if (pearl.dataPearlType == DataPearlType.DelugePearl)
                            DeleteDelugePearl(pearl);
                    }
                }
            }
        }
        catch { Debug.Log("Solace: Failed to scan rooms for bauble deletion");}

        try
        {
            for (int b = 0; b < players.Count; b++)
            {
                var player = players[b].realizedObject as Player;
                if (player?.objectInStomach is DataPearl.AbstractDataPearl pearl && pearl.dataPearlType == DataPearlType.DelugePearl)
                    player.objectInStomach = null;
            }
        }
        catch { Debug.Log("Solace: Failed to scan stomachs for bauble deletion");}
        orig(self, game, survived, newmalnourished);
    }

    public static void DataPearlOnctor(On.DataPearl.orig_ctor orig, DataPearl self, AbstractPhysicalObject abstractphysicalobject, World world)
    {
        orig(self, abstractphysicalobject, world);
        if (self.AbstractPearl.dataPearlType == DataPearlType.DelugePearl)
        {
            self.collisionLayer = 0;
            self.AbstractPearl.hidden = false;
        }
    }

    public static void PhysicalObjectOnGrabbed(On.PhysicalObject.orig_Grabbed orig, PhysicalObject self, Creature.Grasp grasp)
    {
        orig(self, grasp);
        try
        {
            if (self is DataPearl pearl && pearl.AbstractPearl.dataPearlType == DataPearlType.DelugePearl)
            {
                var data = pearl.AbstractPearl.DelugePearlData();

                data.owner.GetDeluge().PearlWasTaken = true;
                data.isAttached = false;
            }
        }
        catch { Debug.Log("Solace: Failed to make bauble stealable"); }
    }

    public static void PlayerOnNewRoom(On.Player.orig_NewRoom orig, Player self, Room newroom)
    {
        orig(self, newroom);
        if (self.GetDeluge().PearlWasTaken)
        {
            return;
        }

        try
        {
            if (self.GetDeluge().pearl != null &&
                self.room != self.GetDeluge().pearl.room)
            {
                self.GetDeluge().pearl = null;
            }
            if (self.slugcatStats.name == Plugin.DelugeName && 
                self.room != null && 
                self.GetDeluge().pearl == null)
            {
                MakeDelugePearl(self, newroom);
            }
        }
        catch { Debug.Log("Solace: Something went wrong in making new bauble!"); }
    }
    
    public static Player.ObjectGrabability PlayerOnGrabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        try
        {
            // if (obj is DataPearl pearl && pearl.AbstractPearl.dataPearlType == DataPearlType.DelugePearl)
            // {
            //     var data = pearl.AbstractPearl.DelugePearlData();
            //     if (data.owner.dead || data.owner == null)
            //     {
            //         return orig(self, obj);
            //     }
            //     else return Player.ObjectGrabability.CantGrab;
            // }
        }
        catch { Debug.Log("Solace: Failed to make bauble ungrabbable");}
        return orig(self, obj);
    }

    public static void DataPearlOnUpdate(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
    {
        orig(self, eu);
        if (self.AbstractPearl.dataPearlType == DataPearlType.DelugePearl)
        {
            var pearl = self.AbstractPearl.DelugePearlData();
            var delugeData = pearl.owner.GetDeluge();

            if (pearl.owner == null || 
                self.room != pearl.owner.room || 
                delugeData.pearl == null || 
                self.room.world != pearl.owner.room.world || 
                self.room == null) 
            { 
                DeleteDelugePearl(self.AbstractPearl); 
                return;
            }
            
            if (!delugeData.PearlWasTaken) pearl.isAttached = true;
        }
    }

    public static void MakeDelugePearl(Player self, Room newroom)
    {
        try
        {
            Color pearlCol = DelugePearlGraphics.DelugePearlColor(self.graphicsModule as PlayerGraphics);
            
            var pos = self.room.GetWorldCoordinate(self.bodyChunks[1].pos);
            AbstractPhysicalObject pearl;
            pearl = new DataPearl.AbstractDataPearl(
                self.room.game.world, 
                AbstractPhysicalObject.AbstractObjectType.DataPearl, 
                null, 
                pos, 
                self.room.game.GetNewID(), -1, -1, null, DataPearlType.DelugePearl);
            pearl.pos = pos;
            pearl.RealizeInRoom();
            pearl.realizedObject.firstChunk.HardSetPosition(self.bodyChunks[1].pos);

            var data = (pearl as DataPearl.AbstractDataPearl).DelugePearlData();
            data.owner = self;
            data.ownerInt = self.playerState.playerNumber;
            self.GetDeluge().pearl = pearl.realizedObject as DataPearl;
            data.color = pearlCol;
            var graphics = (PlayerGraphics)self.graphicsModule;
            data.tailConnection = new BodyChunkBodyPartConnection(pearl.realizedObject.firstChunk, graphics.tail[graphics.tail.Length - 1], 0f,
                PhysicalObject.BodyChunkConnection.Type.Pull, 1f, 0.5f);
            data.buttConnection = new PhysicalObject.BodyChunkConnection(pearl.realizedObject.firstChunk, self.bodyChunks[1], PearlCWT.DelugePearl.BasePearlToButtDist,
                PhysicalObject.BodyChunkConnection.Type.Pull, 1f, PearlCWT.DelugePearl.BaseButtConnectionAssymetry);
        
            Debug.Log("Solace: Bauble manifested!");
        }
        catch
        {
            Debug.Log("Solace: Bauble failed to manifest");
        }
    }
    public static void DeleteDelugePearl(DataPearl.AbstractDataPearl self)
    {
        self.Room?.RemoveEntity(self);
        if (self.realizedObject != null)
            self.realizedObject.RemoveFromRoom();
        Debug.Log("Solace: Bauble destroyed!");
    }
}