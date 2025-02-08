using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;

namespace Adjustments.Puppeteer_Adjustments
{
    public enum MindGrowthType
    {
        Master,
        Subject
    }
    public class Hediff_SoulGrowth : HediffWithComps
    {
        public Pawn Master;
        public Pawn Subject;
        public List<Pawn> Subjects = new List<Pawn>() { };

        public MindGrowthType Type;

        private bool isgrowthactive=true;
        public bool IsGrowthActive
        {
            get
            {
                return isgrowthactive;
            }
            set
            {
                isgrowthactive = value;
            }
        }
        private float currentRework;
        public float CurrentRework
        {
            get
            {
                return currentRework;
            }
            set
            {
                currentRework = value;
            }
        }

        public override bool ShouldRemove =>
            Type == MindGrowthType.Master && Subjects.Count == 0
            || Type == MindGrowthType.Subject && IsGrowthActive == false && CurrentRework == 0f;

        public override string Label => Type == MindGrowthType.Master ? MasterLabel : SubjectLabel;

        public string MasterLabel
        {
            get
            {
                return $"Remolding souls of: {string.Join(", ", Subjects)}";
            }
        }
        public string SubjectLabel
        {
            get
            {
                if (IsGrowthActive)
                    return $"Soul being reworked by {Master} ({CurrentRework * 100})";
                return $"Soul was reworked ({CurrentRework * 100})";
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {

        }
        private int totalTicks = 0;
        public override void Tick()
        {
            base.Tick();

            totalTicks++;
            if (totalTicks % GenDate.TicksPerHour == 0)
            {
                if (Type == MindGrowthType.Subject && IsGrowthActive)
                {
                    var masterSoulLeechHediff = Master.health.hediffSet.GetFirstHediffOfDef(Adjustments.BrainLeechingHediff) as Hediff_SoulLeech;
                    if (masterSoulLeechHediff == null)
                    {
                        Log.Error($"Master {Master} did not have the sould leech hediff!");
                    }

                    if (masterSoulLeechHediff.TryDrawReservePoint(out var point))
                    {
                        Log.Message($"applying a point {point}");
                        ApplyValue(point);
                    }
                    else
                    {
                        Log.Message($"no points on master");
                    }

                }
            }
        }

        private void ApplyValue(float v)
        {
            if (Type != MindGrowthType.Subject)
            {
                Log.Error($"Something is wrong.  Should only happen on Subject!");
                return;
            }
            if (CurrentRework>=1f)
            {
                Log.Message($"max soul growth reached");
                return;
            }

            var cur = CurrentRework * 100;
            var rate=1f;
            if (0 <= cur && cur < 10) rate = 1f;
            else if (10 <= cur && cur < 20) rate = 1f / 2f;
            else if (20 <= cur && cur < 30) rate = 1f / 3f;
            else if (30 <= cur && cur < 40) rate = 1f / 4f;
            else if (40 <= cur && cur < 50) rate = 1f / 5f;
            else if (50 <= cur && cur < 60) rate = 1f / 6f;
            else if (60 <= cur && cur < 70) rate = 1f / 7f;
            else if (70 <= cur && cur < 80) rate = 1f / 8f;
            else if (80 <= cur && cur < 90) rate = 1f / 9f;
            else if (90 <= cur && cur < 100) rate = 1f / 10f;

            Log.Message($"current: {CurrentRework * 100} Rate adjust: {rate} base: {v}");

            var apply = rate * v;

            CurrentRework += apply;
            Subject.health.capacities.Notify_CapacityLevelsDirty();
        }

        void StopRegrowing()
        {
            foreach (var i in Subjects)
            {
                if (i.health.hediffSet.TryGetHediff(Defs.ADJ_SoulGrowth_Hediff, out var h))
                {
                    var soulGrowthHediff = h as Hediff_SoulGrowth;
                    soulGrowthHediff.IsGrowthActive = false;
                }
                else
                {
                    Log.Error($"could no find soul growth hediff on subject {i}!");
                    return;
                }
            }
            Subjects.Clear();
        }

        void StopRegrowth()
        {

            IsGrowthActive = false;
            if (Master.health.hediffSet.TryGetHediff(Defs.ADJ_SoulGrowth_Hediff, out var h))
            {
                var soulGrowthHediff = h as Hediff_SoulGrowth;
                soulGrowthHediff.Subjects.Remove(Subject);
            }
            else
            {
                Log.Error($"count not find soul growth hediff on master {Master}!");
            }
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (Type == MindGrowthType.Master)
                yield return new Command_Action
                {
                    defaultLabel = "Stop regrowing",
                    defaultDesc = "Stop regrowing souls of all subjects",
                    icon = ContentFinder<Texture2D>.Get("Effects/Technomancer/BreakLink/BreakLinkRockConstruct"),
                    action = delegate
                    {
                        StopRegrowing();
                    }
                };
            else if (Type == MindGrowthType.Subject && IsGrowthActive)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Stop regrowth",
                    defaultDesc = "Make " + Master.LabelShort + " stop soul regrwoth on this subject",
                    icon = ContentFinder<Texture2D>.Get("Effects/Technomancer/BreakLink/BreakLinkRockConstruct"),
                    action = delegate
                    {
                        StopRegrowth();
                    }
                };
            }

            var basegiz = base.GetGizmos();
            foreach (var i in basegiz)
                yield return i;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.Look(ref Master, "hed-sg-m");
            Scribe_References.Look(ref Subject, "hed-sg-subj");
            Scribe_Collections.Look(ref Subjects, "nim-sg-subjs", LookMode.Reference);
            //Scribe_Values.Look(ref shouldRemove, "hed-mg-should-rem");

            Scribe_Values.Look(ref Type, "hed-sg-type-at");
            Scribe_Values.Look(ref currentRework, "hed-sg-currew-at");
            Scribe_Values.Look(ref isgrowthactive, "hed-sg-active-at");

            


            if (Subjects == null)
                Subjects = new List<Pawn>();

        }

    }
}
