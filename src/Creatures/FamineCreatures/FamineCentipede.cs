using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using SlugBase;
using TheFriend.WorldChanges;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.Creatures.FamineCreatures;

public abstract class FamineCentipede
{
    public static void Apply()
    {
        On.CentipedeGraphics.ctor += CentipedeGraphics_ctor;
        On.CentipedeGraphics.InitiateSprites += CentipedeGraphics_InitiateSprites;
        On.CentipedeGraphics.DrawSprites += CentipedeGraphics_DrawSprites;
        On.Centipede.Violence += Centipede_Violence;
        On.Centipede.ShortCutColor += Centipede_ShortCutColor;
        On.Centipede.ctor += Centipede_ctor;
        IL.Player.EatMeatUpdate += Player_EatMeatUpdate;
    }

     // Diseased Centipede
    public static float defCentiColor = 0.6f;
    public static float defCentiSat = 0.5f;
    public static Color Centipede_ShortCutColor(On.Centipede.orig_ShortCutColor orig, Centipede self)
    {
        if (!FamineWorld.FamineBool) return orig(self);
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
        if (!self.centipede.AquaCenti && FamineWorld.FamineBool)
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
        if (!self.centipede.AquaCenti && FamineWorld.FamineBool)
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
        if (!self.Red && !self.AquaCenti && !abstractCreature.IsVoided() && FamineWorld.FamineBool)
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
        if (FamineWorld.FamineBool && !(self.AquaCenti || self.Red))
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
        if (FamineWorld.FamineBool && !self.centipede.abstractCreature.IsVoided())
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
            !FamineWorld.FamineBool)
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

    //From FamineWorld.cs
    public static void NourishmentOfCentiEaten(SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject, ref int num)
    {
        if (eatenobject is Centipede centi && centi.Small)
        {
            if (slugcatIndex == SlugcatStats.Name.Red || slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Artificer) num = 1;
            else num = centi.FoodPoints;
        }
    }
}