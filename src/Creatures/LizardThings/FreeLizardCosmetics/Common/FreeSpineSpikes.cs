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
    public override Template ConstructBaseTemplate(LizardGraphics liz, int startsprite)
    {
        return new SpineSpikes(lGraphics, startSprite);
    }
}