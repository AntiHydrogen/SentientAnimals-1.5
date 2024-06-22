using HarmonyLib;
using Verse;
using RimWorld.Planet;

namespace SentientAnimals

{
    [HarmonyPatch(typeof(AnimalPenUtility), "NeedsToBeManagedByRope")]
    public class NeedsToBeManagedByRope_Patch
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (__result == true && pawn.IsSentient())
            {
                if(pawn.RaceProps.Roamer && pawn.IsFormingCaravan())
                {
                    return;//解決組織旅隊的bug
                }
                else
                {
                    __result = false;
                }
                
            }
        }
    }

};

