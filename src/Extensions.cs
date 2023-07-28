using System;

namespace TheFriend;

public static class Extensions
{
    public static float Map(this float x, float in_min, float in_max, float out_min, float out_max, bool clamp = false)
    {
        if (clamp) x = Math.Max(in_min, Math.Min(x, in_max));
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    public static void Tick(this ref int counter)
    {
        if (counter > 0) counter--;
    }

    public static bool IsSmallerThanMe(this Player self, Creature crit) => crit.Template.smallCreature || self.TotalMass > crit.TotalMass;
    public static bool IsSmallerThanMe(this Creature self, Creature crit) => self.TotalMass > crit.TotalMass;
}
