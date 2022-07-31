using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals;

[HarmonyPatch(typeof(InteractionUtility), "CanReceiveRandomInteraction")]
public static class CanReceiveRandomInteraction_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
    {
        var get_Humanlike = AccessTools.Method(typeof(RaceProperties), "get_Humanlike");
        var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentientAndCanTalk");
        var codes = instructions.ToList();
        var label = ilg.DefineLabel();
        var patched = false;
        for (var i = 0; i < codes.Count; i++)
        {
            var instr = codes[i];
            if (!patched && i > 1 && codes[i].opcode == OpCodes.Brfalse_S && codes[i - 1].Calls(get_Humanlike))
            {
                patched = true;
                codes[i + 1].labels.Add(label);
                yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
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
            Log.Error("[SentientAnimals] InteractionUtility:CanReceiveRandomInteraction Transpiler failed");
        }
    }
}