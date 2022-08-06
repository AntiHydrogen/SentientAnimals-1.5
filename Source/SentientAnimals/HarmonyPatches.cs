using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals;

[StaticConstructorOnStartup]
public static class HarmonyPatches
{
    public static readonly MethodInfo pawnToShowInfoAboutMethod;
    private static List<PawnKindDef> allAnimals;

    static HarmonyPatches()
    {
        pawnToShowInfoAboutMethod = AccessTools.Method(typeof(ITab_Pawn_Character), "get_PawnToShowInfoAbout");
        if (pawnToShowInfoAboutMethod == null)
        {
            Log.Warning(
                "[SentientAnimals]: Failed to find PawnToShowInfoAbout-method. Will not be able to see if a pawn is droid or living.");
        }

        var harmony = new Harmony("SentientAnimals.Mod");
        harmony.PatchAll();
        foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(def => def.race?.Animal == true))
        {
            if (thingDef.recipes == null)
            {
                thingDef.recipes = new List<RecipeDef>();
            }

            thingDef.recipes.Add(SA_DefOf.SA_MakeSentient);
        }
    }

    public static List<PawnKindDef> AllAnimals
    {
        get
        {
            if (allAnimals == null || allAnimals.Count == 0)
            {
                allAnimals = (from animal in DefDatabase<PawnKindDef>.AllDefsListForReading
                    where animal.RaceProps?.Animal == true
                    orderby animal.label
                    select animal).ToList();
            }

            return allAnimals;
        }
        set => allAnimals = value;
    }
}