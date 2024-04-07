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
        var bodyList = new List<Color>();
        bodyList.Add(YoungLizardBodyColor(self));
        var colorer2 = self.effectColor;
        colorer2.ChangeHue(self.effectColor.Hue() + 0.4f);
        List<Color> colorlist2 = [self.effectColor, colorer2];

        var num = self.startOfExtraSprites + self.extraSprites;
        var gills = new AxolotlGills(self, num);
        num = self.AddCosmetic(num, new FreeAxolotlGills(gills) { HeadColorForBase = true, FadeColors = colorlist2, SizeBonus = new Vector2(1,1)});
        //num = self.AddCosmetic(num, new BlandAntennae(self, num, 3) { segments = 3 });
    }
}