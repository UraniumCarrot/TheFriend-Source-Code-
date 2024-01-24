using System.ComponentModel;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;

namespace TheFriend.Objects.SolaceScarfObject;

public class SolaceScarfFisob : Fisob
{
    public static readonly AbstractPhysicalObject.AbstractObjectType SolaceScarf = new("SolaceScarf", true);
    public SolaceScarfFisob() : base(SolaceScarf)
    {
    }

    public static readonly SolaceScarfProperties properties = new();
    public override ItemProperties Properties(PhysicalObject forObject) => properties;

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
    {
        string[] p = entitySaveData.CustomData.Split(';');
        if (p.Length < 4) { p = new string[10]; }
        var result = new SolaceScarfAbstract(world, entitySaveData.Pos, entitySaveData.ID)
        {
            mr = float.TryParse(p[0], out var s1) ? s1 : 0,
            mg = float.TryParse(p[1], out var s2) ? s2 : 0,
            mb = float.TryParse(p[2], out var s3) ? s3 : 0,
            hr = float.TryParse(p[3], out var s4) ? s4 : 0,
            hg = float.TryParse(p[4], out var s5) ? s5 : 0,
            hb = float.TryParse(p[5], out var s6) ? s6 : 0,
            wearerID = int.TryParse(p[6], out var s7) ? s7 : -10,
            regionOrigin = (p[7].Length > 1) ? p[7] : "NotARegion",
            IGlow = int.TryParse(p[8], out var s9) ? s9 : 0,
            IBurn = int.TryParse(p[9], out var s10) ? s10 : 0
        };
        return result;
    }
}