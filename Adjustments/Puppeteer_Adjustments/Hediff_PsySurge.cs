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
    public class Hediff_PsySurge : HediffWithComps
    {
  
        public Pawn Master;
        public Pawn Subject;

        public enum State
        {
            Surge,
            Paradox
        }

        private bool shouldRemove;
        private int lastCheck;
        private int Paradox;
        public State state;

        public override bool ShouldRemove => shouldRemove;
        public override string Label => state == State.Surge
            ? "psy surge from: " + Master.LabelShort
            : "dissipating paradox: " + Paradox;

        public override void PostAdd(DamageInfo? dinfo)
        {
            lastCheck = Find.TickManager.TicksGame;
            Paradox = 1;
            state = State.Surge;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {

            yield return new Command_Action
            {
                defaultLabel = "Cancel psychic surge",
                defaultDesc = "Cancel psychic surge and allow paradox to manifest.  New psycic surge cannot be established until all current paradox dissipates.",
                icon = ContentFinder<Texture2D>.Get("Effects/Technomancer/BreakLink/BreakLinkRockConstruct"),
                action = delegate
                {
                    CancelSurge();
                }
            };

            var basegiz = base.GetGizmos();
            foreach (var i in basegiz)
                yield return i;
        }

        private void CancelSurge()
        {
            state = State.Paradox;
            lastCheck = Find.TickManager.TicksGame;

            var h = Master.health.hediffSet.hediffs.FirstOrDefault(v => 
                v.def == Defs.ADJ_PsySurging && (v as Hediff_PsySurging).Master == Master && (v as Hediff_PsySurging).Subject==Subject 
            );
            if (h != null)
            {
                Log.Message("REMOVING");
                Master.health.RemoveHediff(h);
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (shouldRemove)
                return;

            if (state == State.Surge)
            {
                if (Find.TickManager.TicksGame > lastCheck + GenDate.TicksPerHour)
                {
                    lastCheck = Find.TickManager.TicksGame;

                    if (Paradox == 20)
                    {
                        CancelSurge();
                        return;
                    }

                    Paradox++;
                    Log.Message("PARADOX SURGE: " + Paradox);
                }
            }
            else if (state == State.Paradox)
            {
                if (Find.TickManager.TicksGame > lastCheck + GenDate.TicksPerHour / 3 /*20 mins*/)
                {
                    lastCheck = Find.TickManager.TicksGame;

                    Paradox--;
                    Log.Message("PARADOX empty: " + Paradox);
                    ApplyWound();

                    if (Paradox == 0)
                    {
                        shouldRemove = true;
                        return;
                    }
                }
            }

        }

        private void ApplyWound()
        {
            Log.Message("APPLY WOUND");
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref Master, "hed-sp-m");
            Scribe_Values.Look(ref Subject, "hed-sp-subj");
            Scribe_Values.Look(ref shouldRemove, "hed-sp-should-rem");
            Scribe_Values.Look(ref lastCheck, "hed-sp-lastch");
            Scribe_Values.Look(ref Paradox, "hed-sp-paradox");
            Scribe_Values.Look(ref state, "hed-sp-state");
        }

    }

}