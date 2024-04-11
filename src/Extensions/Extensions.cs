using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using UnityEngine;

namespace TheFriend;

public static partial class Extensions
{
    #region RainWorld Specific
    public static int AddFreeCosmetic(this LizardGraphics self, int spriteIndex, FreedCosmeticTemplate cosmetic)
    { // For use if a freed cosmetic's owner isnt added to the lizard in the normal way
        self.cosmetics.Add(cosmetic.owner);
        self.cosmetics.Add(cosmetic);
        spriteIndex += cosmetic.owner.numberOfSprites;
        self.extraSprites += cosmetic.owner.numberOfSprites;
        return spriteIndex;
    }
    public static bool IsSolaceName(this SlugcatStats.Name name)
    {
        return name.value == Plugin.FriendName.value || name.value == Plugin.NoirName.value || name.value == Plugin.DragonName.value;
    }

    public static bool IsSmallerThanMe(this Player self, Creature crit) => crit.Template.smallCreature || self.TotalMass > crit.TotalMass;
    public static bool IsSmallerThanMe(this Creature self, Creature crit) => self.TotalMass > crit.TotalMass;

    public static Vector2 AveragedPosition(this PhysicalObject self)
    {
        Vector2 positionSum = new Vector2();
        if (self.bodyChunks.Length == 0)
        {
            Plugin.LogSource.LogWarning("Solace: This PhysicalObject does not have any bodychunks for AveragedPosition to check.");
            return Vector2.zero;
        }
        foreach (BodyChunk chunk in self.bodyChunks)
            positionSum += chunk.pos;
        return positionSum / self.bodyChunks.Length;
    }

    public static float GetLikeOfObject(this Creature self, PhysicalObject obj)
    {
        var a = self.abstractCreature.state.socialMemory.relationShips.Find(x => x.subjectID == obj.abstractPhysicalObject.ID);
        if (a != null) return a.like;
        return 0;
    }

    public static bool ImAggressiveToYou(this ArtificialIntelligence self, AbstractCreature obj)
    {
        var a = self.DynamicRelationship(obj).type == CreatureTemplate.Relationship.Type.Attacks;
        var b = self.DynamicRelationship(obj).type == CreatureTemplate.Relationship.Type.Eats;
        var c = self.DynamicRelationship(obj).type == CreatureTemplate.Relationship.Type.AgressiveRival;
        var d = self.DynamicRelationship(obj).type == CreatureTemplate.Relationship.Type.Antagonizes;
        return a || b || c || d;
    }
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

    public static float Abs(this float number)
    {
        return Mathf.Abs(number);
    }
    public static int Abs(this int number)
    {
        return Mathf.Abs(number);
    }
    public static float Sign(this float number)
    {
        return (int)Mathf.Sign(number);
    }
    public static int Sign(this int number)
    {
        return (int)Mathf.Sign(number);
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
    
    /// <summary>
    /// Performs a lerp from a to b using t, but goes the opposite way than normal, wrapping around the boundary
    /// </summary>
    /// <param name="a">the value to lerp from</param>
    /// <param name="b">the value to lerp to</param>
    /// <param name="t">The the "progress" of the lerp (0-1)</param>
    /// <param name="max_value">The point at which the lerp should wrap around</param>
    /// <remarks>
    /// <para>normal lerp:  a = 20, b = 80 would go 20>40>50>60>80</para>
    /// <para>rev lerp: a = 20, b = 80, max_value = 100 would go 20>10>00>90>80</para>
    /// <para>Only works for value ranges that start at 0 and go to "max_value".</para>
    /// </remarks>
    public static float ReverseLerp(float a, float b, float t, float max_value)
    {
        // get the distance between "a" and "b", but going opposite way than normal and looping around
        // do this by getting the normal lerp distance and subtracting that from the max value
        var maxDisplacement = max_value - Math.Abs(b - a);

        // Calculate how much we are actually going to move based on t (invert the sign if the terms are backwards)
        var lerpDistance = maxDisplacement * ((b < a) ? Mathf.Clamp01(t) : -Mathf.Clamp01(t));

        // Add max_value to the starting term
        // this is needed because the modulo operator does not wrap around when going to negative numbers, it just inverts its operation
        // ex: mod 3: 4 = 1, 3 = 0, 2 = 2, 1 = 1, 0 = 0, -1 = -1, etc...
        // We need -1 to be max_value-1 for wrapping to work properly
        var startingValue = max_value + a;

        // Modulo handles wrapping around when lerpDistance is positive
        return (startingValue + lerpDistance) % max_value;
    }

    public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> list)
    {
        foreach (var element in list)
            if (!set.Contains(element)) set.Add(element);
    }
    public static void RemoveRange<T>(this HashSet<T> set, IEnumerable<T> list)
    {
        foreach (var element in list)
            if (set.Contains(element)) set.Remove(element);
    }
    #endregion

    #region Reflection //Heavy operations involving the use of System.Reflection

    /// <summary>
    /// Returns the name of the getter method for the property
    /// </summary>
    /// <param name="T">The class the property originates from</param>
    /// <param name="propertyName">The name of the property</param>
    /// <example><code>GetGetMethodName&lt;MyClass&gt;(nameof(MyClass.MyProperty))</code></example>
    public static string GetGetMethodName<T>(string propertyName)
    {
        var result = typeof(T).GetProperty(propertyName)?.GetGetMethod().Name;
        if (result == null)
            throw new ArgumentNullException($"Property {propertyName} of {typeof(T).Name} returned null!");
        return result;
    }
    /// <summary>
    /// Returns the name of the getter method for the property
    /// </summary>
    /// <param name="T">The class the property originates from</param>
    /// <param name="propertyName">The name of the property</param>
    /// <example><code>GetSetMethodName&lt;MyClass&gt;(nameof(MyClass.MyProperty))</code></example>
    public static string GetSetMethodName<T>(string propertyName)
    {
        var result = typeof(T).GetProperty(propertyName)?.GetGetMethod().Name;
        if (result == null)
            throw new ArgumentNullException($"Property {propertyName} of {typeof(T).Name} returned null!");
        return result;
    }
    //For ILHooking
    public static bool MatchGetterCall<T>(this Instruction i, string methodName)
    {
        return i.MatchCallOrCallvirt<T>(GetGetMethodName<T>(methodName));
    }
    //For ILHooking
    public static bool MatchSetterCall<T>(this Instruction i, string methodName)
    {
        return i.MatchCallOrCallvirt<T>(GetSetMethodName<T>(methodName));
    }

    #endregion
}
