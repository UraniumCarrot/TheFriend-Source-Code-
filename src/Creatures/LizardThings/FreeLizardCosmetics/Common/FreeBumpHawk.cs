using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeBumpHawk : FreedCosmeticTemplate
{ // Untested
    public FreeBumpHawk(BumpHawk template) : base(template)
    {
    }
    public FreeBumpHawk(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        ImColored = false;
        newSpriteFade = "";
        base.InitiateSprites(sLeaser,rCam);
        RectifySizeBonusForDraw("XY");
    }
    public override Template ConstructAndAddBaseTemplate(LizardGraphics liz, int startsprite)
    {
        var newCosmetic = new BumpHawk(lGraphics, startSprite);
        liz.AddCosmetic(startSprite, newCosmetic);
        return newCosmetic;
    }
}