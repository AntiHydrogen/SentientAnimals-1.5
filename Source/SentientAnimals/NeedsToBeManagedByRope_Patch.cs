using HarmonyLib;
using Verse;

namespace SentientAnimals;

[HarmonyPatch(typeof(AnimalPenUtility), "NeedsToBeManagedByRope")]
public class NeedsToBeManagedByRope_Patch
{
    public static void Postfix(Pawn pawn, ref bool __result)
    {
        if (pawn.IsSentient())
        {
            __result = false;
        }
    }
}