using System.Collections.Generic;
using System.Linq;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.DragonRideThings;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

public class FreedCosmeticMethods
{
    public static void SizeBonusInit(Template self, Vector2 bonus, RoomCamera.SpriteLeaser sleaser)
    { // Necessary for lizard cosmetics with any scale value that doesn't change
        float actualx = (bonus.x + 1).Abs();
        float actualy = (bonus.y + 1).Abs();
        for (int i = self.startSprite; i < self.startSprite + self.numberOfSprites; i++)
        {
            Debug.Log(i);
            sleaser.sprites[i].scaleX *= actualx;
            sleaser.sprites[i].scaleY *= actualy;
        }
    }

    public static void SizeBonusDraw(Template self, Vector2 bonus, RoomCamera.SpriteLeaser sleaser,
        bool updateX = false, bool updateY = false)
    { // Necessary for lizard cosmetics with a scale value that constantly updates
        if (!updateX && !updateY) return;
        float actualx = (bonus.x + 1).Abs();
        float actualy = (bonus.y + 1).Abs();
        for (int i = self.startSprite; i < self.startSprite + self.numberOfSprites; i++)
        {
            if (updateX) sleaser.sprites[i].scaleX *= actualx;
            if (updateY) sleaser.sprites[i].scaleY *= actualy;
        }
    }
    
    public static void FreedCosmeticTick(Template self, IFreedCosmetic cos, RoomCamera.SpriteLeaser sLeaser)
    { // For every kind of freed cosmetic, this helps very much with percentages
        if (!self.TryGetLiz(out var data)) return;
        if (cos.darkenWithHead ||
            (self is IFreedCycleColors c && c.FadeColors.Count > 1))
            cos.dark = Mathf.Lerp(1,0,data.dark);
        else cos.dark = 1;
        if (self is IFreedCycleColors cyc)
            cyc.timer = (cyc.timer > 1) ? 0 : cyc.timer + cyc.CycleSpeed;
    }

    public static void FreedCosmeticDrawGeneric(Template self, IFreedDownLength cos, RoomCamera.SpriteLeaser sLeaser, int length)
    { // Used by freed cosmetics which don't get mirrored and don't cycle through colors
        if (cos.BaseColors.Any() || cos.FadeColors.Any())
            for (int i = self.startSprite; i < self.startSprite + length; i++)
            {
                float percentage = (float)(i - self.startSprite) / length;
                if (cos.BaseColors.Any())
                    LizColorizer(sLeaser.sprites[i], cos.colorMode[0], cos.BaseColors, percentage);
                if (cos.ImColored)
                {
                    LizColorizer(sLeaser.sprites[i + length], cos.colorMode[1], cos.FadeColors, percentage);
                    sLeaser.sprites[i + length].color = Color.Lerp(
                        sLeaser.sprites[i + length].color,
                        sLeaser.sprites[i].color, 
                        cos.dark);
                }
            }
    }

    public static void FreedCosmeticDrawCycle(IFreedCycleColors cos, int modeIndex, FSprite sprite, List<Color> list, bool affectAlpha = false)
    { // Used by freed cosmetics that cycle through colors. Reactive makes color cycling match with lizard reactions
        bool reactive = cos.CycleSpeed == 0;
        LizColorizer(sprite,cos.colorMode[modeIndex], list, (reactive) ? cos.dark : cos.timer);
        if (affectAlpha) sprite.alpha = cos.dark;
    }
    
    public static void FreedCosmeticDrawMirror(Template self, RoomCamera.SpriteLeaser sLeaser, int length, IFreedMirrorer mir)
    { // Used by freed cosmetics that mirror to the other side of the lizard. May mess up sometimes...
        if (length > 1)
        {
            float percentage = 0;
            for (int i = self.startSprite + length - 1; i >= self.startSprite; i--)
            {
                if ((i - self.startSprite).IsEven())
                    percentage = (float)(i - self.startSprite) / length;
                
                if (mir.BaseColors.Any())
                    LizColorizer(sLeaser.sprites[i + length], mir.colorMode[0], mir.BaseColors, percentage);
                if (mir.ImColored)
                {
                    if (mir.FadeColors.Any())
                        LizColorizer(sLeaser.sprites[i + length], mir.colorMode[1], mir.FadeColors, percentage);
                    else sLeaser.sprites[i + length].color = self.lGraphics.effectColor;
                    sLeaser.sprites[i + length].color = Color.Lerp(
                        sLeaser.sprites[i + length].color,
                        sLeaser.sprites[i].color, 
                        mir.dark);
                }
            }
        }
        else Debug.Log($"Solace: Cosmetic {nameof(self.GetType)} tried to apply IFreedMirrorer interface, but wasn't supported");
    }

    public static void LizColorizer(FSprite sprite, LizColorMode mode, IList<Color> colors, float percent)
    {
        sprite.color = mode == LizColorMode.RGB ? 
            Extensions.RGBMultiLerp(colors,percent) : 
            Extensions.HSLMultiLerp(colors,percent);
    }

    public static void ColorModeGetter(IFreedCosmetic cos, IList<LizColorMode> colorMath)
    {
        for (int i = 0; i < cos.colorMode.Length; i++)
            if (colorMath[i]==LizColorMode.RGB) cos.colorMode[i] = colorMath[i];
            else cos.colorMode[i] = LizColorMode.HSL;
    }
}