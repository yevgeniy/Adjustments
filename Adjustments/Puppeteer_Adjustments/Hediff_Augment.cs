using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments.Puppeteer_Adjustments
{
    public class Hediff_Augment: HediffWithComps
    {
        public Pawn Master;
        public Pawn Subject;

        private bool shouldRemove;
        private int startAt;

        public override bool ShouldRemove => shouldRemove;
        public override string Label => base.Label + ": " + (pawn == Master ? Subject : Master).LabelShort;

        public override void PostAdd(DamageInfo? dinfo)
        {
            startAt = Find.TickManager.TicksGame;
        }
        public override void Tick()
        {
            base.Tick();

            if (Find.TickManager.TicksGame > startAt + GenDate.TicksPerHour * 12f)
                shouldRemove = true;

            
        }

    }
}
