using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals;

[HarmonyPatch(typeof(TrainingCardUtility), "DrawTrainingCard")]
public class DrawTrainingCard_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
    {
        var trainabilityFor = AccessTools.Method(typeof(DrawTrainingCard_Patch), "TrainabilityFor");
        var raceProps = AccessTools.Method(typeof(Pawn), "get_RaceProps");
        var trainabilityField = AccessTools.Field(typeof(RaceProperties), "trainability");
        var codes = instructions.ToList();
        var patched = false;
        for (var i = 0; i < codes.Count; i++)
        {
            var instr = codes[i];
            if (!patched && i > 2 && codes[i - 2].OperandIs(": ") && codes[i].opcode == OpCodes.Ldarg_1 &&
                codes[i + 1].Calls(raceProps) && codes[i + 2].LoadsField(trainabilityField))
            {
                patched = true;
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Call, trainabilityFor);
                i += 3;
            }
            else
            {
                yield return instr;
            }
        }

        if (!patched)
        {
            Log.Error("[SentientAnimals] TrainingCardUtility:DrawTrainingCard Transpiler failed");
        }
    }

    public static TaggedString TrainabilityFor(Pawn pawn)
    {
        return pawn.health.hediffSet.GetFirstHediffOfDef(SA_DefOf.SA_Sentient) != null
            ? TrainabilityDefOf.Advanced.LabelCap
            : pawn.RaceProps.trainability.LabelCap;
    }
}