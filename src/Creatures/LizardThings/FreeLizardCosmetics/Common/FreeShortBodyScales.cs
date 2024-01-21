using System.Collections.Generic;
using System.Linq;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using static TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies.FreedCosmeticMethods;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeShortBodyScales : ShortBodyScales, IFreedMirrorer, ISpriteOverridable, IUsePatterns
{ // Untested
    public LizColorMode[] colorMode => new LizColorMode[2];
    public bool ImColored { get; }
    public List<Color> FadeColors { get; set; }
    public List<Color> BaseColors { get; set; }
    public bool darkenWithHead { get; }
    public float dark { get; set; }
    public string newSprite { get; }
    public Vector2 sizeOverride;
    public LizScalePattern Pattern { get; }

    public FreeShortBodyScales(
        LizardGraphics lGraphics, 
        int startSprite, 
        IList<LizColorMode> colorMath, 
        IList<Color> baseColors = null, 
        IList<Color> fadeColors = null, 
        Vector2? SizeOverride = null,
        bool pulseWithHead = false,
        string SpriteOverride = "pixel",
        LizScalePattern? pattern = null) : base(lGraphics, startSprite)
    {
        ColorModeGetter(this,colorMath);
        if (pattern.HasValue) 
            Pattern = pattern.Value;
        newSprite = SpriteOverride;
        ImColored = colored;
        darkenWithHead = pulseWithHead;
        BaseColors = new List<Color>();
        FadeColors = new List<Color>();
        if (baseColors != null && baseColors.Any())
            BaseColors.AddRange(baseColors);
        if (fadeColors != null && fadeColors.Any())
            FadeColors.AddRange(fadeColors);
        if (SizeOverride.HasValue) 
            sizeOverride = SizeOverride.Value;
        else sizeOverride = new Vector2(2, 3);
    }
    
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser,rCam);
        for (int i = startSprite + scalesPositions.Length - 1; i >= startSprite; i--)
        {
            sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(newSprite);
            sLeaser.sprites[i].scaleX = sizeOverride.x;
            sLeaser.sprites[i].scaleY = sizeOverride.y;
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker,
        Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        FreedCosmeticTick(this, this, sLeaser);
        FreedCosmeticDrawMirror(this, sLeaser, scalesPositions.Length, this);
    }
    
}