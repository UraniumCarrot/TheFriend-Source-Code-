using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeAxolotlGills : FreedCosmeticTemplate
{
    public FreeAxolotlGills(AxolotlGills template) : base(template)
    {
        ImColored = (owner as AxolotlGills)!.colored;
        HeadColorForBase = true;
    }
    public FreeAxolotlGills(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        ImColored = (owner as AxolotlGills)!.colored;
        HeadColorForBase = true;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        ImMirrored = (owner as AxolotlGills)!.scalesPositions.Length.IsEven();
        base.InitiateSprites(sLeaser, rCam);
        RectifySizeBonusForDraw("Y");
    }

    public override Template ConstructBaseTemplate(LizardGraphics liz, int startsprite)
    {
        return new AxolotlGills(lGraphics, startSprite);
    }
}