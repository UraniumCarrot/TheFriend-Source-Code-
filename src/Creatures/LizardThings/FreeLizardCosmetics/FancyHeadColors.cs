using System.Reflection;
using MonoMod.RuntimeDetour;
using TheFriend.Creatures.LizardThings.DragonRideThings;
using TheFriend.Creatures.LizardThings.YoungLizard;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics;

public class FancyHeadColors
{
    public delegate Color orig_HeadColor1(LizardGraphics self);
    public static Color FancyHeadColor1(orig_HeadColor1 orig, LizardGraphics self)
    {
        if (self.lizard.Liz().MyHeadDoesntFlash) return self.effectColor;
        //if (self.lizard.Template.type == CreatureTemplateType.YoungLizard)
            //return YoungLizardGraphics.YoungLizardFlashColor(self);
        
        return orig(self);
    }
}