using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using System.Globalization;
using SlugBase;
using SlugBase.Assets;
using SlugBase.DataTypes;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Solace.WorldChanges;
public class FamineWorld
{
    public static void Apply()
    {
        On.SlugcatStats.NourishmentOfObjectEaten += SlugcatStats_NourishmentOfObjectEaten;
        On.Player.ObjectEaten += Player_ObjectEaten;
        IL.Player.EatMeatUpdate += Player_EatMeatUpdate;

        On.CentipedeGraphics.ctor += CentipedeGraphics_ctor;
        On.CentipedeGraphics.InitiateSprites += CentipedeGraphics_InitiateSprites;
        On.CentipedeGraphics.DrawSprites += CentipedeGraphics_DrawSprites;
        On.Centipede.Violence += Centipede_Violence;
        On.Centipede.ShortCutColor += Centipede_ShortCutColor;
        On.Centipede.ctor += Centipede_ctor;

        On.DangleFruit.ApplyPalette += DangleFruit_ApplyPalette;
        On.DangleFruit.DrawSprites += DangleFruit_DrawSprites;
        On.MoreSlugcats.LillyPuck.ApplyPalette += LillyPuck_ApplyPalette;
        On.MoreSlugcats.LillyPuck.Update += LillyPuck_Update;
        On.MoreSlugcats.GooieDuck.ApplyPalette += GooieDuck_ApplyPalette;
        On.MoreSlugcats.DandelionPeach.ctor += DandelionPeach_ctor;
        On.MoreSlugcats.DandelionPeach.ApplyPalette += DandelionPeach_ApplyPalette;

        On.Fly.ctor += Fly_ctor;
    }

    public static int favoriteFoodTimer = 0;
    public static int sleepCounter = 0;
    public static void PoacherEats(Player self)
    {
        if (self.slugcatStats.name != Solace.DragonName) return;
        if (sleepCounter < 300 && self.bodyMode == Player.BodyModeIndex.Crawl && !self.input[0].AnyInput) sleepCounter++;
        if (sleepCounter >= 300) (self.graphicsModule as PlayerGraphics).blink = 5;
        if (self.bodyMode != Player.BodyModeIndex.Crawl || self.input[0].AnyInput) sleepCounter = 0;
        if (favoriteFoodTimer == 0) return;
        if (Solace.PoacherPupActs() == true)
        {
            if (favoriteFoodTimer > 0 && !self.Stunned && !self.Malnourished)
            {
                favoriteFoodTimer--;
                self.slugcatStats.runspeedFac = 1.5f;
                self.slugcatStats.poleClimbSpeedFac = 1.3f;
                self.dynamicRunSpeed[0] *= 2f;
                self.dynamicRunSpeed[1] *= 2f;
                if (self.bodyMode == Player.BodyModeIndex.Stand) self.input[0].jmp = Random.value < 0.1;
                if (self.bodyMode == Player.BodyModeIndex.Crawl) self.jumpBoost *= 1.2f;
                else self.jumpBoost *= 1.05f;
            }
            if (favoriteFoodTimer < 0 && !self.Stunned && !self.Malnourished)
            {
                favoriteFoodTimer++;
                self.exhausted = true;
                (self.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * 0.2f;
                self.slugcatStats.runspeedFac = 0.7f;
                self.slugcatStats.poleClimbSpeedFac = 0.7f;
                if (favoriteFoodTimer < -500) self.Die();
            }
            if (self.dead) { favoriteFoodTimer = 0; }
            if (favoriteFoodTimer == 0 && !self.Stunned && !self.Malnourished) { self.slugcatStats.runspeedFac = 1f; self.slugcatStats.poleClimbSpeedFac = 1f; }
        }
    }

    public static void Player_ObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible eatenobject)
    {
        orig(self,eatenobject);

        if (self.slugcatStats.name != Solace.DragonName) return;
        if (eatenobject is GlowWeed) { favoriteFoodTimer = 100; Debug.Log("Poacher loves it!"); }
        if (eatenobject is Hazer) { favoriteFoodTimer = 50; }
        if (eatenobject is DangleFruit fruit && !IsDiseasedPipHandler(fruit)) { favoriteFoodTimer = 50; }
        if (eatenobject is LillyPuck) { favoriteFoodTimer = -50; }
        if (eatenobject is JellyFish) { favoriteFoodTimer = -100; }
        if (eatenobject is FireEgg) { favoriteFoodTimer = -600; Debug.Log("Poacher hated this food so much they died! Just kidding, it was full of super acid."); }
    }

