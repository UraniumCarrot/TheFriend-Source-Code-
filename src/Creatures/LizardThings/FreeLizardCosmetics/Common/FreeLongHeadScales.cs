using System.Collections.Generic;
using System.Linq;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using static TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies.FreedCosmeticMethods;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeLongHeadScales : LongHeadScales, IFreedCycleColors, IHaveSizeBonus
{
    public LizColorMode[] colorMode => new LizColorMode[2];
    public bool ImColored { get; }
    public List<Color> FadeColors { get; set; }
    public List<Color> BaseColors { get; set; }
    public bool darkenWithHead { get; }
    public float dark { get; set; }
    public Vector2? sizeBonus { get; set; }
    public float timer { get; set; }
    public float CycleSpeed { get; }
    public FreeLongHeadScales(
        LizardGraphics lGraphics, 
        int startSprite, 
        IList<LizColorMode> colorMath, 
        IList<Color> baseColors = null, 
        IList<Color> fadeColors = null, 
        Vector2? SizeBonus = default,
        float cycleSpeed = 0.001f, 
        bool pulseWithHead = false) : base(lGraphics,startSprite)
    {
        ColorModeGetter(this,colorMath);
        ImColored = colored;
        BaseColors = new List<Color>();
        FadeColors = new List<Color>();
        darkenWithHead = pulseWithHead || cycleSpeed == 0;
        CycleSpeed = cycleSpeed;
        if (SizeBonus.HasValue)
            sizeBonus = new Vector2(Mathf.Abs(SizeBonus.Value.x + 1), Mathf.Abs(SizeBonus.Value.y + 1));
        else sizeBonus = null;
        if (baseColors != null && baseColors.Any())
            BaseColors.AddRange(baseColors);
        if (fadeColors != null && fadeColors.Any())
            FadeColors.AddRange(fadeColors);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser,rCam);
        if (sizeBonus != null) SizeBonusInit(this, sizeBonus.Value, sLeaser);
    }
    
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (sizeBonus != null) SizeBonusDraw(this, new Vector2(sizeBonus.Value.x,0), sLeaser, true);
        FreedCosmeticTick(this, this, sLeaser);
        bool affectAlpha = CycleSpeed != 0;
        for (int i = startSprite + scalesPositions.Length - 1; i >= startSprite; i--)
        {
            if (BaseColors.Any())
                FreedCosmeticDrawCycle(this,0, sLeaser.sprites[i], BaseColors);
            if (colored)
                if (FadeColors.Any())
                    FreedCosmeticDrawCycle(this,1, sLeaser.sprites[i + scalesPositions.Length], FadeColors, affectAlpha);
        }
    }
}