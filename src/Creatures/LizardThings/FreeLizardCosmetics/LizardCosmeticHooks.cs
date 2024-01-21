using System;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics;

public class LizardCosmeticHooks
{
    public static void Apply()
    {
        On.LizardCosmetics.BodyScales.GeneratePatchPattern += BodyScales_GeneratePatchPattern;
        On.LizardCosmetics.BodyScales.GenerateSegments += BodyScales_GenerateSegments;
        On.LizardCosmetics.LongBodyScales.DrawSprites += LongBodyScales_DrawSprites;

        FancyHeadColors.Apply();
    }

    public static void BodyScales_GeneratePatchPattern(On.LizardCosmetics.BodyScales.orig_GeneratePatchPattern orig, BodyScales self, float startPoint, int numOfScales, float maxLength, float lengthExponent)
    {
        if (self.lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard)
        {
            self.GenerateTwoLines(0.07f, 1f, 1.5f, 3f);
            Debug.Log("Solace: Patch scales got WRECKED, SON!");
            return;
        }
        else orig(self, startPoint, numOfScales, maxLength, lengthExponent);
        if (self.lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard) Debug.Log("Solace: My hook was no match. Patch scales not destroyed...");
    }
    public static void BodyScales_GenerateSegments(On.LizardCosmetics.BodyScales.orig_GenerateSegments orig, BodyScales self, float startPoint, float maxLength, float lengthExponent)
    {
        if (self.lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard)
        {
            self.GenerateTwoLines(0.07f, 1f, 1.5f, 3f);
            Debug.Log("Solace: Segment scales got WRECKED, SON!");
            return;
        }
        else orig(self, startPoint, maxLength, lengthExponent);
        if (self.lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard) Debug.Log("Solace: My hook was no match. Segment scales not destroyed...");
    }
    public static void LongBodyScales_DrawSprites(On.LizardCosmetics.LongBodyScales.orig_DrawSprites orig, LongBodyScales self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard && self is LongShoulderScales)
        {
            var x = 1.5f;
            for (int i = self.startSprite + self.scalesPositions.Length - 1; i >= self.startSprite; i--)
            {
                sLeaser.sprites[i].scaleX *= x;
                if (self.colored) sLeaser.sprites[i + self.scalesPositions.Length].scaleX *= x;
            }
        }
    }
    
    public static void BumpHawk_ctor(On.LizardCosmetics.BumpHawk.orig_ctor orig, BumpHawk self, LizardGraphics lGraphics, int startSprite)
    {
        if (lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard)
            self.numberOfSprites = 0;
        else orig(self, lGraphics, startSprite);
    }
    public static void ShortBodyScales_ctor(On.LizardCosmetics.ShortBodyScales.orig_ctor orig, ShortBodyScales self, LizardGraphics lGraphics, int startSprite)
    {
        orig(self, lGraphics, startSprite);
        if (lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard)
        {
            Array.Resize(ref self.scalesPositions, self.scalesPositions.Length - self.numberOfSprites);
            self.numberOfSprites = 0;
        }
    }
    public static void LongHeadScales_ctor(On.LizardCosmetics.LongHeadScales.orig_ctor orig, LongHeadScales self, LizardGraphics lGraphics, int startSprite)
    {
        orig(self, lGraphics, startSprite);
        if (lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard ||
            (lGraphics.cosmetics.Exists(x => x is FreeLongHeadScales) &&
             self.GetType().Name == nameof(LongHeadScales)))
        {
            Array.Resize(ref self.scaleObjects, self.scaleObjects.Length - self.scalesPositions.Length);
            Array.Resize(ref self.scalesPositions, self.scalesPositions.Length - (self.colored ? self.numberOfSprites / 2 : self.numberOfSprites));
            self.numberOfSprites = 0;
        }
    }
}