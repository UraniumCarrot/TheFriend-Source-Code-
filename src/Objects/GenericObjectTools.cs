using System.Linq;
using On.Rewired.Data;

namespace TheFriend.Objects;

public class GenericObjectTools
{
    public static void TrulyDestroy(PhysicalObject self)
    {
        if (self.grabbedBy.Any())
        {
            var a = self.grabbedBy.Find(x => x.grabbed == self);
            var grabber = a.grabber;
            grabber.grasps[grabber.grasps.IndexOf(a)].Release();
        }
        
        if (self.abstractPhysicalObject.stuckObjects.Any())
            foreach (AbstractPhysicalObject.AbstractObjectStick stick in self.abstractPhysicalObject.stuckObjects)
                stick.Deactivate();
        
        if (self.room?.world.game.IsStorySession == true)
            (self.room.game.session as StoryGameSession)?.RemovePersistentTracker(self.abstractPhysicalObject);
        self.RemoveFromRoom();
        self.room?.abstractRoom.RemoveEntity(self.abstractPhysicalObject);
    }

    public static void TrulyAdd(AbstractPhysicalObject self, AbstractRoom room)
    {
        room.AddEntity(self);
        if (room.realizedRoom != null)
            self.RealizeInRoom();
    }
}