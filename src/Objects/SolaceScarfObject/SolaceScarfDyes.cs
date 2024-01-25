using System.Collections.Generic;
using System.Linq;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using RWCustom;
using TheFriend.Objects.LittleCrackerObject;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.Objects.SolaceScarfObject;

public class SolaceScarfDyes
{
    public static void Apply()
    {
        On.Player.ObjectEaten += PlayerOnObjectEaten;
    }

    public static void PlayerOnObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
    {
        if (self.craftingObject && self.grasps.Any(x => x?.grabbed is SolaceScarf)) return;
        orig(self, edible);
    }

    public static bool SolaceScarfCanDyeCheck(Player self)
    {
        if (self.grasps.Count(x => x.grabbed is SolaceScarf) == 2) return false;
        var grasp = self.grasps.IndexOf(self.grasps.First(x => x?.grabbed is not SolaceScarf));
        var item = self.grasps[grasp].grabbed;
        if (item is IPlayerEdible edible)
            switch (edible)
            {
                case VultureGrub grub: return grub.dead;
                case Hazer hazer: return hazer.dead && hazer.hasSprayed;
                case Centipede centi: return centi.dead;
                case GooieDuck duck: return duck.bites < 2;
                case SwollenWaterNut nut: return nut.bites > 1;
                default: return true;
            }
        else if (self.CanBeSwallowed(item))
            switch (item)
            {
                case BubbleGrass: return false;
                case SingularityBomb: return false;
                case Rock and not LittleCracker: return false;
                case NSHSwarmer: return false;
                case ScavengerBomb: return false;
                default: return true;
            }
        return false;
    }

    public static void SolaceScarfDye(Player self)
    {
        var scarfInd = self.grasps.IndexOf(self.grasps.First(x => x?.grabbed is SolaceScarf));
        var itemInd = self.grasps.IndexOf(self.grasps.First(x => x?.grabbed is not SolaceScarf));
        var item = self.grasps[itemInd].grabbed;
        var scarf = self.grasps[scarfInd].grabbed as SolaceScarf;

        if (scarf.Abstr.IBurn>0) scarf.Abstr.IBurn--;
        if (scarf.Abstr.IGlow>0) scarf.Abstr.IGlow--;
        
        List<Color> colors = SolaceScarfDyeColor(item);
        if (item is IPlayerEdible edible)
        {
            edible.BitByPlayer(self.grasps[itemInd], true);
            switch (edible)
            {
                case SlimeMold: if (colors[0] != Color.black) scarf.Abstr.IGlow += 1; break;
                case LillyPuck: scarf.Abstr.IGlow += 2; break;
                case GlowWeed: scarf.Abstr.IGlow += 2; break;
                case SwollenWaterNut: scarf.Abstr.IGlow = 0; scarf.Abstr.IBurn = 0; break;
                case FireEgg: scarf.Abstr.IBurn += 3; break;
                case OracleSwarmer: scarf.Abstr.IGlow += 2; break;
            }
        }
        
        else if (self.CanBeSwallowed(item))
        {
            switch (item)
            {
                case FirecrackerPlant or LittleCracker: scarf.Abstr.IBurn += 3; break;
                case FlareBomb: scarf.Abstr.IGlow += 1; break;
                case Lantern: scarf.Abstr.IGlow += 5; break;
            }
        }

        if (scarf.Abstr.IGlow > 10) scarf.Abstr.IGlow = 10;
        if (scarf.Abstr.IBurn > 10) scarf.Abstr.IBurn = 10;

        if (colors[0] == Color.black) colors[0] = scarf.color;
        if (colors[1] == Color.black) colors[1] = scarf.highlightColor;
        
        scarf.color = colors[0];
        scarf.highlightColor = colors[1];
    }
    
