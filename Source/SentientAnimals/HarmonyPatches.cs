using System;
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
    public static readonly bool AnimalWeaponsLoaded;

    static HarmonyPatches()
    {
        pawnToShowInfoAboutMethod = AccessTools.Method(typeof(ITab_Pawn_Character), "get_PawnToShowInfoAbout");
        AnimalWeaponsLoaded = ModLister.GetActiveModWithIdentifier("Udon.AnimalWeapon") != null;
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


    public static void VerifyAnimalWeapon(ThingWithComps animal, bool enabled)
    {
        var weaponCompType = AccessTools.TypeByName("AMW_Comp_EquipMeleeWeapon");
        var compsField = AccessTools.Field(typeof(ThingWithComps), "comps");
        var allComps = (List<ThingComp>)compsField.GetValue(animal);

        if (enabled)
        {
            if (allComps == null)
            {
                allComps = new List<ThingComp>();
            }

            if (allComps.Any(comp => comp?.props?.compClass == weaponCompType))
            {
                return;
            }

            var thingComp = (ThingComp)Activator.CreateInstance(weaponCompType);
            thingComp.parent = animal;
            allComps.Add(thingComp);
            compsField.SetValue(animal, allComps);

            thingComp.Initialize(thingComp.props);
            return;
        }

        if (!allComps.Any(comp => comp?.props?.compClass == weaponCompType))
        {
            return;
        }

        for (var i = 0; i < allComps.Count; i++)
        {
            if (allComps[i].props.compClass == weaponCompType)
            {
                allComps.RemoveAt(i);
            }
        }

        compsField.SetValue(animal, allComps);
    }
}