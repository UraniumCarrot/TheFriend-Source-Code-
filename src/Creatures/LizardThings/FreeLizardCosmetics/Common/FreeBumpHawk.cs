using System.Collections.Generic;
using System.Linq;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using static TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies.FreedCosmeticMethods;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeBumpHawk : BumpHawk, IFreedCycleColors, ISpriteOverridable
{ // Untested
    public LizColorMode[] colorMode => new LizColorMode[3];
    public List<Color> BaseColors { get; set; }
    public List<Color> FadeColors { get; set; }
    public float dark { get; set; }
    public float timer { get; set; }
    public float CycleSpeed { get; }
    public string newSprite { get; }
    
    public bool ImColored { get { return false; } } // Purposefully unused - bumphawks do not have fade sprites
    public bool darkenWithHead { get { return false; } } // Not actually used (yet?)
    
    public FreeBumpHawk(
        LizardGraphics lGraphics, 
        int startSprite,
        IList<LizColorMode> colorMath, 
        IList<Color> baseColors = null, 
        float cycleSpeed = 0.001f, 
        string SpriteOverride = "Circle20") : base(lGraphics,
        startSprite)
    {
        ColorModeGetter(this,colorMath);
        newSprite = SpriteOverride;
        CycleSpeed = cycleSpeed;
        BaseColors = new List<Color>();
        if (baseColors != null && baseColors.Any())
            BaseColors.AddRange(baseColors);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser,rCam);
        for (int i = startSprite + numberOfSprites - 1; i >= startSprite; i--)
            sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(newSprite);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        FreedCosmeticTick(this, this, sLeaser);
        for (int i = startSprite + numberOfSprites - 1; i >= startSprite; i--)
        {
            float percentage = (float)(i - startSprite) / numberOfSprites;
            if (BaseColors.Any())
                LizColorizer(sLeaser.sprites[i], colorMode[0],BaseColors,percentage);
            else
            {
                float f = Mathf.Lerp(0.05f, spineLength / lGraphics.BodyAndTailLength, Mathf.InverseLerp(startSprite, startSprite + numberOfSprites - 1, i));
                sLeaser.sprites[i].color = lGraphics.BodyColor(f);
            }

            if (FadeColors.Any())
            {
                Color fade = (colorMode[1] == LizColorMode.RGB) ? 
                    Extensions.RGBMultiLerp(FadeColors, percentage) :
                    Extensions.HSLMultiLerp(FadeColors, percentage);
            
                List<Color> flashColor =
                    [sLeaser.sprites[i].color, fade];
                LizColorizer(sLeaser.sprites[i], colorMode[2], flashColor, dark);
            }
        }
    }
}