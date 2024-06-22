using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SentientAnimals
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddUndraftedOrders")]
    public static class AddUndraftedOrders_Patch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            return !pawn.IsSentient();
        }
    }
}
;

