using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using static TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies.FreedCosmeticMethods;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Common;

public class FreeTailTuft : TailTuft, IFreedMirrorer, IHaveSizeBonus, IUsePatterns
{
    public LizColorMode[] colorMode => new LizColorMode[2];
    public bool ImColored { get; }
    public List<Color> FadeColors { get; set; }
    public List<Color> BaseColors { get; set; }
    public bool darkenWithHead { get; }
    public float dark { get; set; }
    public Vector2? sizeBonus { get; set; }
    public LizScalePattern Pattern { get; }

    public FreeTailTuft(
        LizardGraphics lGraphics, 
        int startSprite, 
        IList<LizColorMode> colorMath, 
        IList<Color> baseColors = null,
        IList<Color> fadeColors = null, 
        Vector2? SizeBonus = default, 
        bool pulseWithHead = false,
        LizScalePattern? pattern = null) : base(lGraphics, startSprite)
    {
        ColorModeGetter(this,colorMath);
        ImColored = colored;
        darkenWithHead = pulseWithHead;
        BaseColors = new List<Color>();
        FadeColors = new List<Color>();
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

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker,
        Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (sizeBonus != null) 
            SizeBonusDraw(this, new Vector2(sizeBonus.Value.x,0), sLeaser, true);
        FreedCosmeticTick(this, this, sLeaser);
        if (scalesPositions.Length.IsEven()) FreedCosmeticDrawMirror(this, sLeaser, scalesPositions.Length, this);
        else FreedCosmeticDrawGeneric(this, this, sLeaser, scalesPositions.Length);
    }
}