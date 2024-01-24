using System.Collections.Generic;
using System.Linq;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using RWCustom;
using TheFriend.Objects.LittleCrackerObject;
using UnityEngine;

namespace TheFriend.Objects.SolaceScarfObject;

public class SolaceScarfDyes
{
    /*public static void Apply()
    {
        new Hook(typeof())
    }

    public static bool SolaceScarfCanDyeCheck(Player self)
    {
        if (self.grasps.Any(x => x?.grabbed?.abstractPhysicalObject is AbstractConsumable))
        {
            var grasp = self.grasps.IndexOf(self.grasps.First(x => x?.grabbed?.abstractPhysicalObject is AbstractConsumable));
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
            switch (edible)
            {
                case SlimeMold mold: if (colors[0] != Color.black) scarf.Abstr.IGlow += 1; mold.bites--; break;
                case LillyPuck puck: scarf.Abstr.IGlow += 2; puck.AbstrLillyPuck.bites--; break;
                case GlowWeed weed: scarf.Abstr.IGlow += 2; weed.bites--; break;
                case SwollenWaterNut nut: scarf.Abstr.IGlow = 0; scarf.Abstr.IBurn = 0; nut.bites--; break;
                case FireEgg fire: scarf.Abstr.IBurn += 3; fire.bites--; break;
                
                case DangleFruit fruit: fruit.bites--; break;
                case GooieDuck duck: duck.bites--; break;
                case OracleSwarmer neur: neur.bites--; break;
                case JellyFish fish: fish.bites--; break;
                case DandelionPeach peach: peach.bites--; break;
                case Hazer hazer: hazer.bites--; break;
                case VultureGrub grub: grub.bites--; break;
                case Fly fly: fly.bites--; break;
                case Centipede pede: pede.bites--; break;
                case EggBugEgg egg: egg.bites--; break;
            }
        
        else if (self.CanBeSwallowed(item))
            switch (item)
            {
                case FirecrackerPlant or LittleCracker: scarf.Abstr.IBurn += 3; break;
                case FlareBomb: scarf.Abstr.IGlow += 1; break;
                case Lantern: scarf.Abstr.IGlow += 5; break;
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
    }*/
}