using System;
using LizardCosmetics;
using TheFriend.DragonRideThings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.Creatures.LizardThings.MotherLizard;

public class MotherLizardGraphics
{
    public static void MotherLizardSpritesInit(LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
        if (self.lizard.GetLiz() != null) self.lizard.GetLiz().hybridHead = sLeaser.sprites.Length - 1;
        if (self.lizard.GetLiz() != null) sLeaser.sprites[self.lizard.GetLiz().hybridHead] = new FSprite("LizardHead3.0");
        self.AddToContainer(sLeaser, rCam, null);
    }

    public static void MotherLizardSpritesAddToContainer(LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, FContainer newContainer)
    {
        if (self.lizard.GetLiz() != null && self.lizard.GetLiz().hybridHead < sLeaser.sprites.Length)
        {
            if (newContainer == null) newContainer = sLeaser.sprites[self.SpriteHeadStart].container;
            newContainer.AddChild(sLeaser.sprites[self.lizard.GetLiz().hybridHead]);
        }
    }

    public static void MotherLizardGraphicsCtor(LizardGraphics self, PhysicalObject ow)
    {
        bool rCol = Random.value > 0.5f;
        float r1 = Random.value;
        var num = self.startOfExtraSprites + self.extraSprites;

        var shoulder = new LongShoulderScales(self, num);
        shoulder.graphic = r1 > 0.5f ? 6 : 2;
        shoulder.graphicHeight /= r1 > 0.5f ? 3.5f : 2f;
        shoulder.numberOfSprites = rCol ? shoulder.scalesPositions.Length * 2 : shoulder.scalesPositions.Length;
        shoulder.colored = rCol;
        shoulder.rigor = 10f;
        num = self.AddCosmetic(num, shoulder);

        var scale = new SpineSpikes(self, num);
        scale.graphic = r1 > 0.5f ? 6 : 2;
        scale.sizeRangeMin = r1 > 0.5f ? 0.6f : 0.8f;
        scale.sizeRangeMax = r1 > 0.5f ? 2.6f : 3f;
        scale.spineLength = Mathf.Lerp(0.7f, 0.95f, Random.value) * self.BodyAndTailLength;
        scale.sizeSkewExponent = Random.value;
        if (scale.bumps > 15) scale.bumps = 15;
        scale.colored = rCol ? 1 : 0;
        scale.numberOfSprites = scale.colored > 0 ? scale.bumps * 2 : scale.bumps;
        num = self.AddCosmetic(num, scale);
    }
    
    public static void MotherLizardDrawSprites(LizardGraphics self, RoomCamera.SpriteLeaser sLeaser)
    {
        sLeaser.sprites[self.SpriteHeadStart + 4].color = self.effectColor; // Eyes
            sLeaser.sprites[self.SpriteHeadStart + 1].color = self.effectColor; // Bottom teeth
            sLeaser.sprites[self.SpriteHeadStart + 2].color = self.effectColor; // Top teeth
            sLeaser.sprites[self.SpriteHeadStart].color = self.palette.blackColor; // Bottom jaw
            sLeaser.sprites[self.SpriteHeadStart + 3].color = self.palette.blackColor; // Top jaw

            // Custom head
            if (self.lizard.GetLiz() != null)
            {
                sLeaser.sprites[self.lizard.GetLiz().hybridHead].color = sLeaser.sprites[self.SpriteHeadStart + 3].color;
                sLeaser.sprites[self.lizard.GetLiz().hybridHead].scaleX = sLeaser.sprites[self.SpriteHeadStart + 3].scaleX;
                sLeaser.sprites[self.lizard.GetLiz().hybridHead].scaleY = sLeaser.sprites[self.SpriteHeadStart + 3].scaleY * 0.6f;
                sLeaser.sprites[self.lizard.GetLiz().hybridHead].rotation = sLeaser.sprites[self.SpriteHeadStart + 3].rotation;
                sLeaser.sprites[self.lizard.GetLiz().hybridHead].SetPosition(sLeaser.sprites[self.SpriteHeadStart + 3].GetPosition());
                sLeaser.sprites[self.lizard.GetLiz().hybridHead].MoveBehindOtherNode(sLeaser.sprites[self.SpriteHeadStart]);

                switch (sLeaser.sprites[self.SpriteHeadStart + 3].element.name)
                {
                    case "LizardHead0.5":
                        sLeaser.sprites[self.lizard.GetLiz().hybridHead].element = Futile.atlasManager.GetElementWithName("LizardHead0.0");
                        break;
                    case "LizardHead1.5":
                        sLeaser.sprites[self.lizard.GetLiz().hybridHead].element = Futile.atlasManager.GetElementWithName("LizardHead1.0");
                        break;
                    case "LizardHead2.5":
                        sLeaser.sprites[self.lizard.GetLiz().hybridHead].element = Futile.atlasManager.GetElementWithName("LizardHead2.0");
                        break;
                    case "LizardHead3.5":
                        sLeaser.sprites[self.lizard.GetLiz().hybridHead].element = Futile.atlasManager.GetElementWithName("LizardHead3.0");
                        break;
                }
            }
    }
}