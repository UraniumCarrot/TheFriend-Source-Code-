using UnityEngine;

namespace TheFriend.Objects.FakePlayerEdible.FakeGenericEdible;

public class FakeGenericEdible : FakePlayerEdible
{
    public FakeGenericEdible(AbstractPhysicalObject abstr, UpdatableAndDeletable parent, Vector2 pos) : base(abstr, parent, pos)
    {
    }
}