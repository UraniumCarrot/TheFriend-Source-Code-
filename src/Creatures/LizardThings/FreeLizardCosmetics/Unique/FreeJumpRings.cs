using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Unique;

public class FreeJumpRings : FreedCosmeticTemplate
{ // Untested
    public FreeJumpRings(JumpRings template) : base(template)
    {
        ImColored = true;
        ImMirrored = true;
    }
    public FreeJumpRings(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        ImColored = true;
        ImMirrored = true;
    }
    public override Template ConstructAndAddBaseTemplate(LizardGraphics liz, int startsprite)
    {
        var newCosmetic = new JumpRings(lGraphics, startSprite);
        liz.AddCosmetic(startSprite, newCosmetic);
        return newCosmetic;
    }
}