using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using static HarmonyLib.Code;

namespace Adjustments.Puppeteer_Adjustments
{
    public class Hediff_PsySurge: HediffWithComps, IMinHeatGiver
    {
        public Pawn Master;
        public Pawn Subject;

        enum State
        {
            Surge,
            Paradox
        }

        private bool shouldRemove;
        private int lastCheck;
        private int Paradox;
        private State state;
        
        public override bool ShouldRemove => shouldRemove;
        public override string Label => state == State.Surge
            ? "psy surge from: " + Master.LabelShort
            : "dissipating paradox: " + Paradox;

        bool IMinHeatGiver.IsActive => state == State.Surge;

        int IMinHeatGiver.MinHeat => 20;

        public override void PostAdd(DamageInfo? dinfo)
        {
            lastCheck = Find.TickManager.TicksGame;
            Paradox = 1;
            state = State.Surge;
            Master.Psycasts().AddMinHeatGiver(this);
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
        }

        public override void Tick()
        {
            base.Tick();

            if (shouldRemove)
                return;

            if (state==State.Surge)
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
                }
            }
            else if (state==State.Paradox)
            {
                if (Find.TickManager.TicksGame > lastCheck + GenDate.TicksPerHour/3 /*20 mins*/)
                {
                    lastCheck = Find.TickManager.TicksGame;

                    Paradox--;
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
