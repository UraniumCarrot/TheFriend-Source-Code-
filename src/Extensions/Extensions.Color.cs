using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace TheFriend;

public partial class Extensions
{
    public static Color RandomRGB(float r = 0, float g = 0, float b = 0)
    {
        r = r == 0 ? UnityEngine.Random.value : r;
        g = g == 0 ? UnityEngine.Random.value : g;
        b = b == 0 ? UnityEngine.Random.value : b;
        return new Color(r, g, b);
    }
    public static Vector3 RandomHSL(float h = 0, float s = 0, float l = 0)
    {
        h = h == 0 ? UnityEngine.Random.value : h;
        s = s == 0 ? UnityEngine.Random.value : s;
        l = l == 0 ? UnityEngine.Random.value : l;
        return new Vector3(h,s,l);
    }
    
    // Retrieve the hsl values of a color
    public static float Hue(this Color color)
    {
        return Custom.RGB2HSL(color).x;
    }
    public static float Sat(this Color color)
    {
        return Custom.RGB2HSL(color).y;
    }
    public static float Lit(this Color color)
    {
        return Custom.RGB2HSL(color).z;
    }

    // Change hsl values of an existing color
    public static void ChangeHue(this ref Color color, float newHue)
    {
        var colorHsl = Custom.RGB2HSL(color);
        colorHsl.x = Mathf.Abs(newHue % 1);
        color = colorHsl.HSL2RGB();
    }
    public static void ChangeLit(this ref Color color, float newLit)
    {
        var colorHsl = Custom.RGB2HSL(color);
        colorHsl.z = Mathf.Abs(newLit % 1);
        color = colorHsl.HSL2RGB();
    }
    public static void ChangeSat(this ref Color color, float newSat)
    {
        var colorHsl = Custom.RGB2HSL(color);
        colorHsl.y = Mathf.Abs(newSat % 1);
        color = colorHsl.HSL2RGB();
    }
    
    // Make new colors instead of changing old ones
    public static Color MakeHue(this ref Color color, float newHue)
    {
        var colorHsl = Custom.RGB2HSL(color);
        colorHsl.x = Mathf.Abs(newHue % 1);
        return colorHsl.HSL2RGB();
    }
    public static Color MakeLit(this Color color, float newLit)
    {
        var colorHsl = Custom.RGB2HSL(color);
        colorHsl.z = Mathf.Abs(newLit % 1);
        return colorHsl.HSL2RGB();
    }
    public static Color MakeSat(this Color color, float newSat)
    {
        var colorHsl = Custom.RGB2HSL(color);
        colorHsl.y = Mathf.Abs(newSat % 1);
        return colorHsl.HSL2RGB();
    }

    public static Color HSL2RGB(this Vector3 colorVec)
    {
        return Custom.HSL2RGB(colorVec.x, colorVec.y, colorVec.z);
    }
    public static Vector3 RGB2HSL(this Color color)
    {
        var col = Custom.RGB2HSL(color);
        return new Vector3(col.x, col.y, col.z);
    }

    public static Color HSLMultiLerp(IList<Color> map, float value, bool cycle = false)
    { // Most Multilerp code was created by Vigaro, ask Vigaro if you can use them in your own work
        List<Vector3> newMap = new List<Vector3>();
        foreach (Color col in map)
            newMap.Add(Custom.RGB2HSL(col));
        if (cycle) newMap.Add(Custom.RGB2HSL(map[0]));

        value = Mathf.Clamp01(value);

        var pos = Mathf.Lerp(0, newMap.Count - 1, value);
        var a = Mathf.FloorToInt(pos);
        var b = a + 1;
        var lerpValue = pos - a;

        if (b >= newMap.Count)
        {
            b -= 1;
            if (value >= 1)
            {
                lerpValue = 1;
            }
        }

        var hslLerpResult = Vector3.Lerp(newMap[a], newMap[b], lerpValue);

        if (Mathf.Abs(newMap[b].x - newMap[a].y) > 180) // if its shorter to go the other way (180 is half of the color wheel)
            hslLerpResult.x = ReverseLerp(newMap[a].x, newMap[b].x, lerpValue, 360); // reverse the lerp

        return hslLerpResult.HSL2RGB();
    } // HSLMultiLerp is a small mutation of it made by me, Elliot; feed it colors and get colors back, but internally
      // work inside an HSL space for more vibrant transitioning.

    public static Color RGBMultiLerp(IList<Color> map, float value, bool cycle = false)
    {
        List<Color> newMap = new List<Color>();
        foreach (Color col in map)
            newMap.Add(col);
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

        return Color.Lerp(newMap[a], newMap[b], lerpValue);
    }

    public static Color ColorFromHEX(string hex) //Apparently similar also exists in RWCustom.Custom; Whoops.
    {
        hex = hex.Replace("#", "");
        if (hex.Length == 3)
        {
            hex = "" + hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
        }

        try
        {
            var r = Convert.ToInt32(hex.Substring(0, 2), 16) / 255f;
            var g = Convert.ToInt32(hex.Substring(2, 2), 16) / 255f;
            var b = Convert.ToInt32(hex.Substring(4, 2), 16) / 255f;
            return new Color(r, g, b);
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError("Invalid Color HEX!!");
            Plugin.LogSource.LogError(ex);
            return Color.magenta;
        }
    }

