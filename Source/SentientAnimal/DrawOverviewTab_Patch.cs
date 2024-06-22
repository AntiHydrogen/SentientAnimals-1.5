using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SentientAnimals
{
    [HarmonyPatch(typeof(HealthCardUtility), "DrawOverviewTab")]
    public static class DrawOverviewTab_Patch
    {
        public static void Prefix(Pawn pawn, out bool __state)
        {
            __state = PawnCapacityDefOf.Talking.showOnAnimals;
            if (pawn.IsSentient() && SentientAnimalsMod.settings.enableTalkingForSentient)
            {
                PawnCapacityDefOf.Talking.showOnAnimals = true;
            }
        }

        public static void Postfix(Pawn pawn,bool __state)
        {
            PawnCapacityDefOf.Talking.showOnAnimals = __state;
        }
    }
};

