using System.Linq;
using Fisobs.Core;
using UnityEngine;

namespace TheFriend.Objects.SolaceScarfObject;

public class SolaceScarfAbstract : AbstractPhysicalObject
{
    public SolaceScarfAbstract(World world, WorldCoordinate pos, EntityID ID, string regionOrigin) : base(world, SolaceScarfFisob.SolaceScarf, null, pos, ID)
    {
        this.regionOrigin = regionOrigin;
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
    public int IGlow; // Gives scarf a lightsource
    public int IBurn; // Will make scarf explosive (UNIMPLEMENTED)
    public bool IVoid; // Will give scarf a strange shader (UNIMPLEMENTED)
    
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