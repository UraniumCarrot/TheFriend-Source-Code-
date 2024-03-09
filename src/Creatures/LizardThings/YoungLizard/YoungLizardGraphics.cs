using System.Collections.Generic;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;
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
        var bodyList = new List<Color>();
        bodyList.Add(YoungLizardBodyColor(self));
        var colorer = self.effectColor;
        var colorer2 = self.effectColor;
        colorer.ChangeHue(self.effectColor.Hue() + 0.2f);
        colorer2.ChangeHue(self.effectColor.Hue() + 0.4f);
        var pulseWithHead = Random.value > 0.5f;
        List<Color> colorlist = [colorer2];
        List<Color> colorlist2 = [self.effectColor, colorer, colorer2];
        List<LizColorMode> mode1 = [LizColorMode.HSL, LizColorMode.RGB];

        var num = self.startOfExtraSprites + self.extraSprites;
        /*num = self.AddCosmetic(num, new FreeAntennae(self, num, 3, colorer, colorer2, true, false));
        if (Random.value < 0.2f)
            num = self.AddCosmetic(num, new FreeTailTuft(self, num, mode1,null, colorlist, new Vector2(0.1f,0.1f), pulseWithHead));
        else num = self.AddCosmetic(num, new FreeAxolotlGills(self, num, mode1,null, colorlist2) {colored = true});
        num = self.AddCosmetic(num, new FreeSpineSpikes(self, num, mode1,null, colorlist, new Vector2(0.1f,0.1f), true));*/
        if (Random.value < 0.2f)
            num = self.AddCosmetic(num, new TailTuft(self, num));
    }
}