    public static bool NoFamine() // Famine mechanic override, if the player wants it
    {
        if (Solace.NoFamine() == true) return true;
        else return false;
    }

    public static bool HasFamines(RainWorldGame self)
    {
        if ((self.StoryCharacter == Solace.FriendName || 
            self.StoryCharacter == Solace.DragonName || 
            (Solace.FaminesForAll() && !self.rainWorld.ExpeditionMode) || 
            (self.rainWorld.ExpeditionMode && Solace.ExpeditionFamine())) 
            && !NoFamine() && !self.IsArenaSession)
        {
            FamineBool = true;
            return true;
        }
        else 
        {
            FamineBool = false;
            return false;
        }
    } // Helps majority of the code here tell slugcat has famines
    public static bool FamineBool; // Global bool used to tell if the world has Solace famines, has no requirements unlike above

    public static bool IsDiseased(AbstractConsumable c) // General disease bool handler
    {
        if (!FamineBool) return false;
        if (c.world.region.name == "UG") return false;
        var oldState = Random.state;
        try
        {
            var newState = c.ID.RandomSeed; //c.placedObjectIndex != -1 ? ((c.world.GetAbstractRoom(c.originRoom)?.name.GetHashCode() ?? 0) ^ c.placedObjectIndex) : c.ID.number;
            Random.InitState(newState);
            if (c.type == AbstractPhysicalObject.AbstractObjectType.DangleFruit) return Random.value > 0.2;
            if (c.type == MoreSlugcatsEnums.AbstractObjectType.LillyPuck) return Random.value > 0.05;
            if (c.type == MoreSlugcatsEnums.AbstractObjectType.GooieDuck) return Random.value > 0.9;
            if (c.type == MoreSlugcatsEnums.AbstractObjectType.DandelionPeach) return Random.value > 0.4;
            else return false;
        }
        finally
        {
            Random.state = oldState;
        }
    }
    public static bool IsDiseasedConverter(AbstractPhysicalObject obj) // Needed for food value
    {
        if (obj is AbstractConsumable edible && IsDiseased(edible))
        {
            return true;
        }
        return false;
    }
    public static bool IsDiseasedPipHandler(PhysicalObject obj) // Needed for food value
    {
        var edible = obj.abstractPhysicalObject;
        if (IsDiseasedConverter(edible))
        {
            return true;
        }
        return false;
    }

