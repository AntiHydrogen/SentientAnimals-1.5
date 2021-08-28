using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SentientAnimals
{
    public static class SentientUtility
    {
        public static bool IsSentient(this Pawn pawn)
        {
            return pawn.health.hediffSet.GetFirstHediffOfDef(SA_DefOf.SA_Sentient) != null;
        }

        public static bool IsSentientAndCanTalk(this Pawn pawn)
        {
            return pawn.health.hediffSet.GetFirstHediffOfDef(SA_DefOf.SA_Sentient) != null && SentientAnimalsMod.settings.enableTalkingForSentient && CanTalk(pawn);
        }

        public static bool CanTalk(this Pawn pawn)
        {
            return pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking);
        }

        public static void MakeSentient(this Pawn pawn)
        {
            BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
            if (brain != null && pawn.health.hediffSet.GetFirstHediffOfDef(SA_DefOf.SA_Sentient) is null)
            {
                var hediff = HediffMaker.MakeHediff(SA_DefOf.SA_Sentient, pawn, brain);
                pawn.health.AddHediff(hediff);
            }
        }
    }
}
