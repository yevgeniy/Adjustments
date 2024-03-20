using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public class Mag_Hediff_Subjugation : HediffWithComps
    {
        static Mag_Hediff_Subjugation()
        {
            var subjhedd= DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Subjugation");
            if (subjhedd!=null)
            {
                subjhedd.hediffClass = typeof(Mag_Hediff_Subjugation);
                Log.Message("ATTACHED");
            }
            var subjabil = DefDatabase<VFECore.Abilities.AbilityDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Subjugation");
            if (subjabil!=null)
            {
                subjabil.modExtensions.Add(new Mag_SubjugateAbilityExtention());
                Log.Message("ATTACHED2");
            }
        }
        public Mag_Hediff_Subjugation() : base()
        {
            for (int i = 0; i < actEveryHour.Length; i++)
            {
                actEveryHour[i] = delegate { };
            }
            actEveryHour[0] = Act;
        }

        public Pawn MasterPawn;

        private Action[] actEveryHour = new Action[GenDate.TicksPerHour];

        private long tick;
        public override void Tick()
        {
            tick++;

            base.Tick();

            actEveryHour[tick % GenDate.TicksPerHour ]();
        }
        private void Act()
        {
            Log.Message("ACTING");
            if (pawn.guest == null)
                return;

            var mode = pawn.guest.interactionMode;
            if (mode.defName== "ReduceWill" || mode.defName== "Enslave")
            {
                Log.Message("LOWER WILL");
                LowerWill();
            }

        }

        private void LowerWill()
        {
            //if (pawn.guest.will > 0f)
            //{
            //    float statValue = 1f;
            //    statValue *= initiator.GetStatValue(StatDefOf.NegotiationAbility, true, -1);
            //    statValue = Mathf.Min(statValue, recipient.guest.will);
            //    float single = recipient.guest.will;
            //    recipient.guest.will = Mathf.Max(0f, recipient.guest.will - statValue);
            //    float single1 = recipient.guest.will;
            //    string str = "TextMote_WillReduced".Translate(single.ToString("F1"), recipient.guest.will.ToString("F1"));
            //    if (recipient.needs.mood != null && recipient.needs.mood.CurLevelPercentage < 0.4f)
            //    {
            //        str = str + ("\n(" + "lowMood".Translate()) + ")";
            //    }
            //    MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, str, 8f);
            //    if (recipient.guest.will == 0f)
            //    {
            //        TaggedString taggedString = "MessagePrisonerWillBroken".Translate(initiator, recipient);
            //        if (recipient.guest.interactionMode == PrisonerInteractionModeDefOf.AttemptRecruit)
            //        {
            //            taggedString = taggedString + " " + "MessagePrisonerWillBroken_RecruitAttempsWillBegin".Translate();
            //        }
            //        Messages.Message(taggedString, recipient, MessageTypeDefOf.PositiveEvent, true);
            //        return;
            //    }
            //}
            //else if (recipient.guest.interactionMode != PrisonerInteractionModeDefOf.ReduceWill)
            //{
            //    QuestUtility.SendQuestTargetSignals(recipient.questTags, "Enslaved", recipient.Named("SUBJECT"));
            //    GenGuest.EnslavePrisoner(initiator, recipient);
            //    if (!letterLabel.NullOrEmpty())
            //    {
            //        letterDef = LetterDefOf.PositiveEvent;
            //    }
            //    letterLabel = ("LetterLabelEnslavementSuccess".Translate() + ": ") + recipient.LabelCap;
            //    letterText = "LetterEnslavementSuccess".Translate(initiator, recipient);
            //    letterDef = LetterDefOf.PositiveEvent;
            //    lookTargets = new LookTargets(new TargetInfo[] { recipient, initiator });
            //    if (inspirationDef)
            //    {
            //        initiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Recruitment);
            //    }
            //    extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptAccepted);
            //}
        }
        private Pawn Instigator;
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);


            Log.Message("HAS DINFO: " + dinfo.HasValue);
            if (dinfo == null)
                return;

            Instigator =(Pawn)dinfo.Value.Instigator;

            Log.Message("instigator: " + Instigator);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (this.Severity >= 0.95f)
            {
                if (pawn.guest != null && pawn.Faction != Faction.OfPlayer)
                {
                    if (pawn.guest.Recruitable)
                    {
                        pawn.SetFaction(Faction.OfPlayer);
                        pawn.guest.SetGuestStatus(null);
                        pawn.guest.Released = true;
                    }
                    else
                    {
                        pawn.guest.Recruitable = true;
                    }
                }
                else if (pawn.IsPrisoner)
                {
                    pawn.guest.SetGuestStatus(null);
                    pawn.guest.Released = true;
                }
            }
        }
    }
}
