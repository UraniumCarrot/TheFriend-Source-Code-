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
    }
    public FreeLongHeadScales(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        HeadColorForBase = true;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        ImMirrored = true;
        base.InitiateSprites(sLeaser,rCam);
        RectifySizeBonusForDraw("Y");
    }
    public override Template ConstructAndAddBaseTemplate(LizardGraphics liz, int startsprite)
    {
        var newCosmetic = new LongHeadScales(lGraphics, startSprite);
        liz.AddCosmetic(startSprite, newCosmetic);
        return newCosmetic;
    }
}