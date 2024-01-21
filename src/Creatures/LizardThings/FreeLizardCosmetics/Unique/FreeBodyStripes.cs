using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Unique;

public class FreeBodyStripes : BodyStripes, IFreedCosmetic
{ // Unfinished
    public LizColorMode[] colorMode => new LizColorMode[3];
    public List<Color> FadeColors { get; set; }
    public List<Color> BaseColors { get; set; }
    public bool darkenWithHead { get; }
    public float dark { get; set; }
    
    public FreeBodyStripes(
        LizardGraphics lGraphics, 
        int startSprite, 
        IList<LizColorMode> colorMath, 
        IList<Color> baseColors = null, 
        IList<Color> fadeColors = null, 
        bool pulseWithHead = false) : base(lGraphics,startSprite)
    {
        BaseColors = new List<Color>();
        FadeColors = new List<Color>();
        darkenWithHead = pulseWithHead;
        if (baseColors != null && baseColors.Any())
            BaseColors.AddRange(baseColors);
        if (fadeColors != null && fadeColors.Any())
            FadeColors.AddRange(fadeColors);
    }
}