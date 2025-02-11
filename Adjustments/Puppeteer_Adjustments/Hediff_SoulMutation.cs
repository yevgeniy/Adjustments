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
using Verse.Noise;
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

        public int stageTicks = 0;
        private SoulMutationStage stage;
        private string selection;

        public Pawn Master = null;
        public List<Pawn> Subjects = new List<Pawn>();
        public List<string> Mutations = new List<string>();
        public Pawn Subject = null;
        private float cost;

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
                    return;
                }
            }
            else if (Type == SoulMutationType.Subject)
            {
                this.cost = Utils.MutateCost(Subject.skills.skills.FirstOrDefault(v => v.def.defName == selection));
            }

            Subject.health.capacities.Notify_CapacityLevelsDirty();
            Master.health.capacities.Notify_CapacityLevelsDirty();

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
                    return $"Skill upgrading: {this.selection} left: {this.cost*100f}";
                }
                else if (Stage == SoulMutationStage.Healing)
                {
                    return "Healing from soul mutation.";
                }
                else if (Stage == SoulMutationStage.Stable)
                {
                    return $"Mutated {string.Join(", ", Mutations.Distinct())}";
                }

                return "";
            }
        }



        public override void PostRemoved()
        {

            Subject.health.capacities.Notify_CapacityLevelsDirty();
            Master.health.capacities.Notify_CapacityLevelsDirty();
            base.PostRemoved();
        }

        public override void Tick()
        {
            stageTicks++;
            if (Stage == SoulMutationStage.Mutating && stageTicks % GenDate.TicksPerHour == 0)
            {
                if (Type == SoulMutationType.Master)
                {
                    this.shouldRemove = true;
                }
                else if (Type == SoulMutationType.Subject)
                {
                    ApplyPoint();
                    if (this.cost <= 0f)
                    {
                        DoUpgrade();
                        stageTicks = 0;
                        Stage = SoulMutationStage.Healing;
                        Subject.health.capacities.Notify_CapacityLevelsDirty();
                    }

                }
            }
            if (Stage == SoulMutationStage.Healing && stageTicks >= GenDate.TicksPerYear)
            {
                stageTicks = 0;
                Stage = SoulMutationStage.Stable;
                Subject.health.capacities.Notify_CapacityLevelsDirty();
            }
        }

        private void ApplyPoint()
        {
            var masterLeechHediff = Master.health.hediffSet.GetFirstHediffOfDef(Adjustments.BrainLeechingHediff) as Hediff_SoulLeech;
            if (masterLeechHediff == null)
            {
                Log.Message($"no soul reserve on master");
                return;
            }

            if (masterLeechHediff.TryDrawReservePoint(out var amt))
            {
                this.cost-=amt;
            }
            else
            {
                Log.Message($"no soul points left");
            }
        }

        private void DoUpgrade()
        {
            var skillRecord = Subject.skills.skills.FirstOrDefault(v => v.def.defName == this.selection);
            skillRecord.passion = (Passion)(((byte)skillRecord.passion) + 1);

            Mutations.Add(this.selection);
        }

        public void StopMutation()
        {
            var masterHediff = Master.health.hediffSet.GetFirstHediffOfDef(Defs.ADJ_SoulMutating_Hediff) as Hediff_SoulMutation;
            if (masterHediff != null)
            {
                masterHediff.shouldRemove = true;
            }

            var subjectHediff = Subject.health.hediffSet.GetFirstHediffOfDef(Defs.ADJ_SoulMutating_Hediff) as Hediff_SoulMutation;
            subjectHediff.Stage = SoulMutationStage.Stable;
            subjectHediff.stageTicks = 0;
            if (subjectHediff.Mutations.Count == 0)
            {
                subjectHediff.shouldRemove = true;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (Type == SoulMutationType.Master || (Type == SoulMutationType.Subject && Stage == SoulMutationStage.Mutating))
                yield return new Command_Action
                {
                    defaultLabel = "Stop Mutation",
                    defaultDesc = "Stop Mutation",
                    icon = ContentFinder<Texture2D>.Get("Effects/Technomancer/BreakLink/BreakLinkRockConstruct"),
                    action = delegate
                    {
                        StopMutation();
                    }
                };

            var basegiz = base.GetGizmos();
            foreach (var i in basegiz)
                yield return i;
        }



        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref Type, "hed-smut-type");
            Scribe_Values.Look(ref stage, "hed-smut-stage");
            Scribe_Values.Look(ref selection, "hed-selection-stage");
            Scribe_Values.Look(ref stageTicks, "hed-ticks-stage");
            Scribe_Values.Look(ref shouldRemove, "hed-shouldrem-stage");
            Scribe_Values.Look(ref cost, "hed-cost-stage");


            Scribe_References.Look(ref Master, "hed-smut-m");
            Scribe_References.Look(ref Subject, "hed-smut-subj");
            Scribe_Collections.Look(ref Mutations, "hed-muts-subj");

            if (Mutations == null)
                Mutations = new List<string>();

        }

    }

}
