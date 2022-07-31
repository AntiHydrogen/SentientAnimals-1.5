using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals;

[HarmonyPatch(typeof(Pawn), "SetFaction")]
public static class SetFaction_Patch
{
    private static void Prefix(Pawn __instance, Faction newFaction)
    {
        if (Find.TickManager.gameStartAbsTick <= 0)
        {
            return;
        }

        if (newFaction == __instance.Faction || !__instance.RaceProps.Animal || __instance.IsSentient())
        {
            return;
        }

        if (Rand.Chance(SentientAnimalsMod.settings.naturalSentientChance))
        {
            __instance.MakeSentient();
        }
    }
}