using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals;

[HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
public static class CanTakeOrder_Patch
{
    public static void Postfix(Pawn pawn, ref bool __result)
    {
        if (pawn.IsSentient())
        {
            __result = true;
        }
    }
}