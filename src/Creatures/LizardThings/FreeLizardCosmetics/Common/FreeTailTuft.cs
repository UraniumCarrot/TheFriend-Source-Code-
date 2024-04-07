using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeTailTuft : FreedCosmeticTemplate
{
    public FreeTailTuft(TailTuft template) : base(template)
    {
    }
    public FreeTailTuft(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        ImMirrored = (owner as ShortBodyScales)!.scalesPositions.Length.IsEven();
        base.InitiateSprites(sLeaser, rCam);
        RectifySizeBonusForDraw("Y");
    }
    public override Template ConstructAndAddBaseTemplate(LizardGraphics liz, int startsprite)
    {
        var newCosmetic = new TailTuft(lGraphics, startSprite);
        liz.AddCosmetic(startSprite, newCosmetic);
        return newCosmetic;
    }
}