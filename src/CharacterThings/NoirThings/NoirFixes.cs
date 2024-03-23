using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;

namespace TheFriend.CharacterThings.NoirThings;

public partial class NoirCatto
{
    //Spear not embed into wall fix
    
    /*Surrounding IL***************************************************************************************
    // if (ModManager.MMF && base.firstChunk.vel.magnitude < 10f && !alwaysStickInWalls)
    ---- First GotoNext Lands Here ----
	IL_0aa1: ldsfld bool ModManager::MMF
	IL_0aa6: brfalse.s IL_0aca

	IL_0aa8: ldarg.0
	IL_0aa9: call instance class BodyChunk PhysicalObject::get_firstChunk()
	IL_0aae: ldflda valuetype [UnityEngine.CoreModule]UnityEngine.Vector2 BodyChunk::vel
	IL_0ab3: call instance float32 [UnityEngine.CoreModule]UnityEngine.Vector2::get_magnitude()
	IL_0ab8: ldc.r4 10
	IL_0abd: bge.un.s IL_0aca

	IL_0abf: ldarg.0
	IL_0ac0: ldfld bool Spear::alwaysStickInWalls
	IL_0ac5: brtrue.s IL_0aca

	// flag = false;
	IL_0ac7: ldc.i4.0
	IL_0ac8: stloc.s 5

	// if (flag)
	IL_0aca: ldloc.s 5
	IL_0acc: brfalse.s IL_0b2a
	---- Second GotoNext Lands Here ----
    *******************************************************************************************************/
    internal static void SpearILUpdate(ILContext il)
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

    /*Surrounding IL****************************************************************************************
	// return false;
	IL_008c: ldc.i4.0
	IL_008d: ret

	// switch (karmaCap)
	IL_008e: ldarg.2
	IL_008f: ldc.i4.4
	IL_0090: beq.s IL_0098
	---- GotoNext Lands Here ----

	// (no C# code)
	IL_0092: ldarg.2
	IL_0093: ldc.i4.6
	IL_0094: beq.s IL_00a0
    *******************************************************************************************************/
    internal static void GhostWorldPresenceILSpawnGhost(ILContext il)
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
    
    /*Surrounding IL****************************************************************************************
    // preGhostEncounterKarmaCap = 0;
    IL_0117: ldarg.0
    IL_0118: ldc.i4.0
    IL_0119: stfld int32 Menu.KarmaLadderScreen::preGhostEncounterKarmaCap
    // else if (saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
    IL_011e: br IL_01cb

    IL_0123: ldarg.0
    ---- GotoPrev Lands Here ----
    IL_0124: ldfld class SaveState Menu.KarmaLadderScreen::saveState
    IL_0129: ldfld class SlugcatStats/Name SaveState::saveStateNumber
    IL_012e: ldsfld class SlugcatStats/Name MoreSlugcats.MoreSlugcatsEnums/SlugcatStatsName::Saint
    IL_0133: call bool class ExtEnum`1<class SlugcatStats/Name>::op_Equality(class ExtEnum`1<!0>, class ExtEnum`1<!0>)
    ---- GotoNext Lands Here ----
    IL_0138: brfalse IL_01cb
    *******************************************************************************************************/
    internal static void KarmaLadderScreenILGetDataFromGame(ILContext il)
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
                return self.saveState.saveStateNumber.IsSolaceName();
            });
            c.Emit(OpCodes.Brtrue, label);
            c.Emit(OpCodes.Ldarg_0); //todo improve ILHook's compatibility with other mods
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError("ILHook failed - Karma Ladder");
            Plugin.LogSource.LogError(ex);
        }
    }
    #endregion
}