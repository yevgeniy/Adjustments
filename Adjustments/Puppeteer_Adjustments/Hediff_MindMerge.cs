using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments.Puppeteer_Adjustments
{
    public class Hediff_MindMerge: HediffWithComps
    {
        public Pawn Master;
        public Pawn Subject;

        private bool shouldRemove;
        private int startAt;

        public override bool ShouldRemove => shouldRemove;
        public override string Label => pawn == Master
            ? ("Mind merging with: " + Subject.LabelShort)
            : ("Mind merged with: " + Master.LabelShort);
            
        public override void PostAdd(DamageInfo? dinfo)
        {
            startAt = Find.TickManager.TicksGame;
        }
        public override void Tick()
        {
            base.Tick();

            if (Find.TickManager.TicksGame > startAt + GenDate.TicksPerHour * 3f)
                shouldRemove = true;

        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref Master, "hed-mm-mast");
            Scribe_Values.Look(ref Subject, "hed-mm-sub");
            Scribe_Values.Look(ref shouldRemove, "hed-mm-should-rem");
            Scribe_Values.Look(ref startAt, "hed-mm-start-at");

        }

    }
}
