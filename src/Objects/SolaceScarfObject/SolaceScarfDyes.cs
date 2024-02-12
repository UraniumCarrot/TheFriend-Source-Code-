using System.Collections.Generic;
using System.Linq;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using RWCustom;
using TheFriend.Objects.LittleCrackerObject;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.Objects.SolaceScarfObject;

public class SolaceScarfDyes
{
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
        var edibleDye = false;

        if (scarf == null) {Debug.Log("Solace: Scarf dyeing failed!");return;}

        if (scarf.Abstr.IBurn > 0) scarf.Abstr.IBurn--;
        if (scarf.Abstr.IGlow > 0) scarf.Abstr.IGlow--;
        
        List<Color> colors = SolaceScarfDyeColor(item, scarf.Abstr);
        SolaceScarfDyeSplat(item, colors);
        if (item is IPlayerEdible edible)
        {
            edibleDye = true;
            switch (edible)
            {
                case SlimeMold mold: 
                    if (mold.abstractPhysicalObject.type != MoreSlugcatsEnums.AbstractObjectType.Seed) 
                        scarf.Abstr.IGlow += 1; break;
                case LillyPuck: scarf.Abstr.IGlow += 2; break;
                case GlowWeed: scarf.Abstr.IGlow += 2; break;
                case SwollenWaterNut: scarf.Abstr.IGlow -= 2; scarf.Abstr.IBurn -= 2; if (scarf.Abstr.regionOrigin != "HR") scarf.Abstr.IVoid = false; break;
                case FireEgg: scarf.Abstr.IBurn += 3; scarf.Abstr.IVoid = true; break;
                case OracleSwarmer: scarf.Abstr.IGlow += 2; break;
                case KarmaFlower: scarf.Abstr.IGlow += 2; scarf.Abstr.IVoid = true; break;
                case Mushroom: scarf.Abstr.IGlow += 2; break;
            }
            if (edible.BitesLeft <= 1 || edible is GooieDuck)
            {
                if (self.room?.world.game.IsStorySession == true)
                    (self.room.game.session as StoryGameSession)?.RemovePersistentTracker(self.grasps[itemInd].grabbed.abstractPhysicalObject);
                self.grasps[itemInd].grabbed.RemoveFromRoom();
                self.room?.abstractRoom.RemoveEntity(self.grasps[itemInd].grabbed.abstractPhysicalObject);
                self.ReleaseGrasp(itemInd);   
            }
            else edible.BitByPlayer(self.grasps[itemInd], true);
        }
        else if (self.CanBeSwallowed(item) && item is not DataPearl)
        { // Special effects from non-edible items
            switch (item)
            {
                case FirecrackerPlant or LittleCracker: scarf.Abstr.IBurn += 3; break;
                case FlareBomb bomb: scarf.Abstr.IGlow += 1; 
                    bomb.StartBurn();
                    break;
                case Lantern: scarf.Abstr.IGlow += 5; break;
                case PuffBall ball: ball.Explode(); break;
            }

            if (item is not FlareBomb)
            { // Destroy non-pearls and non-flares immediately when used
                if (self.room?.world.game.IsStorySession == true)
                    (self.room.game.session as StoryGameSession)?.RemovePersistentTracker(self.grasps[itemInd].grabbed.abstractPhysicalObject);
                self.grasps[itemInd].grabbed.RemoveFromRoom();
                self.room?.abstractRoom.RemoveEntity(self.grasps[itemInd].grabbed.abstractPhysicalObject);
            }
            self.ReleaseGrasp(itemInd);
        }

        if (scarf.Abstr.IGlow > 10) scarf.Abstr.IGlow = 10;
        if (scarf.Abstr.IBurn > 10) scarf.Abstr.IBurn = 10;
        
