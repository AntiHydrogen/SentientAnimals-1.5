using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals;

[HarmonyPatch(typeof(Pawn_TrainingTracker), "CanAssignToTrain",
    new[] { typeof(TrainableDef), typeof(bool) },
    new[] { ArgumentType.Normal, ArgumentType.Out })]
public class CanAssignToTrain_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
    {
        var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentient");
        var pawnField = AccessTools.Field(typeof(Pawn_TrainingTracker), "pawn");
        var intelligenceOrderField = AccessTools.Field(typeof(TrainabilityDef), "intelligenceOrder");
        var minBodySizeField = AccessTools.Field(typeof(TrainableDef), "minBodySize");
        var codes = instructions.ToList();
        var patched = false;
        for (var i = 0; i < codes.Count; i++)
        {
            var instr = codes[i];
            yield return instr;
            if (i <= 1 || (!codes[i - 1].LoadsField(intelligenceOrderField) || codes[i].opcode != OpCodes.Bge_S)
                && (!codes[i - 1].LoadsField(minBodySizeField) || codes[i].opcode != OpCodes.Bge_Un_S))
            {
                continue;
            }

            patched = true;
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
            yield return new CodeInstruction(OpCodes.Call, shouldSkip);
            yield return new CodeInstruction(OpCodes.Brtrue_S, instr.operand);
        }

        if (!patched)
        {
            Log.Error("[SentientAnimals] Pawn_TrainingTracker:CanAssignToTrain Transpiler failed");
        }
    }
}