using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals
{
    [HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
    public static class GetStatValue_Patch
    {
        private static void Postfix(Thing thing, StatDef stat, ref float __result)
        {
            if (thing is Pawn pawn && SentientAnimalsMod.settings.disableFilthGenerationForSentient &&
                stat == StatDefOf.FilthRate && pawn.IsSentient())
            {
                __result = 0;
            }
        }
    }
};