        if (colors[0] == colors[1])
            switch (scarfInd)
            {
                case 0: scarf.color = Color.Lerp(scarf.color,colors[0], (edibleDye) ? 0.5f : 1f); break;
                case 1: scarf.highlightColor = Color.Lerp(scarf.highlightColor,colors[0], (edibleDye) ? 0.5f : 1f); break;
            }
        else
        {
            scarf.color = Color.Lerp(scarf.color,colors[0], (edibleDye) ? 0.5f : 1f);
            scarf.highlightColor = Color.Lerp(scarf.highlightColor,colors[1], (edibleDye) ? 0.5f : 1f);
        }
    }
    
    public static List<Color> SolaceScarfDyeColor(PhysicalObject obj, SolaceScarfAbstract scarf)
    {
        List<Color> list = new List<Color>();
        switch (obj)
        {   // Color.clear indicates no color change
            // Color [0] is the main body color
            // Color [1] is the highlight color
            case SwollenWaterNut: 
                // Wash the scarf
                list = SolaceScarfDefaultColors.SolaceScarfDefaultColor(scarf); return list;
            case Hazer: 
                // Completely darken the scarf
                list = [ new Color(0.05f,0.05f,0.1f), new Color(0.05f,0.05f,0.1f)]; return list;
            
            // Normal colors
            case DandelionPeach peach:
                list = [peach.color, peach.color]; break;
            case GooieDuck duck: 
                list = [duck.CoreColor, duck.CoreColor]; break;
            case DangleFruit fruit: 
                list = [fruit.color, fruit.color]; break;
            case VultureGrub: 
                list = [Color.yellow, Color.yellow]; break;
            case Fly: 
                list = [new Color(0.1f,0.3f,0f), new Color(0.1f,0.3f,0f)]; break;
            case EggBugEgg egg: 
                list = [egg.eggColors[1], egg.eggColors[1]]; break;
            case Centipede centi: 
                list = [(centi.graphicsModule as CentipedeGraphics)!.ShellColor,(centi.graphicsModule as CentipedeGraphics)!.ShellColor]; break;
            case JellyFish fish: 
                list = [fish.color, fish.color]; break;
            case OracleSwarmer: 
                list = [Extensions.RandomHSL(0,1,0.5f).HSL2RGB(), Extensions.RandomHSL(0,1,0.5f).HSL2RGB()]; break;
            case GlowWeed weed: 
                list = [weed.color, weed.color]; break;
            case FireEgg fire: 
                list = [fire.eggColors[1],fire.eggColors[1]]; break;
            case SlimeMold mold: 
                if (mold.abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.Seed)
                { list = [new Color(0.9f,0.83f,0.5f), new Color(0.9f,0.83f,0.5f)]; break; }
                else
                { list = [mold.color, mold.color]; break; }
            case LillyPuck puck: 
                list = [puck.flowerColor, puck.flowerColor]; break;
            
            case SporePlant:
                list = [ new Color(0.4f, 0, 0), new Color(0.4f, 0, 0)]; break;
            case OverseerCarcass eye: 
                list = [eye.color, eye.color]; break;
            case FirecrackerPlant or LittleCracker: 
                list = [Color.red, Color.red]; break;
            case FlyLure lure: 
                list = [lure.color, lure.color]; break;
            case PuffBall puff: 
                list = [ puff.sporeColor, puff.sporeColor]; break;
            
            // Special colors
            case DataPearl pearl: 
                list = [pearl.color, (pearl.highlightColor.HasValue) ? pearl.highlightColor.Value : pearl.color]; break;
            case FlareBomb: 
                list = [ new Color(0.98f,0.98f,1f), Color.blue ]; break;
            case Lantern:
                Color col = new Color(1f, 0.2f, 0f);
                list = [ col.MakeLit(0.99f), col]; break;
            case Mushroom mush:
                list = [ new Vector3(mush.hue, 0.6f,0.9f).HSL2RGB(), new Vector3(mush.hue, 1f,0.01f).HSL2RGB()]; break;
        }
        if (!list.Any())
        {
            if (obj is PlayerCarryableItem item)
                list = [item.color, item.color]; // Attempted mod compatibility
            else
            {
                Debug.Log("Solace: Dye for scarf did not have an associated color, using magenta");
                list = [Color.magenta, Color.magenta];
            }
        }
        return list;
    }

    public static void SolaceScarfDyeSplat(PhysicalObject obj, List<Color> color)
    {
        bool watery = false;
        Color colorer = (Random.value > 0.5f) ? color[0] : color[1];
        
        if (obj is IPlayerEdible)
        {
            obj.room.PlaySound(SoundID.Swollen_Water_Nut_Terrain_Impact, obj.firstChunk.pos);
            watery = true;
        }
        else obj.room.PlaySound(SoundID.Snail_Warning_Click, obj.firstChunk.pos);
        
        for (int i = 0; i < 4; i++)
        {
            obj.room.AddObject(
                new Spark(
                    obj.bodyChunks[0].pos, 
                    Custom.RNV() * Mathf.Lerp(5f, 11f, Random.value),
                    colorer,null,
                    7,17));
            if (watery) obj.room.AddObject(
                new WaterDrip(
                    obj.firstChunk.pos, 
                    Custom.RNV() * Mathf.Lerp(5f, 11f, Random.value), 
                    waterColor: true));
        }
    }
}