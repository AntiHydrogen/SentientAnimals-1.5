using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SentientAnimals
{
    internal class SentientAnimalsSettings : ModSettings
    {
        private static readonly Vector2 buttonSize = new Vector2(120f, 25f);
        private static readonly Vector2 searchSize = new Vector2(200f, 25f);
        private static string searchText = "";
        private readonly Color alternateBackground = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        private readonly Vector2 iconSize = new Vector2(48f, 48f);
        public bool alwaysStartWithSentientAnimals;
        public Dictionary<string, float> CustomSpawnChances = new Dictionary<string, float>();
        private List<string> customSpawnChancesKeys;
        private List<float> customSpawnChancesValues;
        public bool disableFilthGenerationForSentient = true;
        public bool enableCleaningJobForSentient = true;
        public bool enableNursingJobForSentient = true;
        public bool enableTalkingForSentient = true;
        public bool inheritSentientFromParent;
        public float naturalSentientChance = 0.05f;
        public bool onlySentientAnimalsGetsWeapons;
        private Vector2 scrollPosition;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref naturalSentientChance, "naturalSentientChance", 0.05f);
            Scribe_Collections.Look(ref CustomSpawnChances, "CustomSpawnChances", LookMode.Value,
                LookMode.Value,
                ref customSpawnChancesKeys, ref customSpawnChancesValues);
            Scribe_Values.Look(ref disableFilthGenerationForSentient, "disableFilthGenerationForSentient", true);
            Scribe_Values.Look(ref enableTalkingForSentient, "enableTalkingForSentient", true);
            Scribe_Values.Look(ref alwaysStartWithSentientAnimals, "alwaysStartWithSentientAnimals");
            Scribe_Values.Look(ref enableCleaningJobForSentient, "enableCleaningJobForSentient", true);
            Scribe_Values.Look(ref enableNursingJobForSentient, "enableNursingJobForSentient", true);
            Scribe_Values.Look(ref inheritSentientFromParent, "inheritSentientFromParent");
            Scribe_Values.Look(ref onlySentientAnimalsGetsWeapons, "onlySentientAnimalsGetsWeapons");
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled("SA.enableTalkingForSentient".Translate(), ref enableTalkingForSentient);
            listingStandard.CheckboxLabeled("SA.alwaysStartWithSentientAnimals".Translate(),
                ref alwaysStartWithSentientAnimals);
            listingStandard.CheckboxLabeled("SA.disableFilthGenerationForSentient".Translate(),
                ref disableFilthGenerationForSentient);
            listingStandard.CheckboxLabeled("SA.enableCleaningJobForSentient".Translate(),
                ref enableCleaningJobForSentient);
            listingStandard.CheckboxLabeled("SA.enableNursingJobForSentient".Translate(), ref enableNursingJobForSentient);
            listingStandard.CheckboxLabeled("SA.inheritSentientFromParent".Translate(), ref inheritSentientFromParent);
            listingStandard.SliderLabeled("SA.NaturalSentientChanceUponTamingOrAfterBirth".Translate(),
                ref naturalSentientChance, naturalSentientChance.ToStringPercent());
            naturalSentientChance = (float)Math.Round(naturalSentientChance, 2);
            if (HarmonyPatches.AnimalWeaponsLoaded)
            {
                listingStandard.CheckboxLabeled("SA.OnlySentientAnimalsGetsWeapons".Translate(),
                    ref onlySentientAnimalsGetsWeapons, "SA.OnlySentientAnimalsGetsWeapons.Tooltip".Translate());
            }

            if (SentientAnimalsMod.currentVersion != null)
            {
                listingStandard.Gap();
                GUI.contentColor = Color.gray;
                listingStandard.Label("SA.modVersion".Translate(SentientAnimalsMod.currentVersion));
                GUI.contentColor = Color.white;
            }

            var lastLabel = listingStandard.Label("SA.animalSpecificChances".Translate());
            listingStandard.End();

            if (CustomSpawnChances == null)
            {
                CustomSpawnChances = new Dictionary<string, float>();
            }

            if (CustomSpawnChances?.Any() == true)
            {
                DrawButton(() =>
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        "SA.resetall.confirm".Translate(),
                        delegate { CustomSpawnChances = new Dictionary<string, float>(); }));
                }, "SA.resetall.button".Translate(),
                    new Vector2(rect.width - buttonSize.x, lastLabel.position.y + buttonSize.y));
            }

            searchText =
                Widgets.TextField(
                    new Rect(
                        lastLabel.position +
                        new Vector2((rect.width / 2) - (searchSize.x / 2), buttonSize.y),
                        searchSize),
                    searchText);

            var scrollListing = new Listing_Standard();
            var borderRect = rect;
            borderRect.y += lastLabel.y + 30;
            borderRect.height -= lastLabel.y + 30;
            var scrollContentRect = rect;
            var animals = HarmonyPatches.AllAnimals;
            if (!string.IsNullOrEmpty(searchText))
            {
                animals = HarmonyPatches.AllAnimals.Where(def =>
                        def.label.ToLower().Contains(searchText.ToLower()) || def.modContentPack?.Name.ToLower()
                            .Contains(searchText.ToLower()) == true)
                    .ToList();
            }

            scrollContentRect.height = animals.Count * 51f;
            scrollContentRect.width -= 20;
            scrollContentRect.x = 0;
            scrollContentRect.y = 0;
            Widgets.BeginScrollView(borderRect, ref scrollPosition, scrollContentRect);
            scrollListing.Begin(scrollContentRect);
            var alternate = false;
            foreach (var animal in animals)
            {
                if (CustomSpawnChances == null)
                {
                    CustomSpawnChances = new Dictionary<string, float>();
                }

                var modInfo = animal.modContentPack?.Name;
                var rowRect = scrollListing.GetRect(50);
                alternate = !alternate;
                if (alternate)
                {
                    Widgets.DrawBoxSolid(rowRect.ExpandedBy(10, 0), alternateBackground);
                }

                var sliderRect = new Rect(rowRect.position + new Vector2(iconSize.x, 0),
                    rowRect.size - new Vector2(iconSize.x, 0));
                var animalTitle = animal.label.CapitalizeFirst();
                if (animalTitle.Length > 30)
                {
                    animalTitle = $"{animalTitle.Substring(0, 27)}...";
                }

                if (modInfo != null && modInfo.Length > 30)
                {
                    modInfo = $"{modInfo.Substring(0, 27)}...";
                }

                var currentValue = naturalSentientChance;
                if (CustomSpawnChances.ContainsKey(animal.defName))
                {
                    GUI.color = Color.green;
                    currentValue = CustomSpawnChances[animal.defName];
                }

                currentValue =
                    (float)Math.Round((decimal)Widgets.HorizontalSlider(
                        sliderRect,
                        currentValue, 0,
                        1f, false,
                        currentValue.ToStringPercent(),
                        animalTitle,
                        modInfo), 2);

                if (currentValue == naturalSentientChance && CustomSpawnChances.ContainsKey(animal.defName))
                {
                    CustomSpawnChances.Remove(animal.defName);
                }

                if (currentValue != naturalSentientChance)
                {
                    CustomSpawnChances[animal.defName] = currentValue;
                }

                GUI.color = Color.white;
                DrawIcon(animal,
                    new Rect(rowRect.position, iconSize));
            }

            scrollListing.End();
            Widgets.EndScrollView();
            Write();
        }

        private void DrawIcon(PawnKindDef pawnKind, Rect rect)
        {
            var texture2D = pawnKind?.lifeStages?.Last()?.bodyGraphicData?.Graphic?.MatSingle?.mainTexture;

            if (texture2D == null)
            {
                return;
            }

            var toolTip = $"{pawnKind.LabelCap}\n{pawnKind.race?.description}";
            if (texture2D.width != texture2D.height)
            {
                var ratio = (float)texture2D.width / texture2D.height;

                if (ratio < 1)
                {
                    rect.x += (rect.width - (rect.width * ratio)) / 2;
                    rect.width *= ratio;
                }
                else
                {
                    rect.y += (rect.height - (rect.height / ratio)) / 2;
                    rect.height /= ratio;
                }
            }

            GUI.DrawTexture(rect, texture2D);
            TooltipHandler.TipRegion(rect, toolTip);
        }

        private static void DrawButton(Action action, string text, Vector2 pos)
        {
            var rect = new Rect(pos.x, pos.y, buttonSize.x, buttonSize.y);
            if (!Widgets.ButtonText(rect, text, true, false, Color.white))
            {
                return;
            }

            SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera();
            action();
        }
    }
};

