using HarmonyLib;
using Verse;
using Verse.AI;

namespace SentientAnimals;

[HarmonyPatch(typeof(MentalStateWorker_Roaming), "CanRoamNow")]
public static class CanRoamNow_Patch
{
    public static void Postfix(Pawn pawn, ref bool __result)
    {
        if (pawn.IsSentient())
        {
            __result = false;
        }
    }
}