using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace TheFriend;

public static partial class Extensions
{
    #region RainWorld Specific
    public static bool IsSolaceName(this SlugcatStats.Name name)
    {
        return name.value == Plugin.FriendName.value || name.value == Plugin.NoirName.value || name.value == Plugin.DragonName.value || name.value == Plugin.DelugeName.value ||
               name.value == Plugin.BelieverName.value;
    }

    public static bool IsSmallerThanMe(this Player self, Creature crit) => crit.Template.smallCreature || self.TotalMass > crit.TotalMass;
    public static bool IsSmallerThanMe(this Creature self, Creature crit) => self.TotalMass > crit.TotalMass;
    #endregion

    #region Custom values extensions
    public static float MaxValue(this Vector2 vector)
    {
        return Mathf.Max(vector.x, vector.y);
    }
    public static float MinValue(this Vector2 vector)
    {
        return Mathf.Min(vector.x, vector.y);
    }

    #endregion

    #region Built-in values extensions
    public static float Map(this float x, float in_min, float in_max, float out_min, float out_max, bool clamp = false)
    {
        if (clamp) x = Math.Max(in_min, Math.Min(x, in_max));
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    public static void Tick(this ref int counter)
    {
        if (counter > 0) counter--;
    }

    public static bool IsEven(this int number)
    {
        return number % 2 == 0;
    }

    public static int Floor(this ref float number)
    {
        return Mathf.FloorToInt(number);
    }
    
    public static float FloatMultiLerp(IList<float> map, float value, bool cycle = false)
    { // Most Multilerp code was created by Vigaro, ask Vigaro if you can use them in your own work
        List<float> newMap = new List<float>();
        foreach (float fl in map)
            newMap.Add(fl);
        if (cycle) newMap.Add(map[0]);
        value = Mathf.Clamp01(value);

        var pos = Mathf.Lerp(0, newMap.Count - 1, value);
        var a = Mathf.FloorToInt(pos);
        var b = a + 1;
        var lerpValue = pos - a;

        if (b >= newMap.Count)
        {
            b -= 1;
            if (value >= 1)
                lerpValue = 1;
        }

        return Mathf.Lerp(newMap[a], newMap[b], lerpValue);
    }
    
    public static Vector3 V3MultiLerp(IList<Vector3> map, float value, bool cycle = false)
    {
        List<Vector3> newMap = new List<Vector3>();
        foreach (Vector3 v3 in map)
            newMap.Add(v3);
        if (cycle) newMap.Add(map[0]);
        value = Mathf.Clamp01(value);

        var pos = Mathf.Lerp(0, newMap.Count - 1, value);
        var a = Mathf.FloorToInt(pos);
        var b = a + 1;
        var lerpValue = pos - a;

        if (b >= newMap.Count)
        {
            b -= 1;
            if (value >= 1) {
                lerpValue = 1;
            }
        }

        return Vector3.Lerp(newMap[a], newMap[b], lerpValue);
    }

    public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> list)
    {
        foreach (var element in list)
            set.Add(element);
    }
    public static void RemoveRange<T>(this HashSet<T> set, IEnumerable<T> list)
    {
        foreach (var element in list)
            set.Remove(element);
    }
    #endregion

    #region Reflection //Heavy operations involving the use of System.Reflection

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
    //For ILHooking
    public static bool MatchGetterCall<T>(this Instruction i, string methodName)
    {
        return i.MatchCallOrCallvirt<T>(typeof(T).GetGetterMethodName(methodName));
    }
    //For ILHooking
    public static bool MatchSetterCall<T>(this Instruction i, string methodName)
    {
        return i.MatchCallOrCallvirt<T>(typeof(T).GetSetterMethodName(methodName));
    }

    #endregion
}
