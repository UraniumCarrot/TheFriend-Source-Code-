using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeTailFin : FreedCosmeticTemplate
{
    public FreeTailFin(TailFin template) : base(template)
    {
        ImColored = template.colored;
    }
    public FreeTailFin(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        ImColored = (owner as TailFin)!.colored;
    }
    public override Template ConstructAndAddBaseTemplate(LizardGraphics liz, int startsprite)
    {
        var newCosmetic = new TailFin(lGraphics, startSprite);
        liz.AddCosmetic(startSprite, newCosmetic);
        return newCosmetic;
    }
}