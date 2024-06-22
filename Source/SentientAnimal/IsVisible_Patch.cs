using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals
{
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "IsVisible", MethodType.Getter)]
    public static class IsVisible_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentient");
            var codes = instructions.ToList();
            var label = ilg.DefineLabel();

            var patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                yield return instr;
                if (patched || codes[i].opcode != OpCodes.Stloc_0)
                {
                    continue;
                }

                patched = true;
                codes[i + 1].labels.Add(label);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                yield return new CodeInstruction(OpCodes.Brfalse_S, label);
                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                yield return new CodeInstruction(OpCodes.Ret);
            }

            if (!patched)
            {
                Log.Error("[SentientAnimals] ITab_Pawn_Gear:IsVisible Transpiler failed");
            }
        }
    }

};

