using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SentientAnimals
{
    class SentientAnimalsSettings : ModSettings
    {
        public float naturalSentientChance = 0.05f;
        public bool enableTalkingForSentient = true;
        public bool disableFilthGenerationForSentient = true;
        public bool enableCleaningJobForSentient = true;
        public bool enableNursingJobForSentient = true;
        public bool inheritSentientFromParent = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref naturalSentientChance, "naturalSentientChance", 0.05f);
            Scribe_Values.Look(ref disableFilthGenerationForSentient, "disableFilthGenerationForSentient", true);
            Scribe_Values.Look(ref enableTalkingForSentient, "enableTalkingForSentient", true);
            Scribe_Values.Look(ref enableCleaningJobForSentient, "enableCleaningJobForSentient", true);
            Scribe_Values.Look(ref enableNursingJobForSentient, "enableNursingJobForSentient", true);
            Scribe_Values.Look(ref inheritSentientFromParent, "inheritSentientFromParent", false);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.SliderLabeled("SA.NaturalSentientChanceUponTamingOrAfterBirth".Translate(), 
                ref naturalSentientChance, (naturalSentientChance * 100f).ToStringDecimalIfSmall() + "%", 0f, 1f);
            listingStandard.CheckboxLabeled("SA.enableTalkingForSentient".Translate(), ref enableTalkingForSentient);
            listingStandard.CheckboxLabeled("SA.disableFilthGenerationForSentient".Translate(), ref disableFilthGenerationForSentient);
            listingStandard.CheckboxLabeled("SA.enableCleaningJobForSentient".Translate(), ref enableCleaningJobForSentient);
            listingStandard.CheckboxLabeled("SA.enableNursingJobForSentient".Translate(), ref enableNursingJobForSentient);
            listingStandard.CheckboxLabeled("SA.inheritSentientFromParent".Translate(), ref inheritSentientFromParent);
            listingStandard.End();
            base.Write();
        }
    }
}
