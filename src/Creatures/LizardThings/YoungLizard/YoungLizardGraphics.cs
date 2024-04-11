using System.Collections.Generic;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.CustomTemps;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Unique;
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
            num = self.AddFreeCosmetic(num, new FreeTailTuft(self, num) { FadeColors = [colorer2], SizeBonus = new Vector2(1,1)});
        else num = self.AddFreeCosmetic(num, new FreeAxolotlGills(self, num) { FadeColors = [colorer2], SizeBonus = new Vector2(1, 1) });
        num = self.AddFreeCosmetic(num, new FreeSpineSpikes(self, num) { FadeColors = [colorer2], sizeMathMode = ToolMethods.MathMode.add, SizeBonus = new Vector2(1.2f,1.2f) });
    }
}