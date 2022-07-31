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

        if (Rand.Chance(SentientAnimalsMod.settings.naturalSentientChance))
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