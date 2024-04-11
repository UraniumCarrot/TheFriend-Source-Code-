using System.Collections.Generic;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.CustomTemps;

public class TipScale : Template
{
    public int tailSegmentOffset;
    public bool horizontalAligned;

    public TipScale(LizardGraphics lGraphics, int startsprite) : base(lGraphics, startsprite)
    {
        spritesOverlap = SpritesOverlap.InFront;
        numberOfSprites += 1;
    }
    
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites[startSprite] = new FSprite("SnailTail");
        sLeaser.sprites[startSprite].anchorY *= 0.2f;
        sLeaser.sprites[startSprite].scaleY = -1;
        sLeaser.sprites[startSprite].color = lGraphics.effectColor;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker,
        Vector2 camPos)
    {
        var tail1 = lGraphics.tail[lGraphics.tail.Length - 1 - tailSegmentOffset];
        var tail2 = lGraphics.tail[lGraphics.tail.Length - 2 - tailSegmentOffset];
        sLeaser.sprites[startSprite].SetPosition(Vector2.Lerp(tail1.lastPos,tail1.pos,timeStacker) - camPos);
        sLeaser.sprites[startSprite].rotation = RWCustom.Custom.AimFromOneVectorToAnother(tail1.pos, tail2.pos);
        if (horizontalAligned) 
            sLeaser.sprites[startSprite].scaleX = Mathf.Lerp(sLeaser.sprites[startSprite].scaleX, Mathf.Lerp(1f, 0.1f, lGraphics.depthRotation.Abs()), timeStacker);
        else sLeaser.sprites[startSprite].scaleX = Mathf.Lerp(sLeaser.sprites[startSprite].scaleX, Mathf.Lerp(0.1f, 1, lGraphics.depthRotation.Abs()), timeStacker);
    }
}

public class FreedTipScale : FreedCosmeticTemplate
{
    public FreedTipScale(TipScale template) : base(template)
    {
    }
    public override Template ConstructBaseTemplate(LizardGraphics liz, int startsprite)
    {
        return new TipScale(lGraphics, startSprite);
    }
}