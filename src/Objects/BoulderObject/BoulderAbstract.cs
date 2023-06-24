using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fisobs.Core;
using UnityEngine;
using System.Threading.Tasks;

namespace TheFriend.Objects.BoulderObject;

internal class BoulderAbstract : AbstractPhysicalObject
{
    public BoulderAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, BoulderFisob.Boulder, null, pos, ID)
    {
        mass = 1;
        spikeCount = 0;
        rockCount = 2;
    }
    public override void Realize()
    {
        base.Realize();
        if (realizedObject == null) realizedObject = new Boulder(this, Room.realizedRoom.MiddleOfTile(pos.Tile), Vector2.zero);
    }

    public float mass;
    public int spikeCount;
    public int rockCount;

}
