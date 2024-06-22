using RimWorld;
using Verse;
using Verse.AI;

namespace SentientAnimals
{

    public class JobGiver_Nurse : ThinkNode_JobGiver
    {
        public ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn pawn2 )|| pawn2 == pawn)
            {
                return false;
            }

            if (!FeedPatientUtility.IsHungry(pawn2))
            {
                return false;
            }

            if (!FeedPatientUtility.ShouldBeFed(pawn2))
            {
                return false;
            }

            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }

            if (FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2,
                    pawn2.needs.food.CurCategory == HungerCategory.Starving,
                    out _, out _, false))
            {
                return true;
            }

            JobFailReason.Is("NoFood".Translate());
            return false;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!SentientAnimalsMod.settings.enableNursingJobForSentient)
            {
                return null;
            }

            bool Predicate(Thing x)
            {
                return HasJobOnThing(pawn, x);
            }

            var t = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, PotentialWorkThingRequest, PathEndMode.OnCell,
                TraverseParms.For(pawn, Danger.Some), 100f, Predicate);
            Job job;
            if (t is Pawn pawn2 && FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2,
                    pawn2.needs.food.CurCategory == HungerCategory.Starving, out var foodSource, out var foodDef, false))
            {
                var nutrition = FoodUtility.GetNutrition(pawn2, foodSource, foodDef);
                job = JobMaker.MakeJob(JobDefOf.FeedPatient);
                job.targetA = foodSource;
                job.targetB = pawn2;
                job.count = FoodUtility.WillIngestStackCountOf(pawn2, foodDef, nutrition);
                return job;
            }

            if (!SentientAnimalsMod.settings.enableTalkingForSentient)
            {
                return null;
            }

            if (!InteractionUtility.CanInitiateInteraction(pawn))
            {
                return null;
            }

            pawn2 = SickPawnVisitUtility.FindRandomSickPawn(pawn, JoyCategory.Low);
            if (pawn2 == null)
            {
                return null;
            }

            job = JobMaker.MakeJob(JobDefOf.VisitSickPawn, pawn2);
            return job;
        }
    }
};

