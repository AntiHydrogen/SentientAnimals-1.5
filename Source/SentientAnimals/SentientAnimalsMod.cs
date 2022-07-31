using Mlie;
using UnityEngine;
using Verse;

namespace SentientAnimals;

internal class SentientAnimalsMod : Mod
{
    public static SentientAnimalsSettings settings;
    public static string currentVersion;

    public SentientAnimalsMod(ModContentPack pack) : base(pack)
    {
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(ModLister.GetActiveModWithIdentifier("Mlie.SentientAnimals"));
        settings = GetSettings<SentientAnimalsSettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        settings.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Sentient Animals";
    }
}