using UnityEngine;
using Verse;

namespace SentientAnimals;

internal class SentientAnimalsSettings : ModSettings
{
    public bool disableFilthGenerationForSentient = true;
    public bool enableCleaningJobForSentient = true;
    public bool enableNursingJobForSentient = true;
    public bool enableTalkingForSentient = true;
    public bool inheritSentientFromParent;
    public float naturalSentientChance = 0.05f;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref naturalSentientChance, "naturalSentientChance", 0.05f);
        Scribe_Values.Look(ref disableFilthGenerationForSentient, "disableFilthGenerationForSentient", true);
        Scribe_Values.Look(ref enableTalkingForSentient, "enableTalkingForSentient", true);
        Scribe_Values.Look(ref enableCleaningJobForSentient, "enableCleaningJobForSentient", true);
        Scribe_Values.Look(ref enableNursingJobForSentient, "enableNursingJobForSentient", true);
        Scribe_Values.Look(ref inheritSentientFromParent, "inheritSentientFromParent");
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        var rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(rect);
        listingStandard.SliderLabeled("SA.NaturalSentientChanceUponTamingOrAfterBirth".Translate(),
            ref naturalSentientChance, (naturalSentientChance * 100f).ToStringDecimalIfSmall() + "%");
        listingStandard.CheckboxLabeled("SA.enableTalkingForSentient".Translate(), ref enableTalkingForSentient);
        listingStandard.CheckboxLabeled("SA.disableFilthGenerationForSentient".Translate(),
            ref disableFilthGenerationForSentient);
        listingStandard.CheckboxLabeled("SA.enableCleaningJobForSentient".Translate(),
            ref enableCleaningJobForSentient);
        listingStandard.CheckboxLabeled("SA.enableNursingJobForSentient".Translate(), ref enableNursingJobForSentient);
        listingStandard.CheckboxLabeled("SA.inheritSentientFromParent".Translate(), ref inheritSentientFromParent);
        if (SentientAnimalsMod.currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("SA.modVersion".Translate(SentientAnimalsMod.currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
        Write();
    }
}