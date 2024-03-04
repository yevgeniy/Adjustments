using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public class SubjugateComp : ThingComp
    {
        static SubjugateComp()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                    thingDef.race != null))
            {
                thingDef.comps.Add(new CompProperties { compClass = typeof(SubjugateComp) });
            }
        }

        public void ActivateSubjugation()
        {
            Log.Message("SUBJUCATION ACTIVATE");
        }
    }
}
