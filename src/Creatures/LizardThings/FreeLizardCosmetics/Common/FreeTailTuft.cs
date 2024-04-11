using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeTailTuft : FreedCosmeticTemplate
{
    public FreeTailTuft(TailTuft template) : base(template)
    {
        ImColored = (owner as TailTuft)!.colored;
    }
    public FreeTailTuft(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        ImColored = (owner as TailTuft)!.colored;
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        ImMirrored = (owner as TailTuft)!.scalesPositions.Length.IsEven();
        base.InitiateSprites(sLeaser, rCam);
        RectifySizeBonusForDraw("Y");
    }
    public override Template ConstructBaseTemplate(LizardGraphics liz, int startsprite)
    {
        return new TailTuft(lGraphics, startSprite);
    }
}