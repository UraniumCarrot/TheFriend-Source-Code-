using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;

namespace TheFriend.CharacterThings.NoirThings;

public partial class NoirCatto
{
    //Spear not embed into wall fix
    private static void SpearILUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(MoveType.Before,
                i => i.MatchLdsfld<ModManager>("MMF"),
                i => i.MatchBrfalse(out _),
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt<PhysicalObject>("get_firstChunk"),
                i => i.MatchLdflda<BodyChunk>("vel"),
                i => i.MatchCallOrCallvirt<UnityEngine.Vector2>("get_magnitude"),
                i => i.MatchLdcR4(out _)
            );
            c.GotoNext(MoveType.After, i => i.MatchBrfalse(out label));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Spear self) =>
            {
                if (self.thrownBy is Player pl && pl.SlugCatClass == Plugin.NoirName && self.alwaysStickInWalls) //MMF? more like SMH
                {
                    return true;
                }
                return false;
            });
            c.Emit(OpCodes.Brtrue, label);
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError("ILHook failed - Spear Fix");
            Plugin.LogSource.LogError(ex);
        }
    }

    #region Ghosts
    private static void GhostWorldPresenceILSpawnGhost(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            var skip = il.DefineLabel();
            c.GotoNext(MoveType.After,
                i => i.MatchLdarg(2),
                i => i.MatchLdcI4(4),
                i => i.MatchBeq(out _)
            );

            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldarg_2);
            c.EmitDelegate((int karma, int karmaCap) =>
            {
                if (Custom.rainWorld.progression.currentSaveState.saveStateNumber == Plugin.NoirName)
                {
                    if (karma >= karmaCap)
                    {
                        return true;
                    }
                }
                return false;
            });
            c.Emit(OpCodes.Brfalse, skip);
            c.Emit(OpCodes.Ldc_I4_1);
            c.Emit(OpCodes.Ret);
            c.MarkLabel(skip);

        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError("ILHook failed - Spawn Ghost");
            Plugin.LogSource.LogError(ex);
        }
    }

    private static void KarmaLadderScreenILGetDataFromGame(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(MoveType.After,
                i => i.MatchBr(out label),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<Menu.KarmaLadderScreen>("saveState"),
                i => i.MatchLdfld<SaveState>("saveStateNumber"),
                i => i.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName>("Saint"),
                i => i.MatchCallOrCallvirt(out _)
            );
            c.GotoPrev(MoveType.After, i => i.MatchLdarg(0));
            c.EmitDelegate((Menu.KarmaLadderScreen self) =>
            {
                if (self.saveState.saveStateNumber == Plugin.NoirName) return true;
                return false;
            });
            c.Emit(OpCodes.Brtrue, label);
            c.Emit(OpCodes.Ldarg_0);
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError("ILHook failed - Karma Ladder");
            Plugin.LogSource.LogError(ex);
        }
    }
    #endregion
}