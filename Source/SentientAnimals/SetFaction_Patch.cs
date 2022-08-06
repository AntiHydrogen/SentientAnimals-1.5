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

        var chance = SentientAnimalsMod.settings.naturalSentientChance;
        if (SentientAnimalsMod.settings.CustomSpawnChances?.ContainsKey(__instance.def.defName) == true)
        {
            chance = SentientAnimalsMod.settings.CustomSpawnChances[__instance.def.defName];
        }

        if (Rand.Chance(chance))
        {
            __instance.MakeSentient();
        }
    }
}