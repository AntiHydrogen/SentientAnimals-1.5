using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals;

[HarmonyPatch(typeof(PriceUtility), "PawnQualityPriceFactor")]
public static class PawnQualityPriceFactor_Patch
{
    private static void Prefix(ref Pawn pawn, out Pawn_SkillTracker __state)
    {
        __state = null;

        if (pawn?.IsSentient() != true)
        {
            return;
        }

        __state = pawn.skills;
        pawn.skills = null;
    }

    private static void Postfix(ref Pawn pawn, Pawn_SkillTracker __state)
    {
        if (__state != null)
        {
            pawn.skills = __state;
        }
    }
}