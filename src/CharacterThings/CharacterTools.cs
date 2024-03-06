using System.Collections.Generic;
using RWCustom;
using TheFriend.SlugcatThings;
using UnityEngine;

namespace TheFriend.CharacterThings;

public static class CharacterTools
{
    public static void Squint(this PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser)
    {
        var face = sLeaser.sprites[9];
        if (self.player.dead) return;
        if (self.player.GetGeneral().squint && !face.element.name.Contains("Stunned"))
            face.element =
                Futile.atlasManager.GetElementWithName(face.element.name.Remove(face.element.name.Length-2, 2) + "Stunned");
    }

    public static void HeadShiver(this PlayerGraphics self, float intensity)
    {
        self.head.vel += Custom.RNV() * intensity;
    }
    public static void LookAtRain(this PlayerGraphics self)
    {
        self.objectLooker.LookAtPoint(new Vector2(
            self.player.room.PixelWidth * Random.value,
            self.player.room.PixelHeight + 100f),
            (1f - self.player.room.world.rainCycle.RainApproaching) * 0.6f);
    }

    public static void ColorChange(this PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, Color color, List<int> indexesToChange)
    {
        foreach (int i in indexesToChange)
            sLeaser.sprites[i].color = color;
    }
    
    public static Color ColorMaker(
        float hue, float sat, float val,
        ToolMethods.MathMode hueMode, ToolMethods.MathMode satMode, ToolMethods.MathMode valMode,
        Color origCol = new Color(), Vector3 origHSL = new Vector3())
    {   // This method is pretty much exclusively for easy Jolly Co-op autocoloring
        // Negative floats can be used to preserve the original value
        Vector3 color = Custom.ColorToVec3(Color.black);
        if (origCol != Color.black) color = Custom.RGB2HSL(origCol);
        if (origHSL != Vector3.zero) color = origHSL;
        float newhue = color.x;
        float newsat = color.y;
        float newval = color.z;

        color.x = hueMode switch
        {
            ToolMethods.MathMode.set => newhue = (hue < 0) ? color.x : hue,
            ToolMethods.MathMode.add => newhue += (hue < 0) ? 0 : hue,
            ToolMethods.MathMode.mult => newhue *= (hue < 0) ? 1 : hue,
            _ => newhue = 0
        };
        color.y = satMode switch
        {
            ToolMethods.MathMode.set => newsat = (sat < 0) ? color.y : sat,
            ToolMethods.MathMode.add => newsat += (sat < 0) ? 0 : sat,
            ToolMethods.MathMode.mult => newsat *= (sat < 0) ? 1 : sat,
            _ => newsat = 0
        };
        color.z = valMode switch
        {
            ToolMethods.MathMode.set => newval = (val < 0) ? color.z : val,
            ToolMethods.MathMode.add => newval += (val < 0) ? 0 : val,
            ToolMethods.MathMode.mult => newval *= (val < 0) ? 1 : val,
            _ => newval = 0
        };

        color.x = newhue;
        color.y = newsat;
        color.z = newval;

        if (color == Vector3.zero) return Color.magenta;
        return Custom.Vec3ToColor(color);
    }

    public static bool TryGetCustomJollyColor(int playerNumber, int bodyPartIndex, out Color color)
    {
        if (ModManager.CoopAvailable && Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM)
        {
            var col = PlayerGraphics.jollyColors;
            if (col.GetLength(0) > playerNumber)
            {
                if (col[playerNumber].Length > bodyPartIndex)
                {
                    if (col[playerNumber][bodyPartIndex].HasValue)
                    {
                        color = col[playerNumber][bodyPartIndex].Value;
                        return true;
                    }
                }
            }
        }
        color = default;
        return false;
    }
}