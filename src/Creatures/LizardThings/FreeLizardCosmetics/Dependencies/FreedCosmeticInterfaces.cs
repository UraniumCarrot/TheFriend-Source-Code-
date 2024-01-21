using System.Collections.Generic;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

public interface IFreedCosmetic
{
    public LizColorMode[] colorMode { get; }
    public bool darkenWithHead { get; }
    public float dark { get; set; }
}

public interface IFreedMirrorer : IFreedDownLength{}

public interface IFreedCycleColors : IFreedDownLength
{
    public float timer { get; set; }
    public float CycleSpeed { get; } // How many ticks until the timer resets?
    // Setting CycleSpeed to 0 should make it change color in sync with the lizard's head
}
public interface IFreedDownLength : IFreedCosmetic
{
    public bool ImColored { get; } // Does this cosmetic have fade sprites? Prevents Index Out Of Bounds
    List<Color> BaseColors { get; set; }
    List<Color> FadeColors { get; set; }
}
public interface IHaveSizeBonus
{
    Vector2? sizeBonus { get; set; }
}

public interface IUsePatterns
{
    LizScalePattern Pattern { get; }
}
public interface ISpriteOverridable
{
    string newSprite { get; }
}

public enum LizColorMode
{
    HSL,
    RGB
}
public enum LizScalePattern
{
    patch,
    twoline,
    segments
}