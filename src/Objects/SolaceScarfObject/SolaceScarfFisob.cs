using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using UnityEngine;

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
        if (p.Length < 7) { p = new string[7]; }
        var result = new SolaceScarfAbstract(world, entitySaveData.Pos, entitySaveData.ID)
        {
            baseCol = (p[0].Length > 1) ? RWCustom.Custom.hexToColor(p[0]) : Color.black,
            highCol = (p[1].Length > 1) ? RWCustom.Custom.hexToColor(p[1]) : Color.black,
            wearerID = int.TryParse(p[2], out var s3) ? s3 : -10,
            regionOrigin = (p[3].Length > 1) ? p[3] : "NotARegion",
            IGlow = int.TryParse(p[4], out var s5) ? s5 : 0,
            IBurn = int.TryParse(p[5], out var s6) ? s6 : 0,
            IVoid = bool.TryParse(p[6], out var s7) ? s7 : false
        };
        return result;
    }
}