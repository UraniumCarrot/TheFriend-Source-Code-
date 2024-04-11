using System.Collections.Generic;
using TheFriend.Creatures.LizardThings.DragonRideThings;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using TheFriend.Creatures.LizardThings.YoungLizard;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics;

public class FancyColors
{
    public delegate Color orig_HeadColor1(LizardGraphics self);
    public static Color FancyHeadColor1(orig_HeadColor1 orig, LizardGraphics self)
    {
        if (self.lizard.Liz().MyHeadDoesntFlash) return self.effectColor;
        if (self.lizard.Template.type == CreatureTemplateType.YoungLizard)
            return YoungLizardGraphics.YoungLizardFlashColor(self);
        
        return orig(self);
    }

    public static void RecolorLimbs(LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, List<Color> newColors, float overrideAlpha = -1, FreedCosmeticTemplate.LizColorMode mode = FreedCosmeticTemplate.LizColorMode.HSL)
    {
        Color col = Color.magenta;
        float dark = self.lizard.Liz().dark;
        if (newColors.Count == 0) col = newColors[0];
        else switch (mode)
        {
            case FreedCosmeticTemplate.LizColorMode.HSL:
                col = Extensions.HSLMultiLerp(newColors, dark); break;
            case FreedCosmeticTemplate.LizColorMode.RGB: 
                col = Extensions.RGBMultiLerp(newColors, dark); break;
        }
        for (int i = self.SpriteLimbsColorStart; i < self.SpriteLimbsColorEnd; i++)
        {
            sLeaser.sprites[i].color = col;
            if (overrideAlpha < -1) sLeaser.sprites[i].alpha = 0;
            else if (overrideAlpha < 0) sLeaser.sprites[i].alpha = dark;
            else sLeaser.sprites[i].alpha = overrideAlpha;
        }
    }
}