    #region LAB, LCH

    public static Vector3 RGBtoLCH(this Color rgb)
    {
        // Convert RGB to Lab
        var labColor = RGBtoLab(rgb);

        // Extract Lab components
        var L = labColor.x;
        var a = labColor.y;
        var b = labColor.z;

        // Calculate LCH components
        var C = Mathf.Sqrt(a * a + b * b);
        var H = Mathf.Atan2(b, a) * (180 / Mathf.PI); // Convert radians to degrees

        // Ensure H is in the [0, 360) range
        H = (H < 0) ? H + 360 : H;

        return new Vector3(L, C, H);
    }

    private static Vector3 RGBtoLab(this Color rgb)
    {
        // Apply gamma correction
        rgb.r = (rgb.r > 0.04045f) ? Mathf.Pow((rgb.r + 0.055f) / 1.055f, 2.4f) : rgb.r / 12.92f;
        rgb.g = (rgb.g > 0.04045f) ? Mathf.Pow((rgb.g + 0.055f) / 1.055f, 2.4f) : rgb.g / 12.92f;
        rgb.b = (rgb.b > 0.04045f) ? Mathf.Pow((rgb.b + 0.055f) / 1.055f, 2.4f) : rgb.b / 12.92f;

        // Convert RGB to XYZ
        var X = rgb.r * 0.4124564f + rgb.g * 0.3575761f + rgb.b * 0.1804375f;
        var Y = rgb.r * 0.2126729f + rgb.g * 0.7151522f + rgb.b * 0.0721750f;
        var Z = rgb.r * 0.0193339f + rgb.g * 0.1191920f + rgb.b * 0.9503041f;

        // Normalize XYZ to D65 illuminant
        X /= 0.95047f;
        Y /= 1.00000f;
        Z /= 1.08883f;

        // Convert XYZ to Lab
        X = (X > 0.008856f) ? Mathf.Pow(X, 1.0f / 3.0f) : (903.3f * X + 16.0f) / 116.0f;
        Y = (Y > 0.008856f) ? Mathf.Pow(Y, 1.0f / 3.0f) : (903.3f * Y + 16.0f) / 116.0f;
        Z = (Z > 0.008856f) ? Mathf.Pow(Z, 1.0f / 3.0f) : (903.3f * Z + 16.0f) / 116.0f;

        var L = Mathf.Max(0.0f, Mathf.Min(100.0f, 116.0f * Y - 16.0f));
        var A = (X - Y) * 500.0f;
        var B = (Y - Z) * 200.0f;

        return new Vector3(L, A, B);
    }

    //LCH TO RGB
    public static Color LCHtoRGB(this Vector3 lch)
    {
        return LabtoRGB(LCHtoLab(lch));
    }

    private static Vector3 LCHtoLab(Vector3 lch)
    {
        // Convert LCH to Lab
        var a = lch.y * Mathf.Cos(lch.z * (Mathf.PI / 180f));
        var b = lch.y * Mathf.Sin(lch.z * (Mathf.PI / 180f));

        return new Vector3(lch.x, a, b);
    }

    private static Color LabtoRGB(this Vector3 lab)
    {
        // Convert Lab to XYZ
        var y = (lab.x + 16.0f) / 116.0f;
        var x = lab.y / 500.0f + y;
        var z = y - lab.z / 200.0f;

        y = Mathf.Pow(y, 3.0f);
        x = Mathf.Pow(x, 3.0f);
        z = Mathf.Pow(z, 3.0f);

        y = (y > 0.008856f) ? y : (y - 16.0f / 116.0f) / 7.787f;
        x = (x > 0.008856f) ? x : (x - 16.0f / 116.0f) / 7.787f;
        z = (z > 0.008856f) ? z : (z - 16.0f / 116.0f) / 7.787f;

        x *= 0.95047f;
        y *= 1.00000f;
        z *= 1.08883f;

        // Convert XYZ to RGB
        var r = x * 3.2406f - y * 1.5372f - z * 0.4986f;
        var g = -x * 0.9689f + y * 1.8758f + z * 0.0415f;
        var b = x * 0.0557f - y * 0.2040f + z * 1.0570f;

        // Apply gamma correction
        r = Mathf.Clamp01(r > 0.0031308f ? 1.055f * Mathf.Pow(r, 1f / 2.4f) - 0.055f : 12.92f * r);
        g = Mathf.Clamp01(g > 0.0031308f ? 1.055f * Mathf.Pow(g, 1f / 2.4f) - 0.055f : 12.92f * g);
        b = Mathf.Clamp01(b > 0.0031308f ? 1.055f * Mathf.Pow(b, 1f / 2.4f) - 0.055f : 12.92f * b);

        return new Color(r, g, b);
    }

    #endregion
}