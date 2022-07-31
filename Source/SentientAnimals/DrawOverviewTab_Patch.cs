using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SentientAnimals;

[HarmonyPatch(typeof(HealthCardUtility), "DrawOverviewTab")]
public static class DrawOverviewTab_Patch
{
    public static void Prefix(Rect leftRect, Pawn pawn, float curY, out bool __state)
    {
        __state = PawnCapacityDefOf.Talking.showOnAnimals;
        if (pawn.IsSentient() && SentientAnimalsMod.settings.enableTalkingForSentient)
        {
            PawnCapacityDefOf.Talking.showOnAnimals = true;
        }
    }

    public static void Postfix(Rect leftRect, Pawn pawn, float curY, bool __state)
    {
        PawnCapacityDefOf.Talking.showOnAnimals = __state;
    }
}