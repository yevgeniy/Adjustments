using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.GraphicsBuffer;

namespace Adjustments.Puppeteer_Adjustments
{
    public enum SoulMutationType
    {
        Master,
        Subject
    }
    public enum SoulMutationStage
    {
        Mutating,
        Healing,
        Stable
    }

    public class Hediff_SoulMutation : HediffWithComps, IMinHeatGiver
    {

        public SoulMutationType Type;
        private bool shouldRemove = false;
        public override bool ShouldRemove => this.shouldRemove;

        private int stageTicks = 0;
        private SoulMutationStage stage;
        private string selection;

        public Pawn Master = null;
        public List<Pawn> Subjects = new List<Pawn>();
        public Pawn Subject = null;

        public SoulMutationStage Stage
        {
            get
            {
                return stage;
            }
            set
            {
                stage = value;
            }
        }

        public int MinHeat => 50;
        public bool IsActive => !ShouldRemove;

        public void Activate(string selection)
        {
            this.selection = selection;
            Stage = SoulMutationStage.Mutating;
            stageTicks = 0;

            if (Type == SoulMutationType.Master)
            {
                if (Master.health.hediffSet.TryGetHediff(VPE_DefOf.VPE_PsycastAbilityImplant, out var h))
                {
                    var psyhediff = h as Hediff_PsycastAbilities;
                    psyhediff.AddMinHeatGiver(this);
                }
                else
                {
                    Log.Error($"COULD NOT FIND PSY CAST HEDIFF ON MASTER");
                }
            }

            Log.Message($"ACTIVATING {Type} {selection}");
        }



        public override string Label => Type == SoulMutationType.Master ? MasterLabel : SubjectLabel;
        public string MasterLabel
        {
            get
            {
                return $"Upgrading {this.selection} on {Subject}";

            }
        }
        public string SubjectLabel
        {
            get
            {
                if (Stage == SoulMutationStage.Mutating)
                {
                    return $"Skill upgrading: {this.selection}";
                }
                else if (Stage == SoulMutationStage.Healing)
                {
                    return "Healing from soul mutation.";
                }
                else if (Stage == SoulMutationStage.Stable)
                {
                    return "Mutated.";
                }

                return "";
            }
        }



        public override void PostRemoved()
        {
            if (Type == SoulMutationType.Subject)
            {
                Log.Error($"SOMEHOW SOUL MUTATION HEDIFF GOT REMOVED!");
            }

            base.PostRemoved();
        }

        public override void Tick()
        {
            stageTicks++;

            /*Mutation stage should last 1 day */
            if (Stage == SoulMutationStage.Mutating && stageTicks >= GenDate.TicksPerHour)
            //if (Stage == SoulMutationStage.Mutating && stageTicks >= GenDate.TicksPerDay)
            {
                if (Type == SoulMutationType.Master)
                {
                    this.shouldRemove = true;
                }
                if (Type == SoulMutationType.Subject)
                {
                    DoUpgrade();
                    stageTicks = 0;
                    Stage = SoulMutationStage.Healing;
                }
            }

            //if (Stage == SoulMutationStage.Healing && stageTicks >= GenDate.TicksPerYear)
            if (Stage == SoulMutationStage.Healing && stageTicks >= GenDate.TicksPerHour)
            {
                stageTicks = 0;
                Stage = SoulMutationStage.Stable;
            }

        }
        private void DoUpgrade()
        {
            var skillRecord = Subject.skills.skills.FirstOrDefault(v => v.def.defName == this.selection);
            var cost = Utils.MutateCost(skillRecord);
            if (cost == -1f)
            {
                Log.Error($"Something happened.  can't find cost of selection");
                this.shouldRemove = true;
                return;
            }

            var masterLeechHediff = Master.health.hediffSet.GetFirstHediffOfDef(Adjustments.BrainLeechingHediff) as Hediff_SoulLeech;
            if (masterLeechHediff == null)
            {
                Log.Error($"Something happened. Master codes not have leeching hediff.");
                this.shouldRemove = true;
                return;
            }

            if (masterLeechHediff.TryDrawReserve(cost, out var _))
            {
                skillRecord.passion = (Passion)(((byte)skillRecord.passion) + 1);

            }
            else
            {
                Log.Error($"Master does not have enought soul points.");
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref Type, "hed-smut-type");
            Scribe_Values.Look(ref stage, "hed-smut-stage");
            Scribe_Values.Look(ref selection, "hed-selection-stage");
            Scribe_Values.Look(ref stageTicks, "hed-ticks-stage");
            Scribe_Values.Look(ref shouldRemove, "hed-shouldrem-stage");


            Scribe_References.Look(ref Master, "hed-smut-m");
            Scribe_References.Look(ref Subject, "hed-smut-subj");

        }

    }

}
