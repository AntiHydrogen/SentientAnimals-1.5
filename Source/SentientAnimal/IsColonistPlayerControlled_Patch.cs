using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals
{
    [HarmonyPatch(typeof(Pawn), "IsColonistPlayerControlled", MethodType.Getter)]
    public static class IsColonistPlayerControlled_Patch
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Pawn __instance, ref bool __result)
        {
            if (!__instance.IsSentient())
            {
                return;
            }

            if (__instance.Spawned && __instance.Faction == Faction.OfPlayer && __instance.MentalStateDef == null)
            {
                __result = true;
            }
        }
    }
};

