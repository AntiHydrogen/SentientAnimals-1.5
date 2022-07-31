using RimWorld;
using Verse;

namespace SentientAnimals;

public static class SentientUtility
{
    public static bool IsSentient(this Pawn pawn)
    {
        return pawn.health.hediffSet.GetFirstHediffOfDef(SA_DefOf.SA_Sentient) != null;
    }

    public static bool IsSentientAndCanTalk(this Pawn pawn)
    {
        return pawn.health.hediffSet.GetFirstHediffOfDef(SA_DefOf.SA_Sentient) != null &&
               SentientAnimalsMod.settings.enableTalkingForSentient && CanTalk(pawn);
    }

    public static bool CanTalk(this Pawn pawn)
    {
        return pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking);
    }

    public static void MakeSentient(this Pawn pawn)
    {
        var brain = pawn.health.hediffSet.GetBrain();
        if (brain == null || pawn.health.hediffSet.GetFirstHediffOfDef(SA_DefOf.SA_Sentient) is not null)
        {
            return;
        }

        var hediff = HediffMaker.MakeHediff(SA_DefOf.SA_Sentient, pawn, brain);
        pawn.health.AddHediff(hediff);
    }
}