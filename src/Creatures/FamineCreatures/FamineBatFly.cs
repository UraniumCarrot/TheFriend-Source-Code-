using TheFriend.WorldChanges;

namespace TheFriend.Creatures.FamineCreatures;

public abstract class FamineBatFly
{
    // Diseased Batfly
    public static void Fly_ctor(On.Fly.orig_ctor orig, Fly self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (FamineWorld.FamineBool && (world.region?.name != "UG" && world.region?.name != "SB"))
            self.Destroy();
        if (FamineWorld.FamineBurdenBool)
            self.Destroy();
    }
}