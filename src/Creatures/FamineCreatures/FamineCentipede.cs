using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using SlugBase;
using TheFriend.WorldChanges;
using TheFriend.WorldChanges.WorldStates.General;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.Creatures.FamineCreatures;

public abstract class FamineCentipede
{
     // Diseased Centipede
    public static Color Centipede_ShortCutColor(On.Centipede.orig_ShortCutColor orig, Centipede self)
    {
        if (self is not null && 
            self.TryGet(out var data))
        {
            if (self.Red) return Custom.HSL2RGB(data.sickHue, 0f, 0.5f);
            else return Custom.HSL2RGB(data.sickHue, data.sickSat, 0.5f);
        }
        else return orig(self);
    }
    public static void CentipedeGraphics_InitiateSprites(On.CentipedeGraphics.orig_InitiateSprites orig, CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.centipede != null && self.centipede.TryGet(out _) && !self.centipede.Red)
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
        if (self.centipede == null) return;
        if (self.centipede.TryGet(out _))
            for (var i = 0; i < self.owner?.bodyChunks?.Count(); i++)
                sLeaser.sprites[self.SegmentSprite(i)].element =
                    Futile.atlasManager.GetElementWithName("LizardHead3.0");
        else return;

        if (!self.centipede.Red) return; // Iridescipede
        var drawPos = self.centipede.bodyChunks.Select(chunk => Vector2.Lerp(chunk.lastPos, chunk.pos, timeStacker)).ToList();
        var minX = drawPos.Min(pos => pos.x);
        var maxX = drawPos.Max(pos => pos.x);

        for (var i = 0; i < self.centipede.bodyChunks.Length; i++)
        {
            for (var j = 0; j < 2; j++)
            {
                var sprite = sLeaser.sprites[self.ShellSprite(i, j)];
                var col = sprite.color;
                var hue = Mathf.InverseLerp(minX + 0.01f, maxX - 0.01f, sprite.x) * 2f;
                if (hue > 1f) hue -= 1f;
                col.ChangeHue(hue);
                sprite.color = col;
            }
        }
    }
    public static void Centipede_ctor(On.Centipede.orig_ctor orig, Centipede self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (self.TryGet(out _))
        {
            for (int i = 0; i < self.bodyChunks?.Count(); i++)
            {
                self.bodyChunks[i].rad *= 0.6f;
                self.bodyChunks[i].mass *= 0.4f;
            }
        }
    }
    public static void Centipede_Violence(On.Centipede.orig_Violence orig, Centipede self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        if (self.TryGet(out _) && !self.Red)
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
        if (self.centipede.TryGet(out var data))
        {
            self.hue = data.sickHue;
            self.saturation = data.sickSat;
        }
    }

    // Diseased centipede food
    public static void Player_EatMeatUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);

            c.GotoNext(i => i.MatchIsinst<Centipede>()); // Find a unique thing closest to where you want to be
            c.GotoNext(i => i.MatchLdsfld<SoundID>("Slugcat_Eat_Meat_B")); // Looking for jump destination
            c.GotoPrev(i => i.MatchLdarg(0)); // THE jump destination
            ILLabel label = il.DefineLabel(c.Next); // This is where the jump destination is! New code location will be right here
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
            !(FamineWorld.FamineBool || FamineWorld.FamineBurdenBool))
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

public static class CentiCWT
{
    public class CentiData
    {
        public bool naturalSickness => !FamineWorld.FamineBurdenBool && QuickWorldData.SolaceCampaign;
        public float sickHue;
        public float sickSat;
        public CentiData(Centipede self)
        {
            var random = Random.Range(-1,1) * 0.05f;
            switch (self.Template.type.value)
            {
                case nameof(CreatureTemplate.Type.RedCentipede): 
                    sickHue = Random.value;
                    sickSat = (naturalSickness) ? 0 : 0.15f;
                    break;
                case nameof(CreatureTemplate.Type.Centiwing):
                    sickHue = (naturalSickness) ? 
                        0.68f + random : 0.5f + random;
                    sickSat = (naturalSickness) ? 0.5f : 0.3f;
                    break;
                case nameof(CreatureTemplate.Type.SmallCentipede) or 
                    nameof(CreatureTemplate.Type.Centipede):
                    sickHue = (naturalSickness) ? 
                        0.65f + random : 0.38f + random;
                    sickSat = (naturalSickness) ? 0.5f : 0.3f;
                    break;
            }

        }
    }
    public static readonly ConditionalWeakTable<Centipede, CentiData> CWT = new();
    public static CentiData GetData(this Centipede centi) => CWT.GetValue(centi, _ => new(centi));

    public static bool TryGet(this Centipede self, out CentiData data)
    {
        if ((FamineWorld.FamineBool || 
            FamineWorld.FamineBurdenBool) && 
            !self.abstractCreature.IsVoided() &&
            !self.AquaCenti)
        {
            data = self.GetData();
            return true;
        }
        data = null;
        return false;
    }

}