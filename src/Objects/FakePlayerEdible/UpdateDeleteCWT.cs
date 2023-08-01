using System.Runtime.CompilerServices;
using UnityEngine;
using Solace.Objects.FakePlayerEdible;

namespace Solace.Objects.FakePlayerEdible;

public static class UpdateDeleteCWT
{
    public static void Apply()
    {
        On.UpdatableAndDeletable.Update += UpdatableAndDeletableOnUpdate;
    }

    public static void UpdatableAndDeletableOnUpdate(On.UpdatableAndDeletable.orig_Update orig, UpdatableAndDeletable self, bool eu)
    {
        orig(self, eu);
        if (self is PhysicalObject obj) self.GetPos().pos = obj.firstChunk.pos;
        else if (self is CosmeticSprite sprite) self.GetPos().pos = sprite.pos;
    }
}

public static class UpdateableDeletableCWT
{
    public class UpdatableAndDeletablePos
    {
        public Player.ObjectGrabability grab;
        public Vector2 pos;
        public bool iHaveObject;
        public UpdatableAndDeletablePos()
        {
            grab = Player.ObjectGrabability.OneHand;
        }
    }
    public static readonly ConditionalWeakTable<UpdatableAndDeletable, UpdatableAndDeletablePos> CWT = new();
    public static UpdatableAndDeletablePos GetPos(this UpdatableAndDeletable obj) => CWT.GetValue(obj, _ => new());
}