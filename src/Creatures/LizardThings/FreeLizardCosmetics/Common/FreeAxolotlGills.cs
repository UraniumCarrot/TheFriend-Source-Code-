using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeAxolotlGills : FreedCosmeticTemplate
{
    public FreeAxolotlGills(AxolotlGills template) : base(template)
    {
        ImColored = (owner as AxolotlGills)!.colored;
        ImMirrored = true;
        HeadColorForBase = true;
    }
    public FreeAxolotlGills(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        ImColored = (owner as AxolotlGills)!.colored;
        ImMirrored = true;
        HeadColorForBase = true;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        RectifySizeBonusForDraw("Y");
    }

    public override Template ConstructAndAddBaseTemplate(LizardGraphics liz, int startsprite)
    {
        var newCosmetic = new AxolotlGills(lGraphics, startSprite);
        liz.AddCosmetic(startSprite, newCosmetic);
        return newCosmetic;
    }
}