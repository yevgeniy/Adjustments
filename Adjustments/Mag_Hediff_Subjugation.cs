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
            var mode = PrisonerInteractionModeDefOf.NoInteraction;
            if (pawn.guest != null)
            {
                mode = pawn.guest.interactionMode;
            }
                
            if (!pawn.IsPrisoner || mode.defName == "Convert" )
            {
                Log.Message("CONVERT");
                ConvertToIdeo();
            }
            else if (mode.defName== "ReduceWill" || mode.defName== "Enslave")
            {
                Log.Message("LOWER WILL");
                LowerWill();
            }
            else if (mode.defName== "AttemptRecruit" || mode.defName== "ReduceResistance")
            {
                Log.Message("LOWER RES");
                LowerRes();
            }
            

        }
        private void LowerRes()
        {
            if (pawn.guest.resistance>0f)
            {
                
                float statValue = MasterPawn.GetPsylinkLevel() * .01f;

                statValue = Mathf.Min(statValue, pawn.guest.resistance);
                float single6 = pawn.guest.resistance;
                pawn.guest.resistance = Mathf.Max(0f, pawn.guest.resistance - statValue);
                
                string str = "TextMote_ResistanceReduced".Translate(single6.ToString("F1"), pawn.guest.resistance.ToString("F1"));
                
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, str, 8f);
                if (pawn.guest.resistance == 0f)
                {
                    TaggedString taggedString1 = "MessagePrisonerResistanceBroken".Translate(pawn.LabelShort, MasterPawn.LabelShort, MasterPawn.Named("WARDEN"), pawn.Named("PRISONER"));
                    if (pawn.guest.interactionMode == PrisonerInteractionModeDefOf.AttemptRecruit)
                    {
                        taggedString1 = taggedString1 + " " + "MessagePrisonerResistanceBroken_RecruitAttempsWillBegin".Translate();
                    }
                    Messages.Message(taggedString1, pawn, MessageTypeDefOf.PositiveEvent, true);
                    return;
                }
            }
        }
        private void ConvertToIdeo()
        {
            if (pawn.Ideo.name == MasterPawn.Ideo.name)
                return;

            Precept_Role role;
            var basestat= MasterPawn.GetPsylinkLevel() * .01f;
            float statValue = 0.06f * basestat *  pawn.GetStatValue(StatDefOf.CertaintyLossFactor, true, -1) * ConversionUtility.ConversionPowerFactor_MemesVsTraits(MasterPawn, pawn, null) * ReliquaryUtility.GetRelicConvertPowerFactorForPawn(pawn, null) * Find.Storyteller.difficulty.CertaintyReductionFactor(MasterPawn, pawn);
            Ideo ideo = pawn.Ideo;
            if (ideo != null)
            {
                role = ideo.GetRole(pawn);
            }
            else
            {
                role = null;
            }
            Precept_Role preceptRole = role;
            if (preceptRole != null)
            {
                statValue *= preceptRole.def.certaintyLossFactor;
            }

            pawn.ideo.IdeoConversionAttempt(statValue, MasterPawn.Ideo, true);
        }

        private void LowerWill()
        {
            if (pawn.guest.will > 0f)
            {
                float statValue = MasterPawn.GetPsylinkLevel() * .01f;
                statValue = Mathf.Min(statValue, pawn.guest.will);

                float single = pawn.guest.will;
                pawn.guest.will = Mathf.Max(0f, pawn.guest.will - statValue);
                string str = "TextMote_WillReduced".Translate(single.ToString("F1"), pawn.guest.will.ToString("F1"));
                
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, str, 8f);
                if (pawn.guest.will == 0f)
                {
                    TaggedString taggedString = "MessagePrisonerWillBroken".Translate(MasterPawn, pawn);
                    if (pawn.guest.interactionMode == PrisonerInteractionModeDefOf.AttemptRecruit)
                    {
                        taggedString = taggedString + " " + "MessagePrisonerWillBroken_RecruitAttempsWillBegin".Translate();
                    }
                    Messages.Message(taggedString, pawn, MessageTypeDefOf.PositiveEvent, true);
                    return;
                }
            }

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
