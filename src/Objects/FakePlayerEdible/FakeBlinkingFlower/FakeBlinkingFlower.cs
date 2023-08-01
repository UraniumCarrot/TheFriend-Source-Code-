using MoreSlugcats;
using UnityEngine;

namespace Solace.Objects.FakePlayerEdible.FakeBlinkingFlower;

public class FakeBlinkingFlower : FakePlayerEdible
{
    public FakeBlinkingFlower(AbstractPhysicalObject abstr, UpdatableAndDeletable parent, Vector2 pos) : base(abstr, parent, pos)
    {
        var flower = parent as BlinkingFlower;
        colors = new Color[3];
        BitesLeft = flower.petals.Length;
        colors[0] = flower.dropColor1;
        colors[2] = flower.dropColor2;
        colors[3] = flower.dropColor3;
    }
}