    // Diseased food value
    public static int SlugcatStats_NourishmentOfObjectEaten(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject)
    {
        int num = orig(slugcatIndex, eatenobject);
        var quarters1 = eatenobject.FoodPoints;
        if (eatenobject is GlowWeed && slugcatIndex == Solace.DragonName) num = 4;
        if (!FamineBool) return num;
        if (eatenobject is PhysicalObject obj && IsDiseasedPipHandler(obj))
        {
            if (slugcatIndex == SlugcatStats.Name.Red || slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Artificer || num == 0)
            {
                num = 0;
            }
            else if (Random.value > 0.50)
            {
                if (quarters1 >= 2)
                {
                    num = quarters1 / 2;
                }
                if (quarters1 < 2)
                {
                    num = 0;
                }
            }
            else num = quarters1;
        }
        if (eatenobject is Centipede centi && centi.Small && FamineBool) 
        {
            if (slugcatIndex == SlugcatStats.Name.Red || slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Artificer) num = 1;
            else num = centi.FoodPoints; 
        }
        return num;
    }
    // Diseased centipede food
    public static void Player_EatMeatUpdate(MonoMod.Cil.ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;

            c.GotoNext(i => i.MatchIsinst<Centipede>()); // Find a unique thing closest to where you want to be
            c.GotoNext(i => i.MatchLdsfld<SoundID>("Slugcat_Eat_Meat_B")); // Looking for jump destination
            c.GotoPrev(i => i.MatchLdarg(0)); // THE jump destination
            label = il.DefineLabel(c.Next); // This is where the jump destination is! New code location will be right here
            c.GotoPrev(MoveType.Before, i => i.MatchLdsfld<ModManager>("MSC")); // The jump beginning, finds match nearest to the unique thing
            // With both the jump beginning and destination set, the code between is selected
            c.Emit(OpCodes.Ldarg_0); // Hook's argument 1 (Player, for this)
            c.Emit(OpCodes.Ldarg_1); // Hook's argument 2 (int Grasp, for this)
            c.EmitDelegate(FamCenti); // You will do this method now, returns bool
            c.Emit(OpCodes.Brtrue, label); // Cancels selected code if above is true (the magic)
        }
        catch (Exception e) { Debug.Log("Solace: IL hook EatMeatUpdate failed!" + e); }
    }
    public static bool FamCenti(Player pl, int grasp)
    {
        var plGrasp = pl.grasps[grasp].grabbed as Creature;
        if (plGrasp is not Centipede)
            return false;
        if (plGrasp.Template.type == CreatureTemplate.Type.RedCentipede ||
            plGrasp.Template.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti ||
            !FamineBool)
            return false;

        if ((SlugBaseCharacter.TryGet(pl.SlugCatClass, out var chara) &&
            SlugBase.Features.PlayerFeatures.Diet.TryGet(chara, out var diet) && diet.GetMeatMultiplier(pl, plGrasp) < 1) ||
            (pl.SlugCatClass == SlugcatStats.Name.Red))
        {
            if (SlugBase.Features.PlayerFeatures.Diet.TryGet(chara, out var diet0) &&
                diet0.GetMeatMultiplier(pl, plGrasp) < 0.5f)
            {
                if (Random.value > 0.75f) pl.AddQuarterFood();
            }
            else if (Random.value > 0.5f) pl.AddQuarterFood();
        }
        else
        {
            pl.AddQuarterFood();
        }

        return true;
    }
    // Diseased DangleFruit
    public static void DangleFruit_ApplyPalette(On.DangleFruit.orig_ApplyPalette orig, DangleFruit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) // Makes it brown
    {
        orig(self, sLeaser, rCam, palette);
        if (self.AbstrConsumable is AbstractConsumable obj && IsDiseased(obj))
        {
            self.color = Color.Lerp(new Color(0.2f, 0.16f, 0.1f), palette.blackColor, palette.darkness);
        }
    }
    public static void DangleFruit_DrawSprites(On.DangleFruit.orig_DrawSprites orig, DangleFruit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) // Makes it small
    {
        if (self.AbstrConsumable is AbstractConsumable obj && IsDiseased(obj))
        {
            sLeaser.sprites[0].scaleX = 0.8f;
            sLeaser.sprites[1].scaleX = 0.8f;
        }
        orig(self,sLeaser,rCam,timeStacker,camPos);
    }

    // Diseased LilyPuck
    public static void LillyPuck_ApplyPalette(On.MoreSlugcats.LillyPuck.orig_ApplyPalette orig, LillyPuck self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.AbstrConsumable is AbstractConsumable obj && IsDiseased(obj))
        {
            self.color = Color.Lerp(new Color(0.3f, 0.24f, 0.18f), palette.blackColor, palette.darkness);
            sLeaser.sprites[0].color = self.color;
            for (int i = 0; i < self.flowerLeavesCount; i++)
            {
                Color a = Color.Lerp(self.color, self.flowerColor, Mathf.Clamp(self.lightFade, 0.3f - 0.3f * palette.darkness, 1f));
                sLeaser.sprites[1 + i * 2].color = Color.Lerp(a, self.color, i / (self.flowerLeavesCount / 2f));
                sLeaser.sprites[2 + i * 2].color = Color.Lerp(sLeaser.sprites[1 + i * 2].color, Color.Lerp(new Color(1f, 1f, 1f), palette.blackColor, palette.darkness), palette.darkness / 20f);
            }
        }
    }
    public static void LillyPuck_Update(On.MoreSlugcats.LillyPuck.orig_Update orig, LillyPuck self, bool eu)
    {
        orig(self,eu);
        if (self.AbstrConsumable is AbstractConsumable obj && IsDiseased(obj))
        {
            self.light.rad = 0f;
            self.lightFade = 0f;
            self.light.stayAlive= false;
        }
    }

    // Diseased GooieDuck
    public static void GooieDuck_ApplyPalette(On.MoreSlugcats.GooieDuck.orig_ApplyPalette orig, GooieDuck self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.AbstrConsumable is AbstractConsumable obj && IsDiseased(obj))
        {
            self.CoreColor = Color.Lerp(new Color(0.2f, 0.21f, 0.32f), palette.blackColor, palette.darkness/ 3f);
            self.HuskColor = Color.Lerp(new Color(0.2f, 0.16f, 0.1f), palette.blackColor, palette.darkness);
        }
    }

    // Diseased DandelionPeach
    public static void DandelionPeach_ctor(On.MoreSlugcats.DandelionPeach.orig_ctor orig, DandelionPeach self, AbstractPhysicalObject abstractPhysicalObject)
    {
        orig(self, abstractPhysicalObject);
        if (self.AbstrConsumable is AbstractConsumable obj && IsDiseased(obj))
        {
            self.airFriction = 0.999f;
            self.gravity = 0.9f;
        }
    }
    public static void DandelionPeach_ApplyPalette(On.MoreSlugcats.DandelionPeach.orig_ApplyPalette orig, DandelionPeach self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.AbstrConsumable is AbstractConsumable obj && IsDiseased(obj))
        {
            self.color = Color.Lerp(new Color(0.87f, 0.85f, 0.75f), palette.blackColor, Mathf.Pow(palette.darkness,2f));
            sLeaser.sprites[1].color = Color.Lerp(Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), 0.5f), palette.blackColor, palette.darkness);
            sLeaser.sprites[2].color = Color.Lerp(self.color, sLeaser.sprites[1].color, 0.3f);
            self.puffCount = 0;
        }
    }

    // Diseased Batfly
    public static void Fly_ctor(On.Fly.orig_ctor orig, Fly self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (FamineBool && (world.region.name != "UG" || world.region.name != "SB"))
        {
            self.Destroy();
        }
    }
    // Diseased Centipede
    public static float defCentiColor = 0.6f;
    public static float defCentiSat = 0.5f;
    public static Color Centipede_ShortCutColor(On.Centipede.orig_ShortCutColor orig, Centipede self)
    {
        if (!FamineBool) return orig(self);
        if (self is not null && !self.abstractCreature.IsVoided() && !self.AquaCenti)
        {
            if (self.Red) return Custom.HSL2RGB(0.68f, 0, 0.5f);
            else if (self.Centiwing) return Custom.HSL2RGB(0.68f, defCentiSat, 0.5f);
            else return Custom.HSL2RGB(defCentiColor, defCentiSat, 0.5f);
        }
        else return orig(self);
    }
    public static void CentipedeGraphics_InitiateSprites(On.CentipedeGraphics.orig_InitiateSprites orig, CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (!self.centipede.AquaCenti && FamineBool)
        {
            for (int i = 0; i < self.owner.bodyChunks.Count(); i++)
            {
                sLeaser.sprites[self.SegmentSprite(i)].scaleY *= 0.8f;
                sLeaser.sprites[self.SegmentSprite(i)].scaleX *= 1.5f;
            }
        }
    }
    public static void CentipedeGraphics_DrawSprites(On.CentipedeGraphics.orig_DrawSprites orig, CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!self.centipede.AquaCenti && FamineBool)
        {
            for (int i = 0; i < self?.owner?.bodyChunks?.Count(); i++)
            {
                sLeaser.sprites[self.SegmentSprite(i)].element = Futile.atlasManager.GetElementWithName("LizardHead3.0");
            }
        }
    }
    public static void Centipede_ctor(On.Centipede.orig_ctor orig, Centipede self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (!self.Red && !self.AquaCenti && !abstractCreature.IsVoided() && FamineBool)
        {
            for (int i = 0; i < self?.bodyChunks?.Count(); i++)
            {
                self.bodyChunks[i].rad *= 0.6f;
                self.bodyChunks[i].mass *= 0.4f;
            }
        }
    }
    public static void Centipede_Violence(On.Centipede.orig_Violence orig, Centipede self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        if (FamineBool && !(self.AquaCenti || self.Red))
        {
            if (type == Creature.DamageType.Bite) damage *= 2.6f;
            if (type == Creature.DamageType.Explosion) damage *= 8f;
            if (type == Creature.DamageType.Stab) damage *= 2.6f;
            if (type == Creature.DamageType.Blunt) damage *= 3f;
        }
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }
    public static void CentipedeGraphics_ctor(On.CentipedeGraphics.orig_ctor orig, CentipedeGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (FamineBool && !self.centipede.abstractCreature.IsVoided())
        {
            if (self.centipede.Red)
            {
                self.hue = 0.68f;
                self.saturation = 0f;
            }
            else if (self.centipede.Small && self.centipede.abstractCreature.superSizeMe)
            {
                self.hue = defCentiColor;
                self.saturation = 0.7f;
            }
            else if (self.centipede.Centiwing)
            {
                self.hue = 0.68f;
                self.saturation = defCentiSat;
            }
            else if (!self.centipede.AquaCenti)
            {
                self.hue = defCentiColor;
                self.saturation = 0.5f;
            }
        }
    }
}