using System;

namespace TheFriend;

public static class Extensions
{
    public static bool IsSolaceName(this SlugcatStats.Name name)
    {
        return name.value == Plugin.FriendName.value || name.value == Plugin.NoirName.value || name.value == Plugin.DragonName.value || name.value == Plugin.DelugeName.value ||
               name.value == Plugin.BelieverName.value;
    }

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

    /// <summary>
    /// Returns the name of the getter method for the property
    /// </summary>
    /// <param name="type">The class the property originates from</param>
    /// <param name="propertyName">The name of the property;</param>
    /// <example>GetGetterMethodName(nameof(<paramref name="propertyName"/>))</example>
    public static string GetGetterMethodName(this Type type, string propertyName)
    {
        var result = type.GetProperty(propertyName)?.GetGetMethod().Name;
        if (result == null)
            throw new ArgumentNullException($"Property {propertyName} of {type.Name} returned null!");
        return result;
    }
    /// <summary>
    /// Returns the name of the setter method for the property
    /// </summary>
    /// <param name="type">The class the property originates from</param>
    /// <param name="propertyName">The name of the property;</param>
    /// <example>GetSetterMethodName(nameof(<paramref name="propertyName"/>))</example>
    public static string GetSetterMethodName(this Type type, string propertyName)
    {
        var result = type.GetProperty(propertyName)?.GetGetMethod().Name;
        if (result == null)
            throw new ArgumentNullException($"Property {propertyName} of {type.Name} returned null!");
        return result;
    }
}
