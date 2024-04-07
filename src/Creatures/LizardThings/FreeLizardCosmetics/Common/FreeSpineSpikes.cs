using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeSpineSpikes : FreedCosmeticTemplate
{
    public FreeSpineSpikes(SpineSpikes template) : base(template)
    {
        ImColored = template.colored > 0;
    }
    public FreeSpineSpikes(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        ImColored = (owner as SpineSpikes)!.colored > 0;
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        RectifySizeBonusForDraw("Y");
    }
    public override Template ConstructAndAddBaseTemplate(LizardGraphics liz, int startsprite)
    {
        var newCosmetic = new SpineSpikes(lGraphics, startSprite);
        liz.AddCosmetic(startSprite, newCosmetic);
        return newCosmetic;
    }
}