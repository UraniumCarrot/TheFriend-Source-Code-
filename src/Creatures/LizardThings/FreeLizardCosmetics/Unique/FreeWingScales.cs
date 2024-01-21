using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using static TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies.FreedCosmeticMethods;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Unique;

public class FreeWingScales : WingScales, IFreedCosmetic
{ // Untested
    public LizColorMode[] colorMode => new LizColorMode[1];
    public List<Color> BaseColors { get; set; }
    public bool darkenWithHead { get; }
    public float dark { get; set; }
    public Vector2? sizeBonus;
    public FreeWingScales(
        LizardGraphics lGraphics, 
        int startSprite, 
        IList<LizColorMode> colorMath, 
        IList<Color> baseColors = null, 
        Vector2? SizeBonus = default, 
        bool pulseWithHead = false) : base(lGraphics,startSprite)
    {
        ColorModeGetter(this,colorMath);
        darkenWithHead = pulseWithHead;
        BaseColors = new List<Color>();
        if (SizeBonus.HasValue)
            sizeBonus = new Vector2(Mathf.Abs(SizeBonus.Value.x + 1), Mathf.Abs(SizeBonus.Value.y + 1));
        if (baseColors != null && baseColors.Any())
            BaseColors.AddRange(baseColors);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser,rCam);
        if (sizeBonus != null) SizeBonusInit(this, sizeBonus.Value, sLeaser);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker,
        Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (sizeBonus != null) SizeBonusDraw(this, sizeBonus.Value, sLeaser, false, true);
        
        float percentage = 0;
        for (int i = startSprite + numberOfSprites - 1; i >= this.startSprite; i--)
        {
            if ((i - startSprite).IsEven())
                percentage = (float)(i - startSprite) / numberOfSprites;

            if (BaseColors.Any())
                LizColorizer(sLeaser.sprites[i + numberOfSprites], colorMode[0], BaseColors, percentage);
        }
    }
}