    public static List<Color> SolaceScarfDyeColor(PhysicalObject obj)
    {
        List<Color> list;
        switch (obj)
        {   // Color.black indicates no color change
            // Color [0] is the main body color
            // Color [1] is the highlight color
            case SwollenWaterNut: 
                // Washes the scarf
                list = [Color.white, Color.white]; return list;
            case Hazer: 
                // Completely darken the scarf
                list = [ new Color(0.05f,0.05f,0.1f), new Color(0.05f,0.05f,0.1f)]; return list;
            
            // Normal colors
            case DandelionPeach peach:
                list = [peach.color, peach.color]; return list;
            case GooieDuck duck: 
                list = [duck.CoreColor, duck.CoreColor]; return list;
            case DangleFruit fruit: 
                list = [fruit.color, fruit.color]; return list;
            case SporePlant:
                list = [ new Color(0.4f, 0, 0), new Color(0.4f, 0, 0)]; return list;
            case VultureGrub: 
                list = [Color.yellow, Color.yellow]; return list;
            case Fly: 
                list = [new Color(0.1f,0.3f,0f), new Color(0.1f,0.3f,0f)]; return list;
            case EggBugEgg egg: 
                list = [Color.black, egg.eggColors[1]]; return list;
            case Centipede centi: 
                list = [(centi.graphicsModule as CentipedeGraphics)!.ShellColor,(centi.graphicsModule as CentipedeGraphics)!.ShellColor]; return list;
            case JellyFish fish: 
                list = [fish.color, fish.color]; return list;
            case FlyLure lure: 
                list = [lure.color, Color.black]; return list;
            case PuffBall puff: 
                list = [Color.black, puff.sporeColor]; return list;
            
            // Special colors
            case OverseerCarcass eye: 
                list = [Color.black, eye.color]; return list;
            case DataPearl pearl: 
                list = [pearl.color, (pearl.highlightColor.HasValue) ? pearl.highlightColor.Value : pearl.color]; return list;
            case OracleSwarmer: 
                list = [Extensions.RandomHSL(0,1,0.5f).HSL2RGB(), Extensions.RandomHSL(0,1,0.5f).HSL2RGB()]; return list;
            case FlareBomb: 
                list = [ new Color(0.98f,0.98f,1f), Color.blue ]; return list;
            case SlimeMold mold: 
                if (mold.abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.Seed)
                { list = [Color.black, new Color(0.9f,0.83f,0.5f)]; return list; }
                else
                { list = [mold.color, mold.color]; return list; }
            case LillyPuck puck: 
                list = [puck.flowerColor, puck.flowerColor]; return list;
            case Lantern lant:
                list = [lant.color, Color.Lerp(lant.color,Color.white, 0.99f)]; return list;
            case FirecrackerPlant or LittleCracker: 
                list = [Color.red, Color.red]; return list;
            case GlowWeed weed: 
                list = [weed.color, weed.color]; return list;
            case FireEgg fire: 
                list = [fire.eggColors[1],fire.eggColors[1]]; return list;
        }
        
        Debug.Log("Solace: Dye item did not have an associated color, returning magenta");
        list = [Color.magenta, Color.magenta];
        return list;
    }

    public static void SolaceScarfDyeSplat(PhysicalObject obj, List<Color> color)
    {
        int missingColors = color.FindAll(x => x == Color.black).Count;
        bool watery = false;
        Color? col;
        if (missingColors == 2) col = null;
        else if (missingColors == 1) col = color.Find(x => x != Color.black);
        else col = Color.black;
        
        if (obj is IPlayerEdible)
        {
            obj.room.PlaySound(SoundID.Swollen_Water_Nut_Terrain_Impact, obj.firstChunk.pos);
            watery = true;
        }
        else
        {
            obj.room.PlaySound(SoundID.Snail_Warning_Click, obj.firstChunk.pos);
        }
        for (int i = 0; i < 4; i++)
        {
            if (col != null) 
            {
                if (col == Color.black) col = (Random.value > 0.5f) ? color[0] : color[1];
                obj.room.AddObject(
                new Spark(
                    obj.bodyChunks[0].pos, 
                    Custom.RNV() * Mathf.Lerp(5f, 11f, Random.value),
                    col.Value,null,
                    7,17));
            }
            if (watery) obj.room.AddObject(
                new WaterDrip(
                    obj.firstChunk.pos, 
                    Custom.RNV() * Mathf.Lerp(5f, 11f, Random.value), 
                    waterColor: true));
        }
    }
}