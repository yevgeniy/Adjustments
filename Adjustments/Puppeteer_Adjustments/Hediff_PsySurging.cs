using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;
using VFECore;

namespace Adjustments.Puppeteer_Adjustments
{
    public class Hediff_PsySurging : HediffWithComps
    {

        public override string Label => string.Join("\n", Subjects.Select(v => "Psy surging: " + v.LabelShort));
        public Pawn Master;
        public List<Pawn> Subjects=new List<Pawn>();
        public override bool ShouldRemove => Subjects.Count == 0;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
        }


        public override void ExposeData()
        {
            
            base.ExposeData();

            Scribe_Deep.Look(ref Master, "hed-mm-mast");
            Scribe_Deep.Look(ref Subjects, "hed-mm-sub");

            if (Subjects == null)
                Subjects = new List<Pawn>();

        }
        public void AddSubject(Pawn n)
        {
            Subjects.Add(n);

        }
        public void RemoveSubject(Pawn n)
        {
            Subjects.RemoveAll(v => v == n);

        }
    }
}
