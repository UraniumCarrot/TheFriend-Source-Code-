using System.Collections.Generic;
using System.Linq;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.DragonRideThings;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.CustomTemps;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

public abstract class FreedCosmeticTemplate : Template
{
    public enum LizColorMode
    {
        HSL,
        RGB
    }
    
    public Template owner;
    
    public bool ImColored; // Does this cosmetic have fade sprites? Prevents Index Out Of Bounds
    public bool ImMirrored; // Does this cosmetic have scales that come in pairs?
    public bool ImNotGradient; // Does this cosmetic with multiple scales go through colors in a gradient, or are they all the same? Requires non-zero CycleSpeed to cycle colors
    public bool ImFadeTrans; // Does this cosmetic's fadesprites get changed alpha? Automatically false if ImColored is false

    public bool HeadColorForBase;
    public bool HeadColorForFade;

    public Color headColor; // Head color of the lizard, if wanted
    public Color skinColor; // Skin color of the lizard, if wanted
    public int EndOfAllSprites => startSprite + owner.numberOfSprites; // FINAL index Ever and Always
    public int EndOfBaseSprites => startSprite - 1 + owner.numberOfSprites/(ImColored ? 2 : 1); // Last index of basesprites
    public int StartOfFadeSprites => (ImColored) ? (EndOfBaseSprites + 1) : EndOfAllSprites; // Starting index of fadesprites
    
    public List<Color> BaseColors; // Possible colors of non-fade sprites
    public List<Color> FadeColors; // Possible colors of fade sprites, unused for cosmetics that lack them
    public LizColorMode[] colorMode; // 0 for base colors, 1 for fade colors; Effects how the cosmetic's multilerp transitions colors

    public float dark; // Unused if ImReactive is false
    public float CycleSpeed; // -1 to change with lizard dark, 0 for never change, above 0 for cycling
    public ToolMethods.MathMode sizeMathMode;
    public Vector2 SizeBonus; // Grants a size bonus to a cosmetic's sprites that is applied a single time
    public Vector2 drawSizeBonus; // Grants a size bonus to a cosmetic's sprites that is applied every frame. Doesn't need to be set

    public string newSprite; // Allows a cosmetic's sprites to be overwritten
    public string newSpriteFade; // Allows a cosmetic's fadesprite to be overwritten
    
    public FreedCosmeticTemplate(Template cosmetic) : base(cosmetic.lGraphics,0)
    { // NOTE - when making a FreedCosmeticTemplate, make sure to construct the original template FIRST.
        owner = cosmetic;
        this.startSprite = cosmetic.startSprite;
        spritesOverlap = cosmetic.spritesOverlap;

        BaseColors = [];
        FadeColors = [];
        colorMode = [LizColorMode.HSL, LizColorMode.HSL];
        sizeMathMode = ToolMethods.MathMode.mult;
        SizeBonus = Vector2.one;
        newSprite = "l";
        newSpriteFade = "l";
        CycleSpeed = -1;
    }

    public FreedCosmeticTemplate(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    { // Always use AddFreeCosmetic instead of AddCosmetic when using a Freed cosmetic.
        var cosmetic = ConstructBaseTemplate(lGraphics, startSprite);
        owner = cosmetic;
        this.startSprite = cosmetic.startSprite;
        spritesOverlap = cosmetic.spritesOverlap;

        BaseColors = [];
        FadeColors = [lGraphics.effectColor];
        colorMode = [LizColorMode.HSL, LizColorMode.HSL];
        sizeMathMode = ToolMethods.MathMode.mult;
        newSprite = "l";
        newSpriteFade = "l";
        CycleSpeed = -1;
    }

    public virtual Template ConstructBaseTemplate(LizardGraphics liz, int index)
    { // Completely override this, following the code written here
        return new TipScale(liz, index);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        ModifySize(sLeaser,rCam, true);
        if (!ImColored) ImFadeTrans = false;
        if (newSprite.Length > 2)
            for (int i = startSprite; i < EndOfBaseSprites; i++)
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(newSprite);
        if (newSpriteFade.Length > 2)
            for (int i = StartOfFadeSprites; i < EndOfAllSprites; i++)
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(newSpriteFade);
        UnityEngine.Debug.Log($"Solace: Freed {owner} constructed");
    }
    
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        skinColor = sLeaser.sprites[lGraphics.SpriteBodyCirclesStart].color;
        headColor = sLeaser.sprites[lGraphics.SpriteHeadStart].color;
        
        // Controls color/fadesprite reactivity, can be either be synced with lizard's head, staticly at 0, or on a timer
        if (CycleSpeed < 0) dark = lGraphics.lizard.Liz().dark;
        else if (CycleSpeed == 0) dark = 0;
        else
        {
            dark += CycleSpeed;
            if (dark > 1) dark = 0;
        }
        ModifySize(sLeaser, rCam, false, timeStacker);
        ColorBaseSprites(sLeaser);
        if (ImColored) ColorFadeSprites(sLeaser);
        if (ImFadeTrans) AlphaControl(sLeaser);
    }

