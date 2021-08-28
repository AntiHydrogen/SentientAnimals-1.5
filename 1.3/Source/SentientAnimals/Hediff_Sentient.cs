using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SentientAnimals
{
    public class Hediff_Sentient : HediffWithComps
    {
		public float customJoyCount;
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
			if (pawn.training is null)
			{
				pawn.training = new Pawn_TrainingTracker(pawn);
			}
			if (pawn.story is null)
            {
				pawn.story = new Pawn_StoryTracker(pawn);
            }
			if (pawn.skills is null)
            {
				pawn.skills = new Pawn_SkillTracker(pawn);
            }
			foreach (TrainableDef allDef in DefDatabase<TrainableDef>.AllDefs)
			{
				if (!pawn.training.HasLearned(allDef))
                {
					pawn.training.SetWantedRecursive(allDef, checkOn: true);
					pawn.training.Train(allDef, null, complete: true);
					if (allDef == TrainableDefOf.Release)
					{
						if (pawn.playerSettings is null)
                        {
							pawn.playerSettings = new Pawn_PlayerSettings(pawn);
                        }
						pawn.playerSettings.followDrafted = true;
					}
				}
			}
		}

        public override void Tick()
        {
            base.Tick();
			if (customJoyCount > 0 && pawn.CurJobDef == JobDefOf.GotoWander)
            {
				customJoyCount = 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref customJoyCount, "customJoyCount");
        }
    }
}
