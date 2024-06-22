using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals
{
    [HarmonyPatch(typeof(RelationsUtility), "TryDevelopBondRelation")]
    public class TryDevelopBondRelation_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentient");
            var intelligenceOrderField = AccessTools.Field(typeof(TrainabilityDef), "intelligenceOrder");
            var codes = instructions.ToList();
            var patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                yield return instr;
                if (i <= 1 || !codes[i - 1].LoadsField(intelligenceOrderField) || codes[i].opcode != OpCodes.Bge_S)
                {
                    continue;
                }

                patched = true;
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                yield return new CodeInstruction(OpCodes.Brtrue_S, instr.operand);
            }

            if (!patched)
            {
                Log.Error("[SentientAnimals] RelationsUtility:TryDevelopBondRelation Transpiler failed");
            }
        }
    }
};

