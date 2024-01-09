using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.YoungLizard;

public class YoungLizardGraphics
{
    public static Color YoungLizardBodyColor(LizardGraphics self)
    { return Color.Lerp(self.lizard.effectColor, Color.white, 0.5f); }
    public static void YoungLizardGraphicsCtor(LizardGraphics self, PhysicalObject ow)
    {
        var colorer = self.effectColor;
        var colorer2 = self.effectColor;
        colorer.ChangeHue(self.effectColor.Hue() + 0.2f);
        colorer2.ChangeHue(self.effectColor.Hue() + 0.4f);
        
        var num = self.startOfExtraSprites + self.extraSprites;
        num = self.AddCosmetic(num, new FreeAntennae(self, num, colorer, colorer2, true,false) { segments = 3, length = 3 } );
        if (Random.value < 0.2f)
            num = self.AddCosmetic(num, new TailTuft(self, num));
    }
}