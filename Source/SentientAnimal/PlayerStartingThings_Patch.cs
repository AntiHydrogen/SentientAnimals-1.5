using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals
{
    [HarmonyPatch(typeof(ScenPart_StartingAnimal), "PlayerStartingThings")]
    public static class PlayerStartingThings_Patch
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> values)
        {
            foreach (var thing in values)
            {
                if (!SentientAnimalsMod.settings.alwaysStartWithSentientAnimals || !(thing is Pawn animal) ||
                    !animal.RaceProps.Animal)
                {
                    yield return thing;
                    continue;
                }

                animal.MakeSentient();
                yield return animal;
            }
        }
    }
};

