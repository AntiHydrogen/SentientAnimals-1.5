using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals 
{
    [HarmonyPatch(typeof(FoodUtility), "BestFoodSourceOnMap")]
    [HarmonyBefore("net.quicksilverfox.rimworld.mod.animalslogic")]
    public static class BestFoodSourceOnMap_NewTemp_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var get_RaceProps = AccessTools.Method(typeof(Pawn), "get_RaceProps");
            var get_ToolUser = AccessTools.Method(typeof(RaceProperties), "get_ToolUser");
            var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentient");
            var fieldfound = false;
            FieldInfo pawnField = null;
            foreach (var type in typeof(FoodUtility).GetNestedTypes(AccessTools.all))
            {
                if(fieldfound)
                {
                    break;
                }
                foreach(var name in type.GetMethodNames())
                {
                    if (fieldfound)
                    {
                        break;
                    }
                    if (name.Contains("BestFoodSourceOnMap"))
                    {
                        foreach(var fld in type.GetFields())
                        {
                            var arr = fld.ToStringSafe().Split(' ');
                            if (arr[0] == "Verse.Pawn" && arr[1] == "getter")
                            {
                                //Log.Message(pawnField.ToStringSafe());
                                pawnField = fld;
                                fieldfound = true;
                                break;
                            }
                        }
                    }
                }
            }
            var codes = instructions.ToList();
            var label = ilg.DefineLabel();
            var patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (!patched && i > 1 && codes[i].opcode == OpCodes.Brfalse_S && codes[i - 1].Calls(get_ToolUser) &&
                    codes[i - 2].Calls(get_RaceProps))
                {
                    patched = true;
                    codes[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, codes[i].operand);
                }
                else
                {
                    yield return instr;
                }
            }

            if (!patched)
            {
                Log.Error("[SentientAnimals] FoodUtility:BestFoodSourceOnMap_NewTemp Transpiler failed");
            }
        }
    }

};

