using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
			Pawn pawn2 = t as Pawn;
			if (pawn2 == null || pawn2 == pawn)
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
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out var _, out var _, canRefillDispenser: false))
			{
				JobFailReason.Is("NoFood".Translate());
				return false;
			}
			return true;
		}

		public override Job TryGiveJob(Pawn pawn)
		{
			if (!SentientAnimalsMod.settings.enableNursingJobForSentient)
            {
				return null;
            }
			Predicate<Thing> predicate = (Thing x) => HasJobOnThing(pawn, x);
			Thing t = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, PotentialWorkThingRequest, PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn), 100f, predicate);
			Job job = null;
			if (t is Pawn pawn2 && FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out var foodSource, out var foodDef, canRefillDispenser: false))
			{
				float nutrition = FoodUtility.GetNutrition(foodSource, foodDef);
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
}