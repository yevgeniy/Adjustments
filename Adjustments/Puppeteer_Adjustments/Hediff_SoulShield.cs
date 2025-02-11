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

namespace Adjustments.Puppeteer_Adjustments
{
    public class Hediff_SoulShield: HediffWithComps
    {
        public Pawn Master;
        public Pawn Subject;

        private bool shouldRemove;
        private int startAt;
        

        public override bool ShouldRemove => shouldRemove;
        public override string Label => $"Consc. shielded ({(Remaining-(originalConsc-lastConc)) *100f})";

        private float remaining;
        public float Remaining
        {
            get
            {
                return remaining;
            }
            set
            {
                remaining = value;
            }
        }
        public override void PostAdd(DamageInfo? dinfo)
        {
            startAt = Find.TickManager.TicksGame;

            Remaining = StartingSieldValue();
        }
        private float StartingSieldValue()
        {
            return .5f + .1f * GetMasterStatPoints();
        }

        public int GetMasterStatPoints()
        {
            return Utils.GetPsyStatPoints(Master);
        }

        private float originalConsc = 0f;
        private float lastConc=0f;
        public float Act(float current)
        {
            lastConc = current;
            Log.Message($"current: {current} remaining: {Remaining}");
            if (originalConsc == 0f)
            {
                Log.Message($"original: {originalConsc}");
                originalConsc = current;
            }

            if (current < originalConsc)
            {
                var dif = originalConsc - current;
                Log.Message($"dif: {dif} remaining: {Remaining}");
                if (dif > Remaining)
                {
                    Log.Message($"expired. resulting {dif - Remaining}");
                    shouldRemove = true;
                    return dif - Remaining;
                }
                else
                {
                    Log.Message($"still holding");
                    return originalConsc;
                }
            }

            Log.Message($"no difference or increased");
            return current;
        }
        public override void Tick()
        {
            base.Tick();

            if (
                Find.TickManager.TicksGame > startAt + GenDate.TicksPerHour * 12f)
            {
                shouldRemove = true;
            }
                
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look(ref Master, "hed-sh-m");
            Scribe_Deep.Look(ref Subject, "hed-sh-subj");
            Scribe_Values.Look(ref shouldRemove, "hed-sh-should-rem");
            Scribe_Values.Look(ref startAt, "hed-sh-start-at");
            Scribe_Values.Look(ref remaining, "hed-sh-rem");
            Scribe_Values.Look(ref originalConsc, "hed-orig-conc-rem");
            



    }

    }
}
