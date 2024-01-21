using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using static TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies.FreedCosmeticMethods;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Unique;

public class FreeJumpRings : JumpRings, IFreedCycleColors, ISpriteOverridable
{ // Untested
    public LizColorMode[] colorMode => new LizColorMode[2];
    public List<Color> BaseColors { get; set; }
    public List<Color> FadeColors { get; set; }
    public string newSprite { get; }
    public float CycleSpeed { get; }
    public float timer { get; set; }
    public float dark { get; set; }

    public bool ImColored { get { return false; } } // Purposefully unused - Jump rings are always colored
    public bool darkenWithHead { get { return false; } } // Purposefully unused - This system would get overwritten

    public FreeJumpRings(
        LizardGraphics lGraphics, 
        int startSprite, 
        IList<LizColorMode> colorMath,
        IList<Color> baseColors = null, 
        IList<Color> innerColors = null, 
        float cycleSpeed = 0.001f,
        string SpriteOverride = "Circle20") : base(lGraphics, startSprite)
    {
        ColorModeGetter(this,colorMath);
        newSprite = SpriteOverride;
        CycleSpeed = cycleSpeed;
        BaseColors = new List<Color>();
        FadeColors = new List<Color>();
        if (baseColors != null && baseColors.Any())
            BaseColors.AddRange(baseColors);
        if (innerColors != null && innerColors.Any())
            FadeColors.AddRange(innerColors);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
                for (int k = 0; k < 2; k++)
                    sLeaser.sprites[RingSprite(i,j,k)].element = Futile.atlasManager.GetElementWithName(newSprite);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        FreedCosmeticTick(this, this, sLeaser);
        var skinCol = new List<Color>();
        skinCol.Add(sLeaser.sprites[lGraphics.SpriteBodyCirclesStart].color);
        var listToUse = (FadeColors.Any()) ? FadeColors : skinCol;
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
            {
                FreedCosmeticDrawCycle(this,0, sLeaser.sprites[RingSprite(i, j, 1)], listToUse);
                if (BaseColors.Any()) 
                    FreedCosmeticDrawCycle(this,1, sLeaser.sprites[RingSprite(i, j, 0)], BaseColors);
            }
    }
}