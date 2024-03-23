using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace TheFriend.PoacherThings;

public class FirecrackerFix
{
    // Code mostly done by Slime_Cubed, i have no idea what a lot of it means
    static readonly ConditionalWeakTable<AbstractPhysicalObject, StrongBox<int>> _lumpsLeft = new();
    
#pragma warning disable CS0649
    static int lumps;
#pragma warning restore CS0649
    
    const string lumpsLeft = "SOLACE_LUMPSLEFT_";
    public static void SetLumpsLeft(AbstractPhysicalObject obj, int amount)
    {
        _lumpsLeft.GetValue(obj, _ => new(lumps)).Value = amount;
    }
    public static int GetLumpsLeft(AbstractPhysicalObject obj)
    {
        return _lumpsLeft.TryGetValue(obj, out var box) ? box.Value : lumps;
    }
    public static void FirecrackerPlant_Update(On.FirecrackerPlant.orig_Update orig, FirecrackerPlant self, bool eu)
    { // Sets the number of lumps the plant should have if it gets abstracted
        orig(self, eu);
        int amount = self.lumpsPopped.Count(i => i == false);
        if (GetLumpsLeft(self.abstractPhysicalObject) != amount) SetLumpsLeft(self.abstractPhysicalObject, amount);
    }
    
    /*Surrounding IL****************************************************************************************
    
    *******************************************************************************************************/
    public static void FirecrackerPlant_ctor(ILContext il)
    { // Forces the plant to have the lumps amount set by above if lumps isnt default
        var c = new ILCursor(il);
        if (!c.TryGotoNext(MoveType.After, x => x.MatchNewarr<FirecrackerPlant.Part>()) && 
             c.TryGotoNext(x => x.MatchNewarr<FirecrackerPlant.Part>()))
        {
            Plugin.LogSource.LogError($"ILHook failed for FirecrackerPlant_ctor");
            return;
        }
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((int defaultLumps, FirecrackerPlant self) => GetLumpsLeft(self.abstractPhysicalObject) > 0 ? GetLumpsLeft(self.abstractPhysicalObject) : defaultLumps);
    }

    public static string SaveState_SetCustomData_AbstractPhysicalObject_string(On.SaveState.orig_SetCustomData_AbstractPhysicalObject_string orig, AbstractPhysicalObject apo, string baseString)
    {
        if (_lumpsLeft.TryGetValue(apo, out var box))
        {
            return orig(apo, baseString) + $"<oA>{lumpsLeft}{box.Value}";
        }
        else return orig(apo, baseString);
    }
    public static AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
    {
        AbstractPhysicalObject obj = orig(world, objString);
        if (obj?.unrecognizedAttributes != null)
        {
            for (int i = 0; i < obj.unrecognizedAttributes.Length; i++)
            {
                string attr = obj.unrecognizedAttributes[i];
                if (attr.StartsWith(lumpsLeft))
                {
                    int.TryParse(attr.Substring(lumpsLeft.Length), out int num);
                    _lumpsLeft.Add(obj, new(num));

                    var list = new List<string>(obj.unrecognizedAttributes);
                    list.RemoveAt(i);
                    obj.unrecognizedAttributes = list.ToArray();
                    break;
                }
            }
        }
        return obj;
    }



    // fixes a bug, if this gets fixed in a rainworld update later then get rid of it
    public static string[] SaveUtils_PopulateUnrecognizedStringAttrs(On.SaveUtils.orig_PopulateUnrecognizedStringAttrs orig, string[] normalStringSplit, int fromIndex)
    {
        var list = new List<string>();
        if (fromIndex < normalStringSplit.Length)
        {
            for (int i = fromIndex; i < normalStringSplit.Length; i++)
            {
                if (normalStringSplit[i].Trim().Length != 0)
                {
                    list.Add(normalStringSplit[i]);
                }
            }
        }
        if (list.Count == 0)
        {
            return null;
        }
        return list.ToArray();
    }
}
