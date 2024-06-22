using HarmonyLib;
using Verse;

namespace SentientAnimals
{
    [HarmonyPatch(typeof(Hediff_Pregnant), "DoBirthSpawn")]
    public static class Patch_DoBirthSpawn
    {
        public static Pawn _mother;
        public static Pawn _father;

        private static void Prefix(Pawn mother, Pawn father)
        {
            _mother = mother;
            _father = father;
        }

        private static void Postfix()
        {
            _mother = null;
            _father = null;
        }
    }
};

