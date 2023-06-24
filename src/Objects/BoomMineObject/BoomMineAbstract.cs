using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fisobs.Core;
using UnityEngine;
using System.Threading.Tasks;

namespace TheFriend.Objects.BoomMineObject;

public class BoomMineAbstract : AbstractPhysicalObject
{
    public BoomMineAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, BoomMineFisob.BoomMine, null, pos, ID)
    {
        slot1 = 0;
        slot2 = 0;
        slot3 = 0;
    }
    public override void Realize()
    {
        base.Realize();
        if (realizedObject == null) realizedObject = new BoomMine(this, Room.realizedRoom.MiddleOfTile(pos.Tile), Vector2.zero);
    }
    public int slot1;
    public int slot2;
    public int slot3;
    public override string ToString()
    {
        return this.SaveToString($"{slot1};{slot2};{slot3}");
    }
}
