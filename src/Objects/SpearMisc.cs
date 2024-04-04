using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TheFriend.PoacherThings;

namespace TheFriend.Objects;

public static class SpearMisc
{
    internal static void SpearILUpdate(ILContext il) //Improves slow-moving spears sticking in walls (undo an MMF line)
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
                if (self.thrownBy is Player pl && pl.SlugCatClass == Plugin.NoirName) return true;

                if (self.abstractSpear.GetSpearData().AttachedLantern != null) return true;

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
}