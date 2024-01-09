using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TheFriend.WorldChanges;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.Creatures.FamineCreatures;

public static class FamineEggBug
{
    public static void Apply()
    {
        On.EggBug.ctor += EggBugOnctor;
        On.EggBugEgg.ctor += EggBugEggOnctor;
        On.EggBugGraphics.ctor += EggBugGraphicsOnctor;
        On.EggBugGraphics.EggColors += EggBugGraphicsOnEggColors;
        On.EggBugGraphics.InitiateSprites += EggBugGraphicsOnInitiateSprites;
        On.EggBugGraphics.DrawSprites += EggBugGraphicsOnDrawSprites;
        On.EggBugGraphics.ApplyPalette += EggBugGraphicsOnApplyPalette;
        IL.EggBugGraphics.ApplyPalette += EggBugGraphicsILApplyPalette;
    }

    #region EggBugCWT
    public static readonly ConditionalWeakTable<EggBug, EggBugData> EggBugDeets = new ConditionalWeakTable<EggBug, EggBugData>();
    public static EggBugData GetEggBugData(this EggBug eggBug) => EggBugDeets.GetValue(eggBug, _ => new(eggBug));
    public class EggBugData
    {
        public bool naturalSickness => !FamineWorld.FamineBurdenBool && FriendWorldState.SolaceWorldstate;
        public int TotalSprites;
        public int[] SpriteIndex;
        public HashSet<(int, int)> EggsToRemove;
        public EggBugData(EggBug eggBug)
        {
            SpriteIndex = new int[TotalNewSprites];
            EggsToRemove = new HashSet<(int, int)>();
        }
    }
    #endregion

    private static void EggBugOnctor(On.EggBug.orig_ctor orig, EggBug self, AbstractCreature abstractcreature, World world)
    {
        orig(self, abstractcreature, world);
        if (!FamineWorld.FamineBool && !FamineWorld.FamineBurdenBool) return;
        self.hue += (self.GetEggBugData().naturalSickness) ? Random.Range(0.05f, 0.25f) : Random.Range(-0.05f, -0.3f);
    }

    private static void EggBugGraphicsOnctor(On.EggBugGraphics.orig_ctor orig, EggBugGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (!FamineWorld.FamineBool && !FamineWorld.FamineBurdenBool) return;

        var eggBugData = self.bug.GetEggBugData();
        var removeHowMany = Random.Range(0, 6); // 0-5
        for (var i = 0; i < removeHowMany; i++)
        {
            var index1 = Random.Range(0, self.eggs.GetLength(0));
            var index2 = Random.Range(0, self.eggs.GetLength(1));
            eggBugData.EggsToRemove.Add((index1, index2));
        }
    }

    private static void EggBugEggOnctor(On.EggBugEgg.orig_ctor orig, EggBugEgg self, AbstractPhysicalObject abstractphysicalobject)
    {
        orig(self, abstractphysicalobject);
        if (!FamineWorld.FamineBool && !FamineWorld.FamineBurdenBool) return;

        var abstractBug = self.abstractBugEgg.Room.entities.FirstOrDefault(x => x is AbstractCreature crit && crit.realizedCreature is EggBug bug && bug.hue == self.abstractBugEgg.hue);
        if (abstractBug == null) return;
        var eggBugData = ((EggBug)((AbstractCreature)abstractBug).realizedCreature).GetEggBugData();
        if (eggBugData.EggsToRemove.Any())
        {
            eggBugData.EggsToRemove.Remove(eggBugData.EggsToRemove.Last());
            self.Destroy();
        }
    }

    private const int TotalNewSprites = 4;
    private static void EggBugGraphicsOnInitiateSprites(On.EggBugGraphics.orig_InitiateSprites orig, EggBugGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam)
    {
        orig(self, sleaser, rcam);
        if (!FamineWorld.FamineBool && !FamineWorld.FamineBurdenBool) return;

        if (!self.bug.FireBug)
        {
            sleaser.sprites[self.EyeSprite(0)] = new FSprite("Symbol_Pearl");
            sleaser.sprites[self.EyeSprite(1)] = new FSprite("Symbol_Pearl");
            sleaser.sprites[self.EyeSprite(0)].scale = 0.65f;
            sleaser.sprites[self.EyeSprite(1)].scale = 0.65f;
        }

        var eggBugData = self.bug.GetEggBugData();
        Array.Resize(ref sleaser.sprites, sleaser.sprites.Length + TotalNewSprites);
        eggBugData.TotalSprites = sleaser.sprites.Length;

        for (var i = 0; i < TotalNewSprites; i++)
        {
            var index = eggBugData.TotalSprites - TotalNewSprites + i;
            eggBugData.SpriteIndex[i] = index;
            sleaser.sprites[index] = new FSprite("CentipedeLegB_Fade");
        }

        self.AddToContainer(sleaser, rcam, null);
    }

