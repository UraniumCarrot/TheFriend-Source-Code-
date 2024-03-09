using TheFriend.WorldChanges;
using TheFriend.WorldChanges.WorldStates.General;

namespace TheFriend.Creatures.FamineCreatures;

public abstract class FamineBatFly
{
    // Diseased Batfly
    public static void Fly_ctor(On.Fly.orig_ctor orig, Fly self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if ((QuickWorldData.FaminesExist && (world.region?.name != "UG" && world.region?.name != "SB")) || QuickWorldData.GuaranteedFamined)
            self.Destroy();
    }
}