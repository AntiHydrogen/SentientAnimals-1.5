using HarmonyLib;
using Verse;

namespace SentientAnimals;

[HarmonyPatch(typeof(Pawn), "SpawnSetup")]
public static class Patch_SpawnSetup
{
    private static void Postfix(Pawn __instance, bool respawningAfterLoad)
    {
        if (respawningAfterLoad || !__instance.RaceProps.Animal || __instance.IsSentient())
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
            return;
        }

        if (!SentientAnimalsMod.settings.inheritSentientFromParent || Patch_DoBirthSpawn._mother == null)
        {
            return;
        }

        var motherIsSentient = Patch_DoBirthSpawn._mother.IsSentient();
        if (Patch_DoBirthSpawn._father != null)
        {
            var fatherIsSentient = Patch_DoBirthSpawn._father.IsSentient();
            if (motherIsSentient && fatherIsSentient)
            {
                __instance.MakeSentient();
                return;
            }

            if (!motherIsSentient && !fatherIsSentient)
            {
                return;
            }

            if (Rand.Chance(0.25f))
            {
                __instance.MakeSentient();
            }

            return;
        }

        if (!motherIsSentient)
        {
            return;
        }

        if (Rand.Chance(0.25f))
        {
            __instance.MakeSentient();
        }
    }
}