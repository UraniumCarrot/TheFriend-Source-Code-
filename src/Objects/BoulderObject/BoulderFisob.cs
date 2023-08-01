using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Experimental.GlobalIllumination;

namespace TheFriend.Objects.BoulderObject;

internal class BoulderFisob : Fisob
{
    public static readonly AbstractPhysicalObject.AbstractObjectType Boulder = new("Boulder", true);
    //public static readonly MultiplayerUnlocks.SandboxUnlockID UnlockBoulder = new("UnlockBoulder", true);

    public BoulderFisob() : base(Boulder)
    {
        Icon = new BoulderIcon();
        SandboxPerformanceCost = new(linear: 0.3f, exponential: 0f);
        //RegisterUnlock(UnlockBoulder, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat);
    }
    private static readonly BoulderProperties properties = new();
    public override ItemProperties Properties(PhysicalObject forObject)
    {
        return properties;
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
    {
        return new BoulderAbstract(world, entitySaveData.Pos, entitySaveData.ID);
    }
}
