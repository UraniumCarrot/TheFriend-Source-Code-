using Fisobs.Core;
using UnityEngine;

namespace TheFriend.Objects.SolaceScarfObject;

public class SolaceScarfAbstract : AbstractPhysicalObject
{
    public SolaceScarfAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, SolaceScarfFisob.SolaceScarf, null, pos, ID)
    {
        //Main
        mr = 0;
        mg = 0;
        mb = 0;
        
        //Highlight
        hr = 0;
        hg = 0;
        hb = 0;
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
    public float mr;
    public float mg;
    public float mb;
    public float hr;
    public float hg;
    public float hb;
    public int wearerID;
    public string regionOrigin;
    public int IGlow;
    public int IBurn;
    
    public override string ToString()
    {
        return this.SaveToString($"" +
                                 $"{mr};" +
                                 $"{mg};" +
                                 $"{mb};" +
                                 $"{hr};" +
                                 $"{hg};" +
                                 $"{hb};" +
                                 $"{wearerID};" +
                                 $"{regionOrigin};" +
                                 $"{IGlow};" +
                                 $"{IBurn}");
    }
}