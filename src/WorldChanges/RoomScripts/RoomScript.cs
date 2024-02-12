using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TheFriend.WorldChanges.ScarfScripts;

public abstract class RoomScript : UpdatableAndDeletable
{
    public static void StoryGameSessionOnctor(StoryGameSession self, SlugcatStats.Name savestatenumber, RainWorldGame game)
    { // Add to list of rooms that will have new scripts
        ScarfScript.FindVistaRooms(self);
    }
    public static void RoomSpecificScriptOnAddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
    { // Create custom scripts for new rooms
        orig(room);
        if (ScarfScript.ListOfRooms.Contains(room.abstractRoom.name)) // Generate scarves
            room.AddObject(new ScarfScript(
                room,
                ScarfScript.ListOfPos
                    [ScarfScript.ListOfRooms.IndexOf(ScarfScript.ListOfRooms.Find(x => x == room.abstractRoom.name))]
                ));
    }
    
    public RoomScript(Room room)
    {
        this.room = room;
    }
    
    public override void Destroy()
    {
        Debug.Log($"Solace: Destroying room script for {room.abstractRoom.name}");
        base.Destroy();
    }

    public static void ScriptMaker(Room room)
    {
        if (room.GetScriptData(out var a) && a.RoomsWithNewScripts.Contains(room.abstractRoom.name))
        {
            room.roomSettings.roomSpecificScript = true;
            Debug.Log($"Solace: Made roomscript true! For: {room.abstractRoom.name}");
        }
    }
}

public static class ScriptDataCWT
{ // Allows room scripts to be found and stored without holding them in memory permanently
    public class GameCWT
    {
        public List<string> RoomsWithNewScripts = new List<string>();
        public StoryGameSession self;
        public GameCWT(StoryGameSession game)
        {
            self = game;
        }
    }
    public static readonly ConditionalWeakTable<StoryGameSession, GameCWT> CWT = new();
    public static GameCWT ScriptData(this StoryGameSession game) => CWT.GetValue(game, _ => new GameCWT(game));
    
    public static bool GetScriptData(this RainWorldGame game, out GameCWT data)
    {
        if (game.GetStorySession != null)
        {
            data = game.GetStorySession.ScriptData();
            return true;
        }
        data = null;
        return false;
    }
    public static bool GetScriptData(this Room room, out GameCWT data) => room.world.game.GetScriptData(out data);
    public static bool GetScriptData(this AbstractWorldEntity obj, out GameCWT data) => obj.world.game.GetScriptData(out data);
    public static bool GetScriptData(this UpdatableAndDeletable obj, out GameCWT data) => obj.room.game.GetScriptData(out data);
}