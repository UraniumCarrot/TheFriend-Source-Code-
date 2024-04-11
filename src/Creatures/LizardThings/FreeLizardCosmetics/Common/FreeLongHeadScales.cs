using System.Collections.Generic;
using System.Linq;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeLongHeadScales : FreedCosmeticTemplate
{
    public FreeLongHeadScales(LongHeadScales template) : base(template)
    {
        HeadColorForBase = true;
        ImColored = (owner as LongHeadScales)!.colored;
    }
    public FreeLongHeadScales(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        HeadColorForBase = true;
        ImColored = (owner as LongHeadScales)!.colored;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        ImMirrored = true;
        base.InitiateSprites(sLeaser,rCam);
        RectifySizeBonusForDraw("Y");
    }
    public override Template ConstructBaseTemplate(LizardGraphics liz, int startsprite)
    {
        return new LongHeadScales(lGraphics, startSprite);
    }
}