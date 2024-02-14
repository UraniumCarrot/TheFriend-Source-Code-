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
        if (room.GetSessionData(out var a) && a.RoomsWithNewScripts.Contains(room.abstractRoom.name))
        {
            room.roomSettings.roomSpecificScript = true;
            Debug.Log($"Solace: Made roomscript true! For: {room.abstractRoom.name}");
        }
    }
}