    private static void EggBugGraphicsOnDrawSprites(On.EggBugGraphics.orig_DrawSprites orig, EggBugGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        if (!FamineWorld.FamineBool && !FamineWorld.FamineBurdenBool) return;

        var eggBugData = self.bug.GetEggBugData();

        var iteration = 0;
        for (var i = 0; i < 2; i++)
        {
            for (var j = 0; j < 2; j++)
            {
                sleaser.sprites[eggBugData.SpriteIndex[iteration]].x = sleaser.sprites[self.LegSprite(i, j, 1)].x;
                sleaser.sprites[eggBugData.SpriteIndex[iteration]].y = sleaser.sprites[self.LegSprite(i, j, 1)].y;
                sleaser.sprites[eggBugData.SpriteIndex[iteration]].anchorX = sleaser.sprites[self.LegSprite(i, j, 1)].anchorX;
                sleaser.sprites[eggBugData.SpriteIndex[iteration]].anchorY = sleaser.sprites[self.LegSprite(i, j, 1)].anchorY;
                sleaser.sprites[eggBugData.SpriteIndex[iteration]].scaleX = sleaser.sprites[self.LegSprite(i, j, 1)].scaleX;
                sleaser.sprites[eggBugData.SpriteIndex[iteration]].scaleY = sleaser.sprites[self.LegSprite(i, j, 1)].scaleY;
                sleaser.sprites[eggBugData.SpriteIndex[iteration]].rotation = sleaser.sprites[self.LegSprite(i, j, 1)].rotation;
                iteration++;
            }
        }

        foreach (var eggIndex in eggBugData.EggsToRemove)
        {
            for (var i = 0; i < 3; i++)
            {
                sleaser.sprites[self.BackEggSprite(eggIndex.Item1, eggIndex.Item2, i)].isVisible = false;
                sleaser.sprites[self.FrontEggSprite(eggIndex.Item1, eggIndex.Item2, i)].isVisible = false;
            }
        }
    }

    private static void EggBugGraphicsOnApplyPalette(On.EggBugGraphics.orig_ApplyPalette orig, EggBugGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, RoomPalette palette)
    {
        orig(self, sleaser, rcam, palette);
        if (!FamineWorld.FamineBool && !FamineWorld.FamineBurdenBool) return;

        var eggBugData = self.bug.GetEggBugData();
        if (!eggBugData.naturalSickness) return;

        self.antennaTipColor = Color.Lerp(self.blackColor, Color.white, Mathf.Pow(1f - self.darkness, 0.2f));
        for (var i = 0; i < 2; i++)
        {
            var colors = ((TriangleMesh)sleaser.sprites[self.AntennaSprite(i)]).verticeColors;
            for (var num = 0; num < colors.Length; num++)
            {
                colors[num] = Color.Lerp(self.blackColor, self.antennaTipColor, Mathf.Pow(num / (colors.Length - 1f), 1.85f));
            }
        }

        for (var i = 0; i < TotalNewSprites; i++)
        {
            sleaser.sprites[eggBugData.SpriteIndex[i]].color = self.antennaTipColor;
        }
    }

    private static void EggBugGraphicsILApplyPalette(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                i => i.MatchCallOrCallvirt<Color>(nameof(Color.Lerp)),
                i => i.MatchStfld<EggBugGraphics>(nameof(EggBugGraphics.blackColor))
            );

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((EggBugGraphics self) =>
            {
                if (!FamineWorld.FamineBool && !FamineWorld.FamineBurdenBool) return;
                self.blackColor.ChangeHue(self.blackColor.Hue() + 0.6f);
            });
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError("ILHook failed - EggBugGraphicsILApplyPallete");
            Plugin.LogSource.LogError(ex);
        }
    }

    private static Color[] EggBugGraphicsOnEggColors(On.EggBugGraphics.orig_EggColors orig, RoomPalette palette, float hue, float darkness)
    {
        var color = orig(palette, hue, darkness);
        if (!FamineWorld.FamineBool && !FamineWorld.FamineBurdenBool) return color;
        // color[0] = Color.white; //Base they connect to
        color[1] = Color.Lerp(color[1], Color.white, 0.40f); //Egg color
        color[2] = Color.Lerp(color[2], Color.white, 0.85f); //The inner dot color
        return color;
    }

}