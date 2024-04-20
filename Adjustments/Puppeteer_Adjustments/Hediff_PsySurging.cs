using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;
using VFECore;

namespace Adjustments.Puppeteer_Adjustments
{
    public class Hediff_PsySurging : HediffWithComps
    {
        public override string Label => "psy surging: " + Subject.LabelShort + " \n" + "That is the question";
        public Pawn Master;
        public Pawn Subject;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref Master, "hed-mm-mast");
            Scribe_Values.Look(ref Subject, "hed-mm-sub");

        }
    }
}
