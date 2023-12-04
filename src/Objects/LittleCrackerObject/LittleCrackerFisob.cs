using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;

namespace TheFriend.Objects.LittleCrackerObject;

public class LittleCrackerFisob : Fisob
{
    public static readonly AbstractPhysicalObject.AbstractObjectType LittleCracker = new("LittleCracker", true);
    public static readonly MultiplayerUnlocks.SandboxUnlockID UnlockLittleCracker = new("UnlockLittleCracker", true);

    public LittleCrackerFisob() : base(LittleCracker)
    {
        Icon = new SimpleIcon("Symbol_Seed", new(58f / 85f, 8f / 51f, 0.11764706f));
        SandboxPerformanceCost = new(linear: 0.1f, exponential: 0f);
        RegisterUnlock(UnlockLittleCracker, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat);
    }
    public static readonly LittleCrackerProperties properties = new();
    public override ItemProperties Properties(PhysicalObject forObject)
    {
        return properties;
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
    {
        return new LittleCrackerAbstract(world, entitySaveData.Pos, entitySaveData.ID);
    }
}
