using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeLongShoulderScales : FreedCosmeticTemplate
{
    public FreeLongShoulderScales(LongShoulderScales template) : base(template)
    {
        ImColored = (owner as LongShoulderScales)!.colored;
    }
    public FreeLongShoulderScales(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        ImColored = (owner as LongShoulderScales)!.colored;
    }
    
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        ImMirrored = true;
        base.InitiateSprites(sLeaser,rCam);
        RectifySizeBonusForDraw("Y");
    }
    public override Template ConstructBaseTemplate(LizardGraphics liz, int startsprite)
    {
        return new LongShoulderScales(lGraphics, startSprite);
    }
}