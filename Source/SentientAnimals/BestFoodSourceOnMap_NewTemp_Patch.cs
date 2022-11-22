using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals;

[HarmonyPatch(typeof(FoodUtility), "BestFoodSourceOnMap_NewTemp")]
[HarmonyBefore("net.quicksilverfox.rimworld.mod.animalslogic")]
public static class BestFoodSourceOnMap_NewTemp_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
    {
        var get_RaceProps = AccessTools.Method(typeof(Pawn), "get_RaceProps");
        var get_ToolUser = AccessTools.Method(typeof(RaceProperties), "get_ToolUser");
        var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentient");
        var pawnField = AccessTools.Field(typeof(FoodUtility).GetNestedTypes(AccessTools.all)
            .First(c => c.Name.Contains("c__DisplayClass19_0")), "getter");

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