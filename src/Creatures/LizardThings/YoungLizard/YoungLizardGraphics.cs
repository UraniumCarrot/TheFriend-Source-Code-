using System.Collections.Generic;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.CustomTemps;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.YoungLizard;

public class YoungLizardGraphics
{
    public static Color YoungLizardBodyColor(LizardGraphics self)
    { return Color.Lerp(self.lizard.effectColor, Color.white, 0.5f); }
    public static Color YoungLizardFlashColor(LizardGraphics self)
    {
        var col = self.effectColor;
        col.ChangeHue(col.Hue()+0.4f);
        return col;
    }
    public static void YoungLizardGraphicsCtor(LizardGraphics self, PhysicalObject ow)
    {
        var colorer2 = self.effectColor;
        colorer2.ChangeHue(self.effectColor.Hue() + 0.4f);

        var num = self.startOfExtraSprites + self.extraSprites;
        num = self.AddCosmetic(num, new FlavoredAntennae(self, num, 2) { AntennaeColors = [Color.black, Color.black, colorer2, colorer2] });
        if (Random.value < 0.2f)
            num = self.AddFreeCosmetic(num, new FreeTailTuft(self, num) { FadeColors = [colorer2, self.effectColor], SizeBonus = new Vector2(1,1)});
        else num = self.AddFreeCosmetic(num, new FreeAxolotlGills(self, num) { colorMode = [FreedCosmeticTemplate.LizColorMode.RGB, FreedCosmeticTemplate.LizColorMode.RGB], ImNotGradient = true, FadeColors = [self.effectColor, colorer2], SizeBonus = new Vector2(1, 1) });
        num = self.AddFreeCosmetic(num, new FreeSpineSpikes(self, num) { colorMode = [FreedCosmeticTemplate.LizColorMode.RGB, FreedCosmeticTemplate.LizColorMode.RGB], ImNotGradient = true, FadeColors = [self.effectColor, colorer2], sizeMathMode = ToolMethods.MathMode.add, SizeBonus = new Vector2(1.2f,1.2f) });
    }

    public static void YoungLizardDrawSprites(LizardGraphics self, RoomCamera.SpriteLeaser sLeaser)
    {
        var color2 = self.effectColor;
        color2.ChangeHue(self.effectColor.Hue() + 0.4f);
        List<Color> colors = [self.effectColor, color2];
        FancyColors.RecolorLimbs(self, sLeaser, colors, 1);
    }
}