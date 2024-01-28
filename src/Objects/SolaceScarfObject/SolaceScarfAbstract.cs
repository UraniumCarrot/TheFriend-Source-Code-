using System.Linq;
using Fisobs.Core;
using UnityEngine;

namespace TheFriend.Objects.SolaceScarfObject;

public class SolaceScarfAbstract : AbstractPhysicalObject
{
    public SolaceScarfAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, SolaceScarfFisob.SolaceScarf, null, pos, ID)
    {
        wearerID = -10;
        baseCol = Color.clear;
        highCol = Color.clear;
    }
    public override void Realize()
    {
        base.Realize();
        if (realizedObject == null) 
            realizedObject = new SolaceScarf(
                this, 
                Room.realizedRoom.MiddleOfTile(pos.Tile), 
                Vector2.zero);
    }

    public Color baseCol;
    public Color highCol;
    public int wearerID;
    public string regionOrigin;
    public int IGlow;
    public int IBurn;
    public bool IVoid;
    
    public override string ToString()
    {
        return this.SaveToString($"" +
                                 $"{RWCustom.Custom.colorToHex(baseCol)};" +
                                 $"{RWCustom.Custom.colorToHex(highCol)};" +
                                 $"{wearerID};" +
                                 $"{regionOrigin};" +
                                 $"{IGlow};" +
                                 $"{IBurn};" +
                                 $"{IVoid}");
    }
}