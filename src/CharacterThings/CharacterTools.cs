using System.Collections.Generic;
using System.Linq;
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

    public static bool CanGrabSafely(this Player self)
    {
        bool result = true;

        bool notHoldingAnything =
            self.grasps[0]?.grabbed == null &&
            self.grasps[1]?.grabbed == null;
        
        bool notHoldingSwallowables = 
            !self.CanBeSwallowed(self.grasps[0]?.grabbed) &&
            !self.CanBeSwallowed(self.grasps[1]?.grabbed);
        
        bool notHoldingFoods = 
            self.grasps[0]?.grabbed is not IPlayerEdible &&
            self.grasps[1]?.grabbed is not IPlayerEdible;

        if (self.input[0].y != 0) result = false;
        if (notHoldingAnything && self.pickUpCandidate != null) result = false;
        if (!notHoldingFoods && !self.ImFull()) result = false;
        if (!notHoldingSwallowables && self.objectInStomach != null) result = false;
        return result || self.dontGrabStuff > 0;
    }

    public static bool ImFull(this Player self)
    {
        return self.FoodInStomach >= self.MaxFoodInStomach;
    }

    public static void InputTapCheck(Player self, string inputToCheck, out int taps)
    { // Code by Slime_Cubed. DUDE IS AWESOME.
        var inputList = self.GetGeneral().ExtendedInput;
        
        int i = 0;
        taps = 0;
        int tapsTemp = 0;
    
        // Skip leading trues
        while (i < inputList.Length && GetInputFromString(inputList[i], inputToCheck).Abs() == 1)
            i += 1;
    
        while (i < inputList.Length)
        {
            // Find the start of a press
            while (i < inputList.Length && GetInputFromString(inputList[i], inputToCheck) == 0)
                i += 1;
        
            // Measure the length of a press
            int length = 0;
            while (i < inputList.Length && GetInputFromString(inputList[i], inputToCheck).Abs() == 1)
            {
                i += 1;
                length += 1;
            }
        
            // Check if the press is short enough and doesn't hit the right side
            if (i < inputList.Length && length < 6)
                tapsTemp += 1;
        }

        if (GetInputFromString(inputList[0], inputToCheck) == 0 &&
            GetInputFromString(inputList[1], inputToCheck) == 0 &&
            GetInputFromString(inputList[2], inputToCheck) == 0 &&
            GetInputFromString(inputList[3], inputToCheck) == 0 &&
            GetInputFromString(inputList[4], inputToCheck) == 0 &&
            GetInputFromString(inputList[5], inputToCheck) == 0)
            taps = tapsTemp;
    }
    public static int GetInputFromString(Player.InputPackage package, string input)
    {
        switch (input.ToLower())
        {
            case "y": return package.y;
            case "x": return package.x;
            case "grab" or "grb": return package.pckp ? 1 : 0;
            case "pickup" or "pckp": return package.pckp ? 1 : 0;
            case "jump" or "jmp": return package.jmp ? 1 : 0;
            case "map" or "mp": return package.mp ? 1 : 0;
            case "throw" or "thrw": return package.thrw ? 1 : 0;
        }
        Plugin.LogSource.LogWarning("Solace: GetInputsFromString was fed an invalid string: " + input);
        return 0;
    }
    public static bool CompareInputs(Player.InputPackage one, Player.InputPackage two, string input)
    {
        switch (input.ToLower())
        {
            case "grab" or "grb": input = "pckp"; break;
            case "pickup": input = "pckp"; break;
            case "jump": input = "jmp"; break;
            case "map": input = "mp"; break;
            case "throw": input = "thrw"; break;
            default: input = input.ToLower(); break;
        }
        switch (input)
        {
            case "x": return one.x == two.x;
            case "y": return one.y == two.y;
            case "pckp": return one.pckp == two.pckp;
            case "jmp": return one.jmp == two.jmp;
            case "mp": return one.mp == two.mp;
            case "thrw": return one.thrw == two.thrw;
        }
        Plugin.LogSource.LogWarning("Solace: CompareInputs was fed an invalid string: " + input);
        return false;
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