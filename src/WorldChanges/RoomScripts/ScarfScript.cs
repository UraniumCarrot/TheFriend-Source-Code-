using System.Collections.Generic;
using System.Linq;
using Expedition;
using TheFriend.Objects;
using TheFriend.Objects.SolaceScarfObject;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.WorldChanges.ScarfScripts;

public class ScarfScript : RoomScript
{
    public static List<string> ListOfRooms = new List<string>(); // Rooms scarves should generate in
    public static List<Vector2> ListOfPos = new List<Vector2>(); // Default positions for the scarves to spawn
    
    public static void FindVistaRooms(StoryGameSession self)
    {
    //    if (ListOfRooms.Count < 1 || ListOfPos.Count < 1)
    //    {
    //        ListOfRooms.Clear();
    //        ListOfPos.Clear();
    //        if (ChallengeTools.VistaLocations == null)
    //        {
    //            ChallengeTools.GenerateVistaLocations();
    //            ListOfRooms.AddRange(ChallengeTools.VistaLocations!.Keys);
    //            ListOfPos.AddRange(ChallengeTools.VistaLocations.Values);
    //            ChallengeTools.VistaLocations.Clear();
    //            ChallengeTools.VistaLocations = null;
    //        }
    //        else
    //        {
    //            ListOfRooms.AddRange(ChallengeTools.VistaLocations.Keys);
    //            ListOfPos.AddRange(ChallengeTools.VistaLocations.Values);
    //        }
    //    }
    //    self.SessionData().RoomsWithNewScripts.AddRange(ListOfRooms);
    }

    public SolaceScarfAbstract scarf;
    public Vector2 originalPos;
    public bool ThisRoomHasNoPoles;
    public ScarfScript(Room room, Vector2 pos) : base(room)
    {
        var newScarf = new SolaceScarfAbstract(room.world,room.GetWorldCoordinate(pos), room.game.GetNewID(), room.world.name);
        GenericObjectTools.TrulyAdd(newScarf, room.abstractRoom);
        this.scarf = newScarf;
        originalPos = pos;
        Debug.Log($"Solace: Ran scarf script, scarf pos: {pos}, room: {room.abstractRoom.name}");
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (!room.GetTile(originalPos).AnyBeam || room.GetTile(originalPos).Solid) FindPoles();
        
        if (scarf.realizedObject != null)
        {
            var real = scarf.realizedObject;
            if (real.grabbedBy.Any() || ThisRoomHasNoPoles)
            {
                Destroy();
                return;
            }
            real.firstChunk.vel = Vector2.zero;
            real.firstChunk.pos = originalPos;
            real.firstChunk.lastPos = originalPos;
        }
        scarf.pos = room.GetWorldCoordinate(originalPos);
    }

    public void FindPoles()
    {
        List<Room.Tile> list = new List<Room.Tile>();
        var state = Random.state;
        Random.InitState(room.abstractRoom.index);
        
        foreach (Room.Tile tile in room.Tiles)
        {
            if (tile.AnyBeam &&
                !tile.AnyWater && 
                !tile.Solid)
                list.Add(tile);
        }

        if (list.Count > 0)
        {
            var chosenTile = list[Random.Range(0, list.Count - 1)];
            originalPos = room.MiddleOfTile(chosenTile.X, chosenTile.Y);
            (scarf.realizedObject as SolaceScarf)?.ResetRag();
        }
        else ThisRoomHasNoPoles = true;
        Random.state = state;
    }
}