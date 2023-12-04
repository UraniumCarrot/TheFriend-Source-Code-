using UnityEngine;

namespace TheFriend.Objects.LittleCrackerObject;

public class LittleCrackerAbstract : AbstractPhysicalObject
{
    public LittleCrackerAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, LittleCrackerFisob.LittleCracker, null, pos, ID)
    {
    }
    public override void Realize()
    {
        base.Realize();
        if (realizedObject == null) realizedObject = new LittleCracker(this, Room.realizedRoom.MiddleOfTile(pos.Tile), Vector2.zero);
    }
}
