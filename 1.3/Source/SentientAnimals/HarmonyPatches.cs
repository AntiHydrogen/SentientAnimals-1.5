using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SentientAnimals
{
    [DefOf]
    public static class SA_DefOf
    {
        public static HediffDef SA_Sentient;
        public static RecipeDef SA_MakeSentient;
    }

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("SentientAnimals.Mod");
            harmony.PatchAll();
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (thingDef.race != null && thingDef.race.Animal)
                {
                    thingDef.recipes.Add(SA_DefOf.SA_MakeSentient);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Hediff_Pregnant), "DoBirthSpawn")]
    public static class Patch_DoBirthSpawn
    {
        public static Pawn _mother;
        public static Pawn _father;
        private static void Prefix(Pawn mother, Pawn father)
        {
            _mother = mother;
            _father = father;
        }
        private static void Postfix(Pawn mother, Pawn father)
        {
            _mother = null;
            _father = null;
        }
    }

    [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    public static class Patch_SpawnSetup
    {
        private static void Postfix(Pawn __instance, bool respawningAfterLoad)
        {
            if (!respawningAfterLoad && __instance.RaceProps.Animal && !__instance.IsSentient())
            {
                if (Rand.Chance(SentientAnimalsMod.settings.naturalSentientChance))
                {
                    __instance.MakeSentient();
                }
                else if (SentientAnimalsMod.settings.inheritSentientFromParent && Patch_DoBirthSpawn._mother != null)
                {
                    bool motherIsSentient = Patch_DoBirthSpawn._mother.IsSentient();
                    if (Patch_DoBirthSpawn._father != null)
                    {
                        bool fatherIsSentient = Patch_DoBirthSpawn._father.IsSentient();
                        if (motherIsSentient && fatherIsSentient)
                        {
                            __instance.MakeSentient();
                        }
                        else if (motherIsSentient || fatherIsSentient)
                        {
                            if (Rand.Chance(0.25f))
                            {
                                __instance.MakeSentient();
                            }
                        }
                    }
                    else if (motherIsSentient)
                    {
                        if (Rand.Chance(0.25f))
                        {
                            __instance.MakeSentient();
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "SetFaction")]
    public static class SetFaction_Patch
    {
        private static void Prefix(Pawn __instance, Faction newFaction, Pawn recruiter = null)
        {
            if (Find.TickManager.gameStartAbsTick > 0)
            {
                if (newFaction != __instance.Faction && __instance.RaceProps.Animal && !__instance.IsSentient())
                {
                    if (Rand.Chance(SentientAnimalsMod.settings.naturalSentientChance))
                    {
                        __instance.MakeSentient();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(JoyUtility), "JoyTickCheckEnd")]
    public static class JoyTickCheckEnd_Patch
    {
        public static bool Prefix(ref bool __result, Pawn pawn, JoyTickFullJoyAction fullJoyAction = JoyTickFullJoyAction.EndJob, float extraJoyGainFactor = 1f, Building joySource = null)
        {
            if (pawn.IsSentient())
            {
                __result = JoyTickCheckEnd(pawn, fullJoyAction, extraJoyGainFactor, joySource);
                return false;
            }
            return true;
        }

        public static bool JoyTickCheckEnd(Pawn pawn, JoyTickFullJoyAction fullJoyAction = JoyTickFullJoyAction.EndJob, float extraJoyGainFactor = 1f, Building joySource = null)
        {
            Job curJob = pawn.CurJob;
            if (curJob.def.joyKind == null)
            {
                Log.Warning("This method can only be called for jobs with joyKind.");
                return false;
            }
            if (joySource != null)
            {
                if (joySource.def.building.joyKind != null && pawn.CurJob.def.joyKind != joySource.def.building.joyKind)
                {
                    Log.ErrorOnce("Joy source joyKind and jobDef.joyKind are not the same. building=" + joySource.ToStringSafe() + ", jobDef=" + pawn.CurJob.def.ToStringSafe(), joySource.thingIDNumber ^ 0x343FD5CC);
                }
                extraJoyGainFactor *= joySource.GetStatValue(StatDefOf.JoyGainFactor);
            }
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(SA_DefOf.SA_Sentient) as Hediff_Sentient;
            hediff.customJoyCount += extraJoyGainFactor * curJob.def.joyGainRate * 0.36f / 2500f;
            if (hediff.customJoyCount > 0.9999f && !curJob.doUntilGatheringEnded)
            {
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
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(SickPawnVisitUtility), "CanVisit")]
    public static class CanVisit_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var get_RaceProps = AccessTools.Method(typeof(Pawn), "get_RaceProps");
            var get_Humanlike = AccessTools.Method(typeof(RaceProperties), "get_Humanlike");
            var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentientAndCanTalk");

            var codes = instructions.ToList();
            var label = ilg.DefineLabel();
            bool patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (!patched && i > 1 && codes[i].opcode == OpCodes.Brfalse && codes[i - 1].Calls(get_Humanlike) && codes[i - 2].Calls(get_RaceProps))
                {
                    patched = true;
                    codes[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, codes[i].operand);
                }
                else
                {
                    yield return instr;
                }
            }
            if (!patched)
            {
                Log.Error("[SentientAnimals] SickPawnVisitUtility:CanVisit Transpiler failed");
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "BestFoodSourceOnMap")]
    public static class BestFoodSourceOnMap_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var get_RaceProps = AccessTools.Method(typeof(Pawn), "get_RaceProps");
            var get_ToolUser = AccessTools.Method(typeof(RaceProperties), "get_ToolUser");
            var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentient");
            var pawnField = AccessTools.Field(typeof(FoodUtility).GetNestedTypes(AccessTools.all)
                                .First(c => c.Name.Contains("c__DisplayClass12_0")), "getter");

            var codes = instructions.ToList();
            var label = ilg.DefineLabel();
            bool patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (!patched && i > 1 && codes[i].opcode == OpCodes.Brfalse_S && codes[i - 1].Calls(get_ToolUser) && codes[i - 2].Calls(get_RaceProps))
                {
                    patched = true;
                    codes[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, codes[i].operand);
                }
                else
                {
                    yield return instr;
                }
            }
            if (!patched)
            {
                Log.Error("[SentientAnimals] FoodUtility:BestFoodSourceOnMap Transpiler failed");
            }
        }
    }

    [HarmonyPatch(typeof(InteractionUtility), "CanReceiveRandomInteraction")]
    public static class CanReceiveRandomInteraction_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var get_Humanlike = AccessTools.Method(typeof(RaceProperties), "get_Humanlike");
            var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentientAndCanTalk");
            var codes = instructions.ToList();
            var label = ilg.DefineLabel();
            bool patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (!patched && i > 1 && codes[i].opcode == OpCodes.Brfalse_S && codes[i - 1].Calls(get_Humanlike))
                {
                    patched = true;
                    codes[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, codes[i].operand);

                }
                else
                {
                    yield return instr;
                }
            }
            if (!patched)
            {
                Log.Error("[SentientAnimals] InteractionUtility:CanReceiveRandomInteraction Transpiler failed");
            }
        }
    }

    [HarmonyPatch(typeof(InteractionUtility), "CanInitiateRandomInteraction")]
    public static class CanInitiateRandomInteraction_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var get_Humanlike = AccessTools.Method(typeof(RaceProperties), "get_Humanlike");
            var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentientAndCanTalk");
            var codes = instructions.ToList();
            var label = ilg.DefineLabel();
            bool patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (!patched && i > 1 && codes[i].opcode == OpCodes.Brfalse_S && codes[i - 1].Calls(get_Humanlike))
                {
                    patched = true;
                    codes[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, codes[i].operand);

                }
                else
                {
                    yield return instr;
                }
            }
            if (!patched)
            {
                Log.Error("[SentientAnimals] InteractionUtility:CanInitiateRandomInteraction Transpiler failed");
            }
        }
    }

    [HarmonyPatch(typeof(ITab_Pawn_Character))]
    [HarmonyPatch("IsVisible", MethodType.Getter)]
    public static class NoBioForMachines
    {
        public static void Postfix(ITab_Pawn_Character __instance, ref bool __result)
        {
            var pawn = __instance.PawnToShowInfoAbout;
            if (pawn.IsSentient())
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(HealthCardUtility), "DrawOverviewTab")]
    public static class DrawOverviewTab_Patch
    {
        public static void Prefix(Rect leftRect, Pawn pawn, float curY, out bool __state)
        {
            __state = PawnCapacityDefOf.Talking.showOnAnimals;
            if (pawn.IsSentient() && SentientAnimalsMod.settings.enableTalkingForSentient)
            {
                PawnCapacityDefOf.Talking.showOnAnimals = true;
            }
        }
        public static void Postfix(Rect leftRect, Pawn pawn, float curY, bool __state)
        {
            PawnCapacityDefOf.Talking.showOnAnimals = __state;
        }
    }

    [HarmonyPatch(typeof(ITab_Pawn_Gear), "IsVisible", MethodType.Getter)]
    public static class IsVisible_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentient");
            var codes = instructions.ToList();
            var label = ilg.DefineLabel();

            bool patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                yield return instr;
                if (!patched && codes[i].opcode == OpCodes.Stloc_0)
                {
                    patched = true;
                    codes[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, label);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ret);
                }
            }
            if (!patched)
            {
                Log.Error("[SentientAnimals] ITab_Pawn_Gear:IsVisible Transpiler failed");
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "IsColonistPlayerControlled", MethodType.Getter)]
    public static class IsColonistPlayerControlled_Patch
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Pawn __instance, ref bool __result)
        {
            if (__instance.IsSentient())
            {
                if (__instance.Spawned && __instance.Faction == Faction.OfPlayer && __instance.MentalStateDef == null)
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddUndraftedOrders")]
    public static class AddUndraftedOrders_Patch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (pawn.IsSentient())
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
    public static class CanTakeOrder_Patch
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (pawn.IsSentient())
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddDraftedOrders")]
    public static class FloatMenuMakerMap_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentient");
            var codes = instructions.ToList();
            var pawnField = AccessTools.Field(typeof(FloatMenuMakerMap).GetNestedTypes(AccessTools.all)
                                .First(c => c.Name.Contains("c__DisplayClass8_0")), "pawn");
            var skillsField = AccessTools.Field(typeof(Pawn), "skills");
            var constructionDefField = AccessTools.Field(typeof(SkillDefOf), "Construction");
            bool patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (!patched && codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].LoadsField(pawnField) && codes[i + 2].LoadsField(skillsField) 
                    && codes[i + 3].LoadsField(constructionDefField))
                {
                    patched = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_0).MoveLabelsFrom(codes[i]);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, codes[i + 6].operand);
                }
                yield return instr;
            }

            if (!patched)
            {
                Log.Error("[SentientAnimals] FloatMenuMakerMap:AddDraftedOrders Transpiler failed");
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_GetGizmos_Patch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            if (__instance.Faction == Faction.OfPlayer && __instance.IsSentient())
            {
                Command_Toggle command_Toggle = new Command_Toggle();
                command_Toggle.hotKey = KeyBindingDefOf.Command_ColonistDraft;
                command_Toggle.isActive = (() => __instance.Drafted);
                command_Toggle.toggleAction = delegate
                {
                    __instance.jobs.debugLog = true;
                    if (__instance.drafter is null)
                    {
                        if (__instance.RaceProps.Animal)
                        {
                            __instance.equipment = new Pawn_EquipmentTracker(__instance);
                        }
                        __instance.drafter = new Pawn_DraftController(__instance);
                        __instance.drafter.Drafted = true;
                    }
                    else
                    {
                        __instance.drafter.Drafted = !__instance.Drafted;
                    }
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Drafting, KnowledgeAmount.SpecificInteraction);
                    if (__instance.drafter.Drafted)
                    {
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.QueueOrders, OpportunityType.GoodToKnow);
                    }
                };
                command_Toggle.defaultDesc = "CommandToggleDraftDesc".Translate();
                command_Toggle.icon = TexCommand.Draft;
                command_Toggle.turnOnSound = SoundDefOf.DraftOn;
                command_Toggle.turnOffSound = SoundDefOf.DraftOff;
                command_Toggle.groupKey = 81729172;
                command_Toggle.defaultLabel = (__instance.Drafted ? "CommandUndraftLabel" : "CommandDraftLabel").Translate();
                if (__instance.Downed)
                {
                    command_Toggle.Disable("IsIncapped".Translate(__instance.LabelShort, __instance));
                }
                if (!__instance.Drafted)
                {
                    command_Toggle.tutorTag = "Draft";
                }
                else
                {
                    command_Toggle.tutorTag = "Undraft";
                }
                yield return command_Toggle;
            }

            foreach (var g in __result)
            {
                if (__instance.IsSentient() && g is Command_Toggle command && command.defaultDesc == "CommandToggleDraftDesc".Translate())
                {
                    continue;
                }
                yield return g;
            }
        }
    }

    [HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
    public static class GetStatValue_Patch
    {
        private static void Postfix(Thing thing, StatDef stat, bool applyPostProcess, ref float __result)
        {
            if (thing is Pawn pawn && SentientAnimalsMod.settings.disableFilthGenerationForSentient && stat == StatDefOf.FilthRate && pawn.IsSentient())
            {
                __result = 0;
            }
        }
    }

    [HarmonyPatch(typeof(AnimalPenUtility), "NeedsToBeManagedByRope")]
    public class NeedsToBeManagedByRope_Patch
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (pawn.IsSentient())
            {
                __result = false;
            }
        }
    }
    [HarmonyPatch(typeof(Pawn_TrainingTracker), "TrainingTrackerTickRare")]
    public class TrainingTrackerTickRare_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentient");
            var pawnField = AccessTools.Field(typeof(Pawn_TrainingTracker), "pawn");
            var animalTypeField = AccessTools.Field(typeof(RaceProperties), "animalType");

            var codes = instructions.ToList();
            var label = ilg.DefineLabel();

            bool patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (!patched && i > 5 && codes[i - 4].LoadsField(animalTypeField) && codes[i - 2].opcode == OpCodes.Bne_Un_S && codes[i - 1].opcode == OpCodes.Ret && codes[i].opcode == OpCodes.Ldarg_0)
                {
                    patched = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instr);
                    instr.labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, label);
                    yield return new CodeInstruction(OpCodes.Ret);
                }
                yield return instr;
            }

            if (!patched)
            {
                Log.Error("[SentientAnimals] Pawn_TrainingTracker:TrainingTrackerTickRare Transpiler failed");
            }
        }
    }

    [HarmonyPatch(typeof(TrainingCardUtility), "DrawTrainingCard")]
    public class DrawTrainingCard_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var trainabilityFor = AccessTools.Method(typeof(DrawTrainingCard_Patch), "TrainabilityFor");
            var raceProps = AccessTools.Method(typeof(Pawn), "get_RaceProps");
            var trainabilityField = AccessTools.Field(typeof(RaceProperties), "trainability");
            var codes = instructions.ToList();
            bool patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (!patched && i > 2 && codes[i - 2].OperandIs(": ") && codes[i].opcode == OpCodes.Ldarg_1 && codes[i + 1].Calls(raceProps) && codes[i + 2].LoadsField(trainabilityField))
                {
                    patched = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, trainabilityFor);
                    i += 3;
                }
                else
                {
                    yield return instr;
                }
            }

            if (!patched)
            {
                Log.Error("[SentientAnimals] TrainingCardUtility:DrawTrainingCard Transpiler failed");
            }
        }

        public static TaggedString TrainabilityFor(Pawn pawn)
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(SA_DefOf.SA_Sentient) != null)
            {
                return TrainabilityDefOf.Advanced.LabelCap;
            }
            return pawn.RaceProps.trainability.LabelCap;
        }
    }

    [HarmonyPatch(typeof(RelationsUtility), "TryDevelopBondRelation")]
    public class TryDevelopBondRelation_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentient");
            var intelligenceOrderField = AccessTools.Field(typeof(TrainabilityDef), "intelligenceOrder");
            var codes = instructions.ToList();
            bool patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                yield return instr;
                if (i > 1 && codes[i - 1].LoadsField(intelligenceOrderField) && codes[i].opcode == OpCodes.Bge_S)
                {
                    patched = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, instr.operand);
                }
            }
            if (!patched)
            {
                Log.Error("[SentientAnimals] RelationsUtility:TryDevelopBondRelation Transpiler failed");
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_TrainingTracker), "CanAssignToTrain", 
        new Type[] { typeof(TrainableDef), typeof(bool)},
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out })]
    public class CanAssignToTrain_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var shouldSkip = AccessTools.Method(typeof(SentientUtility), "IsSentient");
            var pawnField = AccessTools.Field(typeof(Pawn_TrainingTracker), "pawn");
            var intelligenceOrderField = AccessTools.Field(typeof(TrainabilityDef), "intelligenceOrder");
            var minBodySizeField = AccessTools.Field(typeof(TrainableDef), "minBodySize");
            var codes = instructions.ToList();
            bool patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                yield return instr;
                if (i > 1 && (codes[i - 1].LoadsField(intelligenceOrderField) && codes[i].opcode == OpCodes.Bge_S
                    || codes[i - 1].LoadsField(minBodySizeField) && codes[i].opcode == OpCodes.Bge_Un_S))
                {
                    patched = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, instr.operand);
                }
            }

            if (!patched)
            {
                Log.Error("[SentientAnimals] Pawn_TrainingTracker:CanAssignToTrain Transpiler failed");
            }
        }
    }
}