    public virtual void ModifySize(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, bool init, float timestacker = 1)
    {
        for (int i = startSprite; i < EndOfAllSprites; i++)
        {
            float copyX = sLeaser.sprites[i].scaleX;
            float copyY = sLeaser.sprites[i].scaleY;
            if (init) switch (sizeMathMode)
            {
                case ToolMethods.MathMode.add: 
                    copyX += SizeBonus.x;
                    copyY += SizeBonus.y;
                    break;
                case ToolMethods.MathMode.mult: 
                    copyX *= SizeBonus.x;
                    copyY *= SizeBonus.y;
                    break;
                case ToolMethods.MathMode.set: 
                    copyX = SizeBonus.x;
                    copyY = SizeBonus.y;
                    break;
            }
            else switch (sizeMathMode)
            {
                case ToolMethods.MathMode.add: 
                    copyX += drawSizeBonus.x;
                    copyY += drawSizeBonus.y;
                    break;
                case ToolMethods.MathMode.mult: 
                    copyX *= drawSizeBonus.x;
                    copyY *= drawSizeBonus.y;
                    break;
                case ToolMethods.MathMode.set: 
                    copyX = drawSizeBonus.x;
                    copyY = drawSizeBonus.y;
                    break;
            }
            sLeaser.sprites[i].scaleX = copyX;
            sLeaser.sprites[i].scaleY = copyY;
        }
    }

    public void RectifySizeBonusForDraw(string axis)
    {
        drawSizeBonus = SizeBonus;
        switch (sizeMathMode)
        {
            case ToolMethods.MathMode.add:
                switch (axis.ToLower())
                {
                    case "x": drawSizeBonus.x = 0; break;
                    case "y": drawSizeBonus.y = 0; break;
                    case "both" or "xy" or "yx": drawSizeBonus.x = 0; drawSizeBonus.y = 0; break;
                    default: UnityEngine.Debug.Log("Solace: RectifySizeBonusForDraw was fed an invalid string!"); break;
                }
                break;
            case ToolMethods.MathMode.mult:
                switch (axis.ToLower())
                {
                    case "x": drawSizeBonus.x = 1; break;
                    case "y": drawSizeBonus.y = 1; break;
                    case "both" or "xy" or "yx": drawSizeBonus.x = 1; drawSizeBonus.y = 1; break;
                    default: UnityEngine.Debug.Log("Solace: RectifySizeBonusForDraw was fed an invalid string!"); break;
                }
                break;
        }
    }

    public virtual void AlphaControl(RoomCamera.SpriteLeaser sLeaser)
    { // Only use if ImFadeTrans is true
        for (int i = StartOfFadeSprites; i < EndOfAllSprites; i++)
            sLeaser.sprites[i].alpha = Mathf.Lerp(1,0,dark);
    }

    public void ColorBaseSprites(RoomCamera.SpriteLeaser sLeaser)
    {
        float percent = 0;
        for (int i = startSprite; i < EndOfBaseSprites; i++)
        {
            if (ImMirrored)
            {
                if ((EndOfBaseSprites - i).IsEven())
                    percent = (float)(i - startSprite) / (EndOfBaseSprites - startSprite);
            }
            else percent = (float)(i - startSprite) / (EndOfBaseSprites - startSprite);
            percent += dark;
            if (percent > 1) percent--;

            if (BaseColors.Any()) ColorSprite(sLeaser.sprites[i], colorMode[0], BaseColors, percent);
            else if (HeadColorForBase) sLeaser.sprites[i].color = headColor;
            else sLeaser.sprites[i].color = skinColor;
        }
    }

    public void ColorFadeSprites(RoomCamera.SpriteLeaser sLeaser)
    { // Only use if ImColored is true
        float percent = 0;
        for (int i = StartOfFadeSprites; i < EndOfAllSprites; i++)
        {
            if (!ImNotGradient)
                if (ImMirrored)
                {
                    if ((i - EndOfAllSprites).IsEven())
                    {
                        percent = (i - StartOfFadeSprites) / ((float)owner.numberOfSprites/2);
                        percent += dark;
                    }
                }
                else
                {
                    percent = (i - StartOfFadeSprites) / ((float)owner.numberOfSprites/2);
                    percent += dark;
                }
            else percent = (ImFadeTrans) ? 0 : dark;
            
            if (FadeColors.Any()) ColorSprite(sLeaser.sprites[i], colorMode[1], FadeColors, percent);
            else if (HeadColorForFade) sLeaser.sprites[i].color = headColor;
            else sLeaser.sprites[i].color = skinColor;
        }
    }
    
    public void ColorSprite(FSprite sprite, LizColorMode mode, IList<Color> colors, float percent)
    { // Sets the color of a sprite. Feed it the list of possible colors it can have,
        // pick a percent to determine where it'll be on the list,
        // and pick a mode to specify how its color will be calculated.
        // HSL is more vibrant but also less predictable - RGB is duller but more consistent
        
        sprite.color = mode == LizColorMode.RGB ? 
            Extensions.RGBMultiLerp(colors,percent) : 
            Extensions.HSLMultiLerp(colors,percent);
    }
}