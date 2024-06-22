using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SentientAnimals
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_GetGizmos_Patch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            if (__instance.Faction == Faction.OfPlayer && __instance.IsSentient())
            {
                var command_Toggle = new Command_Toggle
                {
                    hotKey = KeyBindingDefOf.Command_ColonistDraft,
                    isActive = () => __instance.Drafted,
                    toggleAction = delegate
                    {
#if DEBUG
                        __instance.jobs.debugLog = true;
#endif
                        if (__instance.drafter is null)
                        {
                            if (__instance.RaceProps.Animal)
                            {
                                __instance.equipment = new Pawn_EquipmentTracker(__instance);
                            }

                            __instance.drafter = new Pawn_DraftController(__instance)
                            {
                                Drafted = true
                            };
                        }
                        else
                        {
                            __instance.drafter.Drafted = !__instance.Drafted;
                        }

                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Drafting,
                            KnowledgeAmount.SpecificInteraction);
                        if (__instance.drafter.Drafted)
                        {
                            LessonAutoActivator.TeachOpportunity(ConceptDefOf.QueueOrders, OpportunityType.GoodToKnow);
                        }
                    },
                    defaultDesc = "CommandToggleDraftDesc".Translate(),
                    icon = TexCommand.Draft,
                    turnOnSound = SoundDefOf.DraftOn,
                    turnOffSound = SoundDefOf.DraftOff,
                    groupKey = 81729172,
                    defaultLabel = (__instance.Drafted ? "CommandUndraftLabel" : "CommandDraftLabel").Translate()
                };
                if (__instance.Downed)
                {
                    command_Toggle.Disable("IsIncapped".Translate(__instance.LabelShort, __instance));
                }

                command_Toggle.tutorTag = !__instance.Drafted ? "Draft" : "Undraft";

                yield return command_Toggle;
            }

            foreach (var g in __result)
            {
                if (__instance.IsSentient() && g is Command_Toggle command &&
                    command.defaultDesc == "CommandToggleDraftDesc".Translate())
                {
                    continue;
                }

                yield return g;
            }
        }
    }
};

