using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals;

[HarmonyPatch(typeof(ITab_Pawn_Character))]
[HarmonyPatch("IsVisible", MethodType.Getter)]
public static class NoBioForMachines
{
    public static void Postfix(ITab_Pawn_Character __instance, ref bool __result)
    {
        if (HarmonyPatches.pawnToShowInfoAboutMethod == null)
        {
            return;
        }

        var pawn = (Pawn)HarmonyPatches.pawnToShowInfoAboutMethod.Invoke(__instance, null);

        if (pawn?.IsSentient() == true)
        {
            __result = false;
        }
    }
}