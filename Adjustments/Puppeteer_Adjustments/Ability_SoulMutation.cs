using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using static UnityEngine.GraphicsBuffer;
using Ability = VFECore.Abilities.Ability;

namespace Adjustments.Puppeteer_Adjustments
{
    public class Ability_SoulMutation : Ability
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            var subject = target.Pawn;
            var master = this.pawn;

            if (subject == master)
            {
                Messages.Message($"Cannot target self.", MessageTypeDefOf.NeutralEvent);
                return false;
            }

            var puppetHediff = subject.health.hediffSet.GetFirstHediffOfDef(Adjustments.VPEP_PuppetHediff_HediffDef);
            if (puppetHediff != null)
            {
                Messages.Message($"Target cannot be a puppet", MessageTypeDefOf.NeutralEvent);
                return false;
            }

            if (subject.Dead)
            {
                Messages.Message($"Target cannot be dead", MessageTypeDefOf.NeutralEvent);
                return false;
            }

            var masterHediff = master.health.hediffSet.GetFirstHediffOfDef(Defs.ADJ_SoulMutating_Hediff) as Hediff_SoulMutation;
            if (masterHediff != null)
            {
                Messages.Message($"Can only mutate a single subject at a time", MessageTypeDefOf.NeutralEvent);
                return false;
            }

            if (subject.IsColonist || subject.IsSlaveOfColony)
            {
                var subjectHediff = subject.health.hediffSet.GetFirstHediffOfDef(Defs.ADJ_SoulMutating_Hediff) as Hediff_SoulMutation;
                if (subjectHediff != null)
                {
                    if (subjectHediff.Stage == SoulMutationStage.Mutating)
                    {
                        Messages.Message($"Mutation in process", MessageTypeDefOf.NeutralEvent);
                        return false;
                    }
                    else if (subjectHediff.Stage == SoulMutationStage.Healing)
                    {
                        Messages.Message($"Healing in process", MessageTypeDefOf.NeutralEvent);
                        return false;
                    }
                }


                return true;
            }



            Messages.Message($"Invalid target", MessageTypeDefOf.NeutralEvent);
            return false;

        }
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            var subject = targets[0].Thing as Pawn;
            var master = this.pawn;

            SoundDefOf.InfoCard_Open.PlayOneShotOnCamera();
            Find.WindowStack.Add(new Dialogue_SelectMutation(master, subject, (selection) =>
            {
                var cost = Utils.MutateCost(subject.skills.skills.FirstOrDefault(v => v.def.defName == selection));
                if (cost == -1f)
                {
                    Log.Error($"CANT FIND SKILL RECORD?  SOME ERROR? {selection}");
                    return;
                }
                var masterLeechHediff = master.health.hediffSet.GetFirstHediffOfDef(Adjustments.BrainLeechingHediff) as Hediff_SoulLeech;
                if (masterLeechHediff == null)
                {
                    Messages.Message("No soul leech points on master.", MessageTypeDefOf.NeutralEvent);
                    return;
                }


                var subjectHediff = subject.health.hediffSet.GetFirstHediffOfDef(Defs.ADJ_SoulMutating_Hediff) as Hediff_SoulMutation;
                if (subjectHediff == null)
                {
                    subjectHediff = HediffMaker.MakeHediff(Defs.ADJ_SoulMutating_Hediff, subject, subject.health.hediffSet.GetBrain()) as Hediff_SoulMutation;
                    subjectHediff.Type = SoulMutationType.Subject;
                    subjectHediff.Subject = subject;

                    subject.health.AddHediff(subjectHediff, subject.health.hediffSet.GetBrain());
                }
                subjectHediff.Master = master;
                subjectHediff.Activate(selection);


                var masterHediff = HediffMaker.MakeHediff(Defs.ADJ_SoulMutating_Hediff, master, master.health.hediffSet.GetBrain()) as Hediff_SoulMutation;
                masterHediff.Type = SoulMutationType.Master;
                masterHediff.Master = master;
                masterHediff.Subject = subject;
                master.health.AddHediff(masterHediff, master.health.hediffSet.GetBrain());
                masterHediff.Activate(selection);


                var effectdef = DefDatabase<EffecterDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_PsycastSkipFlashPurple");
                if (effectdef != null)
                {
                    this.AddEffecterToMaintain(SpawnEffecter(effectdef, subject, this.pawn.Map, new Vector3(), 0.3f), subject.Position, 60);
                }
            }));


        }

        public Effecter SpawnEffecter(EffecterDef effecterDef, Thing target, Map map, Vector3 offset, float scale)
        {
            Effecter effecter = new Effecter(effecterDef);
            effecter.offset = offset;
            effecter.scale = scale;
            TargetInfo targetInfo = new TargetInfo(target.Position, map);
            effecter.Trigger(targetInfo, targetInfo);
            return effecter;
        }


    }

    public class Dialogue_SelectMutation : Window
    {
        private Action<string> onSubmit;
        private Dictionary<string, float> skills;
        private Pawn master;
        private Pawn subject;

        public Dialogue_SelectMutation(Pawn master, Pawn subject, Action<string> onSubmit)
        {
            this.master = master;
            this.subject = subject;
            this.onSubmit = onSubmit;

            this.skills = subject.skills.skills.ToDictionary(v => v.def.defName, v => Utils.MutateCost(v));
        }


        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            foreach (var i in this.skills)
            {
                var skillName = i.Key;
                var operationCost = i.Value;
                if (operationCost == -1f)
                    continue;

                if (listingStandard.ButtonText($"{skillName} ({operationCost * 100})", $"Select to upgrade {skillName}"))
                {
                    this.onSubmit(skillName);
                    Close();
                }
                listingStandard.Gap();
            }

            listingStandard.End();
        }
    }
}
