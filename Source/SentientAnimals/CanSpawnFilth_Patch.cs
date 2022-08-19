using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals;

[HarmonyPatch(typeof(CompSpawnerFilth), "CanSpawnFilth", MethodType.Getter)]
public static class CanSpawnFilth_Patch
{
    private static void Postfix(CompSpawnerFilth __instance, ref bool __result)
    {
        if (!__result)
        {
            return;
        }

        if (__instance.parent is not Pawn pawn)
        {
            return;
        }

        __result = !pawn.IsSentient();
    }
}