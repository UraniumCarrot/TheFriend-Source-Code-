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

namespace TheFriend.Objects.BoomMineObject;

public class BoomMineFisob : Fisob
{
    public static readonly AbstractPhysicalObject.AbstractObjectType BoomMine = new("BoomMine", true);
    //public static readonly MultiplayerUnlocks.SandboxUnlockID UnlockBoomMine = new("UnlockBoomMine", true);

    public BoomMineFisob() : base(BoomMine)
    {
        Icon = new BoomMineIcon();
        SandboxPerformanceCost = new(linear: 0.1f, exponential: 0f);
       // RegisterUnlock(UnlockBoomMine, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat);
    }
    private static readonly BoomMineProperties properties = new();
    public override ItemProperties Properties(PhysicalObject forObject)
    {
        return properties;
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
    {
        string[] p = entitySaveData.CustomData.Split(';');
        if (p.Length < 3) { p = new string[3]; }
        var result = new BoomMineAbstract(world, entitySaveData.Pos, entitySaveData.ID)
        {
            slot1 = int.TryParse(p[0], out var s1) ? s1 : 0,
            slot2 = int.TryParse(p[1], out var s2) ? s2 : 0,
            slot3 = int.TryParse(p[2], out var s3) ? s3 : 0,
        };

        return result;
    }
}
