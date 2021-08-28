using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SentientAnimals
{
	public class Recipe_MakeSentient : RecipeWorker
	{
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
			if (thing is Pawn pawn && pawn.IsSentient())
            {
				return false;
            }
            return base.AvailableOnNow(thing, part);
        }

        private static readonly SimpleCurve MedicineMedicalPotencyToSurgeryChanceFactor = new SimpleCurve
		{
			new CurvePoint(0f, 0.7f),
			new CurvePoint(1f, 1f),
			new CurvePoint(2f, 1.3f)
		};
		protected bool CheckSurgeryFail(Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
		{
			if (bill.recipe.surgerySuccessChanceFactor >= 99999f)
			{
				return false;
			}
			float num = 1f;

			var medicineChance = (surgeon.skills.GetSkill(SkillDefOf.Medicine)?.levelInt ?? 0f) / 10f;
			var animalsChance = (surgeon.skills.GetSkill(SkillDefOf.Animals)?.levelInt ?? 0f) / 10f;
			var intellectualChance = (surgeon.skills.GetSkill(SkillDefOf.Intellectual)?.levelInt ?? 0f) / 10f;
			num *= (medicineChance + animalsChance + intellectualChance) / 3f;

			if (!recipe.surgeryIgnoreEnvironment && patient.InBed())
			{
				num *= patient.CurrentBed().GetStatValue(StatDefOf.SurgerySuccessChanceFactor);
			}
			num *= MedicineMedicalPotencyToSurgeryChanceFactor.Evaluate(GetAverageMedicalPotency(ingredients, bill));
			num *= recipe.surgerySuccessChanceFactor;
			if (surgeon.InspirationDef == InspirationDefOf.Inspired_Surgery && !patient.RaceProps.IsMechanoid)
			{
				num *= 2f;
				surgeon.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Surgery);
			}
			num = Mathf.Min(num, 0.98f);

			if (!Rand.Chance(num))
			{
				BodyPartRecord brain = patient.health.hediffSet.GetBrain();
				if (Rand.Chance(recipe.deathOnFailedSurgeryChance))
				{
					if (brain != null)
					{
						patient.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, num, 9999f, -1f, surgeon, brain));
					}
					if (!patient.Dead)
					{
						patient.Kill(null, null);
					}
					Find.LetterStack.ReceiveLetter("LetterLabelSurgeryFailed".Translate(patient.Named("PATIENT")), "MessageMedicalOperationFailureFatal".Translate(surgeon.LabelShort, patient.LabelShort, recipe.LabelCap, surgeon.Named("SURGEON"), patient.Named("PATIENT")), LetterDefOf.NegativeEvent, patient);
				}
				else if (Rand.Chance(0.5f))
				{
					if (Rand.Chance(0.1f))
					{
						Find.LetterStack.ReceiveLetter("LetterLabelSurgeryFailed".Translate(patient.Named("PATIENT")), "MessageMedicalOperationFailureRidiculous".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT"), recipe.Named("RECIPE")), LetterDefOf.NegativeEvent, patient);
						HealthUtility.GiveInjuriesOperationFailureRidiculous(patient);
						if (brain != null)
						{
							patient.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, num, 10f, -1f, surgeon, brain));
						}
					}
					else
					{
						Find.LetterStack.ReceiveLetter("LetterLabelSurgeryFailed".Translate(patient.Named("PATIENT")), "MessageMedicalOperationFailureCatastrophic".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT"), recipe.Named("RECIPE")), LetterDefOf.NegativeEvent, patient);
						HealthUtility.GiveInjuriesOperationFailureCatastrophic(patient, part);
						patient.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, num, 20f, -1f, surgeon, brain));
					}
				}
				else
				{
					Find.LetterStack.ReceiveLetter("LetterLabelSurgeryFailed".Translate(patient.Named("PATIENT")), "MessageMedicalOperationFailureMinor".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT"), recipe.Named("RECIPE")), LetterDefOf.NegativeEvent, patient);
					HealthUtility.GiveInjuriesOperationFailureMinor(patient, part);
					patient.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, num, 1f, -1f, surgeon, brain));
					Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Dementia, patient, brain);
					patient.health.AddHediff(hediff);
				}
				if (!patient.Dead)
				{
					TryGainBotchedSurgeryThought(patient, surgeon);
				}
				return true;
			}
			return false;
		}
		private void TryGainBotchedSurgeryThought(Pawn patient, Pawn surgeon)
		{
			if (patient.RaceProps.Humanlike && patient.needs.mood != null)
			{
				patient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BotchedMySurgery, surgeon);
			}
		}
		private float GetAverageMedicalPotency(List<Thing> ingredients, Bill bill)
		{
			ThingDef thingDef = (bill as Bill_Medical)?.consumedInitialMedicineDef;
			int num = 0;
			float num2 = 0f;
			if (thingDef != null)
			{
				num++;
				num2 += thingDef.GetStatValueAbstract(StatDefOf.MedicalPotency);
			}
			for (int i = 0; i < ingredients.Count; i++)
			{
				Medicine medicine = ingredients[i] as Medicine;
				if (medicine != null)
				{
					num += medicine.stackCount;
					num2 += medicine.GetStatValue(StatDefOf.MedicalPotency) * (float)medicine.stackCount;
				}
			}
			if (num == 0)
			{
				return 1f;
			}
			return num2 / (float)num;
		}

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
			pawn.MakeSentient();
        }
    }
}
