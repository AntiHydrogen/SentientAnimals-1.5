using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SentientAnimals
{
    [HarmonyPatch(typeof(JoyUtility), "JoyTickCheckEnd")]
    public static class JoyTickCheckEnd_Patch
    {
        public static bool Prefix(ref bool __result, Pawn pawn,
            JoyTickFullJoyAction fullJoyAction = JoyTickFullJoyAction.EndJob, float extraJoyGainFactor = 1f,
            Building joySource = null)
        {
            if (!pawn.IsSentient())
            {
                return true;
            }

            __result = JoyTickCheckEnd(pawn, fullJoyAction, extraJoyGainFactor, joySource);
            return false;
        }

        public static bool JoyTickCheckEnd(Pawn pawn, JoyTickFullJoyAction fullJoyAction = JoyTickFullJoyAction.EndJob,
            float extraJoyGainFactor = 1f, Building joySource = null)
        {
            var curJob = pawn.CurJob;
            if (curJob.def.joyKind == null)
            {
                Log.Warning("This method can only be called for jobs with joyKind.");
                return false;
            }

            if (joySource != null)
            {
                if (joySource.def.building.joyKind != null && pawn.CurJob.def.joyKind != joySource.def.building.joyKind)
                {
                    Log.ErrorOnce(
                        "Joy source joyKind and jobDef.joyKind are not the same. building=" + joySource.ToStringSafe() +
                        ", jobDef=" + pawn.CurJob.def.ToStringSafe(), joySource.thingIDNumber ^ 0x343FD5CC);
                }

                extraJoyGainFactor *= joySource.GetStatValue(StatDefOf.JoyGainFactor);
            }

            if (!(pawn.health.hediffSet.GetFirstHediffOfDef(SA_DefOf.SA_Sentient) is Hediff_Sentient hediff))
            {
                return false;
            }

            hediff.customJoyCount += extraJoyGainFactor * curJob.def.joyGainRate * 0.36f / 2500f;
            if (!(hediff.customJoyCount > 0.9999f) || curJob.doUntilGatheringEnded)
            {
                return false;
            }

            switch (fullJoyAction)
            {
                case JoyTickFullJoyAction.EndJob:
                    hediff.customJoyCount = 0;
                    pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
                    return true;
                case JoyTickFullJoyAction.GoToNextToil:
                    hediff.customJoyCount = 0;
                    pawn.jobs.curDriver.ReadyForNextToil();
                    return true;
            }

            return false;
        }
    }
};

