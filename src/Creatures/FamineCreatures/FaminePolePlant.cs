using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TheFriend.WorldChanges;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.Creatures.FamineCreatures;

public abstract class FaminePolePlant
{
    public static void Apply()
    {
        On.PoleMimic.ctor += PoleMimicOnctor;
        On.PoleMimicGraphics.DrawSprites += PoleMimicGraphicsOnDrawSprites;
        IL.PoleMimicGraphics.InitiateSprites += PoleMimicGraphicsILInitiateSprites;
    }

    private const float SpawnChance = 0.85f;
    private static void PoleMimicOnctor(On.PoleMimic.orig_ctor orig, PoleMimic self, AbstractCreature abstractcreature, World world)
    {
        orig(self, abstractcreature, world);
        if (!FamineWorld.FamineBool) return;
        if (Random.value > SpawnChance) self.Destroy();
    }

    private static void PoleMimicGraphicsOnDrawSprites(On.PoleMimicGraphics.orig_DrawSprites orig, PoleMimicGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        if (!FamineWorld.FamineBool) return;
        for (var i = 0; i < self.leafPairs; i++)
        {
            for (var j = 0; j < 2; j++)
            {
                if (i >= self.decoratedLeafPairs) continue;
                sleaser.sprites[self.LeafDecorationSprite(i, j)].color = Color.Lerp(sleaser.sprites[self.LeafDecorationSprite(i, j)].color, Color.cyan, Mathf.Pow((float)i / (self.leafPairs - 1), 0.425f));
                sleaser.sprites[self.LeafDecorationSprite(i, j)].color = Color.Lerp(sleaser.sprites[self.LeafDecorationSprite(i, j)].color, Color.white, Mathf.Pow((float)i / (self.leafPairs - 1), 2.5f));
                sleaser.sprites[self.LeafDecorationSprite(i, j)].alpha = Mathf.Lerp(sleaser.sprites[self.LeafDecorationSprite(i, j)].alpha, 1, Mathf.Pow((float)i / (self.leafPairs - 1), 2.5f));
            }
        }
    }

    private static void PoleMimicGraphicsILInitiateSprites(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                i => i.MatchStfld<PoleMimicGraphics>(nameof(PoleMimicGraphics.decoratedLeafPairs))
            );
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((PoleMimicGraphics self) =>
            {
                if (!FamineWorld.FamineBool) return;
                self.decoratedLeafPairs = self.leafPairs;
            });
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError("ILHook failed - PoleMimicGraphicsILInitiateSprites");
            Plugin.LogSource.LogError(ex);
        }
    }
}